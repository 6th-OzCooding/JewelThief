using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmPopupUI : UIBase
{
    [Header("컴포넌트 연결")]
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    private Action _onConfirmAction;

    private void Start()
    {
        _yesButton.onClick.AddListener(OnClickYes);
        _noButton.onClick.AddListener(OnClickNo);
    }

    public void SetUI(string message, Action onConfirm)
    {
        _messageText.text = message;
        _onConfirmAction = onConfirm;
    }

    private void OnClickYes()
    {
        if (_onConfirmAction != null)
        {
            _onConfirmAction.Invoke();
        }

        GameManager.UI.ClosePopupUI(UIType.ConfirmPopup);
    }

    private void OnClickNo()
    {
        GameManager.UI.ClosePopupUI(UIType.ConfirmPopup);
    }
}
