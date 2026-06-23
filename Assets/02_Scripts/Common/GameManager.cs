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

    #region Manager Varialbes

    private ResourceManager _resourceManager = new();
    private SoundManager _soundManager = new();
    private PoolManager _poolManager = new();
    private AlertManager _alertManager = new();
    private DataTable _dataTable = new();
    private UIManager _uiManager = new();
    private WFCMapGeneration _wfcMapGeneration = new();

    #endregion

    #region Variables

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
        _uiManager.Init();
        InitAsync().Forget();
    }

    private async UniTaskVoid InitAsync()
    {
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

        // TODO(김익환 2026-06-21): 본부로 이동
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
