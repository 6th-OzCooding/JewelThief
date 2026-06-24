using TMPro;
using UnityEngine;

/// <summary>
/// 상점 아이템 정보를 표시하는 Hover 팝업 UI입니다.
/// </summary>
public class ShopInfoPopupUI : HoverPopupUIBase
{
    [Header("Text Assignment")]
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private TMP_Text _currentMoneyText;
    [SerializeField] private TMP_Text _promptText;

    protected override Vector2 DefaultPopupOffset => new(400f, 80f);

    protected override void Awake()
    {
        base.Awake();
        CacheTextComponents();
    }

    /// <summary>
    /// 상점 Hover 팝업 내용을 갱신합니다.
    /// </summary>
    public void SetInfo(PopupDisplayData displayData)
    {
        if (displayData == null)
            return;

        CacheTextComponents();
        SetText(_itemNameText, displayData.Title);
        SetText(_descriptionText, displayData.Description);
        SetText(_priceText, FormatMoney(displayData.Price));
        SetText(_currentMoneyText, FormatMoney(displayData.CurrentMoney));
        SetText(_promptText, displayData.Prompt);
    }

    private void CacheTextComponents()
    {
        _itemNameText ??= FindTextByName("Text_ItemName");
        _descriptionText ??= FindTextByName("Text_Description");
        _priceText ??= FindTextByName("Text_PriceData");
        _currentMoneyText ??= FindTextByName("Text_CurrentMoneyData");
        _promptText ??= FindTextByName("Text_InteractionKeyInfo");
    }

    private string FormatMoney(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "-";

        return value.EndsWith("$") ? value : $"{value}$";
    }
}
