using TeamConvention.Interfaces;
using UnityEngine;

public class StageSelectChair : MonoBehaviour, IInteractable
{
    [SerializeField] private string _name = "스테이지 선택";
    [SerializeField] private StageSelectController _stageSelectController;

    public string GetId => _name;

    public string GetName => _name;

    public bool CanInteract() => true;

    public void Interact(IInteractor interactor)
    {
        if (_stageSelectController == null)
        {
            Debug.LogError("StageSelectController가 연결되지 않았습니다.");
            return;
        }

        _stageSelectController.EnterStageSelect();
    }
}