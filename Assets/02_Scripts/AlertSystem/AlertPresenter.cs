using UnityEngine;

public class AlertPresenter : MonoBehaviour
{
    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Alert.OnAlertLevelChanged += HandleAlertLevelChanged;
        GameManager.Alert.OnTimeUp += HandleTimeUp;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Alert.OnAlertLevelChanged -= HandleAlertLevelChanged;
        GameManager.Alert.OnTimeUp -= HandleTimeUp;
    }

    private void HandleAlertLevelChanged(AlertLevel level)
    {
        UpdateBGMPitch(level);
    }

    private void HandleTimeUp()
    {
        // TODO(김경훈 2026-06-16): 제한시간 소진 연출 추가
    }

    // 경계 레벨에 따라 BGM 재생 속도 조정
    private void UpdateBGMPitch(AlertLevel level)
    {
        float pitch;

        switch (level)
        {
            case AlertLevel.Low:    pitch = 1.0f;
                break;
            case AlertLevel.Mid:    pitch = 1.25f;
                break;
            case AlertLevel.High:   pitch = 1.5f;
                break;
            default: pitch = 1.0f;
                break;
        }

        GameManager.Sound.SetBGMPitch(pitch);
    }
}