using TeamConvention.Interfaces;
using UnityEngine;

public class InteractableDoor : BaseInteractableObject
{
    private bool _isOpen;

    protected override bool CheckCanInteract()
    {
        return !_isOpen;
    }

    protected override void OnInteract(IInteractor interactor)
    {
        _isOpen = true;

        // 문 열 때 수행하는 로직을 넣으세요
    }

    protected override void LoadData(string id)
    {
        // data = GameManager.DataTalbe.GetDoorData(id); 문 관련 데이터 테이블 어딘지 모르겠네요.
    }
}
