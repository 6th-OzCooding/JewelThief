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

        SellResult result = GameManager.Shop.SellAllItem(inventoryOwner);

        if (!result.HasSold)
        {
            return;
        }

        GameManager.Sound.PlaySFX(SoundId.SFX_Gain01);
    }
}