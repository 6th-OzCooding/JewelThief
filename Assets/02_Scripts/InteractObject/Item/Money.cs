using TeamConvention.Interfaces;
using UnityEngine;

/// <summary>
/// 월드에 스폰된 돈 오브젝트. 상호작용 시 골드로 환전
/// </summary>
public class Money : BaseInteractableObject
{
    [Header("Money Data")]
    [SerializeField] private int _amount;

    public void SetAmount(int amount)
    {
        _amount = amount;
    }

    protected override bool CheckCanInteract() => _amount > 0;

    protected override void OnInteract(IInteractor interactor)
    {
        // TODO(김경훈 2026-06-23): 전역 재화 관리 헬퍼 클래스로 통일할지 패턴 합의 필요
        GameManager.Instance.AddGold(_amount);

        // TODO(김경훈 2026-06-23): 풀링 도입 시 SetActive(false) 대신 PoolManager.Release 사용
        gameObject.SetActive(false);
    }

    protected override void LoadData(string id)
    {
        // TODO(김경훈 2026-06-23): 돈은 데이터 테이블 의존이 없음 (액수는 SetAmount로 직접 주입).
        // InitFromSpawner를 통한 스폰 경로를 쓸지, 직접 Instantiate + SetAmount로만 쓸지 스포너 설계 시 결정 필요.
    }

    protected override void OnInitalized()
    {
        // TODO(김경훈 2026-06-23): 현재 LoadData에서 할 일이 없어 별도 초기화 로직 없음.
    }
}