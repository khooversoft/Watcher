namespace WatcherSdk.Services.ServiceState
{
    public interface IStateService
    {
        ServiceStateType ServiceState { get; }

        void SetState(ServiceStateType stateType);
    }
}