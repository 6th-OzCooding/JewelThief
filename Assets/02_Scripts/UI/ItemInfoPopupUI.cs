using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 중앙 Hover 대상의 아이템 정보를 표시하는 팝업 UI입니다.
/// </summary>
public class ItemInfoPopupUI : UIBase
{
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

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform == null)
            return;

        CacheDefaultBoxHeight();
    }

    private void OnEnable()
    {
        ApplyPopupOffset();
    }

    private void OnDisable()
    {
        CancelOpenAnimation();
    }

    private void OnDestroy()
    {
        CancelOpenAnimation();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        ApplyPopupOffset();
    }

    /// <summary>
    /// Hover 정보 팝업의 열림 연출을 처음부터 다시 재생합니다.
    /// </summary>
    public void RestartOpenAnimation()
    {
        ApplyPopupOffset();
        PlayOpenAnimationAsync().Forget();
    }

    private void ApplyPopupOffset()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        if (_rectTransform == null)
            return;

        _rectTransform.anchoredPosition = _popupOffset;
    }

    private async UniTaskVoid PlayOpenAnimationAsync()
    {
        CancelOpenAnimation();
        _openAnimationCts = new CancellationTokenSource();
        CancellationToken cancellationToken = _openAnimationCts.Token;

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
