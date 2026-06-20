using Cysharp.Threading.Tasks;
using System;

public class GameManager : SingletonBehaviour<GameManager>
{
    public static ResourceManager Resource { get { return Instance._resourceManager; } }
    public static SoundManager Sound { get { return Instance._soundManager; } }
    public static PoolManager Pool { get { return Instance._poolManager; } }
    public static AlertManager Alert { get { return Instance._alertManager; } }
    public static DataTable DataTable { get { return Instance._dataTable; } }
    

    #region Manager Varialbes

    private ResourceManager _resourceManager = new();
    private SoundManager _soundManager = new();
    private PoolManager _poolManager = new();
    private AlertManager _alertManager = new();
    private DataTable _dataTable = new();

    #endregion


    /// <summary>
    /// 데이터 드리븐 초기화 -> 로딩(어드레서블 불러오기) -> 사운드 및 풀 초기화
    /// </summary>
    protected override void Init()
    {
        base.Init();

        _dataTable.LoadAllData();
        InitAsync().Forget();
    }

    private void Update()
    {
        _alertManager.OnUpdate();
    }

    private async UniTaskVoid InitAsync()
    {
        UIBase loadingUIBase = UIManager.Instance.OpenLoadingUI();

        if (loadingUIBase != null && loadingUIBase.TryGetComponent(out LoadingUI loadingUI))
        {
            await loadingUI.StartLoading();
        }
        else
        {
            throw new Exception("Failed to open loading UI");
        }

        InitNonAsync();
    }

    private void InitNonAsync()
    {
        _soundManager.Init(this.gameObject);
        _poolManager.Init();
    }

    public void EnterGamePlay()
    {
        UIManager.Instance.CloseUI(UIType.TitleUI);
        UIManager.Instance.EnterGameplayCursorMode();

        // 추후 게임 플레이어 입장 시 필요한 로직 추가
        // TODO(김경훈 2026-06-20): 본부 - 선택된 스테이지 Id로 교체 필요. 현재는 테스트용 고정값.
        StageData stageData = _dataTable.GetStageData("Stage_01");
        if (stageData != null)
        {
            _soundManager.PlayBGM(SoundId.BGM_PlayTheme);
            _alertManager.Init(stageData.TimeLimit - 60);   // TODO(김경훈 2026-06-20): 테스트용으로 스테이지 시작 전 60초를 제외하고 시작하도록 설정.
        }
    }

    /// <summary>
    /// InGame 이탈 시점 호출
    /// </summary>
    public void ExitGamePlay()
    {

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
