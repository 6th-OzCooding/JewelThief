public interface IInteractable
{
    string InteractPrompt { get; }

    bool CanInteract { get; }

    void TryInteract(IInteractor interactor);
}

public interface IInteractor
{
    UnityEngine.Transform Transform { get; }
}