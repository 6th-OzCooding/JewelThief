using TeamConvention.Interfaces;
using UnityEngine;

/// <summary>
/// 손에 든 아이템을 즉시 골드로 환전하는 판매소(돈 세탁기)
/// 데이터 테이블 의존 없이 로비에 고정 배치되는 독립 오브젝트
/// </summary>
public class MoneyLaundry : MonoBehaviour, IInteractable
{
    public string GetId => "MoneyLaundry";
    public string GetName => "돈 세탁기";

    public bool CanInteract() => true;
    // TODO(김경훈 2026-06-23): 손에 든 아이템이 없으면 false 반환하도록 보강 필요 (PlayerInventory 확정 후)

    public void Interact(IInteractor interactor)
    {
        // TODO(김경훈 2026-06-23): IInteractor에 손에 든 아이템 접근 멤버가 없어 임시로 캐스팅.
        // PlayerInventory 구조 확인 후 IInteractor 확장 또는 별도 인터페이스로 교체 필요.
        if (interactor is not Component component || !component.TryGetComponent(out PlayerInventory playerInventory))
        {
            Debug.LogWarning("PlayerInventory를 찾을 수 없어 판매를 진행할 수 없습니다.");
            return;
        }

        // TODO(김경훈 2026-06-23): "손에 든 아이템" 조회 메서드 확정 필요
        InventoryItem heldItem = null; // 임시

        if (heldItem == null)
        {
            Debug.Log("손에 든 아이템이 없습니다.");
            return;
        }

        // TODO(김경훈 2026-06-23): 아이템 판매가는 ItemData에 SellPrice 필드가 있어야 조회 가능.
        // 현재 ItemData 구조 미확정이라 임시 고정값 사용.
        int sellPrice = 0; // 임시

        // TODO(김경훈 2026-06-23): 손에서 아이템 제거 메서드 확정 필요. 제거 성공 시에만 골드 지급해야 함 (순서 중요).

        GameManager.Instance.AddGold(sellPrice);

        // TODO(김경훈 2026-06-23): 재화 표시 UI 갱신 트리거 필요.

    }
}