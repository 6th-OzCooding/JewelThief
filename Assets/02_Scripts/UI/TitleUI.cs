using UnityEngine;
using Cysharp.Threading.Tasks;

public class TitleUI : UIBase
{
    [Header("버튼 컴포넌트 연결")]
    [SerializeField] private TitleButtonElement _newGameButton;
    [SerializeField] private TitleButtonElement _settingButton;
    [SerializeField] private TitleButtonElement _creditButton;
    [SerializeField] private TitleButtonElement _exitButton;

    private void Start()
    {
        _newGameButton.Init(OnClickNewGame);
        _settingButton.Init(OnClickSetting);
        _creditButton.Init(OnClickCredit);
        _exitButton.Init(OnClickExit);
    }

    private void OnClickNewGame()
    {
        Debug.Log("새 게임 시작!");
        PlayerController playerController = GameManager.Instance.EnterLobby(true);
        GameManager.UI.ShowStartupUIOnGameStart(playerController);

        // 후추 : 인게임 진입 로직
    }

    private void OnClickSetting()
    {
        Debug.Log("환경설정 창 열기");

        GameManager.UI.OpenPopupUI(UIType.SettingPopup);
    }

    private void OnClickCredit()
    {
        GameManager.UI.OpenPopupUI(UIType.CreditPopup);
    }

    private void OnClickExit()
    {
        UIBase popupBase = GameManager.UI.OpenPopupUI(UIType.ConfirmPopup);

        if (popupBase != null && popupBase.TryGetComponent(out ConfirmPopupUI confirmUI))
        {
            confirmUI.SetUI("정말 게임을 종료하시겠습니까?", GameManager.Instance.QuitGame);
        }
    } 
}
