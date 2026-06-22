using UnityEngine;

public class PressurePlateTrap : MonoBehaviour
{
    [Header("데이터 정보")]
    [SerializeField] private int _trapId = 40000005;

    [Header("압력판 작동 조건")]
    [SerializeField] private float _triggerWeightThreshold = 5.0f;    // 작동 기준 무게 (예: 5kg)

    [Header("감지 패널티 설정")]
    [SerializeField] private float _timeReductionAmount = 20f;     // 작동 시 차감할 제한시간 (초)

    private bool _isDisarmed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isDisarmed) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            float currentPayloadWeight = GetPlayerTotalWeight(player);    // 플레이어의 인벤토리 무게를 받아옴

            Debug.Log($"[압력판 밟음] ID: {_trapId} - 현재 플레이어가 소지한 보석의 무게: {currentPayloadWeight}kg (기준: {_triggerWeightThreshold}kg)");

            if (currentPayloadWeight >= _triggerWeightThreshold)    // 작동 기준 무게 이하일 경우 작동하지 않음
            {
                TriggerPressurePlateAlert();
            }
            else
            {
                Debug.Log("[압력판 통과] 플레이어가 가벼워서 센서가 작동하지 않았습니다.");
            }
        }
    }

    private float GetPlayerTotalWeight(PlayerController player)
    {
        // 나중에 가방/인벤토리 함수로 대체할 것.
        // 예시 구조: return player.GetComponent<Inventory>().GetTotalWeight();

        // 컴파일에러 방지용 임시코드
        // 7kg 들고 있다고 가정
        float temporaryTestWeight = 7.0f;
        return temporaryTestWeight;
    }

    private void TriggerPressurePlateAlert()
    {
        Debug.LogWarning($"[압력판 발동] 중량 초과. ID: {_trapId} - 제한시간이 {_timeReductionAmount}초 차감됩니다.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SendMessage("ReduceTimer", _timeReductionAmount, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void DisarmTrap()
    {
        _isDisarmed = true;

        Debug.Log($"[함정 해제] ID: {_trapId} - 압력판 센서 고정 장치가 무력화되어 밟아도 안전합니다.");
    }
}