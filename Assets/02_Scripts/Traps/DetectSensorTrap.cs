using UnityEngine;

public class DetectionSensorTrap : MonoBehaviour
{
    [Header("데이터 정보")]
    [SerializeField] private int _trapId = 40000004;

    [Header("센서 패널티 설정")]
    [SerializeField] private float _speedDebuffRate = 0.99f;   // 진입 시 이동 속도 저하 (이하 수치 미설정)
    [SerializeField] private float _timeReductionAmount = 1f; // 진입 시 차감되는 제한 시간

    private bool _isDisarmed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isDisarmed) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log($"[센서 감지] ID: {_trapId} - 감지 레이더가 플레이어를 포착했습니다.");

            // GameManager.Instance.AlertManager.ReduceTimer(_timeReductionAmount);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SendMessage("ReduceTimer", _timeReductionAmount, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.Log($"[임시 디버그] 싱글톤 매니저가 없어 임시 수치 차감: {_timeReductionAmount}초");
            }

            //player.SetSpeedModifier(_speedDebuffRate);     // 이동 속도 감소

        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            //player.SetSpeedModifier(1f);     // 센서 영역을 벗어나면 이동속도 원래대로 복구
            Debug.Log($"[센서 이탈] ID: {_trapId} - 플레이어가 센서 범위를 벗어났습니다.");
        }
    }

    public void DisarmTrap()
    {
        _isDisarmed = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log($"[함정 해제] ID: {_trapId} - 감지 센서 전원 차단 완료.");
    }
}