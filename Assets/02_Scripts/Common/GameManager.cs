using Cysharp.Threading.Tasks;
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

    #region Manager Varialbes

    private ResourceManager _resourceManager = new();
    private SoundManager _soundManager = new();
    private PoolManager _poolManager = new();
    private AlertManager _alertManager = new();
    private DataTable _dataTable = new();
    private UIManager _uiManager = new();
    private WFCMapGeneration _wfcMapGeneration = new();
    private UserDataManager _userDataManager = new();

    #endregion

    #region Variables

    [Header("Test Options")]
    [SerializeField] private bool _skipStartupUIForTest;

    private bool _isPlaying = false;

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
        _poolManager.Init();
    }

    private void Update()
    {
        if(_isPlaying)
        {
            _alertManager.OnUpdate();

        }
    }

    public void EnterLobby()
    {
        UI.CloseUI(UIType.TitleUI);
        UI.EnterGameplayCursorMode();

        GameObject lobbyPrefab = _resourceManager.GetLoadedAsset<GameObject>("Lobby");
        if (lobbyPrefab == null)
        {
            Debug.LogError("Lobby 프리팹을 로드하지 못했습니다.");
        }
        else
        {
            GameObject lobbyInstance = Instantiate(lobbyPrefab);

            if (lobbyInstance.TryGetComponent(out LobbyController lobbyController))
                lobbyController.Enter();
            else
                Debug.LogError("Lobby 프리팹에 LobbyController 컴포넌트가 없습니다.");
        }
    }

    public void EnterGamePlay(string StageId)
    {
        // TODO(김익환 2026-06-21): 맵 로딩 ui가 필요한지 몰라서 일단은 로딩화면 없이 바로 생성
        _wfcMapGeneration.StartGenerateMap().Forget();

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
    public void ExitStage()
    {
        _isPlaying = false;

        _wfcMapGeneration.Release();

        // 점수 계산
        int finalScore = JewelPuzzleUIManager.Instance.GetTotalBagPrice();
        string bestName = JewelPuzzleUIManager.Instance.GetMostExpensiveJewelName();
         // 경찰에 잡혔는지 안잡혔는지를 나중에 확인하는 것을 보안 해서 후추

        // 보석 인벤토리 열려있다면 닫기
        if (JewelPuzzleUIManager.Instance != null && JewelPuzzleUIManager.Instance.IsPuzzleActive)
        {
            JewelPuzzleUIManager.Instance.ClosePuzzleInventory();
        }

        // TODO(김익환 2026-06-21): 본부로 이동

        UI.ExitGameplayCursorMode();

        // 스코어 팝업 UI 열기
        UIBase uiBase = UI.OpenPopupUI(UIType.ScorePopupUI);
        if (uiBase != null && uiBase.TryGetComponent(out ScorePopupUI scoreUI))
        {
            scoreUI.DisplayScore(finalScore, bestName, false);
        }

        // 인벤토리 보석 비우기
        JewelPuzzleUIManager.Instance.ClearAllJewelsOnCaught();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // 보석 인벤토리 열고 닫기 관리
    public void ToggleJewelPuzzleUI()
    {
        Debug.Log("ToggleJewelPuzzleUI 호출됨!");

        if (JewelPuzzleUIManager.Instance == null)
        {
            Debug.LogError("JewelPuzzleUIManager.Instance가 null입니다!");
            return;
        }

        if (JewelPuzzleUIManager.Instance.IsPuzzleActive)
        {
            JewelPuzzleUIManager.Instance.ClosePuzzleInventory();
        }
        else
        {
            JewelPuzzleUIManager.Instance.OpenPuzzleInventory();
        }
    }

    // 보석 인벤토리 열려있는 동안 게임 진행관련 관리
    public void PauseGameForPuzzle()
    {
        // 보석 인벤토리를 플레리어가 보고 있는 동안
        // 실제 플레이어와 게임에 영향이 가지 않도록 조치 필요
        // 1. 스테이지 타이머 일시 정지
        // 2. 플레이어 무적 및 이동 불가 처리
        // 3. 씬 내의 몬스터들 행동 정지
        // 후추 필요

        PlayerInputHandler input = FindAnyObjectByType<PlayerInputHandler>();
        if (input != null) input.SetMode(PlayerInputMode.UIOnly);

        Debug.Log("퍼즐 룸에 진입하여 게임 진행이 일시 정지되었습니다. (물리는 정상 작동)");
    }

    // 보석 인벤토리 닫힐때 게임 진행관련 관리
    public void ResumeGameFromPuzzle()
    {
        // 보석 인벤토리를 닫았으니 조치 했던 것들 
        // 정지 처리등 해제 필요
        // 후추 필요

        PlayerInputHandler input = FindAnyObjectByType<PlayerInputHandler>();
        if (input != null) input.SetMode(PlayerInputMode.Gameplay);

        Debug.Log("퍼즐 룸에서 나와 게임이 재개되었습니다.");
    }
}
