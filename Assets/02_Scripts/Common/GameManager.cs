using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : SingletonBehaviour<GameManager>
{
    public static ResourceManager Resource { get { return Instance._resourceManager; } }
    public static SoundManager Sound { get { return Instance._soundManager; } }
    public static PoolManager Pool { get { return Instance._poolManager; } }
    public static AlertManager Alert { get { return Instance._alertManager; } }
    public static DataTable DataTable { get { return Instance._dataTable; } }
    public static UIManager UI { get { return Instance._uiManager; } }
    public static UserDataManager UserData { get { return Instance._userDataManager; } }
    public static ShopManager Shop { get { return Instance._shopManager; } }

    #region Manager Varialbes

    private ResourceManager _resourceManager = new();
    private SoundManager _soundManager = new();
    private PoolManager _poolManager = new();
    private AlertManager _alertManager = new();
    private DataTable _dataTable = new();
    private UIManager _uiManager = new();
    private UserDataManager _userDataManager = new();
    private ShopManager _shopManager = new();

    private WFCMapGeneration _wfcMapGeneration;
    private LobbyController _lobbyController;
    private GameObject _jewelPuzzleInstance;

    #endregion

    #region Variables

    [Header("Test Options")]
    [SerializeField] private bool _skipStartupUIForTest;
    [Header("InGame Spawn")]
    [SerializeField] private float _inGameSpawnHeightOffset = 1f;

    public bool _isInGame { get; private set; } = false;
    public bool _isPaused { get; private set; } = false;

    public bool IsEnteringInGame { get; private set; } = false;

    private PlayerController _playerController;

    private Transform _mapRoot = null;
    private Transform _poolRoot = null;

    private GameObject _lobbyPrefab;
    private GameObject _lobbyInstance;

    private NavMeshSurface _navMeshSurface = null;

    private string[] _removeToolIdsWhenInGameExit = { "Item_Tool_MasterKey", };

    #endregion

    #region Events

    public event Action<string[]> OnExitInGame;
    public event Action OnPlayerCaught;
    public event Action OnPlayerEscape;

    #endregion

    #region Getters

    /// <summary>
    /// 현재 플레이어가 실제 스테이지 플레이 상태에 있는지 반환합니다.
    /// </summary>
    public bool IsInGame => _isInGame;

    /// <summary>
    /// 현재 게임플레이가 일시정지 상태인지 반환합니다.
    /// </summary>
    public bool IsPaused => _isPaused;

    #endregion

    #region Gold_Info

    // 전역 데이터 추가
    public int Gold { get; private set; }
    public string SelectedStageId;

    // 골드 증가 (판매소 등)
    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        Gold += amount;
    }

    // 골드 차감 시도 (상점 등)
    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || Gold < amount)
            return false;

        Gold -= amount;
        return true;
    }

    #endregion

    /// <summary>
    /// 데이터 드리븐 초기화 -> UIManager 초기화 -> 로딩(어드레서블 불러오기) -> 사운드 및 풀 초기화
    /// </summary>
    protected override void Init()
    {
        base.Init();

        _dataTable.LoadAllData();

        _userDataManager.Init();
        _userDataManager.LoadUserData();

        _uiManager.Init();
        InitAsync().Forget();

        AddGold(2000);
    }

    private async UniTaskVoid InitAsync()
    {
        if (_skipStartupUIForTest)
        {
            await Resource.Init();
            InitNonAsync();

            _playerController = EnterLobby(true);
            UI.ShowStartupUIOnGameStart(_playerController);
            return;
        }

        UIBase loadingUIBase = UI.OpenLoadingUI();


        if (loadingUIBase == null)
        {
            throw new Exception("Failed to open loading UI");
        }
        else if (!loadingUIBase.TryGetComponent(out LoadingUI loadingUI))
        {
            throw new Exception("Failed to get LoadingUI component from loading UI");
        }
        else
        {
            await loadingUI.StartLoading();
        }

        InitNonAsync();

        UI.OpenUI(UIRootType.ContentUI, UIType.TitleUI);
    }

    private void InitNonAsync()
    {
        _soundManager.Init(this.gameObject);
        _wfcMapGeneration = new();
        PoolInit();
    }

    private void Update()
    {
        if(_isInGame && !_isPaused)
        {
            _alertManager.OnUpdate();
        }
    }

    #region GameFlow

    public PlayerController EnterLobby(bool isFirstEnter = false)
    {
        if(isFirstEnter)
            UI.CloseUI(UIType.TitleUI);

        if(isFirstEnter)
        {
            _lobbyPrefab = _resourceManager.GetLoadedAsset<GameObject>("Lobby");
            if (_lobbyPrefab == null)
            {
                Debug.LogError("Lobby 프리팹을 로드하지 못했습니다.");
                return null;
            }

            _lobbyInstance = Instantiate(_lobbyPrefab);
            _lobbyInstance.SetActive(true);

            // --- [추가] 보석 인벤토리 시스템 소환 ---
            GameObject puzzlePrefab = _resourceManager.GetLoadedAsset<GameObject>("JewelInventory");
            if (puzzlePrefab != null)
            {
                Vector3 spawnPosition = new Vector3(10000f, 10000f, 10000f);
                _jewelPuzzleInstance = Instantiate(puzzlePrefab, spawnPosition, Quaternion.identity);
                _jewelPuzzleInstance.SetActive(true);
            }
            else
            {
                Debug.LogError("JewelInventory 프리팹을 찾을 수 없습니다.");
            }

            if (_lobbyInstance.TryGetComponent(out _lobbyController))
            {
                _playerController = _lobbyController.Enter();
                return _playerController;
            }
            else
                Debug.LogError("Lobby 프리팹에 LobbyController 컴포넌트가 없습니다.");
        }
        else
        {
            if (_lobbyInstance == null || _lobbyController == null)
            {
                Debug.LogError("Lobby 인스턴스가 생성되지 않았습니다.");
                return null;
            }

            _lobbyInstance.SetActive(true);
            _playerController = _lobbyController.Enter();
            return _playerController;
        }

        return null;
    }

    public void EnterInGame(string StageId)
    {
        EnterInGameAsync(StageId).Forget();

    }

    private async UniTaskVoid EnterInGameAsync(string stageId)
    {
        IsEnteringInGame = true;
        UI.OpenInGameLoadingUI();

        if (_lobbyInstance != null)
            _lobbyInstance.SetActive(false);

        await GenerateMap();

        RespawnPlayerToStartTile();

        StageData stageData = _dataTable.GetStageData(stageId);
        if (stageData != null)
        {
            _soundManager.PlayBGM(SoundId.BGM_PlayTheme);
            _alertManager.Init(stageData.TimeLimit);
        }

        if (JewelInventoryManager.Instance != null)
            JewelInventoryManager.Instance.InitStageStartPrice();

        _isInGame = true;
        _isPaused = false;

        await UniTask.Delay(2000);  // 플레이어 이동이 노출되는 것을 방지

        UI.CloseInGameLoadingUI();
        IsEnteringInGame = false;
    }

    private void RespawnPlayerToStartTile()
    {
        if (_playerController == null)
        {
            Debug.LogError("PlayerController가 없어 시작 좌표 재스폰을 건너뜁니다.");
            return;
        }

        if (!_wfcMapGeneration.TryGetStartTileWorldPosition(out Vector3 startPosition))
        {
            Debug.LogError("시작 타일 좌표를 찾지 못해 재스폰을 건너뜁니다.");
            return;
        }

        _playerController.Teleport(startPosition + Vector3.up * _inGameSpawnHeightOffset);
    }

    public void ReturnToLobby()
    {
        _isInGame = false;
        _isPaused = false;

        _wfcMapGeneration.Release();

        OnExitInGame?.Invoke(_removeToolIdsWhenInGameExit);

        EnterLobby();
        RespawnPlayerToLobby();
    }

    private void RespawnPlayerToLobby()
    {
        if (_playerController == null || _lobbyController == null)
            return;

        _playerController.Teleport(_lobbyController.SpawnPosition);
    }

    public void GameOver()
    {
        _isPaused = true;

        int totalValue = 0;
        string bestGemName = "";
        if (JewelInventoryManager.Instance != null)
        {
            totalValue = JewelInventoryManager.Instance.GetCurrentStageScore();
            bestGemName = JewelInventoryManager.Instance.GetMostExpensiveJewelName();          
        }

        float remainingTime = _alertManager != null ? _alertManager.GetRemainingTime() : 0f;

        OnPlayerCaught?.Invoke();
        _playerController.ResetPlayerStat();        

        ScorePopupUI scorePopupUI = UI.OpenScorePopupUI();
        if (scorePopupUI != null)
        {
            scorePopupUI.DisplayScore(totalValue, bestGemName, remainingTime, isCaught: true);
        }
        else
        {
            Debug.LogError("ScorePopupUI를 열지 못했습니다.");
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    #endregion

    private async UniTask GenerateMap()
    {
        if (null == _mapRoot)
        {
            _mapRoot = Utils.CreateEmptyGameObject("MapRoot", this.gameObject.transform).transform;
            _navMeshSurface = Utils.GetOrAddComponent<NavMeshSurface>(_mapRoot.gameObject);
            _navMeshSurface.collectObjects = CollectObjects.Children;
            _navMeshSurface.layerMask = LayerMask.GetMask("Floor");
        }

        await _wfcMapGeneration.StartGenerateMap(_navMeshSurface, _mapRoot);
    }

    public void PauseGame()
    {
        _isPaused = true;
    }

    public void ResumeGame()
    {
        _isPaused = false;
    }

   private void EscapeSuccessful()  // TODO (한재덕 - 26.06.29) 탈출 성공시 메서드 호출 필요
    {
        _isPaused = true;

        int totalValue = 0;
        string bestGemName = "";
        if (JewelInventoryManager.Instance != null)
        {
            bestGemName = JewelInventoryManager.Instance.GetMostExpensiveJewelName();
            totalValue = JewelInventoryManager.Instance.GetStageScoreAndFinalize();
        }

        OnPlayerEscape?.Invoke();
        float remainingTime = _alertManager != null ? _alertManager.GetRemainingTime() : 0f;

        ScorePopupUI scorePopupUI = UI.OpenScorePopupUI();
        if (scorePopupUI != null)
        {
            scorePopupUI.DisplayScore(totalValue, bestGemName, remainingTime, isCaught: false);
        }
        else
        {
            Debug.LogError("ScorePopupUI를 열지 못했습니다.");
        }
    }

    private void PoolInit()
    {
        if (null == _poolRoot)
        {
            _poolRoot = Utils.CreateEmptyGameObject("PoolRoot", this.gameObject.transform).transform;
        }

        _poolManager.Init(_poolRoot);
    }
}
