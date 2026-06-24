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

    /// <summary>
    /// Quick slot HUD owned by this MainHUD.
    /// </summary>
    public QuickSlotHUD QuickSlotHUD => _quickSlotHUD;

    private void OnEnable()
    {
        _timerHUD?.ResetTimer();
        RefreshHUD();
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
        _playerStatusHUD?.SetPlayerController(playerController);
        _playerStatusHUD?.Refresh();
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

    /// <summary>
    /// Displays remaining time in mm:ss format.
    /// </summary>
    public void SetTimer(float remainingSeconds)
    {
        _timerHUD?.SetTimer(remainingSeconds);
    }

    private void RefreshHUD()
    {
        _playerStatusHUD?.Refresh();
        _timerHUD?.Refresh();
    }
}
