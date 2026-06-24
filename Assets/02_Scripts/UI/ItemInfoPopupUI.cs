using TMPro;
using UnityEngine;

/// <summary>
/// 화면 중앙 Hover 대상의 아이템 정보를 표시하는 팝업 UI입니다.
/// </summary>
public class ItemInfoPopupUI : HoverPopupUIBase
{
    [Header("Text Assignment")]
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _rarityText;
    [SerializeField] private TMP_Text _weightText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private TMP_Text _promptText;

    protected override void Awake()
    {
        base.Awake();
        CacheTextComponents();
    }

    /// <summary>
    /// 아이템 Hover 팝업 내용을 갱신합니다.
    /// </summary>
    public void SetInfo(PopupDisplayData displayData)
    {
        if (displayData == null)
            return;

        CacheTextComponents();
        SetText(_itemNameText, displayData.Title);
        SetText(_rarityText, displayData.Rarity);
        SetText(_weightText, displayData.Weight);
        SetText(_priceText, FormatMoney(displayData.Price));
        SetText(_promptText, displayData.Prompt);
    }

    private void CacheTextComponents()
    {
        _itemNameText ??= FindTextByName("Text_ItemName");
        _rarityText ??= FindTextByName("Text_RarityData");
        _weightText ??= FindTextByName("Text_WeightData");
        _priceText ??= FindTextByName("Text_PriceData");
        _promptText ??= FindTextByName("Text_InteractionKeyInfo");
    }

    private string FormatMoney(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.EndsWith("$") ? value : $"{value}$";
    }
}
