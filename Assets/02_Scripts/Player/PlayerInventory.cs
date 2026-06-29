using System.Collections.Generic;
using System;
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
    [Header("무게 설정")]
    [SerializeField] private float _baseCarryWeightLimit = 50f;
    [SerializeField] private float _bagAdditionalWeightLimit = 0f;

    [Header("가방 설정")]
    [SerializeField] private int _bagMaxCapacity = 10;

    [Header("손 아이템 표시")]
    [SerializeField] private PlayerHandItemViewer _handItemViewer;

    private readonly List<InventoryItem> _bagItems = new List<InventoryItem>();
    private readonly List<InventoryItem> _toolItems = new List<InventoryItem>();

    /// <summary>
    /// 왼손에 든 아이템입니다.
    /// </summary>
    public InventoryItem LeftHandItem { get; private set; }

    /// <summary>
    /// 오른손에 든 아이템입니다.
    /// </summary>
    public InventoryItem RightHandItem { get; private set; }

    public JewelInventoryManager JewelInventory { get; private set; }

    /// <summary>
    /// 가방에 들어 있는 아이템 목록입니다.
    /// </summary>
    public IReadOnlyList<InventoryItem> BagItems => _bagItems;

    /// <summary>
    /// 구매해서 보유 중인 Tool 아이템 목록입니다. Tool은 무게 계산과 가방 용량에서 제외됩니다.
    /// </summary>
    public IReadOnlyList<InventoryItem> ToolItems => _toolItems;

    /// <summary>
    /// Tool 전용 인벤토리 목록이 변경될 때 호출됩니다.
    /// </summary>
    public event Action<IReadOnlyList<InventoryItem>> OnToolItemsChanged;

    /// <summary>
    /// 플레이어가 기본으로 버틸 수 있는 무게 제한입니다.
    /// </summary>
    public float BaseCarryWeightLimit => _baseCarryWeightLimit;

    /// <summary>
    /// 현재 가방이 추가로 제공하는 무게 제한입니다.
    /// </summary>
    public float BagAdditionalWeightLimit => _bagAdditionalWeightLimit;

    /// <summary>
    /// 현재 가방에 넣을 수 있는 최대 아이템 개수입니다.
    /// </summary>
    public int BagMaxCapacity => _bagMaxCapacity;

    /// <summary>
    /// 현재 가방이 사용 중인 용량입니다.
    /// </summary>
    public int CurrentBagCapacity => _bagItems.Count;

    /// <summary>
    /// 현재 플레이어가 패널티 없이 들 수 있는 전체 기준 무게입니다.
    /// </summary>
    public float MaxCarryWeight => _baseCarryWeightLimit + _bagAdditionalWeightLimit;

    private void Awake()
    {
        InitializeHandItemViewer();
    }

    private void Start()
    {
        JewelInventory = JewelInventoryManager.Instance;
    }

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

        if (JewelInventory != null)
        {
            jewelWeight = JewelInventory.GetTotalJewelWeight();
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

        return CurrentBagCapacity < _bagMaxCapacity;
    }

    /// <summary>
    /// 현재 소지 무게가 기준 무게를 초과했는지 확인합니다.
    /// </summary>
    public bool IsOverweight()
    {
        return GetTotalCarryWeight() > MaxCarryWeight;
    }

    /// <summary>
    /// 현재 무게 상태 기준으로 달릴 수 있는지 확인합니다.
    /// </summary>
    public bool CanSprint()
    {
        return !IsOverweight();
    }

    /// <summary>
    /// Pocket 타입 아이템을 가방에 넣습니다.
    /// </summary>
    public bool TryAddBagItem(ItemData itemData, HoldType holdType)
    {
        if (itemData == null)
            return false;

        if (holdType != HoldType.Pocket)
            return false;

        if (CurrentBagCapacity >= _bagMaxCapacity)
        {
            LogBagCapacityFull(itemData);
            return false;
        }

        InventoryItem inventoryItem = new InventoryItem(itemData, holdType);
        _bagItems.Add(inventoryItem);

        LogBagItemAdded(itemData);
        LogCarryWeightState();
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

        // Tool은 Hold 타입이어도 구매 즉시 손에 들지 않고 Tool 전용 인벤토리에 보관합니다.
        if (IsToolItem(itemData))
        {
            return TryAddToolItem(itemData);
        }

        if (holdType == HoldType.Pocket)
        {
            if (!TryAddBagItem(itemData, holdType))
            {
                return false;
            }

            return true;
        }

        if (holdType == HoldType.Hold)
        {
            if (TryEquipOrReplaceHoldItem(itemData, holdType, out PlayerHandType equippedHandType, out InventoryItem replacedItem))
            {
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
            SetHandItem(PlayerHandType.Left, new InventoryItem(itemData, holdType));
            equippedHandType = PlayerHandType.Left;
            LogHandEquip(itemData, equippedHandType);
            LogCarryWeightState();
            return true;
        }

        if (RightHandItem == null)
        {
            SetHandItem(PlayerHandType.Right, new InventoryItem(itemData, holdType));
            equippedHandType = PlayerHandType.Right;
            LogHandEquip(itemData, equippedHandType);
            LogCarryWeightState();
            return true;
        }

        replacedItem = LeftHandItem;
        SetHandItem(PlayerHandType.Left, new InventoryItem(itemData, holdType));
        equippedHandType = PlayerHandType.Left;
        LogHandReplace(itemData, replacedItem, equippedHandType);
        LogCarryWeightState();
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

        if (IsToolItem(itemData))
            return handType == PlayerHandType.Right && RightHandItem == null;

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
            SetHandItem(handType, inventoryItem);
            LogHandEquip(itemData, handType);
            LogCarryWeightState();
            return true;
        }

        if (handType == PlayerHandType.Right)
        {
            SetHandItem(handType, inventoryItem);
            LogHandEquip(itemData, handType);
            LogCarryWeightState();
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
            SetHandItem(handType, null);
            LogHandClear(removedItem, handType);
            return removedItem;
        }

        if (handType == PlayerHandType.Right)
        {
            InventoryItem removedItem = RightHandItem;
            SetHandItem(handType, null);
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
        // TODO: 다음 PR에서 Tool 타입 검증, 최대 4개 제한, 중복 등록 정책을 정한 뒤 구현합니다.
        return false;
    }

    /// <summary>
    /// 퀵슬롯에 등록된 Tool 아이템을 오른손에 장착합니다.
    /// </summary>
    public bool TryEquipQuickSlotTool(int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= _toolItems.Count)
            return false;

        InventoryItem toolItem = _toolItems[quickSlotIndex];
        if (toolItem == null || toolItem.ItemData == null || !IsToolItem(toolItem.ItemData))
            return false;

        // 퀵슬롯 Tool 전환은 드롭이 아니라 오른손의 활성 Tool을 바꾸는 동작입니다.
        InventoryItem previousRightHandItem = RightHandItem;
        SetHandItem(PlayerHandType.Right, toolItem);

        if (previousRightHandItem == null)
        {
            LogHandEquip(toolItem.ItemData, PlayerHandType.Right);
            return true;
        }

        if (IsToolItem(previousRightHandItem.ItemData))
        {
            Debug.Log($"{previousRightHandItem.ItemData.Name}을(를) {toolItem.ItemData.Name}(으)로 교체했습니다.");
            return true;
        }

        Debug.Log($"{previousRightHandItem.ItemData.Name}을(를) {toolItem.ItemData.Name}(으)로 교체했습니다. 실제 드롭 생성은 다음 단계에서 연결합니다.");
        LogCarryWeightState();
        return true;
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

        Debug.Log($"{inventoryItem.ItemData.Name}을(를) 가방에서 제거했습니다. 현재 가방 용량: {CurrentBagCapacity}/{BagMaxCapacity}");
        LogCarryWeightState();
        return inventoryItem;
    }

    private void LogBagItemAdded(ItemData itemData)
    {
        Debug.Log($"{itemData.Name}을(를) 가방에 넣었습니다. 현재 가방 용량: {CurrentBagCapacity}/{BagMaxCapacity}");
    }

    private void LogBagCapacityFull(ItemData itemData)
    {
        Debug.Log($"{itemData.Name}을(를) 가방에 넣을 수 없습니다. 가방 용량이 부족합니다. 현재 가방 용량: {CurrentBagCapacity}/{BagMaxCapacity}");
    }

    private bool TryAddToolItem(ItemData itemData)
    {
        if (itemData == null || !IsToolItem(itemData))
            return false;

        InventoryItem inventoryItem = new InventoryItem(itemData, HoldType.Hold);
        _toolItems.Add(inventoryItem);

        Debug.Log($"{itemData.Name}을(를) Tool 전용 인벤토리에 보관했습니다. Tool은 가방 용량과 무게 계산에서 제외됩니다.");
        NotifyToolItemsChanged();
        return true;
    }

    private void LogCarryWeightState()
    {
        float currentWeight = GetTotalCarryWeight();
        float maxWeight = MaxCarryWeight;

        if (currentWeight > maxWeight)
        {
            Debug.LogWarning($"무게 제한을 초과했습니다. 현재 보유 아이템 무게: {currentWeight:0.##}/{maxWeight:0.##}. 이동속도 감소 및 달리기 불가 상태입니다.");
            return;
        }

        Debug.Log($"현재 보유 아이템 무게: {currentWeight:0.##}/{maxWeight:0.##}");
    }

    private void LogHandEquip(ItemData itemData, PlayerHandType handType)
    {
        string handName = GetHandName(handType);
        Debug.Log($"{itemData.Name}을(를) {handName}에 들었습니다.");
    }

    private void LogHandReplace(ItemData itemData, InventoryItem replacedItem, PlayerHandType handType)
    {
        string handName = GetHandName(handType);
        string replacedItemName = replacedItem?.ItemData?.Name ?? "알 수 없는 아이템";
        Debug.Log($"{handName}의 {replacedItemName}을(를) {itemData.Name}(으)로 교체했습니다.");
    }

    private void LogHandClear(InventoryItem removedItem, PlayerHandType handType)
    {
        if (removedItem == null || removedItem.ItemData == null)
            return;

        string handName = GetHandName(handType);
        Debug.Log($"{handName}의 {removedItem.ItemData.Name}을(를) 비웠습니다.");
        LogCarryWeightState();
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

    private bool IsToolItem(ItemData itemData)
    {
        return itemData != null && itemData.GetItemType() == ItemType.Tool;
    }

    private void InitializeHandItemViewer()
    {
        if (_handItemViewer == null)
        {
            _handItemViewer = GetComponent<PlayerHandItemViewer>();
        }

        if (_handItemViewer == null)
        {
            _handItemViewer = gameObject.AddComponent<PlayerHandItemViewer>();
        }

        _handItemViewer.RefreshHands(LeftHandItem, RightHandItem);
    }

    private void SetHandItem(PlayerHandType handType, InventoryItem inventoryItem)
    {
        if (handType == PlayerHandType.Left)
        {
            LeftHandItem = inventoryItem;
        }
        else if (handType == PlayerHandType.Right)
        {
            RightHandItem = inventoryItem;
        }
        else
        {
            return;
        }

        if (_handItemViewer != null)
        {
            _handItemViewer.SetHandItem(handType, inventoryItem);
        }
    }

    public void FindToolAndRemove(string[] toolIds)
    {
        if (toolIds == null)
            return;

        foreach (string toolId in toolIds)
        {
            if (LeftHandItem?.ItemData?.Id == toolId)
                ClearHandItem(PlayerHandType.Left);
            else if (RightHandItem?.ItemData?.Id == toolId)
                ClearHandItem(PlayerHandType.Right);

            RemoveToolItems(toolId);
        }
    }

    private void RemoveToolItems(string toolId)
    {
        if (string.IsNullOrEmpty(toolId))
            return;

        bool isRemoved = false;
        for (int i = _toolItems.Count - 1; i >= 0; i--)
        {
            if (_toolItems[i]?.ItemData?.Id != toolId)
                continue;

            Debug.Log($"{_toolItems[i].ItemData.Name}을(를) Tool 전용 인벤토리에서 제거했습니다.");
            _toolItems.RemoveAt(i);
            isRemoved = true;
        }

        if (isRemoved)
        {
            NotifyToolItemsChanged();
        }
    }

    private void NotifyToolItemsChanged()
    {
        OnToolItemsChanged?.Invoke(ToolItems);
    }

    public void AddJewel(Jewel gem)
    {
        if (JewelInventory != null)
        {
            JewelInventory.AddJewelToTempQueue(gem);
        }
    }
}
