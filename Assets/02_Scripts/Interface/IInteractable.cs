using UnityEngine;

namespace TeamConvention.Interfaces
{
    public interface IInteractable
    {
        string InteractPrompt { get; }
        bool CanInteract();
        void Interact(IInteractor interactor);
    }

    public interface IInteractor
    {
        public Vector3 Position { get; }
    }
}