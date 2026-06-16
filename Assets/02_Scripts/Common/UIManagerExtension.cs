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
    LoadingUI,
    CenterPointUI
}

/// <summary>
/// UIManager에서 자주 쓰는 UI 경로와 열기 기능을 모아둔 확장 클래스입니다.
/// </summary>
public static class UIManagerExtension
{
    /// <summary>
    /// UI 루트 종류와 UI 종류를 Resources 폴더 기준 경로 문자열로 변환합니다.
    /// </summary>
    public static string GetUIPath(this UIManager uiManager, UIRootType uiRootType, UIType uiType)
    {
        return $"Prefabs/UI/{uiRootType}/{uiType}";
    }

    /// <summary>
    /// 게임 시작 시 기본으로 필요한 UI를 여는 진입점입니다.
    /// </summary>
    public static void ShowStartupUIOnGameStart(this UIManager uiManager)
    {
        uiManager.OpenLoadingUI();
        uiManager.OpenUI(UIRootType.MainUI, UIType.MainUI);
        uiManager.OpenCenterPointUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LockGameplayCursor();
        }
    }

    /// <summary>
    /// 로딩 UI를 엽니다.
    /// </summary>
    public static UIBase OpenLoadingUI(this UIManager uiManager)
    {
        return uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
    }

    /// <summary>
    /// 로딩 UI를 닫습니다.
    /// </summary>
    public static void CloseLoadingUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.LoadingUI);
    }

    /// <summary>
    /// 게임 플레이 중 화면 중앙 기준점을 보여주는 UI를 엽니다.
    /// </summary>
    public static UIBase OpenCenterPointUI(this UIManager uiManager)
    {
        return uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.CenterPointUI);
    }

    /// <summary>
    /// 게임 플레이 중 화면 중앙 기준점을 보여주는 UI를 닫습니다.
    /// </summary>
    public static void CloseCenterPointUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.CenterPointUI);
    }

    /// <summary>
    /// 게임 플레이 화면용 중앙 포인터 UI를 켜고 마우스 커서를 잠급니다.
    /// </summary>
    public static void EnterGameplayCursorMode(this UIManager uiManager)
    {
        uiManager.OpenCenterPointUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LockGameplayCursor();
        }
    }

    /// <summary>
    /// UI 조작 화면용으로 중앙 포인터 UI를 끄고 마우스 커서 잠금을 해제합니다.
    /// </summary>
    public static void ExitGameplayCursorMode(this UIManager uiManager)
    {
        uiManager.CloseCenterPointUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnlockGameplayCursor();
        }
    }
}
