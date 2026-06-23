namespace TeamConvention.Interfaces
{
    public interface IDisarmable
    {
        
        bool IsDisarmed { get; }
        bool CanDisarm();
        void Disarm(IInteractor interactor);
    }
}

