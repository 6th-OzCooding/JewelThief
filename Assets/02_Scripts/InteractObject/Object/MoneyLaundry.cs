using TeamConvention.Interfaces;
using UnityEngine;

// 가방과 양손의 보석을 골드로 환전하는 판매소(돈 세탁기). 로비에 고정 배치.
public class MoneyLaundry : MonoBehaviour, IInteractable
{
    public string GetId => "MoneyLaundry";
    public string GetName => "돈 세탁기";

    public bool CanInteract() => true;

    public void Interact(IInteractor interactor)
    {
        if (interactor is not IInventoryOwner inventoryOwner)
        {
            Debug.LogWarning("인벤토리 정보를 가져올 수 없어 판매를 진행할 수 없습니다.");
            return;
        }

        SellResult result = GameManager.Shop.SellAllJewels(inventoryOwner);

        if (!result.HasSold)
        {
            Debug.Log("판매할 수 있는 보석을 보유하고 있지 않습니다.");
            return;
        }

        GameManager.Sound.PlaySFX(SoundId.SFX_Gain01);
        Debug.Log($"보석 {soldCount}개를 판매, {totalSellPrice} 골드를 획득");
    }

    // 가방/손 공통 - 판매 가능 여부(보석 종류) 판정
    private bool IsSellableItem(InventoryItem item)
    {
        return item != null && item.ItemData != null && item.ItemData.GetItemType() == ItemType.Jewel;
    }
}