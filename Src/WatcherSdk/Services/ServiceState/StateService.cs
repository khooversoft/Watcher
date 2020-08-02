namespace WatcherSdk.Services.ServiceState
{
    public class StateService : IStateService
    {
        public ServiceStateType ServiceState { get; private set; }

        public void SetState(ServiceStateType stateType) => ServiceState = stateType;
    }
}
