using TMPro;
using UnityEngine;

/// <summary>
/// Displays the gameplay timer text.
/// </summary>
public class TimerHUD : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private TMP_Text _timeLimitText;
    [SerializeField] private TMP_Text _currentGoldText;

    [Header("Timer Presentation")]
    [SerializeField] private Color _normalTextColor = Color.white;
    [SerializeField] private Color _dangerTextColor = Color.red;
    [SerializeField] private float _dangerTextScale = 1.2f;

    private float _stageTimeLimit = 0f;
    private Vector3 _baseTextScale = Vector3.one;

    private void OnEnable()
    {
        CacheBaseTextScale();
        CacheStageTimeLimit();
        Refresh();
    }

    private void CacheStageTimeLimit()
    {
        if (GameManager.Instance == null)
            return;

        StageData stageData = GameManager.DataTable.GetStageData(GameManager.Instance.SelectedStageId);
        if (stageData != null)
            _stageTimeLimit = stageData.TimeLimit;
    }

    /// <summary>
    /// Displays remaining time in mm:ss format.
    /// </summary>
    public void SetTimer(float remainingSeconds)
    {
        if (_timeLimitText == null) return;

        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        _timeLimitText.text = $"{minutes:00}:{seconds:00}";
        UpdateTimerPresentation(remainingSeconds);
    }

    /// <summary>
    /// Refreshes the timer display from AlertManager.
    /// </summary>
    public void Refresh()
    {
        if (GameManager.Instance == null) return;

        if (!GameManager.Instance.IsInGame)
        {
            ShowLobbyGold();
            return;
        }

        SetTimer(GameManager.Alert.GetRemainingTime());
        ShowStageTimer();
        
    }

    private void ShowLobbyGold()
    {
        SetTextActive(_timeLimitText, false);
        SetTextActive(_currentGoldText, true);

        if (_currentGoldText == null) return;

        _currentGoldText.text = $"보유 자금\n{GameManager.Instance.Gold}$";
    }

    private void ShowStageTimer()
    {
        SetTextActive(_currentGoldText, false);
        SetTextActive(_timeLimitText, true);
    }

    private void SetTextActive(TMP_Text text, bool isActive)
    {
        if (text == null) return;

        text.enabled = isActive;
        text.gameObject.SetActive(isActive);
    }

    private void CacheBaseTextScale()
    {
        if (_timeLimitText == null) return;

        _baseTextScale = _timeLimitText.transform.localScale;
    }

    private void UpdateTimerPresentation(float remainingSeconds)
    {
        if (_timeLimitText == null) return;

        float maxSeconds = Mathf.Max(1f, _stageTimeLimit);
        float normalizedTime = Mathf.Clamp01(remainingSeconds / maxSeconds);
        float dangerRate = 1f - normalizedTime;

        _timeLimitText.color = Color.Lerp(_normalTextColor, _dangerTextColor, dangerRate);
        _timeLimitText.transform.localScale = _baseTextScale * Mathf.Lerp(1f, _dangerTextScale, dangerRate);
    }
}