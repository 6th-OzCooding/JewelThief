using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using UnityEngine;

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
    private WFCMapGeneration _wfcMapGeneration = new();
    private UserDataManager _userDataManager = new();
    private ShopManager _shopManager = new();

    private LobbyController _lobbyController;
    private GameObject _jewelPuzzleInstance;

    #endregion

    #region Variables

    [Header("Test Options")]
    [SerializeField] private bool _skipStartupUIForTest;

    private bool _isInGame = false;
    private bool _isPaused = false;

    private Transform _mapRoot = null;
    private Transform _poolRoot = null;

    private GameObject _lobbyPrefab;
    private GameObject _lobbyInstance;

    private string[] _removeToolIdsWhenInGameExit = { "Item_Tool_MasterKey", };

    #endregion

    #region Events

    public event Action<string[]> OnExitInGame;

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

    // 전역 데이터 추가
    public int _gold;
    public string _selectedStageId;

    public int Gold => _gold;

    // 골드 증가 (판매소 등)
    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        _gold += amount;
    }

    // 골드 차감 시도 (상점 등)
    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || _gold < amount)
            return false;

        _gold -= amount;
        return true;
    }

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
    }

    private async UniTaskVoid InitAsync()
    {
        if (_skipStartupUIForTest)
        {
            await Resource.Init();
            InitNonAsync();

            PlayerController playerController = EnterLobby(true);
            UI.ShowStartupUIOnGameStart(playerController);
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
        PoolInit();
    }

    private void Update()
    {
        if(_isInGame && !_isPaused)
        {
            _alertManager.OnUpdate();
        }
    }

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
                return _lobbyController.Enter();
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
            return _lobbyController.Enter();
        }

        return null;
    }

    public void EnterInGame(string StageId)
    {
        GenerateMap();

        if (_lobbyInstance != null)
            _lobbyInstance.SetActive(false);

        StageData stageData = _dataTable.GetStageData(StageId);
        if (stageData != null)
        {
            _soundManager.PlayBGM(SoundId.BGM_PlayTheme);
            _alertManager.Init(stageData.TimeLimit);
        }

        _isInGame = true;
        _isPaused = false;
    }

    /// <summary>
    /// InGame 이탈 시점 호출
    /// </summary>
    // 경찰에게 잡힘 판정 임시용 bool isCaught = false (추후 수정)
    public void ExitInGame(bool isCaught = false)
    {
        _isInGame = false;
        _isPaused = false;

        float leftTime = _alertManager.GetRemainingTime();

        int currentStageEarnedGold = 0;
        string bestGemName = "없음";

        if (JewelInventoryManager.Instance != null)
        {
            // 경찰에게 잡힘 판정을 받아오는 곳 알면 추후 수정
            // 경찰에게 잡혔다면 정산 전 몰수 처리 먼저 수행
            if (isCaught)
            {
                JewelInventoryManager.Instance.ClearAllJewelsOnCaught();
            }

            // 이번 판에서 번 순수 골드 (현재 가방 총액 - 스테이지 시작 시 가방 총액)
            // 잡혔다면 몰수되어 0원이 나옵니다.
            currentStageEarnedGold = JewelInventoryManager.Instance.GetCurrentStageScore();

            // 이번 판에서 얻은 가장 비싼 보석 이름
            // 잡혔다면 "없음"이 나옵니다.
            bestGemName = JewelInventoryManager.Instance.GetMostExpensiveJewelName();
        }
        else
        {
            Debug.LogError("JewelInventoryManager 인스턴스를 찾을 수 없어 0점 처리합니다.");
        }

        UIBase popupBase = UI.OpenPopupUI(UIType.ScorePopupUI);
        if (popupBase != null && popupBase.TryGetComponent(out ScorePopupUI scoreUI))
        {
            // 실제 가방에서 긁어온 데이터와 남은 시간, 체포 여부 전달
            // 경찰에게 잡힘 판정은 추후 구현이므로 임시로 false 전달
            scoreUI.DisplayScore(currentStageEarnedGold, bestGemName, leftTime, false);
        }

        _wfcMapGeneration.Release();

        OnExitInGame?.Invoke(_removeToolIdsWhenInGameExit);

        EnterLobby();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void GenerateMap()
    {
        // TODO(김익환 2026-06-25): 맵 로딩 ui 필요
        if(null == _mapRoot)
        {
            _mapRoot = Utils.CreateEmptyGameObject("MapRoot", this.gameObject.transform).transform;
        }

        _wfcMapGeneration.StartGenerateMap(_mapRoot).Forget();
    }

    /// <summary>
    /// 현재 인게임 상태를 유지한 채 게임플레이 진행을 일시정지합니다.
    /// </summary>
    public void PauseGame()
    {
        _isPaused = true;
    }

    /// <summary>
    /// 게임플레이 일시정지를 해제합니다.
    /// </summary>
    public void ResumeGame()
    {
        _isPaused = false;
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
