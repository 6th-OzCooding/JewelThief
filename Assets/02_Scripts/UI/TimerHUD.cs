using TMPro;
using UnityEngine;

/// <summary>
/// Displays the gameplay timer text.
/// </summary>
public class TimerHUD : MonoBehaviour
{
    private enum TimerSourceType
    {
        TestLocalTimer,
        AlertManager
    }

    [Header("Timer")]
    [SerializeField] private TMP_Text _timeLimitText;
    [SerializeField] private TMP_Text _currentGoldText;
    [SerializeField] private TimerSourceType _timerSourceType = TimerSourceType.TestLocalTimer;
    [SerializeField] private float _testTimerSeconds = 600f;

    [Header("Timer Presentation")]
    [SerializeField] private Color _normalTextColor = Color.white;
    [SerializeField] private Color _dangerTextColor = Color.red;
    [SerializeField] private float _dangerTextScale = 1.2f;

    [Header("Test Options")]
    [SerializeField] private float _testDecreaseSeconds = 60f;

    private float _currentTestTimerSeconds;
    private Vector3 _baseTextScale = Vector3.one;

    private void OnEnable()
    {
        CacheBaseTextScale();
        ResetTimer();
        Refresh();
    }

    /// <summary>
    /// Resets the local test timer to the configured test duration.
    /// </summary>
    public void ResetTimer()
    {
        _currentTestTimerSeconds = Mathf.Max(0f, _testTimerSeconds);
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
    /// Refreshes the timer display from the configured timer source.
    /// </summary>
    public void Refresh()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsInGame)
        {
            ShowLobbyGold();
            return;
        }

        ShowStageTimer();

        if (_timerSourceType == TimerSourceType.AlertManager)
        {
            if (GameManager.Instance == null) return;

            SetTimer(GameManager.Alert.GetRemainingTime());
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9))
            DecreaseTestTimer(_testDecreaseSeconds);

        _currentTestTimerSeconds = Mathf.Max(0f, _currentTestTimerSeconds - Time.deltaTime);
        SetTimer(_currentTestTimerSeconds);
    }

    private void ShowLobbyGold()
    {
        SetTextActive(_timeLimitText, false);
        SetTextActive(_currentGoldText, true);

        if (_currentGoldText == null || GameManager.Instance == null) return;

        _currentGoldText.text = $"보유 자금\n{GameManager.Instance._gold}$";
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

    private void DecreaseTestTimer(float decreaseSeconds)
    {
        _currentTestTimerSeconds = Mathf.Max(0f, _currentTestTimerSeconds - decreaseSeconds);
    }

    private void CacheBaseTextScale()
    {
        if (_timeLimitText == null) return;

        _baseTextScale = _timeLimitText.transform.localScale;
    }

    private void UpdateTimerPresentation(float remainingSeconds)
    {
        if (_timeLimitText == null) return;

        float maxSeconds = Mathf.Max(1f, _testTimerSeconds);
        float normalizedTime = Mathf.Clamp01(remainingSeconds / maxSeconds);
        float dangerRate = 1f - normalizedTime;

        _timeLimitText.color = Color.Lerp(_normalTextColor, _dangerTextColor, dangerRate);
        _timeLimitText.transform.localScale = _baseTextScale * Mathf.Lerp(1f, _dangerTextScale, dangerRate);
    }
}
