using TeamConvention.Interfaces;
using UnityEngine;

/// <summary>
/// 월드에 배치된 인벤토리 획득 아이템입니다.
/// </summary>
public class InventoryPickupItem : MonoBehaviour, IInteractable
{
    [Header("Item Data")]
    [SerializeField] private string _itemDataId;

    /// <summary>
    /// 이 월드 아이템이 참조하는 아이템 데이터 ID입니다.
    /// </summary>
    public string ItemDataId => _itemDataId;

    public string GetId => _itemDataId;
    public string GetName => GetCachedItemData()?.Name ?? "알 수 없는 아이템";

    public bool CanInteract() => !string.IsNullOrEmpty(_itemDataId);

    public void Interact(IInteractor interactor)
    {
        if (interactor is not IInventoryOwner inventoryOwner)
        {
            Debug.LogWarning("인벤토리 정보를 가져올 수 없어 아이템을 획득할 수 없습니다.");
            return;
        }

        TryPickup(inventoryOwner);
    }

    /// <summary>
    /// 플레이어 인벤토리에 이 아이템을 획득시킵니다.
    /// </summary>
    public bool TryPickup(IInventoryOwner inventoryOwner)
    {
        if (inventoryOwner == null)
        {
            Debug.LogWarning("IInventoryOwner가 없어 아이템을 획득할 수 없습니다.");
            return false;
        }

        if (GameManager.Instance == null || GameManager.DataTable == null)
        {
            Debug.LogWarning("DataTable이 준비되지 않아 아이템을 획득할 수 없습니다.");
            return false;
        }

        ItemData itemData = GameManager.DataTable.GetItemDataTable().TryGetValue(_itemDataId, out var data) ? data : null;
        if (itemData == null)
        {
            Debug.LogWarning($"ItemData를 찾을 수 없습니다. Id: {_itemDataId}");
            return false;
        }

        // 보석용 인벤토리 위해 추가 
        if (itemData.CurrentItemType == ItemType.Jewel)
        {
            ItemBase itemBase = GetComponent<ItemBase>();
            if (itemBase == null)
            {
                Debug.LogError($"보석 아이템[{gameObject.name}]에 ItemBase 컴포넌트가 없습니다!");
                return false;
            }

            if (JewelPuzzleUIManager.Instance != null && JewelPuzzleUIManager.Instance.CanPickupJewel(itemData))
            {
                JewelPuzzleUIManager.Instance.AddJewelToTempQueue(itemBase);
                return true;
            }
            else
            {
                Debug.Log($"{itemData.Name}을(를) 주울 수 없습니다. (공간/무게 부족)");
                return false;
            }
        }

        InventoryTypeData inventoryTypeData = GameManager.DataTable.GetInventoryTypeDataTable().TryGetValue(_itemDataId, out var typeData) ? typeData : null;
        if (inventoryTypeData == null)
        {
            Debug.LogWarning($"InventoryTypeData를 찾을 수 없습니다. Id: {_itemDataId}");
            return false;
        }

        HoldType holdType = typeData.GetHoldType();
        bool isAcquired = inventoryOwner.TryAcquireItem(itemData, holdType, out InventoryItem acquiredItem, out string resultMessage);

        if (!isAcquired)
        {
            Debug.Log(resultMessage);
            return false;
        }

        gameObject.SetActive(false);
        return true;
    }

    // GetName에서 ItemData 조회 시 매번 DataTable을 타지 않도록 캐시
    private ItemData _cachedItemData;
    private ItemData GetCachedItemData()
    {
        if (_cachedItemData != null)
            return _cachedItemData;

        if (GameManager.Instance == null || GameManager.DataTable == null)
            return null;

        _cachedItemData = GameManager.DataTable.GetItemDataTable().TryGetValue(_itemDataId, out var data) ? data : null;
        return _cachedItemData;
    }
}