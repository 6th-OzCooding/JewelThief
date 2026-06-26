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

    #endregion

    #region Variables

    [Header("Test Options")]
    [SerializeField] private bool _skipStartupUIForTest;

    private bool _isPlaying = false;

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

    public bool IsPlaying => _isPlaying;

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
            UI.ShowInventorySystemTestUI();
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
        if(_isPlaying)
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
            else
            {
                _lobbyInstance = Instantiate(_lobbyPrefab);
                _lobbyInstance.SetActive(true);

                if (_lobbyInstance.TryGetComponent(out _lobbyController))
                    return _lobbyController.Enter();
                else
                    Debug.LogError("Lobby 프리팹에 LobbyController 컴포넌트가 없습니다.");
            }
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

        _isPlaying = true;
    }

    /// <summary>
    /// InGame 이탈 시점 호출
    /// </summary>
    public void ExitInGame()
    {
        _isPlaying = false;

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

    // 보석 인벤토리 열림
    public void PauseGameForPuzzle()
    {
        _isPlaying = false;
    }

    // 보석 이벤토리 닫힘
    public void ResumeGameFromPuzzle()
    {
        _isPlaying = true;
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
