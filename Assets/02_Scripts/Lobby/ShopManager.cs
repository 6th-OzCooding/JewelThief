using System.Collections.Generic;
using TeamConvention.Interfaces;

public struct SellResult
{
    public int SoldCount;
    public int TotalPrice;
    public bool HasSold => SoldCount > 0;
}

public class ShopManager
{
    // 도구 아이템 구매. 진열대는 도구 전용이므로 HoldType.Hold로 고정한다.
    // TODO(김경훈 2026-06-24): 현재는 골드 차감 후 습득 실패 시 환불하는 임시 방식.
    //   인벤토리/인터페이스에 수용 가능 여부 검사(CanAcquireItem)가 추가되면
    //   "검사 먼저 -> 골드 차감 -> 습득" 순서로 변경할 것.
    public bool TryBuyItem(IInventoryOwner inventoryOwner, string itemId)
    {

        if (inventoryOwner == null)
        {
            return false;
        }

        ItemData itemData = GameManager.DataTable?.GetItemData(itemId);
        if (itemData == null)
        {
            return false;
        }

        if (!GameManager.Instance.TrySpendGold(itemData.Price))
        {
            return false;
        }

        bool isAcquired = inventoryOwner.TryAcquireItem(itemData, HoldType.Hold);
        if (!isAcquired)
        {
            GameManager.Instance.AddGold(itemData.Price);
            return false;
        }

        return true;
    }

    public SellResult SellAllJewels(IInventoryOwner inventoryOwner)
    {
        SellResult result = new SellResult();

        if (inventoryOwner == null)
            return result;

        List<InventoryItem> sellableBagItems = new List<InventoryItem>();
        foreach (InventoryItem bagItem in inventoryOwner.BagItems)
        {
            if (IsSellableItem(bagItem))
                sellableBagItems.Add(bagItem);
        }

        foreach (InventoryItem sellableItem in sellableBagItems)
        {
            InventoryItem removedItem = inventoryOwner.RemoveBagItem(sellableItem);
            if (removedItem == null)
                continue;

            result.TotalPrice += removedItem.ItemData.Price;
            result.SoldCount++;
        }

        // 양손 판매
        result = TrySellHandItem(inventoryOwner, PlayerHandType.Right, result);
        result = TrySellHandItem(inventoryOwner, PlayerHandType.Left, result);

        if (result.SoldCount > 0)
            GameManager.Instance.AddGold(result.TotalPrice);

        return result;
    }

    // 지정한 손의 아이템이 판매 가능하면 비우고 합산
    private SellResult TrySellHandItem(IInventoryOwner inventoryOwner, PlayerHandType handType, SellResult result)
    {
        InventoryItem handItem = handType == PlayerHandType.Right ? inventoryOwner.RightHandItem : inventoryOwner.LeftHandItem;
        if (!IsSellableItem(handItem))
            return result;

        InventoryItem removedItem = inventoryOwner.ClearHandItem(handType);
        if (removedItem != null)
        {
            result.TotalPrice += removedItem.ItemData.Price;
            result.SoldCount++;
        }

        return result;
    }

    // 판매 가능 여부(보석 종류) 판정
    private bool IsSellableItem(InventoryItem item)
    {
        return item != null && item.ItemData != null && item.ItemData.ItemType == ItemType.Jewel;
    }
}