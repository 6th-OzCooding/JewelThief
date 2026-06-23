using TeamConvention.Interfaces;
using UnityEngine;

/// <summary>
/// 손에 든 아이템을 돈으로 변환하는 판매소(돈 세탁기)
/// </summary>
public class MoneyLaundry : MonoBehaviour, IInteractable
{
    [Header("스폰 위치")]
    [SerializeField] private Transform _moneySpawnPoint;

    public string GetId => "MoneyLaundry";
    public string GetName => "돈 세탁기";

    public bool CanInteract() => true; // TODO(김경훈 2026-06-23): 손에 든 아이템이 없으면 false 반환하도록 보강 필요

    public void Interact(IInteractor interactor)
    {
        // TODO(김경훈 2026-06-23): IInteractor에 손에 든 아이템 접근 멤버가 없어 임시로 캐스팅.
        // PlayerInventory 구조 확인 후 IInteractor 확장 또는 별도 인터페이스(IHeldItemProvider 등)로 교체 필요.
        if (interactor is not Component component || !component.TryGetComponent(out PlayerInventory playerInventory))
        {
            Debug.LogWarning("PlayerInventory를 찾을 수 없어 판매를 진행할 수 없습니다.");
            return;
        }

        // TODO(김경훈 2026-06-23): "손에 든 아이템" 조회 메서드 필요 (예: playerInventory.GetHeldItem())
        InventoryItem heldItem = null; // 임시

        if (heldItem == null)
        {
            Debug.Log("손에 든 아이템이 없습니다.");
            return;
        }

        // TODO(김경훈 2026-06-23): 아이템 판매가 조회 로직 추가
        // 현재 ItemData 구조 미확정이라 임시 고정값 사용.
        int sellPrice = 0; // 임시: ItemData.SellPrice 등으로 교체 예정

        // TODO(김경훈 2026-06-23): 손에서 아이템 제거 기능 필요 (예: playerInventory.RemoveHeldItem())

        Vector3 spawnPosition = _moneySpawnPoint != null ? _moneySpawnPoint.position : transform.position;
        Quaternion spawnRotation = _moneySpawnPoint != null ? _moneySpawnPoint.rotation : transform.rotation;

        MoneySpawner.SpawnMoney(sellPrice, spawnPosition, spawnRotation);
    }
}