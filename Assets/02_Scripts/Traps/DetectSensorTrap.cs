using UnityEngine;

public class DetectSensorTrap : BaseDisarmableObejct
{
    [Header("센서 패널티 설정")]
    [SerializeField] private float _speedDamageAmount = 3f;     // 발판을 밝을 경우 속도 차감
    [SerializeField] private float _myTimeReductionAmount = 10f;     // 제한 시간 차감

    protected override void LoadData(string id)
    {
        _disarmObjName = "적외선 감지 센서";

        if (_timeReductionAmountList == null)
        {
            _timeReductionAmountList = new System.Collections.Generic.List<float>();
        }

        _timeReductionAmountList.Clear();
        _timeReductionAmountList.Add(_myTimeReductionAmount);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isDisarmed || !_isInitialized) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.LogWarning($"{_disarmObjName} (ID: {_disarmObjId}) 발동.");

            if (GameManager.Instance != null)
            {
                float finalReduction = (_timeReductionAmountList != null && _timeReductionAmountList.Count > 0)
                    ? _timeReductionAmountList[0]
                    : _myTimeReductionAmount;

                GameManager.Instance.SendMessage("ReduceTimer", finalReduction, SendMessageOptions.DontRequireReceiver);
            }

            player.TakePlayerMoveSpeedDamage(_speedDamageAmount);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<Collider>().GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log($"플레이어가 {_disarmObjName}를 벗어나 이동 속도를 ({_speedDamageAmount})만큼 돌려받습니다.");

            player.AddPlayerMoveSpeed(_speedDamageAmount);
        }
    }

    protected override void OnDisarm()
    {
        Debug.Log($"ID: {_disarmObjId} - {_disarmObjName}가 무력화됩니다.");

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    protected override void OnInitalized()
    {
        base.OnInitalized();

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
    }
}