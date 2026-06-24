using TeamConvention.Interfaces;
using UnityEngine;

/// <summary>
/// 하이어라키에 직접 배치한 오브젝트의 Hover 팝업을 확인하기 위한 테스트용 상호작용 컴포넌트입니다.
/// </summary>
public class TestPopupInteractable : MonoBehaviour, IInteractable
{
    [Header("Test Data")]
    [SerializeField] private string _id;
    [SerializeField] private string _name;

    /// <summary>
    /// 팝업과 상호작용 테스트에 사용할 데이터 ID입니다.
    /// </summary>
    public string GetId => _id;

    /// <summary>
    /// 원본 데이터 조회 실패 시 대체로 사용할 표시 이름입니다.
    /// </summary>
    public string GetName => _name;

    /// <summary>
    /// 테스트 대상은 항상 상호작용 가능한 것으로 처리합니다.
    /// </summary>
    public bool CanInteract()
    {
        return true;
    }

    /// <summary>
    /// 테스트용 상호작용 로그만 출력합니다.
    /// </summary>
    public void Interact(IInteractor interactor)
    {
        Debug.Log($"테스트 상호작용 실행. Id: {_id}, Name: {_name}");
    }
}
