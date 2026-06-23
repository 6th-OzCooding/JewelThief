using TeamConvention.Interfaces;
using UnityEngine;

/// <summary>
/// 월드에 스폰된 돈 오브젝트. 상호작용 시 골드로 환전
/// </summary>
public class Money : MonoBehaviour, IInteractable
{
    [Header("Money Data")]
    [SerializeField] private int _amount;

    public string GetId => "Money"; // TODO(김경훈 2026-06-23): 돈도 아이템 데이터화할지 합의 필요
    public string GetName => "돈";

    public void SetAmount(int amount)
    {
        _amount = amount;
    }

    public bool CanInteract() => _amount > 0;

    public void Interact(IInteractor interactor)
    {
        if (!CanInteract())
            return;

        // TODO(김경훈 2026-06-23): GameManager.Instance.AddGold -> static 헬퍼로 통일할지 패턴 합의 필요
        GameManager.Instance.AddGold(_amount);

        // TODO(김경훈 2026-06-23): 풀링 도입 시 SetActive(false) 대신 PoolManager.Release 사용
        gameObject.SetActive(false);

    }
}