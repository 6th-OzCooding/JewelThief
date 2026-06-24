using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hover popup UIs share offset positioning and open animation behavior.
/// </summary>
public abstract class HoverPopupUIBase : UIBase
{
    private static readonly Vector2 BaseDefaultPopupOffset = new(220f, 0f);

    [Header("Position")]
    [SerializeField] private Vector2 _popupOffset = new(220f, 0f);

    [Header("Open Animation")]
    [SerializeField] private RectTransform _animatedBox;
    [SerializeField] private RectTransform[] _hiddenLayoutsDuringOpen;
    [SerializeField] private float _openDuration = 0.08f;

    private RectTransform _rectTransform;
    private Vector2 _defaultBoxSizeDelta;
    private float _defaultBoxHeight;
    private bool _hasDefaultBoxHeight;
    private CancellationTokenSource _openAnimationCts;

    /// <summary>
    /// Default screen offset used when the prefab has not overridden this popup's offset.
    /// </summary>
    protected virtual Vector2 DefaultPopupOffset => BaseDefaultPopupOffset;

    protected virtual void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        CacheRectComponents();
        CacheDefaultBoxHeight();
    }

    protected virtual void OnEnable()
    {
        ApplyPopupOffset();
    }

    protected virtual void OnDisable()
    {
        CancelOpenAnimation();
    }

    protected virtual void OnDestroy()
    {
        CancelOpenAnimation();
    }

    protected virtual void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        ApplyPopupOffset();
    }

    /// <summary>
    /// Restarts the popup opening animation from the beginning.
    /// </summary>
    public void RestartOpenAnimation()
    {
        ApplyPopupOffset();
        PlayOpenAnimationAsync().Forget();
    }

    protected TMP_Text FindTextByName(string objectName)
    {
        TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text textComponent in textComponents)
        {
            if (textComponent != null && textComponent.gameObject.name == objectName)
                return textComponent;
        }

        Debug.LogWarning($"{GetType().Name}에서 텍스트 오브젝트를 찾지 못했습니다. Name: {objectName}");
        return null;
    }

    protected void SetText(TMP_Text targetText, string value)
    {
        if (targetText == null)
            return;

        targetText.text = string.IsNullOrEmpty(value) ? string.Empty : value;
    }

    private void ApplyPopupOffset()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        if (_rectTransform == null)
            return;

        _rectTransform.anchoredPosition = GetEffectivePopupOffset();
    }

    private Vector2 GetEffectivePopupOffset()
    {
        if (DefaultPopupOffset != BaseDefaultPopupOffset && _popupOffset == BaseDefaultPopupOffset)
            return DefaultPopupOffset;

        return _popupOffset;
    }

    private void CacheRectComponents()
    {
        _animatedBox ??= FindRectTransformByName("Image_UIBox");

        if (_hiddenLayoutsDuringOpen != null && _hiddenLayoutsDuringOpen.Length > 0)
            return;

        List<RectTransform> hiddenLayouts = new();
        RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform rectTransform in rectTransforms)
        {
            if (rectTransform == null || rectTransform == _rectTransform)
                continue;

            if (rectTransform.gameObject.name.StartsWith("Layout_"))
                hiddenLayouts.Add(rectTransform);
        }

        _hiddenLayoutsDuringOpen = hiddenLayouts.ToArray();
    }

    private RectTransform FindRectTransformByName(string objectName)
    {
        RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>(true);
        foreach (RectTransform rectTransform in rectTransforms)
        {
            if (rectTransform != null && rectTransform.gameObject.name == objectName)
                return rectTransform;
        }

        Debug.LogWarning($"{GetType().Name}에서 RectTransform 오브젝트를 찾지 못했습니다. Name: {objectName}");
        return null;
    }

    private async UniTaskVoid PlayOpenAnimationAsync()
    {
        CancelOpenAnimation();
        _openAnimationCts = new CancellationTokenSource();
        CancellationToken cancellationToken = _openAnimationCts.Token;

        CacheRectComponents();
        CacheDefaultBoxHeight();
        SetContentActive(false);

        if (_animatedBox == null || _defaultBoxHeight <= 0f || _openDuration <= 0f)
        {
            SetBoxHeight(_defaultBoxHeight);
            SetContentActive(true);
            return;
        }

        SetBoxHeight(0f);

        float elapsedTime = 0f;
        try
        {
            while (elapsedTime < _openDuration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                elapsedTime += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsedTime / _openDuration);
                SetBoxHeight(Mathf.Lerp(0f, _defaultBoxHeight, progress));
            }

            SetBoxHeight(_defaultBoxHeight);
            SetContentActive(true);
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    private void CacheDefaultBoxHeight()
    {
        if (_hasDefaultBoxHeight && _defaultBoxHeight > 0f)
            return;

        if (_animatedBox == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_animatedBox);

        _defaultBoxSizeDelta = _animatedBox.sizeDelta;
        _defaultBoxHeight = _animatedBox.rect.height;
        if (_defaultBoxHeight <= 0f)
            _defaultBoxHeight = _animatedBox.sizeDelta.y;

        _hasDefaultBoxHeight = true;
    }

    private void SetBoxHeight(float height)
    {
        if (_animatedBox == null)
            return;

        Vector2 sizeDelta = _defaultBoxSizeDelta;
        if (Mathf.Approximately(_animatedBox.anchorMin.y, _animatedBox.anchorMax.y))
            sizeDelta.y = height;
        else
            sizeDelta.y = _defaultBoxSizeDelta.y - (_defaultBoxHeight - height);

        _animatedBox.sizeDelta = sizeDelta;
    }

    private void SetContentActive(bool isActive)
    {
        if (_hiddenLayoutsDuringOpen == null)
            return;

        foreach (RectTransform hiddenLayout in _hiddenLayoutsDuringOpen)
        {
            if (hiddenLayout == null)
                continue;

            hiddenLayout.gameObject.SetActive(isActive);
        }
    }

    private void CancelOpenAnimation()
    {
        if (_openAnimationCts == null)
            return;

        _openAnimationCts.Cancel();
        _openAnimationCts.Dispose();
        _openAnimationCts = null;

        if (_animatedBox != null && _hasDefaultBoxHeight)
            _animatedBox.sizeDelta = _defaultBoxSizeDelta;
    }
}
