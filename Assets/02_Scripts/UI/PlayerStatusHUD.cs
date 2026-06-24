using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays player HP and stamina status bars.
/// </summary>
public class PlayerStatusHUD : MonoBehaviour
{
    [Header("Player Source")]
    [SerializeField] private PlayerController _playerController;

    [Header("Status Bars")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Slider _staminaSlider;

    [Header("Status Warnings")]
    [SerializeField] private GameObject[] _statusWarningImages;
    [SerializeField] private Image _playerWarningImage;

    [Header("Warning Effect")]
    [SerializeField] private float _playerWarningFadeDuration = 0.25f;
    [SerializeField] private float _playerWarningInterval = 0.05f;
    [SerializeField, Range(0f, 1f)] private float _playerWarningMaxAlpha = 1f;

    private int _activeStatusWarningIndex = -1;
    private Coroutine _playerWarningRoutine;

    private void OnEnable()
    {
        HideStatusWarnings();
    }

    private void OnDisable()
    {
        StopPlayerWarningEffect();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
            ShowNextTestStatusWarning();
    }

    /// <summary>
    /// Sets the player displayed by this status HUD.
    /// </summary>
    public void SetPlayerController(PlayerController playerController)
    {
        _playerController = playerController;
        Refresh();
    }

    /// <summary>
    /// Displays the current HP ratio on the HP slider.
    /// </summary>
    public void SetHp(float currentHp, float maxHp)
    {
        SetSliderValue(_hpSlider, currentHp, maxHp);
    }

    /// <summary>
    /// Displays the current stamina ratio on the stamina slider.
    /// </summary>
    public void SetStamina(float currentStamina, float maxStamina)
    {
        SetSliderValue(_staminaSlider, currentStamina, maxStamina);
    }

    /// <summary>
    /// Displays only one status warning image by index.
    /// </summary>
    public void ShowStatusWarning(int warningIndex)
    {
        if (_statusWarningImages == null) return;

        if (warningIndex < 0 || warningIndex >= _statusWarningImages.Length)
        {
            HideStatusWarnings();
            return;
        }

        _activeStatusWarningIndex = warningIndex;

        for (int i = 0; i < _statusWarningImages.Length; i++)
        {
            if (_statusWarningImages[i] == null) continue;

            _statusWarningImages[i].SetActive(i == _activeStatusWarningIndex);
        }

        StartPlayerWarningEffect();
    }

    /// <summary>
    /// Hides every status warning image.
    /// </summary>
    public void HideStatusWarnings()
    {
        _activeStatusWarningIndex = -1;

        if (_statusWarningImages == null) return;

        foreach (GameObject statusWarningImage in _statusWarningImages)
        {
            if (statusWarningImage == null) continue;

            statusWarningImage.SetActive(false);
        }

        StopPlayerWarningEffect();
    }

    /// <summary>
    /// Refreshes status bars from the currently assigned player.
    /// </summary>
    public void Refresh()
    {
        if (_playerController == null)
        {
            _playerController = Object.FindFirstObjectByType<PlayerController>();
        }

        if (_playerController == null) return;

        SetStamina(_playerController.CurrentStamina, _playerController.MaxStamina);
    }

    private void SetSliderValue(Slider targetSlider, float currentValue, float maxValue)
    {
        if (targetSlider == null) return;

        targetSlider.value = maxValue <= 0f ? 0f : Mathf.Clamp01(currentValue / maxValue);
    }

    private void ShowNextTestStatusWarning()
    {
        if (_statusWarningImages == null || _statusWarningImages.Length == 0) return;

        int nextWarningIndex = _activeStatusWarningIndex + 1;
        if (nextWarningIndex >= _statusWarningImages.Length)
        {
            HideStatusWarnings();
            return;
        }

        ShowStatusWarning(nextWarningIndex);
    }

    private void StartPlayerWarningEffect()
    {
        if (_playerWarningImage == null) return;
        if (_playerWarningRoutine != null) return;

        _playerWarningRoutine = StartCoroutine(PlayPlayerWarningEffect());
    }

    private void StopPlayerWarningEffect()
    {
        if (_playerWarningRoutine != null)
        {
            StopCoroutine(_playerWarningRoutine);
            _playerWarningRoutine = null;
        }

        SetPlayerWarningAlpha(0f);
    }

    private IEnumerator PlayPlayerWarningEffect()
    {
        while (true)
        {
            yield return FadePlayerWarningAlpha(0f, _playerWarningMaxAlpha);
            yield return new WaitForSeconds(_playerWarningInterval);
            yield return FadePlayerWarningAlpha(_playerWarningMaxAlpha, 0f);
            yield return new WaitForSeconds(_playerWarningInterval);
        }
    }

    private IEnumerator FadePlayerWarningAlpha(float fromAlpha, float toAlpha)
    {
        float duration = Mathf.Max(0.01f, _playerWarningFadeDuration);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            SetPlayerWarningAlpha(Mathf.Lerp(fromAlpha, toAlpha, progress));
            yield return null;
        }

        SetPlayerWarningAlpha(toAlpha);
    }

    private void SetPlayerWarningAlpha(float alpha)
    {
        if (_playerWarningImage == null) return;

        Color color = _playerWarningImage.color;
        color.a = Mathf.Clamp01(alpha);
        _playerWarningImage.color = color;
    }
}
