using UnityEngine;

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
    CenterPointUI,
    TitleUI,
    ItemInfoPopupUI,
    ConfirmPopup,
    CreditPopup,
    SettingPopup,
    StageSelectUI,
    SimplePopupUI,
    ShopInfoPopupUI
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
        uiManager.EnterGameplayCursorMode();
    }

    /// <summary>
    /// 인벤토리 시스템 테스트용 게임 플레이 UI를 엽니다.
    /// </summary>
    public static void ShowInventorySystemTestUI(this UIManager uiManager)
    {
        uiManager.CloseLoadingUI();
        uiManager.EnterGameplayCursorMode();
    }

    /// <summary>
    /// 로딩 UI를 엽니다.
    /// </summary>
    public static UIBase OpenLoadingUI(this UIManager uiManager)
    {
        return uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
    }

    public static UIBase OpenMainUI(this UIManager uiManager)
    {
        return uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.TitleUI);
    }

    // TODO: (김경훈 - 26.06.22) 임시 추가, 계층 및 루트 확정 필요
    public static UIBase OpenStageSelectUI(this UIManager uimanager)
    {
        return uimanager.OpenUI(UIRootType.MainUI, UIType.StageSelectUI);
    }

    // TODO: (김경훈 - 26.06.22) 임시 추가, 계층 및 루트 확정 필요
    public static void CloseStageSelectUI(this UIManager uimanager)
    {
        uimanager.CloseUI(UIType.StageSelectUI);
    }

    /// <summary>
    /// 로딩 UI를 닫습니다.
    /// </summary>
    public static void CloseLoadingUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.LoadingUI);
    }

    public static void CloseMainUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIType.TitleUI);
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
    /// 화면 중앙 Hover 대상의 아이템 정보 팝업 UI를 엽니다.
    /// </summary>
    public static ItemInfoPopupUI OpenItemInfoPopupUI(this UIManager uiManager)
    {
        UIBase uiBase = uiManager.OpenPopupUI(UIType.ItemInfoPopupUI);
        if (uiBase == null)
            return null;

        if (!uiBase.TryGetComponent(out ItemInfoPopupUI itemInfoPopupUI))
        {
            Debug.LogWarning("ItemInfoPopupUI 프리팹에 ItemInfoPopupUI 컴포넌트가 없습니다.");
            return null;
        }

        itemInfoPopupUI.RestartOpenAnimation();
        return itemInfoPopupUI;
    }

    /// <summary>
    /// 이름과 상호작용 문구만 표시하는 간단한 Hover 팝업 UI를 엽니다.
    /// </summary>
    public static SimplePopupUI OpenSimplePopupUI(this UIManager uiManager)
    {
        UIBase uiBase = uiManager.OpenPopupUI(UIType.SimplePopupUI);
        if (uiBase == null)
            return null;

        if (!uiBase.TryGetComponent(out SimplePopupUI simplePopupUI))
        {
            Debug.LogWarning("SimplePopupUI 프리팹에 SimplePopupUI 컴포넌트가 없습니다.");
            return null;
        }

        return simplePopupUI;
    }

    /// <summary>
    /// 상점용 Hover 팝업 UI를 엽니다.
    /// </summary>
    public static ShopInfoPopupUI OpenShopInfoPopupUI(this UIManager uiManager)
    {
        UIBase uiBase = uiManager.OpenPopupUI(UIType.ShopInfoPopupUI);
        if (uiBase == null)
            return null;

        if (!uiBase.TryGetComponent(out ShopInfoPopupUI shopInfoPopupUI))
        {
            Debug.LogWarning("ShopInfoPopupUI 프리팹에 ShopInfoPopupUI 컴포넌트가 없습니다.");
            return null;
        }

        return shopInfoPopupUI;
    }

    /// <summary>
    /// 화면 중앙 Hover 대상의 아이템 정보 팝업 UI를 닫습니다.
    /// </summary>
    public static void CloseItemInfoPopupUI(this UIManager uiManager)
    {
        uiManager.ClosePopupUI(UIType.ItemInfoPopupUI);
    }

    /// <summary>
    /// 화면 중앙 Hover 대상 정보 팝업을 모두 닫습니다.
    /// </summary>
    public static void CloseHoverPopupUI(this UIManager uiManager)
    {
        uiManager.ClosePopupUI(UIType.ItemInfoPopupUI);
        uiManager.ClosePopupUI(UIType.SimplePopupUI);
        uiManager.ClosePopupUI(UIType.ShopInfoPopupUI);
    }

    /// <summary>
    /// 게임 플레이 화면용 중앙 포인터 UI를 켜고 마우스 커서를 잠급니다.
    /// </summary>
    public static void EnterGameplayCursorMode(this UIManager uiManager)
    {
        uiManager.OpenCenterPointUI();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// UI 조작 화면용으로 중앙 포인터 UI를 끄고 마우스 커서 잠금을 해제합니다.
    /// </summary>
    public static void ExitGameplayCursorMode(this UIManager uiManager)
    {
        uiManager.CloseCenterPointUI();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
