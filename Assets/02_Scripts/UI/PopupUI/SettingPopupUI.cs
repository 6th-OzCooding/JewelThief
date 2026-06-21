using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingPopupUI : UIBase
{
    [Header("UI 컴포넌트 연결")]
    [SerializeField] private Slider _volumeSlider; 
    [SerializeField] private Slider _controlSlider;
    [SerializeField] private TMP_Dropdown _displayDropdown; 

    [Header("버튼 연결 (BottomRight)")]
    [SerializeField] private TitleButtonElement _resetButton;
    [SerializeField] private TitleButtonElement _saveButton; 
    [SerializeField] private TitleButtonElement _backButton;

    private float _savedVolume;
    private float _savedSensitivity;
    private int _savedDisplayMode;

    private void OnEnable()
    {
        _savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        _savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 50.0f);
        _savedDisplayMode = PlayerPrefs.GetInt("DisplayMode", 0);

        _volumeSlider.value = _savedVolume;
        _controlSlider.value = _savedSensitivity;
        _displayDropdown.value = _savedDisplayMode;
    }

    private void Start()
    {
        _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        _controlSlider.onValueChanged.AddListener(OnSensitivityChanged);
        _displayDropdown.onValueChanged.AddListener(OnDisplayModeChanged);

        _resetButton.Init(OnClickReset);
        _saveButton.Init(OnClickSave);
        _backButton.Init(OnClickBack);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickBack();
        }
    }

    private void OnVolumeChanged(float value)
    {
        GameManager.Sound.SetMasterVolume(value);
    }

    private void OnSensitivityChanged(float value)
    {
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetMouseSensitivity(value);
        }
    }

    private void OnDisplayModeChanged(int index)
    {
        // 창모드
        if (index == 0) Screen.fullScreenMode = FullScreenMode.Windowed;
        // 전체화면
        else if (index == 1) Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }

    private void OnClickReset()
    {
        _volumeSlider.value = 1.0f;
        _controlSlider.value = 50.0f;
        _displayDropdown.value = 0;

        GameManager.Sound.SetMasterVolume(1.0f);

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMouseSensitivity(50.0f);
    }

    private void OnClickSave()
    {
        PlayerPrefs.SetFloat("MasterVolume", _volumeSlider.value);
        PlayerPrefs.SetFloat("MouseSensitivity", _controlSlider.value);
        PlayerPrefs.SetInt("DisplayMode", _displayDropdown.value);
        PlayerPrefs.Save();

        _savedVolume = _volumeSlider.value;
        _savedSensitivity = _controlSlider.value;
        _savedDisplayMode = _displayDropdown.value;

        Debug.Log("환경설정 저장 완료!");
    }

    private void OnClickBack()
    {
        GameManager.Sound.SetMasterVolume(_savedVolume);

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null) player.SetMouseSensitivity(_savedSensitivity);

        if (_savedDisplayMode == 0) Screen.fullScreenMode = FullScreenMode.Windowed;
        else if (_savedDisplayMode == 1) Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

        UIManager.Instance.ClosePopupUI(UIType.SettingPopup);
    }
}
