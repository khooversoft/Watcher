namespace WatcherSdk.Services.State
{
    public class RunningStateService : IRunningStateService
    {
        public RunningState ServiceState { get; private set; }

        public void SetState(RunningState stateType) => ServiceState = stateType;
    }
}
