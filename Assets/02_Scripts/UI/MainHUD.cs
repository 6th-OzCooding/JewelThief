using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Root controller that groups the gameplay HUD parts.
/// </summary>
public class MainHUD : UIBase
{
    [Header("HUD Parts")]
    [SerializeField] private PlayerStatusHUD _playerStatusHUD;
    [SerializeField] private TimerHUD _timerHUD;
    [SerializeField] private QuickSlotHUD _quickSlotHUD;

    private PlayerInventory _playerInventory;

    /// <summary>
    /// Quick slot HUD owned by this MainHUD.
    /// </summary>
    public QuickSlotHUD QuickSlotHUD => _quickSlotHUD;

    private void OnEnable()
    {
        SubscribeQuickSlotHUD();
        SubscribeToolInventory();
        RefreshQuickSlotHUD();
        RefreshHUD();
    }

    private void OnDisable()
    {
        UnsubscribeQuickSlotHUD();
        UnsubscribeToolInventory();
    }

    private void Update()
    {
        RefreshHUD();
    }

    /// <summary>
    /// Sets the player displayed by this HUD.
    /// </summary>
    public void SetPlayerController(PlayerController playerController)
    {
        UnsubscribeToolInventory();
        // HUD는 플레이어를 직접 찾지 않고, 생성 흐름에서 전달받은 PlayerController의 인벤토리를 구독합니다.
        _playerInventory = playerController != null ? playerController.Inventory : null;
        SubscribeToolInventory();

        _playerStatusHUD?.SetPlayerController(playerController);
        _playerStatusHUD?.Refresh();
        RefreshQuickSlotHUD();
    }

    /// <summary>
    /// Displays the current HP ratio on the HP slider.
    /// </summary>
    public void SetHp(float currentHp, float maxHp)
    {
        _playerStatusHUD?.SetHp(currentHp, maxHp);
    }

    /// <summary>
    /// Displays the current stamina ratio on the stamina slider.
    /// </summary>
    public void SetStamina(float currentStamina, float maxStamina)
    {
        _playerStatusHUD?.SetStamina(currentStamina, maxStamina);
    }

    private void RefreshHUD()
    {
        _playerStatusHUD?.Refresh();
        _timerHUD?.Refresh();
    }

    private void SubscribeToolInventory()
    {
        if (_playerInventory == null)
            return;

        _playerInventory.OnToolItemsChanged -= HandleToolItemsChanged;
        _playerInventory.OnToolItemsChanged += HandleToolItemsChanged;
    }

    private void UnsubscribeToolInventory()
    {
        if (_playerInventory == null)
            return;

        _playerInventory.OnToolItemsChanged -= HandleToolItemsChanged;
    }

    private void HandleToolItemsChanged(IReadOnlyList<InventoryItem> toolItems)
    {
        // Tool 인벤토리 변경을 HUD 표시로 반영합니다. 실제 장착 처리는 슬롯 선택 시 PlayerInventory가 담당합니다.
        _quickSlotHUD?.RefreshToolSlots(toolItems);
    }

    private void RefreshQuickSlotHUD()
    {
        _quickSlotHUD?.RefreshToolSlots(_playerInventory?.ToolItems);
    }

    private void SubscribeQuickSlotHUD()
    {
        if (_quickSlotHUD == null)
            return;

        _quickSlotHUD.OnSlotSelected -= HandleQuickSlotSelected;
        _quickSlotHUD.OnSlotSelected += HandleQuickSlotSelected;
    }

    private void UnsubscribeQuickSlotHUD()
    {
        if (_quickSlotHUD == null)
            return;

        _quickSlotHUD.OnSlotSelected -= HandleQuickSlotSelected;
    }

    private void HandleQuickSlotSelected(int slotIndex)
    {
        if (_playerInventory == null)
            return;

        _playerInventory.TryEquipQuickSlotTool(slotIndex);
    }
}