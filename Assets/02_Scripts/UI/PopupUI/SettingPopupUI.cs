using System;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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

    [Header("텍스트 연결")]
    [SerializeField] private TMP_Text _controlText;


    [Header("인풋 시스템 연결")]
    [SerializeField] private PlayerInput playerInput; // 인스펙터에서 PlayerInput 컴포넌트 할당
    InputAction lookAction;

    private float _savedVolume;
    private float _savedSensitivity;
    private int _savedDisplayMode;

   
    private void OnEnable()
    {
        _savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        _savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        _savedDisplayMode = PlayerPrefs.GetInt("DisplayMode", 0);

        _volumeSlider.value = _savedVolume;
        _controlSlider.value = _savedSensitivity;
        _displayDropdown.value = _savedDisplayMode;

        _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        _controlSlider.onValueChanged.AddListener(OnSensitivityChanged);
        _displayDropdown.onValueChanged.AddListener(OnDisplayModeChanged);

        _resetButton.Init(OnClickReset);
        _saveButton.Init(OnClickSave);
        _backButton.Init(OnClickBack);


        if (playerInput != null)
        {
            lookAction = playerInput.actions.FindAction("Look");
            lookAction?.Enable(); // 액션이 확실히 활성화되어 있도록 보장
        }
        ApplySensitivity(_savedSensitivity);
    }

    private void OnDisable()
    {
        if (_volumeSlider != null) _volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        if (_controlSlider != null) _controlSlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        if (_displayDropdown != null) _displayDropdown.onValueChanged.RemoveListener(OnDisplayModeChanged);

        if (_resetButton != null) _resetButton.Init(null);
        if (_saveButton != null) _saveButton.Init(null);
        if (_backButton != null) _backButton.Init(null);
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
        ApplySensitivity(value);

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
        _controlSlider.value = 0.5f;
        _displayDropdown.value = 0;

        GameManager.Sound.SetMasterVolume(1.0f);

        ApplySensitivity(0.5f);
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

        ApplySensitivity(_savedSensitivity);

        if (_savedDisplayMode == 0) Screen.fullScreenMode = FullScreenMode.Windowed;
        else if (_savedDisplayMode == 1) Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

        GameManager.UI.ClosePopupUI(UIType.SettingPopup);
    }
        private void ApplySensitivity(float scrollValue)
    {
        if (lookAction == null) return;

        // 0 ~ 1 인 스크롤값을 0 ~ 4로 변경
        float newSensitivity = scrollValue * 4;
        InputBinding binding = lookAction.bindings[1];
        // 안전하게 해당 액션의 2번째 바인딩에 프로세서 오버라이드를 직접 적용합니다.
        binding.overrideProcessors = $"scaleVector2(x={newSensitivity},y={newSensitivity})";
        lookAction.ApplyBindingOverride(1, binding);

        _controlText.text = $"{newSensitivity:F2}";
    }
}

