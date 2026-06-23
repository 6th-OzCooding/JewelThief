using UnityEngine;
using TeamConvention.Interfaces;

public class FakeItemTrap : BaseDisarmableObejct
{
    [Header("함정 패널티 설정")]
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _soundRadius = 20f;      // 소음이 퍼지는 범위의 반지름

    protected override void LoadData(string id)
    {
        Debug.Log($"[데이터 테이블 로드] 함정 ID: {id}");
    }

    protected override void OnInitalized()
    {
        base.OnInitalized();
        Debug.Log($"[함정 초기화 완료] {GetName} (ID: {GetId})");
    }

    protected override void OnDisarm()
    {
        Debug.LogWarning($"함정 발동. {GetName} (ID: {GetId})");

        if (GameManager.Instance != null)
        {
            float temporaryReductionAmount = 10f;
            GameManager.Instance.SendMessage("ReduceTimer", temporaryReductionAmount, SendMessageOptions.DontRequireReceiver);
        }

        TriggerNoise();

        Destroy(gameObject, 0.3f);     // 발동 0.3초 후 삭제
    }

    private void TriggerNoise()
    {
        Collider[] caughtEnemies = Physics.OverlapSphere(transform.position, _soundRadius);

        foreach (Collider col in caughtEnemies)
        {
            // 몬스터 AI 시스템 머지 완료 시 HearNoise 함수 활성화
            /*
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.HearNoise(transform.position); // 몬스터에게 소음이 발생한 위치를 제보해 추적당하게 함
            }
            */
        }
        Debug.Log($"반지름 {_soundRadius}m 이내의 적들이 소음을 듣고 몰려옵니다.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _soundRadius);
    }
}