using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum BoxState
{
    Idle,
    Open
}

public class InteractableContainerAnimeController : MonoBehaviour
{
    [Header("애니메이터")]
    [SerializeField] private Animator _animatorBox;

    private BoxState _currentStat;

    public void InitMeshAnime(GameObject meshObject)
    {
        if (meshObject == null) return;

        _animatorBox = meshObject.GetComponent<Animator>();
    }

    public void SetStat(BoxState newStat)
    {
        if (newStat == BoxState.Idle)
        {
            return;
        }

        _currentStat = newStat;

        switch(_currentStat)
        {
            case BoxState.Open:
                _animatorBox.SetBool("isOpend", true);
                break;
        }
    }
}
