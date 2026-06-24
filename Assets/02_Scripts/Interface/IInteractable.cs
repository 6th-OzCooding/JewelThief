using System.Collections.Generic;
using UnityEngine;

namespace TeamConvention.Interfaces
{
    public interface IInteractable
    {
        string GetId { get; }
        string GetName { get; }
        bool CanInteract();
        void Interact(IInteractor interactor);
    }

    public interface IInteractor
    {
        public Vector3 Position { get; }
    }

    // 인벤토리를 보유함을 나타내는 역할 인터페이스
    public interface IInventoryOwner
    {
        IReadOnlyList<InventoryItem> BagItems { get; }
        InventoryItem LeftHandItem { get; }
        InventoryItem RightHandItem { get; }

        bool TryAcquireItem(ItemData itemData, HoldType holdType, out InventoryItem acquiredItem, out string resultMessage);
        InventoryItem RemoveBagItem(InventoryItem inventoryItem);
        InventoryItem ClearHandItem(PlayerHandType handType);
    }
}