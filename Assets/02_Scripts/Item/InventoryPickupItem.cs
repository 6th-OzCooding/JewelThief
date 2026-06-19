using UnityEngine;

/// <summary>
/// 월드에 배치된 테스트용 인벤토리 획득 아이템입니다.
/// </summary>
public class InventoryPickupItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private string _itemDataId;

    /// <summary>
    /// 이 월드 아이템이 참조하는 아이템 데이터 ID입니다.
    /// </summary>
    public string ItemDataId => _itemDataId;

    /// <summary>
    /// 플레이어 인벤토리에 이 아이템을 획득시킵니다.
    /// </summary>
    public bool TryPickup(PlayerInventory playerInventory)
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("PlayerInventory가 없어 아이템을 획득할 수 없습니다.");
            return false;
        }

        if (GameManager.Instance == null || GameManager.DataTable == null)
        {
            Debug.LogWarning("DataTable이 준비되지 않아 아이템을 획득할 수 없습니다.");
            return false;
        }

        ItemData itemData = GameManager.DataTable.GetPoolingItemDataTable().TryGetValue(_itemDataId, out var data) ? data : null;
        if (itemData == null)
        {
            Debug.LogWarning($"ItemData를 찾을 수 없습니다. Id: {_itemDataId}");
            return false;
        }

        InventoryTypeData inventoryTypeData = GameManager.DataTable.GetPoolingInventoryTypeDataTable().TryGetValue(_itemDataId, out var typeData) ? typeData : null;
        if (inventoryTypeData == null)
        {
            Debug.LogWarning($"InventoryTypeData를 찾을 수 없습니다. Id: {_itemDataId}");
            return false;
        }

        var holdType = inventoryTypeData.CurrentHoldType;
        bool isAcquired = playerInventory.TryAcquireItem(itemData, holdType, out InventoryItem acquiredItem, out string resultMessage);

        if (!isAcquired)
        {
            Debug.Log(resultMessage);
            return false;
        }

        gameObject.SetActive(false);
        return true;
    }
}
