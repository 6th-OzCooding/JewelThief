using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays quick slot items and selection state.
/// </summary>
public class QuickSlotHUD : MonoBehaviour
{
    private const string TEST_ICON_OBJECT_NAME = "Image_TestQuickSlotIcon";

    [Header("Slot Roots")]
    [SerializeField] private Transform[] _slotIconRoots;
    [SerializeField] private GameObject[] _slotSelectImages;
    [SerializeField] private GameObject[] _slotCountImages;
    [SerializeField] private TMP_Text[] _slotCountTexts;

    [Header("Test Options")]
    [SerializeField] private string _testIconResourcesPath = "Images/MasterkeyIconTest";
    [SerializeField] private Vector2 _testIconSize = new(80f, 80f);

    private Sprite _cachedDefaultToolIconSprite;
    private int _selectedSlotIndex = -1;

    /// <summary>
    /// 퀵슬롯이 선택되었을 때 선택된 슬롯 인덱스를 전달합니다.
    /// </summary>
    public event Action<int> OnSlotSelected;

    private void OnEnable()
    {
        HideAllSelectImages();
        HideAllCountImages();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            CreateTestIconInNextEmptySlot();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            SelectSlot(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            SelectSlot(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            SelectSlot(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            SelectSlot(3);

        float scrollValue = Input.mouseScrollDelta.y;
        if (scrollValue < 0f)
            SelectNextSlot(1);
        else if (scrollValue > 0f)
            SelectNextSlot(-1);
    }

    /// <summary>
    /// Creates the test icon image in the first empty quick slot root.
    /// </summary>
    public void CreateTestIconInNextEmptySlot()
    {
        Sprite testIconSprite = GetDefaultToolIconSprite();
        if (testIconSprite == null) return;
        if (_slotIconRoots == null) return;

        for (int i = 0; i < _slotIconRoots.Length; i++)
        {
            if (HasIcon(i)) continue;

            CreateTestIcon(_slotIconRoots[i], testIconSprite);
            return;
        }
    }

    /// <summary>
    /// Tool 전용 인벤토리 목록을 퀵슬롯 아이콘에 반영합니다.
    /// </summary>
    public void RefreshToolSlots(IReadOnlyList<InventoryItem> toolItems)
    {
        ClearAllSlotIcons();
        HideAllCountImages();

        if (toolItems == null || _slotIconRoots == null)
            return;

        // 1차 구현에서는 Tool 인벤토리의 순서를 그대로 퀵슬롯 표시 순서로 사용합니다.
        int slotCount = GetSlotCount();
        int toolCount = Mathf.Min(toolItems.Count, slotCount);
        for (int i = 0; i < toolCount; i++)
        {
            ItemData itemData = toolItems[i]?.ItemData;
            if (itemData == null)
                continue;

            Sprite iconSprite = LoadToolIconSprite(itemData);
            if (iconSprite == null)
                continue;

            CreateIcon(_slotIconRoots[i], iconSprite);
            RefreshSlotCount(i, toolItems[i]);
        }

        if (_selectedSlotIndex >= 0 && !HasIcon(_selectedSlotIndex))
        {
            HideAllSelectImages();
        }
    }

    public void SelectSlot(int slotIndex)
    {
        if (!HasIcon(slotIndex))
            return;

        _selectedSlotIndex = slotIndex;

        if (_slotSelectImages != null)
        {
            for (int i = 0; i < _slotSelectImages.Length; i++)
            {
                if (_slotSelectImages[i] == null) continue;

                _slotSelectImages[i].SetActive(i == _selectedSlotIndex);
            }
        }

        OnSlotSelected?.Invoke(_selectedSlotIndex);
    }

    private void SelectNextSlot(int direction)
    {
        int slotCount = GetSlotCount();
        if (slotCount <= 0) return;

        int startIndex = _selectedSlotIndex;
        if (startIndex < 0 || startIndex >= slotCount)
            startIndex = direction > 0 ? -1 : 0;

        for (int i = 0; i < slotCount; i++)
        {
            int nextIndex = GetWrappedSlotIndex(startIndex + direction, slotCount);
            if (HasIcon(nextIndex))
            {
                SelectSlot(nextIndex);
                return;
            }

            startIndex = nextIndex;
        }
    }

    private Sprite LoadToolIconSprite(ItemData itemData)
    {
        if (itemData != null && !string.IsNullOrEmpty(itemData.IconPath))
        {
            Sprite iconSprite = Resources.Load<Sprite>(itemData.IconPath);
            if (iconSprite != null)
                return iconSprite;

            Debug.LogWarning($"QuickSlotHUD: tool icon sprite not found. Item: {itemData.Id}, Path: {itemData.IconPath}");
        }

        // IconPath가 아직 비어 있는 Tool은 임시 기본 아이콘을 사용합니다.
        return GetDefaultToolIconSprite();
    }

    private Sprite GetDefaultToolIconSprite()
    {
        if (_cachedDefaultToolIconSprite != null)
            return _cachedDefaultToolIconSprite;

        _cachedDefaultToolIconSprite = Resources.Load<Sprite>(_testIconResourcesPath);
        if (_cachedDefaultToolIconSprite == null)
        {
            Debug.LogWarning($"QuickSlotHUD: default tool icon sprite not found. Path: {_testIconResourcesPath}");
        }

        return _cachedDefaultToolIconSprite;
    }

    private void CreateTestIcon(Transform slotIconRoot, Sprite testIconSprite)
    {
        CreateIcon(slotIconRoot, testIconSprite);
    }

    private void CreateIcon(Transform slotIconRoot, Sprite iconSprite)
    {
        if (slotIconRoot == null) return;
        if (iconSprite == null) return;

        ClearSlotIconRoot(slotIconRoot);

        GameObject iconObject = new(TEST_ICON_OBJECT_NAME);
        iconObject.transform.SetParent(slotIconRoot, false);

        RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();
        iconRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        iconRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        iconRectTransform.pivot = new Vector2(0.5f, 0.5f);
        iconRectTransform.anchoredPosition = Vector2.zero;
        iconRectTransform.sizeDelta = _testIconSize;

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
    }

    private void ClearAllSlotIcons()
    {
        if (_slotIconRoots == null)
            return;

        for (int i = 0; i < _slotIconRoots.Length; i++)
        {
            ClearSlotIconRoot(_slotIconRoots[i]);
        }
    }

    private void HideAllCountImages()
    {
        if (_slotCountImages == null)
            return;

        for (int i = 0; i < _slotCountImages.Length; i++)
        {
            SetSlotCountVisible(i, false);
        }
    }

    private void RefreshSlotCount(int slotIndex, InventoryItem inventoryItem)
    {
        if (inventoryItem == null)
        {
            SetSlotCountVisible(slotIndex, false);
            return;
        }

        int remainingUseCount = inventoryItem.RemainingUseCount;
        if (remainingUseCount <= 0)
        {
            SetSlotCountVisible(slotIndex, false);
            return;
        }

        SetSlotCountVisible(slotIndex, true);

        TMP_Text countText = GetSlotCountText(slotIndex);
        if (countText != null)
        {
            countText.text = remainingUseCount.ToString();
        }
    }

    private void SetSlotCountVisible(int slotIndex, bool isVisible)
    {
        GameObject countImage = GetSlotCountImage(slotIndex);
        if (countImage != null)
        {
            countImage.SetActive(isVisible);
        }

        TMP_Text countText = GetSlotCountText(slotIndex);
        if (countText != null)
        {
            countText.gameObject.SetActive(isVisible);
        }
    }

    private GameObject GetSlotCountImage(int slotIndex)
    {
        if (_slotCountImages == null)
            return null;

        if (slotIndex < 0 || slotIndex >= _slotCountImages.Length)
            return null;

        return _slotCountImages[slotIndex];
    }

    private TMP_Text GetSlotCountText(int slotIndex)
    {
        if (_slotCountTexts == null)
            return null;

        if (slotIndex < 0 || slotIndex >= _slotCountTexts.Length)
            return null;

        return _slotCountTexts[slotIndex];
    }

    private void ClearSlotIconRoot(Transform slotIconRoot)
    {
        if (slotIconRoot == null) return;

        for (int i = slotIconRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = slotIconRoot.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private bool HasIcon(int slotIndex)
    {
        if (_slotIconRoots == null)
            return false;

        if (slotIndex < 0 || slotIndex >= _slotIconRoots.Length)
            return false;

        Transform slotIconRoot = _slotIconRoots[slotIndex];
        return slotIconRoot != null && slotIconRoot.childCount > 0;
    }

    private int GetSlotCount()
    {
        return _slotIconRoots == null ? 0 : _slotIconRoots.Length;
    }

    private int GetWrappedSlotIndex(int slotIndex, int slotCount)
    {
        if (slotIndex < 0)
            return slotCount - 1;

        if (slotIndex >= slotCount)
            return 0;

        return slotIndex;
    }

    private void HideAllSelectImages()
    {
        _selectedSlotIndex = -1;

        if (_slotSelectImages == null) return;

        foreach (GameObject selectImage in _slotSelectImages)
        {
            if (selectImage == null) continue;

            selectImage.SetActive(false);
        }
    }
}
