using UnityEngine;

/// <summary>
/// Moves CenterPointUI to the current mouse position while this component is enabled.
/// </summary>
public class CenterPointUICursorMode : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Vector2 _defaultAnchorMin;
    private Vector2 _defaultAnchorMax;
    private Vector2 _defaultAnchoredPosition;
    private bool _isInitialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        if (!_isInitialized)
            return;

        _rectTransform.anchorMin = _defaultAnchorMin;
        _rectTransform.anchorMax = _defaultAnchorMax;
        _rectTransform.anchoredPosition = _defaultAnchoredPosition;
    }

    private void Update()
    {
        _rectTransform.position = Input.mousePosition;
    }

    /// <summary>
    /// Enables mouse cursor mode for CenterPointUI.
    /// </summary>
    public void EnableCursorMode()
    {
        enabled = true;
    }

    /// <summary>
    /// Restores CenterPointUI to its default centered mode.
    /// </summary>
    public void DisableCursorMode()
    {
        enabled = false;
    }

    private void Initialize()
    {
        if (_isInitialized)
            return;

        _rectTransform = GetComponent<RectTransform>();
        _defaultAnchorMin = _rectTransform.anchorMin;
        _defaultAnchorMax = _rectTransform.anchorMax;
        _defaultAnchoredPosition = _rectTransform.anchoredPosition;
        _isInitialized = true;
    }
}
