/// <summary>
/// UI 프리팹이 배치될 Canvas 루트 종류입니다.
/// </summary>
public enum UIRootType
{
    None = 0,
    BackgroundUI,
    MainUI,
    ContentUI,
    PopupUI,
    VeryFrontUI
}

/// <summary>
/// UIManager가 생성하고 열 수 있는 UI 종류입니다.
/// </summary>
public enum UIType
{
    None = 0,
    MainUI,
    LoadingUI
}

/// <summary>
/// UIManager에서 자주 쓰는 UI 경로와 열기 기능을 모아둔 확장 클래스입니다.
/// </summary>
public static class UIManagerExtension
{
    /// <summary>
    /// UI 루트 종류와 UI 종류를 Addressables 주소 문자열로 변환합니다.
    /// </summary>
    public static string GetUIAddress(this UIManager uiManager, UIRootType uiRootType, UIType uiType)
    {
        return $"Prefabs/UI/{uiRootType}/{uiType}";
    }

    /// <summary>
    /// 게임 시작 시 기본으로 필요한 UI를 여는 진입점입니다.
    /// </summary>
    public static async Cysharp.Threading.Tasks.UniTask ShowStartupUIOnGameStartAsync(this UIManager uiManager)
    {
        await uiManager.OpenUIAsync(UIRootType.VeryFrontUI, UIType.LoadingUI);
        await uiManager.OpenUIAsync(UIRootType.MainUI, UIType.MainUI);
    }

    /// <summary>
    /// 로딩 UI를 엽니다.
    /// </summary>
    public static Cysharp.Threading.Tasks.UniTask<UIBase> OpenLoadingUIAsync(this UIManager uiManager)
    {
        return uiManager.OpenUIAsync(UIRootType.VeryFrontUI, UIType.LoadingUI);
    }

    /// <summary>
    /// 로딩 UI를 닫습니다.
    /// </summary>
    public static void CloseLoadingUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.LoadingUI);
    }
}
