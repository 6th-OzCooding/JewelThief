using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays PlayerInventory.BagItems in the 3D bag view and routes bag-item drop input.
/// </summary>
public class BagInventoryViewController : MonoBehaviour
{
    private const string DROP_PROMPT_TEXT = "[E] : 버리기";

    [Header("View Root")]
    [SerializeField] private GameObject _viewRoot;
    [SerializeField] private Transform _dropItemRoot;
    [SerializeField] private Transform _viewRotationRoot;

    [Header("View Camera")]
    [SerializeField] private Camera _viewCamera;
    [SerializeField] private LayerMask _viewItemLayerMask = ~0;
    [SerializeField] private float _hoverDetectDistance = 20f;

    [Header("Item Spawn")]
    [SerializeField] private string _itemObjectPrefabAddress = "ItemObject";
    [SerializeField] private float _dropPositionJitter = 0.12f;

    private readonly Dictionary<InventoryItem, GameObject> _viewItemObjects = new();

    private PlayerInventory _playerInventory;
    private PlayerInputHandler _playerInput;
    private InteractionHoverDetector _playerHoverDetector;
    private PlayerInputMode _previousInputMode = PlayerInputMode.Gameplay;
    private BagInventoryViewItem _hoveredViewItem;
    private CenterPointUICursorMode _centerPointCursorMode;
    private bool _wasPlayerHoverDetectorEnabled;
    private bool _isOpen;

    /// <summary>
    /// Whether the 3D bag view is currently open.
    /// </summary>
    public bool IsOpen => _isOpen;

    private void Awake()
    {
        SetViewOpen(false);
    }

    private void OnDestroy()
    {
        UnbindPlayer();
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        UpdateHover();
    }

    /// <summary>
    /// Connects the bag view to the player's inventory and input.
    /// </summary>
    public void BindPlayer(PlayerInventory playerInventory, PlayerInputHandler playerInput)
    {
        UnbindPlayer();

        _playerInventory = playerInventory;
        _playerInput = playerInput;
        _playerHoverDetector = _playerInput != null ? _playerInput.GetComponentInParent<InteractionHoverDetector>() : null;

        if (_playerInventory != null)
        {
            _playerInventory.OnBagItemsChanged += HandleBagItemsChanged;
            SyncViewItems(_playerInventory.BagItems);
        }

        if (_playerInput != null)
        {
            _playerInput.OnJewelryInventoryToggleEvent += ToggleView;
            _playerInput.OnInteractEvent += HandleDropInput;
        }
    }

    /// <summary>
    /// Disconnects the current player references from the bag view.
    /// </summary>
    public void UnbindPlayer()
    {
        if (_playerInventory != null)
        {
            _playerInventory.OnBagItemsChanged -= HandleBagItemsChanged;
        }

        if (_playerInput != null)
        {
            _playerInput.OnJewelryInventoryToggleEvent -= ToggleView;
            _playerInput.OnInteractEvent -= HandleDropInput;
        }

        ClearHover();
        ClearViewItems();

        _playerInventory = null;
        _playerInput = null;
        _playerHoverDetector = null;
    }

    /// <summary>
    /// Opens the 3D bag view.
    /// </summary>
    public void OpenView()
    {
        if (IsOpen)
            return;

        if (_playerInput != null)
        {
            _previousInputMode = _playerInput.CurrentMode;
            _playerInput.SetMode(PlayerInputMode.UIOnly);
        }

        OpenBagCursorUI();
        SetPlayerHoverDetectorEnabled(false);

        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();

        SetViewOpen(true);
    }

    /// <summary>
    /// Closes the 3D bag view.
    /// </summary>
    public void CloseView()
    {
        if (!IsOpen)
            return;

        ClearHover();
        SetViewOpen(false);

        CloseBagCursorUI();

        if (_playerInput != null)
        {
            _playerInput.SetMode(_previousInputMode);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        SetPlayerHoverDetectorEnabled(true);

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    private void ToggleView()
    {
        if (IsOpen)
        {
            CloseView();
            return;
        }

        OpenView();
    }

    private void HandleBagItemsChanged(IReadOnlyList<InventoryItem> bagItems)
    {
        SyncViewItems(bagItems);
    }

    private void SyncViewItems(IReadOnlyList<InventoryItem> bagItems)
    {
        ClearHover();

        if (bagItems == null)
        {
            ClearViewItems();
            return;
        }

        RemoveMissingViewItems(bagItems);
        for (int i = 0; i < bagItems.Count; i++)
        {
            InventoryItem inventoryItem = bagItems[i];
            if (inventoryItem == null || _viewItemObjects.ContainsKey(inventoryItem))
                continue;

            CreateViewItem(inventoryItem);
        }
    }

    private void CreateViewItem(InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.ItemData == null || _dropItemRoot == null)
            return;

        GameObject prefab = GameManager.Resource.GetLoadedAsset<GameObject>(_itemObjectPrefabAddress);
        if (prefab == null)
            return;

        Vector3 spawnPosition = GetDropSpawnPosition();
        Transform itemParent = _viewRotationRoot != null ? _viewRotationRoot : _dropItemRoot.parent;
        GameObject viewObject = Instantiate(prefab, spawnPosition, Quaternion.identity, itemParent);
        viewObject.name = $"{inventoryItem.ItemData.Id}_BagView";

        if (viewObject.TryGetComponent(out BaseInteractableObject interactableObject))
        {
            interactableObject.InitFromSpawner(inventoryItem.ItemData.Id);
            interactableObject.enabled = false;
        }

        InitializeViewItemPhysics(viewObject);

        BagInventoryViewItem viewItem = viewObject.GetComponent<BagInventoryViewItem>();
        if (viewItem == null)
            viewItem = viewObject.AddComponent<BagInventoryViewItem>();

        viewItem.Initialize(inventoryItem);
        _viewItemObjects.Add(inventoryItem, viewObject);
    }

    private void RemoveMissingViewItems(IReadOnlyList<InventoryItem> bagItems)
    {
        List<InventoryItem> removedItems = null;
        foreach (InventoryItem inventoryItem in _viewItemObjects.Keys)
        {
            if (ContainsInventoryItem(bagItems, inventoryItem))
                continue;

            removedItems ??= new List<InventoryItem>();
            removedItems.Add(inventoryItem);
        }

        if (removedItems == null)
            return;

        for (int i = 0; i < removedItems.Count; i++)
        {
            RemoveViewItem(removedItems[i]);
        }
    }

    private bool ContainsInventoryItem(IReadOnlyList<InventoryItem> bagItems, InventoryItem inventoryItem)
    {
        for (int i = 0; i < bagItems.Count; i++)
        {
            if (bagItems[i] == inventoryItem)
                return true;
        }

        return false;
    }

    private void RemoveViewItem(InventoryItem inventoryItem)
    {
        if (!_viewItemObjects.TryGetValue(inventoryItem, out GameObject viewObject))
            return;

        if (viewObject != null)
            Destroy(viewObject);

        _viewItemObjects.Remove(inventoryItem);
    }

    private Vector3 GetDropSpawnPosition()
    {
        if (_dropItemRoot == null)
            return transform.position;

        Vector3 jitter = new Vector3(
            Random.Range(-_dropPositionJitter, _dropPositionJitter),
            0f,
            Random.Range(-_dropPositionJitter, _dropPositionJitter)
        );

        return _dropItemRoot.position + jitter;
    }

    private void InitializeViewItemPhysics(GameObject viewObject)
    {
        Rigidbody rigidbody = viewObject.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            Debug.LogError($"{viewObject.name}에 Rigidbody가 없어 3D 가방 뷰에서 물리 드롭을 적용할 수 없습니다.");
            return;
        }

        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    private void UpdateHover()
    {
        BagInventoryViewItem detectedViewItem = DetectViewItem();
        if (detectedViewItem == _hoveredViewItem)
            return;

        _hoveredViewItem = detectedViewItem;

        if (_hoveredViewItem == null)
        {
            ClearHover();
            return;
        }

        ShowHoverPopup(_hoveredViewItem);
    }

    private BagInventoryViewItem DetectViewItem()
    {
        if (_viewCamera == null)
            return null;

        Ray ray = _viewCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, _hoverDetectDistance, _viewItemLayerMask, QueryTriggerInteraction.Ignore);
        BagInventoryViewItem closestViewItem = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            PopupInfoTarget popupInfoTarget = hit.collider.GetComponentInParent<PopupInfoTarget>();
            if (popupInfoTarget == null)
                continue;

            BagInventoryViewItem viewItem = popupInfoTarget.GetComponentInParent<BagInventoryViewItem>();
            if (viewItem != null && !_viewItemObjects.ContainsValue(viewItem.gameObject))
                continue;

            if (viewItem == null || hit.distance >= closestDistance)
                continue;

            closestViewItem = viewItem;
            closestDistance = hit.distance;
        }

        return closestViewItem;
    }

    private void ShowHoverPopup(BagInventoryViewItem viewItem)
    {
        if (GameManager.UI == null || viewItem?.InventoryItem?.ItemData == null)
            return;

        SimplePopupUI simplePopupUI = GameManager.UI.OpenSimplePopupUI();
        if (simplePopupUI == null)
            return;

        PopupDisplayData displayData = new PopupDisplayData
        {
            PopupType = PopupType.Simple,
            Title = viewItem.InventoryItem.ItemData.Name,
            Prompt = DROP_PROMPT_TEXT
        };

        simplePopupUI.SetInfo(displayData);
        simplePopupUI.RestartOpenAnimation();
    }

    private void ClearHover()
    {
        _hoveredViewItem = null;

        if (GameManager.UI != null)
            GameManager.UI.CloseHoverPopupUI();
    }

    private void SetPlayerHoverDetectorEnabled(bool isEnabled)
    {
        if (_playerHoverDetector == null)
            return;

        if (!isEnabled)
        {
            _wasPlayerHoverDetectorEnabled = _playerHoverDetector.enabled;
            _playerHoverDetector.enabled = false;
            if (GameManager.UI != null)
                GameManager.UI.CloseHoverPopupUI();
            return;
        }

        _playerHoverDetector.enabled = _wasPlayerHoverDetectorEnabled;
    }

    private void HandleDropInput()
    {
        if (!IsOpen || _hoveredViewItem == null || _playerInventory == null)
            return;

        InventoryItem inventoryItem = _hoveredViewItem.InventoryItem;
        if (!_playerInventory.TryDropBagItem(inventoryItem))
            return;

        ClearHover();
    }

    private void ClearViewItems()
    {
        foreach (GameObject viewObject in _viewItemObjects.Values)
        {
            if (viewObject != null)
                Destroy(viewObject);
        }

        _viewItemObjects.Clear();
    }

    private void OpenBagCursorUI()
    {
        if (GameManager.UI == null)
            return;

        GameManager.UI.CloseUI(UIType.MainHUD);
        GameManager.UI.CloseCenterPointUI();

        UIBase centerPointUI = GameManager.UI.OpenCenterPointUI();
        if (centerPointUI != null && centerPointUI.TryGetComponent(out _centerPointCursorMode))
        {
            _centerPointCursorMode.EnableCursorMode();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    private void CloseBagCursorUI()
    {
        if (GameManager.UI == null)
            return;

        if (_centerPointCursorMode != null)
        {
            _centerPointCursorMode.DisableCursorMode();
            _centerPointCursorMode = null;
        }

        GameManager.UI.CloseCenterPointUI();

        if (_previousInputMode == PlayerInputMode.Gameplay)
        {
            GameManager.UI.OpenMainHUD();
            GameManager.UI.EnterGameplayCursorMode();
        }
    }

    private void SetViewOpen(bool isOpen)
    {
        _isOpen = isOpen;

        if (_viewCamera != null)
            _viewCamera.gameObject.SetActive(isOpen);
    }
}
