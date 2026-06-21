using TeamConvention.Interfaces;

public class PlayerInputBinder
{
    private PlayerInputHandler _inputHandler;
    private IInteractInput _player;

    public PlayerInputBinder(PlayerInputHandler inputHandler) { _inputHandler = inputHandler; }

    public void Init(IInteractInput player)
    {
        BindInput(player);
    }

    private void BindInput(IInteractInput player)
    {
        UnbindInput();

        _player = player;
        if(null == _player)
        {
            UnityEngine.Debug.LogWarning("Failed Input Bind: IInteractInput is None");
            return;
        }

        _inputHandler.OnInteractEvent += _player.TryInteract;
    }

    private void UnbindInput()
    {
        if (null == _player) return;

        _inputHandler.OnInteractEvent -= _player.TryInteract;
    }
}
