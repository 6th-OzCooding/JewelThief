using TMPro;
using UnityEngine;

public class ScorePopupUI : UIBase
{
    [Header("등급 기준 설정 (최소 금액)")]
    [SerializeField] private int _aRankMin = 1000;
    [SerializeField] private int _bRankMin = 500;
    [SerializeField] private int _cRankMin = 100;

    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI _arrestText;
    [SerializeField] private TextMeshProUGUI _expensiveText;
    [SerializeField] private TextMeshProUGUI _sumText;
    [SerializeField] private TextMeshProUGUI _ratingText;

    private void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            ClosePopup();
        }
    }

    private void ClosePopup()
    {
        GameManager.UI.CloseUI(UIType.ScorePopupUI);

        GameManager.UI.EnterGameplayCursorMode();
    }

    // 경찰에게 잡협을때는 유죄로 뛰움 
    // 후추 or 후수
    public void DisplayScore(bool isCaught = false)
    {
        if (JewelPuzzleUIManager.Instance == null) return;

        int totalValue = isCaught ? 0 : JewelPuzzleUIManager.Instance.GetTotalBagPrice();
        string bestGemName = isCaught ? "없음" : JewelPuzzleUIManager.Instance.GetMostExpensiveJewelName();

        _arrestText.text = isCaught ? "절도 : 유죄" : "절도 : 무죄";

        _sumText.text = $"총액: {totalValue:N0} Gold";
        _expensiveText.text = $"최고 보석: {bestGemName}";

        _ratingText.text = $"등급: {CalculateRating(isCaught, totalValue)}";
    }

    private string CalculateRating(bool isCaught, int totalValue)
    {
        if (isCaught) return "D"; // 유죄면 무조건 D

        if (totalValue >= _aRankMin) return "A";
        if (totalValue >= _bRankMin) return "B";
        if (totalValue >= _cRankMin) return "C";
        return "D";
    }
}
