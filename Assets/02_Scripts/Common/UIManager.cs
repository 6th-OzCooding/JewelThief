using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 프리팹을 생성, 캐싱, 열기, 닫기 처리하는 매니저입니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// 현재 씬에서 사용하는 UIManager 인스턴스입니다.
    /// </summary>
    public static UIManager Instance { get; private set; }

    [Header("UI Root Canvases")]
    [SerializeField] private Canvas _backgroundRoot;
    [SerializeField] private Canvas _mainRoot;
    [SerializeField] private Canvas _contentRoot;
    [SerializeField] private Canvas _popupRoot;
    [SerializeField] private Canvas _veryFrontRoot;

    private readonly Dictionary<UIType, UIBase> _createdUIDic = new();
    private readonly HashSet<UIType> _openedUIDic = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// UI 프리팹을 생성하거나 이미 만든 UI를 다시 열어 반환합니다.
    /// </summary>
    public UIBase OpenUI(UIRootType uiRootType, UIType uiType)
    {
        UIBase uiBase = GetCreatedUI(uiRootType, uiType);
        if (uiBase == null)
            return null;

        if (!_openedUIDic.Contains(uiType))
            _openedUIDic.Add(uiType);

        uiBase.gameObject.SetActive(true);
        return uiBase;
    }

    /// <summary>
    /// 열린 UI를 닫고 비활성화합니다.
    /// </summary>
    public void CloseUI(UIType uiType)
    {
        if (!_openedUIDic.Contains(uiType))
            return;

        if (!_createdUIDic.TryGetValue(uiType, out UIBase uiBase))
        {
            Debug.LogWarning($"닫으려는 UI가 생성 목록에 없습니다. UIType: {uiType}");
            _openedUIDic.Remove(uiType);
            return;
        }

        uiBase.gameObject.SetActive(false);
        _openedUIDic.Remove(uiType);
    }

    /// <summary>
    /// Content 루트에 UI를 엽니다.
    /// </summary>
    public UIBase OpenContentUI(UIType uiType)
    {
        return OpenUI(UIRootType.ContentUI, uiType);
    }

    /// <summary>
    /// Popup 루트에 UI를 엽니다.
    /// </summary>
    public UIBase OpenPopupUI(UIType uiType)
    {
        return OpenUI(UIRootType.PopupUI, uiType);
    }

    /// <summary>
    /// Content 루트에 열린 UI를 닫습니다.
    /// </summary>
    public void CloseContentUI(UIType uiType)
    {
        CloseUI(uiType);
    }

    /// <summary>
    /// Popup 루트에 열린 UI를 닫습니다.
    /// </summary>
    public void ClosePopupUI(UIType uiType)
    {
        CloseUI(uiType);
    }

    private UIBase GetCreatedUI(UIRootType uiRootType, UIType uiType)
    {
        if (_createdUIDic.TryGetValue(uiType, out UIBase createdUI))
            return createdUI;

        return CreateUI(uiRootType, uiType);
    }

    private UIBase CreateUI(UIRootType uiRootType, UIType uiType)
    {
        if (uiRootType == UIRootType.None || uiType == UIType.None)
        {
            Debug.LogWarning($"UI 생성 요청 값이 올바르지 않습니다. RootType: {uiRootType}, UIType: {uiType}");
            return null;
        }

        Transform root = GetRootTransform(uiRootType);
        if (root == null)
        {
            Debug.LogWarning($"UI 루트 Canvas가 연결되지 않았습니다. RootType: {uiRootType}");
            return null;
        }

        string path = this.GetUIPath(uiRootType, uiType);
        GameObject prefab = Utils.ResourcesLoad<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"UI 프리팹을 로드하지 못했습니다. Path: {path}");
            return null;
        }

        GameObject instance = Instantiate(prefab, root);
        if (!instance.TryGetComponent(out UIBase uiBase))
        {
            Debug.LogWarning($"UI 프리팹에 UIBase 컴포넌트가 없습니다. Path: {path}");
            Destroy(instance);
            return null;
        }

        _createdUIDic.Add(uiType, uiBase);

        return uiBase;
    }

    private Transform GetRootTransform(UIRootType uiRootType)
    {
        return uiRootType switch
        {
            UIRootType.BackgroundUI => _backgroundRoot != null ? _backgroundRoot.transform : null,
            UIRootType.MainUI => _mainRoot != null ? _mainRoot.transform : null,
            UIRootType.ContentUI => _contentRoot != null ? _contentRoot.transform : null,
            UIRootType.PopupUI => _popupRoot != null ? _popupRoot.transform : null,
            UIRootType.VeryFrontUI => _veryFrontRoot != null ? _veryFrontRoot.transform : null,
            _ => null
        };
    }
}
