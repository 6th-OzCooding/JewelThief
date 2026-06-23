using TMPro;
using UnityEngine;

/// <summary>
/// 이름과 상호작용 프롬프트만 표시하는 간단한 Hover 팝업 UI입니다.
/// </summary>
public class SimplePopupUI : HoverPopupUIBase
{
    [Header("Text Assignment")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _promptText;

    protected override void Awake()
    {
        base.Awake();
        CacheTextComponents();
    }

    /// <summary>
    /// 간단한 Hover 팝업 내용을 갱신합니다.
    /// </summary>
    public void SetInfo(PopupDisplayData displayData)
    {
        if (displayData == null)
            return;

        CacheTextComponents();
        SetText(_nameText, displayData.Title);
        SetText(_promptText, displayData.Prompt);
    }

    private void CacheTextComponents()
    {
        _nameText ??= FindTextByName("Text_Name");
        _promptText ??= FindTextByName("Text_InteractionKeyInfo");
    }
}
