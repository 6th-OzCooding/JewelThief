using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어가 아이템을 들 수 있는 손의 종류입니다.
/// </summary>
public enum PlayerHandType
{
    None = 0,
    Left,
    Right
}

public enum HoldType
{
    None = 0,
    Hold,   // 손에 들고 있는 상태
    Pocket  // 가방에 넣은 상태
}

/// <summary>
/// 플레이어 인벤토리 안에서 관리되는 런타임 아이템 정보입니다.
/// </summary>
public class InventoryItem
{
    /// <summary>
    /// 아이템의 기본 데이터입니다.
    /// </summary>
    public ItemData ItemData { get; }

    /// <summary>
    /// 아이템의 보관 방식입니다.
    /// </summary>
    public HoldType CurrentHoldType { get; }

    /// <summary>
    /// 인벤토리에서 사용할 아이템 정보를 생성합니다.
    /// </summary>
    public InventoryItem(ItemData itemData, HoldType holdType)
    {
        ItemData = itemData;
        CurrentHoldType = holdType;
    }
}

/// <summary>
/// 플레이어의 손과 가방에 들어 있는 아이템 상태를 관리합니다.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("가방 설정")]
    [SerializeField] private float _bagMaxWeight = 20f;

    private readonly List<InventoryItem> _bagItems = new List<InventoryItem>();

    /// <summary>
    /// 왼손에 든 아이템입니다.
    /// </summary>
    public InventoryItem LeftHandItem { get; private set; }

    /// <summary>
    /// 오른손에 든 아이템입니다.
    /// </summary>
    public InventoryItem RightHandItem { get; private set; }

    /// <summary>
    /// 가방에 들어 있는 아이템 목록입니다.
    /// </summary>
    public IReadOnlyList<InventoryItem> BagItems => _bagItems;

    /// <summary>
    /// 현재 가방이 담을 수 있는 최대 무게입니다.
    /// </summary>
    public float BagMaxWeight => _bagMaxWeight;

    /// <summary>
    /// 현재 플레이어가 들고 다닐 수 있는 전체 기준 무게입니다.
    /// </summary>
    public float MaxCarryWeight => _bagMaxWeight;

    /// <summary>
    /// 현재 가방에 들어 있는 아이템의 총 무게를 반환합니다.
    /// </summary>
    public float GetCurrentBagWeight()
    {
        float totalWeight = 0f;

        for (int i = 0; i < _bagItems.Count; i++)
        {
            totalWeight += GetItemWeight(_bagItems[i]);
        }

        return totalWeight;
    }

    /// <summary>
    /// 양손과 가방을 포함한 전체 소지 무게를 반환합니다.
    /// </summary>
    public float GetTotalCarryWeight()
    {
        // 보석 인벤토리 무게도 추가 해서 수정함
        float normalItemWeight = GetCurrentBagWeight() + GetItemWeight(LeftHandItem) + GetItemWeight(RightHandItem);

        float jewelWeight = 0f;
        if (JewelInventoryManager.Instance != null)
        {
            jewelWeight = JewelInventoryManager.Instance.GetTotalJewelWeight();
        }

        return normalItemWeight + jewelWeight;
    }

    /// <summary>
    /// 해당 아이템을 가방에 넣을 수 있는지 확인합니다.
    /// </summary>
    public bool CanAddBagItem(ItemData itemData, HoldType holdType)
    {
        if (itemData == null)
            return false;

        if (holdType != HoldType.Pocket)
            return false;

        return GetCurrentBagWeight() + itemData.Weight <= _bagMaxWeight;
    }

    /// <summary>
    /// Pocket 타입 아이템을 가방에 넣습니다.
    /// </summary>
    public bool TryAddBagItem(ItemData itemData, HoldType holdType)
    {
        if (!CanAddBagItem(itemData, holdType))
            return false;

        InventoryItem inventoryItem = new InventoryItem(itemData, holdType);
        _bagItems.Add(inventoryItem);
        Debug.Log($"{itemData.Name}을(를) 가방에 넣었습니다. 현재 가방 무게: {GetCurrentBagWeight():0.##}/{BagMaxWeight:0.##}, 현재 보유 아이템 무게: {GetTotalCarryWeight():0.##}/{MaxCarryWeight:0.##}");
        return true;
    }

    /// <summary>
    /// 아이템 보관 타입에 맞춰 가방 또는 빈 손에 아이템을 추가합니다.
    /// </summary>
    public bool TryAcquireItem(ItemData itemData, HoldType holdType)
    {
        if (itemData == null)
        {
            Debug.LogError("아이템 데이터가 없습니다.");
            return false;
        }

        if (holdType == HoldType.Pocket)
        {
            if (!TryAddBagItem(itemData, holdType))
            {
                Debug.Log($"{itemData.Name}을(를) 가방에 넣을 수 없습니다. 현재 가방 무게: {GetCurrentBagWeight():0.##}/{BagMaxWeight:0.##}");
                return false;
            }

            Debug.Log(_bagItems[_bagItems.Count - 1]);
            Debug.Log($"{itemData.Name}을(를) 가방에 넣었습니다. 현재 가방 무게: {GetCurrentBagWeight():0.##}/{BagMaxWeight:0.##}, 현재 보유 아이템 무게: {GetTotalCarryWeight():0.##}/{MaxCarryWeight:0.##}");
            return true;
        }

        if (holdType == HoldType.Hold)
        {
            if (TryEquipOrReplaceHoldItem(itemData, holdType, out PlayerHandType equippedHandType, out InventoryItem replacedItem))
            {
                string handName = equippedHandType == PlayerHandType.Left ? "왼손" : "오른손";
                if (replacedItem == null)
                {
                    Debug.Log($"{itemData.Name}을(를) {handName}에 들었습니다. 현재 보유 아이템 무게: {GetTotalCarryWeight():0.##}/{MaxCarryWeight:0.##}");
                }
                else
                {
                    Debug.Log($"{handName}의 {replacedItem.ItemData.Name}을(를) {itemData.Name}(으)로 교체했습니다. 현재 보유 아이템 무게: {GetTotalCarryWeight():0.##}/{MaxCarryWeight:0.##}");
                }

                return true;
            }

            Debug.Log($"{itemData.Name}을(를) 들 수 없습니다.");
            return false;
        }

        Debug.Log($"{itemData.Name}의 보관 타입이 올바르지 않습니다. HoldType: {holdType}");
        return false;
    }

    private bool TryEquipOrReplaceHoldItem(ItemData itemData, HoldType holdType, out PlayerHandType equippedHandType, out InventoryItem replacedItem)
    {
        equippedHandType = PlayerHandType.None;
        replacedItem = null;

        if (itemData == null || holdType != HoldType.Hold)
            return false;

        if (LeftHandItem == null)
        {
            LeftHandItem = new InventoryItem(itemData, holdType);
            equippedHandType = PlayerHandType.Left;
            LogHandEquip(itemData, equippedHandType);
            return true;
        }

        if (RightHandItem == null)
        {
            RightHandItem = new InventoryItem(itemData, holdType);
            equippedHandType = PlayerHandType.Right;
            LogHandEquip(itemData, equippedHandType);
            return true;
        }

        replacedItem = LeftHandItem;
        LeftHandItem = new InventoryItem(itemData, holdType);
        equippedHandType = PlayerHandType.Left;
        LogHandReplace(itemData, replacedItem, equippedHandType);
        return true;
    }

    /// <summary>
    /// 지정한 손에 Hold 타입 아이템을 장착할 수 있는지 확인합니다.
    /// </summary>
    public bool CanEquipHandItem(ItemData itemData, HoldType holdType, PlayerHandType handType)
    {
        if (itemData == null)
            return false;

        if (holdType != HoldType.Hold)
            return false;

        if (handType == PlayerHandType.Left)
            return LeftHandItem == null;

        if (handType == PlayerHandType.Right)
            return RightHandItem == null;

        return false;
    }

    /// <summary>
    /// 지정한 손에 Hold 타입 아이템을 장착합니다.
    /// </summary>
    public bool TryEquipHandItem(ItemData itemData, HoldType holdType, PlayerHandType handType)
    {
        if (!CanEquipHandItem(itemData, holdType, handType))
            return false;

        InventoryItem inventoryItem = new InventoryItem(itemData, holdType);

        if (handType == PlayerHandType.Left)
        {
            LeftHandItem = inventoryItem;
            LogHandEquip(itemData, handType);
            return true;
        }

        if (handType == PlayerHandType.Right)
        {
            RightHandItem = inventoryItem;
            LogHandEquip(itemData, handType);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 지정한 손에 든 아이템을 비우고 반환합니다.
    /// </summary>
    public InventoryItem ClearHandItem(PlayerHandType handType)
    {
        if (handType == PlayerHandType.Left)
        {
            InventoryItem removedItem = LeftHandItem;
            LeftHandItem = null;
            LogHandClear(removedItem, handType);
            return removedItem;
        }

        if (handType == PlayerHandType.Right)
        {
            InventoryItem removedItem = RightHandItem;
            RightHandItem = null;
            LogHandClear(removedItem, handType);
            return removedItem;
        }

        return null;
    }

    /// <summary>
    /// 지정한 손에 든 아이템을 월드에 버립니다.
    /// </summary>
    public bool TryDropHandItem(PlayerHandType handType)
    {
        // TODO: 다음 PR에서 손 위치 소켓 또는 플레이어 기준 위치를 받아 월드 아이템 생성/낙하 처리까지 연결합니다.
        return false;
    }

    /// <summary>
    /// Tool 아이템을 퀵슬롯에 등록합니다.
    /// </summary>
    public bool TryRegisterQuickSlotTool(ItemData itemData)
    {
        // TODO: 다음 PR에서 Tool 타입 검증, 최대 5개 제한, 중복 등록 정책을 정한 뒤 구현합니다.
        return false;
    }

    /// <summary>
    /// 퀵슬롯에 등록된 Tool 아이템을 오른손에 장착합니다.
    /// </summary>
    public bool TryEquipQuickSlotTool(int quickSlotIndex)
    {
        // TODO: 다음 PR에서 선택한 Tool을 오른손에 장착하고, 기존 오른손 아이템 드롭 규칙을 연결합니다.
        return false;
    }

    /// <summary>
    /// 가방에 들어 있는 특정 아이템을 제거하고 반환합니다.
    /// </summary>
    public InventoryItem RemoveBagItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null)
            return null;

        if (!_bagItems.Remove(inventoryItem))
            return null;

        Debug.Log($"{inventoryItem.ItemData.Name}을(를) 가방에서 제거했습니다. 현재 가방 무게: {GetCurrentBagWeight():0.##}/{BagMaxWeight:0.##}, 현재 보유 아이템 무게: {GetTotalCarryWeight():0.##}/{MaxCarryWeight:0.##}");
        return inventoryItem;
    }

    private void LogHandEquip(ItemData itemData, PlayerHandType handType)
    {
        string handName = GetHandName(handType);
        Debug.Log($"{itemData.Name}을(를) {handName}에 들었습니다. 현재 보유 아이템 무게: {GetTotalCarryWeight():0.##}/{MaxCarryWeight:0.##}");
    }

    private void LogHandReplace(ItemData itemData, InventoryItem replacedItem, PlayerHandType handType)
    {
        string handName = GetHandName(handType);
        string replacedItemName = replacedItem?.ItemData?.Name ?? "알 수 없는 아이템";
        Debug.Log($"{handName}의 {replacedItemName}을(를) {itemData.Name}(으)로 교체했습니다. 현재 보유 아이템 무게: {GetTotalCarryWeight():0.##}/{MaxCarryWeight:0.##}");
    }

    private void LogHandClear(InventoryItem removedItem, PlayerHandType handType)
    {
        if (removedItem == null || removedItem.ItemData == null)
            return;

        string handName = GetHandName(handType);
        Debug.Log($"{handName}의 {removedItem.ItemData.Name}을(를) 비웠습니다. 현재 보유 아이템 무게: {GetTotalCarryWeight():0.##}/{MaxCarryWeight:0.##}");
    }

    private string GetHandName(PlayerHandType handType)
    {
        if (handType == PlayerHandType.Left)
            return "왼손";

        if (handType == PlayerHandType.Right)
            return "오른손";

        return "알 수 없는 손";
    }

    private float GetItemWeight(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.ItemData == null)
            return 0f;

        return inventoryItem.ItemData.Weight;
    }

    public void FindToolAndRemove(string[] toolIds)
    {
        foreach (string toolId in toolIds)
        {
            if (LeftHandItem.ItemData.Id == toolId)
                LeftHandItem = null;
            else if (RightHandItem.ItemData.Id == toolId)
                RightHandItem = null;

            // TODO(김익환, 26.06.22): 가방에 들어 있는 Tool 아이템 제거 로직 추가 필요
        }
    }
}
