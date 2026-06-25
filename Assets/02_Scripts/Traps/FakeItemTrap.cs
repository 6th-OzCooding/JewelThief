using UnityEngine;

public class FakeItemTrap : BaseDisarmableObejct
{
    [Header("가짜 보석 패널티 설정")]
    [SerializeField] private float _spDamage = 20f;        // 플레이어 sp 차감
    [SerializeField] private float _soundRadius = 15f;      // 소음 범위

    private bool _hasExploded = false;

    protected override void LoadData(string id)
    {
        _disarmObjName = "가짜 보석";

        if (_timeReductionAmountList == null)
        {
            _timeReductionAmountList = new System.Collections.Generic.List<float>();
        }
        _timeReductionAmountList.Clear();
        _timeReductionAmountList.Add(10f);
    }

    protected override void OnDisarm()
    {
        if (_hasExploded) return;     // 중복 키입력 방지 로직
        _hasExploded = true;

        Debug.LogWarning($"{_disarmObjName} 보석인 줄 알고 열어봤지만 내용물은 (ID: {_disarmObjId})였습니다.");

        if (GameManager.Instance != null)
        {
            float finalReduction = (_timeReductionAmountList != null && _timeReductionAmountList.Count > 0)
                ? _timeReductionAmountList[0]
                : 10f;
            GameManager.Instance.SendMessage("ReduceTimer", finalReduction, SendMessageOptions.DontRequireReceiver);
        }

        Collider[] caughtPlayers = Physics.OverlapSphere(transform.position, 2f);     // 함정 주변 2m 탐색
        foreach (Collider col in caughtPlayers)
        {
            PlayerController player = col.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakePlayerSpDamage(_spDamage);
                break;
            }
        }

        TriggerNoise();
        Destroy(gameObject, 0.3f);
    }

    private void TriggerNoise()
    {
        Physics.OverlapSphere(transform.position, _soundRadius);
        Debug.Log($"함정이 발동되어 반지름 {_soundRadius}m 범위의 적들이 소음을 확인합니다.");
    }
}