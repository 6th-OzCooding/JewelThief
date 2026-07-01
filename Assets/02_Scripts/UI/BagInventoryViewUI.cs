using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 가방 상태를 화면에 표시하는 UI입니다.
/// </summary>
public class BagInventoryViewUI : UIBase
{
    [Header("Bag Status Text")]
    [SerializeField] private TMP_Text _totalWeightText;
    [SerializeField] private TMP_Text _bagCapacityText;

    [Header("Weight Color")]
    [SerializeField] private Image _backPackImage;
    [SerializeField] private Transform _backPackTextRoot;
    [SerializeField] private Color _safeColor = new Color(0.2f, 0.9f, 0.25f);
    [SerializeField] private Color _warningColor = new Color(1f, 0.55f, 0.05f);
    [SerializeField] private Color _overweightColor = new Color(1f, 0.1f, 0.05f);

    private PlayerInventory _playerInventory;
    private TMP_Text[] _backPackTexts;

    private void OnDisable()
    {
        UnbindPlayerInventory();
    }

    /// <summary>
    /// 표시할 플레이어 인벤토리를 연결하고 현재 상태를 즉시 갱신합니다.
    /// </summary>
    public void BindPlayerInventory(PlayerInventory playerInventory)
    {
        if (_playerInventory == playerInventory)
        {
            Refresh();
            return;
        }

        UnbindPlayerInventory();

        _playerInventory = playerInventory;

        if (_playerInventory != null)
            _playerInventory.OnBagItemsChanged += HandleBagItemsChanged;

        Refresh();
    }

    /// <summary>
    /// 현재 연결된 플레이어 인벤토리 기준으로 표시 텍스트를 갱신합니다.
    /// </summary>
    public void Refresh()
    {
        if (_playerInventory == null)
        {
            SetText(_totalWeightText, "[ 0 / 0 ]");
            SetText(_bagCapacityText, "[ 00 / 00 ]");
            return;
        }

        float currentWeight = _playerInventory.GetTotalCarryWeight();
        float maxWeight = _playerInventory.MaxCarryWeight;
        string currentWeightText = FormatWeight(currentWeight);
        string maxWeightText = FormatWeight(_playerInventory.MaxCarryWeight);
        string currentCapacityText = FormatCapacity(_playerInventory.CurrentBagCapacity, _playerInventory.BagMaxCapacity);
        string maxCapacityText = FormatCapacity(_playerInventory.BagMaxCapacity, _playerInventory.BagMaxCapacity);

        SetText(_totalWeightText, $"[ {currentWeightText} / {maxWeightText} ]");
        SetText(_bagCapacityText, $"[ {currentCapacityText} / {maxCapacityText} ]");
        ApplyWeightColor(currentWeight, maxWeight);
    }

    private void HandleBagItemsChanged(IReadOnlyList<InventoryItem> bagItems)
    {
        Refresh();
    }

    private void UnbindPlayerInventory()
    {
        if (_playerInventory != null)
            _playerInventory.OnBagItemsChanged -= HandleBagItemsChanged;

        _playerInventory = null;
    }

    private string FormatWeight(float weight)
    {
        return weight.ToString("0.##");
    }

    private string FormatCapacity(int value, int maxCapacity)
    {
        if (maxCapacity <= 10)
            return value.ToString("D2");

        return value.ToString();
    }

    private void ApplyWeightColor(float currentWeight, float maxWeight)
    {
        Color targetColor = GetWeightColor(currentWeight, maxWeight);

        if (_backPackImage != null)
            _backPackImage.color = targetColor;

        TMP_Text[] texts = GetBackPackTexts();
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] == null)
                continue;

            texts[i].color = targetColor;
        }
    }

    private Color GetWeightColor(float currentWeight, float maxWeight)
    {
        if (maxWeight <= 0f)
            return _safeColor;

        float weightRatio = currentWeight / maxWeight;
        if (weightRatio > 1f)
            return _overweightColor;

        return Color.Lerp(_safeColor, _warningColor, Mathf.Clamp01(weightRatio));
    }

    private TMP_Text[] GetBackPackTexts()
    {
        if (_backPackTexts != null)
            return _backPackTexts;

        if (_backPackTextRoot == null)
        {
            _backPackTexts = new TMP_Text[0];
            return _backPackTexts;
        }

        _backPackTexts = _backPackTextRoot.GetComponentsInChildren<TMP_Text>(true);
        return _backPackTexts;
    }

    private void SetText(TMP_Text targetText, string text)
    {
        if (targetText == null)
            return;

        targetText.text = text;
    }
}
