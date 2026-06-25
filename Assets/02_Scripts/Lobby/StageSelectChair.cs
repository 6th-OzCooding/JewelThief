using TeamConvention.Interfaces;
using UnityEngine;

public class StageSelectChair : MonoBehaviour, IInteractable
{
    private const string POPUP_DATA_ID = "StageSelectChair";

    [SerializeField] private string _name = "스테이지 선택";
    [SerializeField] private StageSelectController _stageSelectController;

    public string GetId => POPUP_DATA_ID;

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
