using TeamConvention.Interfaces;
using UnityEngine;

// 가방과 양손의 보석을 골드로 환전하는 판매소(돈 세탁기). 로비에 고정 배치.
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

        SellResult result = GameManager.Shop.SellAllJewels(inventoryOwner);

        if (!result.HasSold)
        {
            Debug.Log("판매할 수 있는 보석을 보유하고 있지 않습니다.");
            return;
        }

        GameManager.Sound.PlaySFX(SoundId.SFX_Gain01);
        Debug.Log($"보석 {result.SoldCount}개를 판매, {result.TotalPrice} 골드를 획득");
    }
}