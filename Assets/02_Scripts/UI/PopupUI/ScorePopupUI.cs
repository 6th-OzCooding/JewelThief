using TMPro;
using UnityEngine;

public class ScorePopupUI : UIBase
{
    [Header("등급 기준 설정 (최소 점수)")]
    [SerializeField] private int _aRankMin = 1000;
    [SerializeField] private int _bRankMin = 500;
    [SerializeField] private int _cRankMin = 100;

    [Header("시간 보너스 점수 설정")]
    [SerializeField] private int _bonus5Min = 50; 
    [SerializeField] private int _bonus3Min = 30; 
    [SerializeField] private int _bonus1Min = 10;

    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI _arrestText;
    [SerializeField] private TextMeshProUGUI _expensiveText;
    [SerializeField] private TextMeshProUGUI _sumText;
    [SerializeField] private TextMeshProUGUI _ratingText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _timeBonusText;

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
    public void DisplayScore(int totalValue, string bestGemName, float remainingTime, bool isCaught = false)
    {
        _arrestText.text = isCaught ? "임무 : 실패" : "임무 : 성공";
        _sumText.text = $"총액: {totalValue:N0} Gold";
        _expensiveText.text = $"최고 보석: {bestGemName}";
        _ratingText.text = $"등급: {CalculateRating(isCaught, totalValue)}";

        if (remainingTime < 0) remainingTime = 0f;
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        _timeText.text = $"남은 시간: {minutes:D2}:{seconds:D2}";

        int timeBonus = CalculateTimeBonus(remainingTime);
        if (_timeBonusText != null)
        {
            _timeBonusText.text = $"시간 보너스: +{timeBonus} 점";
        }

        _ratingText.text = $"등급: {CalculateRating(isCaught, totalValue + timeBonus)}";
    }

    private int CalculateTimeBonus(float remainingTime)
    {
        if (remainingTime >= 300f) return _bonus5Min;
        if (remainingTime >= 180f) return _bonus3Min; 
        if (remainingTime >= 60f) return _bonus1Min;
        return 0;
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
