using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleButtonElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Button _buttonComponent;

    private System.Action _onClickAction;

    private void Awake()
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.fillAmount = 0f;
        }
    }

    public void Init(System.Action onClickAction)
    {
        _onClickAction = onClickAction;
    }

    // 마우스가 버튼 위에 올라왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.fillAmount = 1f;
        }
    }

    // 마우스가 버튼 밖으로 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.fillAmount = 0f;
        }
    }

    // 버튼을 클릭했을 때
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_onClickAction != null)
        {
            _onClickAction.Invoke();
        }
    }
}
