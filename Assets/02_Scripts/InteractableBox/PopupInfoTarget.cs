using UnityEngine;

/// <summary>
/// 상호작용 오브젝트가 어떤 Hover 팝업 규칙을 사용할지 지정합니다.
/// </summary>
public class PopupInfoTarget : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private PopupTargetType _targetType = PopupTargetType.None;

    /// <summary>
    /// 이 오브젝트가 사용할 팝업 대상 타입입니다.
    /// </summary>
    public PopupTargetType TargetType => _targetType;

    /// <summary>
    /// PopupViewData.json에서 사용할 규칙 ID를 반환합니다.
    /// </summary>
    public string GetPopupViewDataId()
    {
        if (_targetType == PopupTargetType.None)
            return string.Empty;

        return $"TargetType_{_targetType}";
    }
}
