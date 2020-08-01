namespace TestRestApiServer.Services
{
    public interface IStateService
    {
        ServiceStateType ServiceState { get; }

        void SetState(ServiceStateType stateType);
    }
}