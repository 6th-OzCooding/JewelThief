using UnityEngine;

public enum InteractableObjectAnimState
{
    Idle,
    Open
}

public class InteractableContainerAnimeController : MonoBehaviour
{
    private Animator _animatorInteractableObject;
    private InteractableObjectAnimState _currentStat;

    public bool HasAnimator => _animatorInteractableObject != null;

    public void InitMeshAnime(GameObject meshObject)
    {
        _animatorInteractableObject = null;

        if (meshObject == null)
            return;

        _animatorInteractableObject = meshObject.GetComponentInChildren<Animator>(true);

        if (_animatorInteractableObject == null)
        {
            Debug.LogWarning($"{meshObject.name}에는 Animator가 없습니다. 애니메이션 없이 처리합니다.");
        }
    }

    public void SetStat(InteractableObjectAnimState newStat)
    {
        if (newStat == InteractableObjectAnimState.Idle)
            return;

        if (_animatorInteractableObject == null)
            return;

        _currentStat = newStat;

        switch (_currentStat)
        {
            case InteractableObjectAnimState.Open:
                _animatorInteractableObject.SetBool("isOpend", true);
                break;
        }
    }
}