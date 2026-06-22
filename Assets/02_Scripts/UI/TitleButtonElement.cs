using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleButtonElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Button _buttonComponent;

    [Header("텍스트 색상 반전")]
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Color _normalTextColor = new Color(1f, 0.5f, 0f); // 기본 주황색
    [SerializeField] private Color _hoverTextColor = Color.black; // 마우스 올렸을 때 검정색

    private System.Action _onClickAction;

    private void Awake()
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.fillAmount = 0f;
        }

        if (_buttonText != null)
        {
            _buttonText.color = _normalTextColor;
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

        if (_buttonText != null)
        {
            _buttonText.color = _hoverTextColor;
        }
    }

    // 마우스가 버튼 밖으로 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.fillAmount = 0f;
        }

        if (_buttonText != null)
        {
            _buttonText.color = _normalTextColor;
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
