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

    [Header("Test Options")]
    [SerializeField] private string _testIconResourcesPath = "Images/MasterkeyIconTest";
    [SerializeField] private Vector2 _testIconSize = new(80f, 80f);

    private Sprite _cachedTestIconSprite;
    private int _selectedSlotIndex = -1;

    private void OnEnable()
    {
        HideAllSelectImages();
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
        Sprite testIconSprite = GetTestIconSprite();
        if (testIconSprite == null) return;
        if (_slotIconRoots == null) return;

        for (int i = 0; i < _slotIconRoots.Length; i++)
        {
            if (HasIcon(i)) continue;

            CreateTestIcon(_slotIconRoots[i], testIconSprite);
            return;
        }
    }

    public void SelectSlot(int slotIndex)
    {
        if (!HasIcon(slotIndex))
            return;

        _selectedSlotIndex = slotIndex;

        if (_slotSelectImages == null) return;

        for (int i = 0; i < _slotSelectImages.Length; i++)
        {
            if (_slotSelectImages[i] == null) continue;

            _slotSelectImages[i].SetActive(i == _selectedSlotIndex);
        }
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

    private Sprite GetTestIconSprite()
    {
        if (_cachedTestIconSprite != null)
            return _cachedTestIconSprite;

        _cachedTestIconSprite = Resources.Load<Sprite>(_testIconResourcesPath);
        if (_cachedTestIconSprite == null)
        {
            Debug.LogWarning($"QuickSlotHUD: test icon sprite not found. Path: {_testIconResourcesPath}");
        }

        return _cachedTestIconSprite;
    }

    private void CreateTestIcon(Transform slotIconRoot, Sprite testIconSprite)
    {
        if (slotIconRoot == null) return;

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
        iconImage.sprite = testIconSprite;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
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
