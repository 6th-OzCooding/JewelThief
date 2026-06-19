
using Cysharp.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

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
        UIBase loadingUIBase = UIManager.Instance.OpenLoadingUI();

        if (loadingUIBase != null && loadingUIBase.TryGetComponent(out LoadingUI loadingUI))
        {
            await loadingUI.StartLoading();
        }
        else
        {
            await _resourceManager.Init();
        }

        _soundManager.Init(this.gameObject);
        _poolManager.Init();

        UIManager.Instance.CloseLoadingUI();
        UIManager.Instance.ExitGameplayCursorMode();
        UIManager.Instance.OpenUI(UIRootType.MainUI, UIType.TitleUI);
    }

    public void EnterGamePlayer()
    {
        UIManager.Instance.CloseUI(UIType.TitleUI);

        UIManager.Instance.EnterGameplayCursorMode();

        // 후추
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
