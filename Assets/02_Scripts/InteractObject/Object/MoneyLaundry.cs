using System.Collections.Generic;
using TeamConvention.Interfaces;
using UnityEngine;

/// <summary>
/// 가방과 양손에 보유한 보석을 한 번에 골드로 환전하는 판매소(돈 세탁기)
/// 데이터 테이블 의존 없이 로비에 고정 배치되는 독립 오브젝트
/// 보석(Jewel) 종류 아이템만 판매 가능.
/// </summary>
public class MoneyLaundry : MonoBehaviour, IInteractable
{
    public string GetId => "MoneyLaundry";
    public string GetName => "돈 세탁기";

    public bool CanInteract() => true;
    // TODO(김경훈 2026-06-23): CanInteract 단계에서 가방·손 내 보석 보유 여부를 미리 검증하기 어려움 (interactor 인자 없음).

    public void Interact(IInteractor interactor)
    {
        if (interactor is not IInventoryOwner inventoryOwner)
        {
            Debug.LogWarning("인벤토리 정보를 가져올 수 없어 판매를 진행할 수 없습니다.");
            return;
        }

        int totalSellPrice = 0;
        int soldCount = 0;

        List<InventoryItem> sellableBagItems = new List<InventoryItem>();
        foreach (InventoryItem bagItem in inventoryOwner.BagItems)
        {
            if (IsSellableItem(bagItem))
            {
                sellableBagItems.Add(bagItem);
            }
        }


        // TODO: (김경훈 - 26.06.22) 상점 구매 스크립트로 이동
        foreach (InventoryItem sellableItem in sellableBagItems)
        {
            InventoryItem removedItem = inventoryOwner.RemoveBagItem(sellableItem);
            if (removedItem == null)
                continue;

            totalSellPrice += removedItem.ItemData.Price;
            soldCount++;
        }

        if (IsSellableItem(inventoryOwner.RightHandItem))
        {
            InventoryItem removedItem = inventoryOwner.ClearHandItem(PlayerHandType.Right);
            if (removedItem != null)
            {
                totalSellPrice += removedItem.ItemData.Price;
                soldCount++;
            }
        }

        if (IsSellableItem(inventoryOwner.LeftHandItem))
        {
            InventoryItem removedItem = inventoryOwner.ClearHandItem(PlayerHandType.Left);
            if (removedItem != null)
            {
                totalSellPrice += removedItem.ItemData.Price;
                soldCount++;
            }
        }

        if (soldCount == 0)
        {
            Debug.Log("판매할 수 있는 보석을 보유하고 있지 않습니다.");
            return;
        }

        GameManager.Instance.AddGold(totalSellPrice);
        GameManager.Sound.PlaySFX(SoundId.SFX_Gain01);
        Debug.Log($"보석 {soldCount}개를 판매, {totalSellPrice} 골드를 획득");
    }

    // 가방/손 공통 - 판매 가능 여부(보석 종류) 판정
    private bool IsSellableItem(InventoryItem item)
    {
        return item != null && item.ItemData != null && item.ItemData.ItemType == ItemType.Jewel;
    }
}