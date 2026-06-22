using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AlertSystem 테스트용 스크립트
/// <summary>
public class AlertTest : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _enterGameplayButton;
    [SerializeField] private Button _resetButton;

    [Header("타이머 텍스트")]
    [SerializeField] private TMP_Text _timerText;

    [Header("테스트용 커서 고정 해제")]
    [SerializeField] private bool _forceUnlockCursor = true;
    private void Awake()
    {
        if (_enterGameplayButton != null)
            _enterGameplayButton.onClick.AddListener(OnClickEnterGameplay);

        if (_resetButton != null)
            _resetButton.onClick.AddListener(OnClickReset);
    }

    private void OnDestroy()
    {
        if (_enterGameplayButton != null)
            _enterGameplayButton.onClick.RemoveListener(OnClickEnterGameplay);

        if (_resetButton != null)
            _resetButton.onClick.RemoveListener(OnClickReset);
    }

    private void Update()
    {
        UpdateTimerText();
        if (_forceUnlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnClickEnterGameplay()
    {
        GameManager.Instance.EnterGamePlay();
    }

    private void OnClickReset()
    {
        GameManager.Alert.ResetTimer();
    }

    // 남은 시간을 단순 초 단위 정수로 표시 (120, 119, 118...)
    private void UpdateTimerText()
    {
        if (_timerText == null) return;
        if (GameManager.Instance == null) return;

        int remainingSeconds = Mathf.CeilToInt(GameManager.Alert.GetRemainingTime());
        _timerText.text = remainingSeconds.ToString();
    }
}