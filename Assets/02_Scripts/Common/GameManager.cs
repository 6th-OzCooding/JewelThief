
using Cysharp.Threading.Tasks;
using System.Diagnostics;

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



    protected override void Init()
    {
        base.Init();

        _dataTable.LoadAllData();
        InitAsync().Forget();
    }

    private async UniTaskVoid InitAsync()
    {

        // TODO(김익환 2026-06-14): 리소스 비동기로 미리 로드하기 로딩창에서 로딩할 것임
        // 추후 로딩 UI가 생기면 아래 Init함수의 매개변수로 로딩 진행률을 전달할 수 있도록 수정하기
        await _resourceManager.Init();


        _soundManager.Init(this.gameObject);
        _poolManager.Init();
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
