using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class StageButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;

    private Button _button;
    private StageData _stageData;
    private StageSelectUI _ui;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClickButton);
    }

    public void Init(StageData stageData, StageSelectUI ui)
    {
        _stageData = stageData;
        _ui = ui;

        if (_nameText != null)
        {
            _nameText.text = _stageData.Name;
        }

        gameObject.SetActive(true);
    }

    private void OnClickButton()
    {
        if (_stageData != null && _ui != null)
        {
            _ui.OnStageButtonClicked(_stageData);
        }
    }
}