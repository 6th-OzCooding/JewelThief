using UnityEngine;

namespace TeamConvention.Interfaces
{
    public interface IInteractable
    {
        string GetId { get; }
        string GetName { get; }
        bool CanInteract();
        void Interact(IInteractor interactor);
    }

    public interface IInteractor
    {
        public Vector3 Position { get; }
    }
}