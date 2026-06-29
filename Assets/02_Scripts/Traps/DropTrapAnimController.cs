using UnityEngine;
public enum TrapAnimState
{
    Idle,
    Trapped,
    Broken
}
public class DropTrapAnimController : MonoBehaviour
{
    [SerializeField] private Animator _animatorTrap;
    private TrapAnimState _currentState;
    public void SetState(TrapAnimState newState)
    {
        if (newState == _currentState)
            return;

        if (_animatorTrap == null)
            return;

        _currentState = newState;

        switch (_currentState)
        {
            case TrapAnimState.Idle:
                ResetState();
                break;
            case TrapAnimState.Trapped:
                _animatorTrap.SetBool("IsTrapped", true);
                break;
            case TrapAnimState.Broken:
                _animatorTrap.SetBool("IsBroken", true);
                break;
        }
    }
    private void ResetState() 
    {
        _animatorTrap.SetBool("IsTrapped", false);
        _animatorTrap.SetBool("IsBroken", false);
    }
}
