using System;
using System.Collections.Generic;
using System.Linq;
using TeamConvention.Interfaces;
using UnityEngine;

/// <summary>
/// 상호작용 대상 데이터와 PopupViewData를 합쳐 Hover 팝업 출력 데이터를 만듭니다.
/// </summary>
public static class PopupViewDataBuilder
{
    private const string STAGE_SELECT_CHAIR_DATA_ID = "StageSelectChair";
    private const string MONEY_LAUNDRY_DATA_ID = "MoneyLaundry";
    private const string WASHER_DATA_ID_PREFIX = "Washer_";
    private const string TOTAL_PRICE_TOKEN = "{TotalPrice}";

    private static Dictionary<string, RawItemPopupData> _rawItemPopupDataTable;

    /// <summary>
    /// 상호작용 대상과 팝업 설정을 기준으로 UI 출력 데이터를 생성합니다.
    /// </summary>
    public static bool TryBuild(IInteractable interactable, PopupInfoTarget popupInfoTarget, PlayerController playerController, out PopupDisplayData displayData)
    {
        displayData = null;

        if (interactable == null || GameManager.DataTable == null)
            return false;

        string dataId = interactable.GetId;
        string popupViewDataId = ResolvePopupViewDataId(dataId, popupInfoTarget);
        PopupViewData popupViewData = GameManager.DataTable.GetPopupViewData(popupViewDataId);
        if (popupViewData == null)
        {
            Debug.LogWarning($"PopupViewData를 찾을 수 없습니다. Id: {popupViewDataId}, TargetDataId: {dataId}");
            return false;
        }

        PopupType popupType = popupViewData.GetPopupType();
        displayData = new PopupDisplayData
        {
            PopupType = popupType,
            Title = interactable.GetName,
            Prompt = ResolvePrompt(dataId, popupViewData, playerController),
            CurrentMoney = ResolveCurrentMoney(popupType)
        };

        FillSourceData(dataId, displayData);
        return true;
    }

    /// <summary>
    /// 세탁기처럼 상호작용 성공 후 출력할 금액 포함 문구를 만듭니다.
    /// </summary>
    public static string BuildPurchaseSuccessPrompt(PopupTargetType targetType, int totalPrice)
    {
        if (targetType == PopupTargetType.None || GameManager.DataTable == null)
            return string.Empty;

        PopupViewData popupViewData = GameManager.DataTable.GetPopupViewData($"TargetType_{targetType}");
        if (popupViewData == null)
            return string.Empty;

        return FormatTotalPricePrompt(popupViewData.PurchaseSuccessPrompt, totalPrice);
    }

    private static string ResolvePopupViewDataId(string dataId, PopupInfoTarget popupInfoTarget)
    {
        string explicitId = popupInfoTarget != null ? popupInfoTarget.GetPopupViewDataId() : string.Empty;
        if (!string.IsNullOrEmpty(explicitId))
            return explicitId;

        if (string.IsNullOrEmpty(dataId))
            return string.Empty;

        if (dataId.StartsWith("Item_"))
            return "TargetType_Item";

        if (dataId.StartsWith("Object_"))
            return "TargetType_Box";

        if (dataId.StartsWith("Door_"))
            return "TargetType_Door";

        if (dataId.StartsWith("Trap_"))
            return "TargetType_Trap";

        if (dataId == STAGE_SELECT_CHAIR_DATA_ID)
            return "TargetType_StageSelectChair";

        if (dataId == MONEY_LAUNDRY_DATA_ID || dataId.StartsWith(WASHER_DATA_ID_PREFIX))
            return "TargetType_Washer";

        return string.Empty;
    }

    private static string FormatTotalPricePrompt(string prompt, int totalPrice)
    {
        if (string.IsNullOrEmpty(prompt))
            return string.Empty;

        return prompt.Replace(TOTAL_PRICE_TOKEN, totalPrice.ToString());
    }

    private static void FillSourceData(string dataId, PopupDisplayData displayData)
    {
        if (string.IsNullOrEmpty(dataId) || displayData == null)
            return;

        ItemData itemData = GameManager.DataTable.GetItemData(dataId);
        if (itemData != null)
        {
            displayData.Title = itemData.Name;
            displayData.Description = itemData.Description;
            displayData.Rarity = ResolveItemGradeText(itemData);
            displayData.Weight = itemData.Weight.ToString("0.##");
            displayData.Price = itemData.Price.ToString();
            return;
        }

        InteractableContainerData containerData = GameManager.DataTable.GetInteractableContainerData(dataId);
        if (containerData != null)
        {
            displayData.Title = containerData.ContainerName;
            displayData.Description = containerData.ContainerComment;
            return;
        }

        Door doorData = GameManager.DataTable.GetDoorData(dataId);
        if (doorData != null)
        {
            displayData.Title = doorData.DoorName;
            displayData.Description = doorData.DoorComment;
        }
    }

    private static string ResolvePrompt(string dataId, PopupViewData popupViewData, PlayerController playerController)
    {
        if (popupViewData == null)
            return string.Empty;

        ItemData itemData = GameManager.DataTable.GetItemData(dataId);
        if (itemData != null && IsOverweight(itemData, playerController) && !string.IsNullOrEmpty(popupViewData.OverweightPrompt))
            return popupViewData.OverweightPrompt;

        if (IsLocked(dataId) && !string.IsNullOrEmpty(popupViewData.LockedPrompt))
            return popupViewData.LockedPrompt;

        return popupViewData.DefaultPrompt;
    }

    private static string ResolveCurrentMoney(PopupType popupType)
    {
        if (popupType != PopupType.ShopInfo || GameManager.Instance == null)
            return string.Empty;

        return GameManager.Instance._gold.ToString();
    }

    private static bool IsLocked(string dataId)
    {
        InteractableContainerData containerData = GameManager.DataTable.GetInteractableContainerData(dataId);
        if (containerData != null)
            return containerData.IsContainerDisarm;

        Door doorData = GameManager.DataTable.GetDoorData(dataId);
        if (doorData != null)
            return doorData.IsDisarm;

        return false;
    }

    private static bool IsOverweight(ItemData itemData, PlayerController playerController)
    {
        if (itemData == null || playerController == null || playerController.Inventory == null)
            return false;

        InventoryTypeData inventoryTypeData = GameManager.DataTable.GetInventoryTypeData(itemData.Id);
        if (inventoryTypeData == null)
            return false;

        HoldType holdType = inventoryTypeData.GetHoldType();
        if (holdType != HoldType.Pocket)
            return false;

        return !playerController.Inventory.CanAddBagItem(itemData, holdType);
    }

    private static string ResolveItemGradeText(ItemData itemData)
    {
        if (itemData == null)
            return string.Empty;

        if (itemData.GetItemGrade() != ItemGrade.None)
            return itemData.GetItemGrade().ToString();

        RawItemPopupData rawData = GetRawItemPopupData(itemData.Id);
        if (rawData != null && !string.IsNullOrEmpty(rawData.CurrentItemGrade))
            return rawData.CurrentItemGrade;

        return ItemGrade.None.ToString();
    }

    private static RawItemPopupData GetRawItemPopupData(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;

        _rawItemPopupDataTable ??= LoadRawItemPopupDataTable();
        return _rawItemPopupDataTable.TryGetValue(itemId, out RawItemPopupData rawData) ? rawData : null;
    }

    private static Dictionary<string, RawItemPopupData> LoadRawItemPopupDataTable()
    {
        TextAsset textAsset = Utils.ResourcesLoad<TextAsset>("JsonOutput/ItemData");
        if (textAsset == null)
            return new Dictionary<string, RawItemPopupData>();

        try
        {
            RawItemPopupDataWrapper wrapper = JsonUtility.FromJson<RawItemPopupDataWrapper>($"{{\"items\":{textAsset.text}}}");
            if (wrapper == null || wrapper.items == null)
                return new Dictionary<string, RawItemPopupData>();

            return wrapper.items
                .Where(item => item != null && !string.IsNullOrEmpty(item.Id))
                .ToDictionary(item => item.Id);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PopupViewDataBuilder] ItemData 원본 레어도 로드 오류: {ex.Message}");
            return new Dictionary<string, RawItemPopupData>();
        }
    }

    [Serializable]
    private class RawItemPopupData
    {
        public string Id;
        public string CurrentItemGrade;
    }

    [Serializable]
    private class RawItemPopupDataWrapper
    {
        public List<RawItemPopupData> items;
    }
}
