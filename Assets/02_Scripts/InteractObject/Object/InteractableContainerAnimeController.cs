using UnityEngine;

public enum BoxState
{
    Idle,
    Open
}

public class InteractableContainerAnimeController : MonoBehaviour
{
    private Animator _animatorBox;
    private BoxState _currentStat;

    public bool HasAnimator => _animatorBox != null;

    public void InitMeshAnime(GameObject meshObject)
    {
        _animatorBox = null;

        if (meshObject == null)
            return;

        _animatorBox = meshObject.GetComponentInChildren<Animator>(true);

        if (_animatorBox == null)
        {
            Debug.LogWarning($"{meshObject.name}에는 Animator가 없습니다. 애니메이션 없이 처리합니다.");
        }
    }

    public void SetStat(BoxState newStat)
    {
        if (newStat == BoxState.Idle)
            return;

        if (_animatorBox == null)
            return;

        _currentStat = newStat;

        switch (_currentStat)
        {
            case BoxState.Open:
                _animatorBox.SetBool("isOpend", true);
                break;
        }
    }
}