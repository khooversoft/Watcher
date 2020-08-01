using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestRestApiServer.Services
{
    public class StateService : IStateService
    {
        public ServiceStateType ServiceState { get; private set; }

        public void SetState(ServiceStateType stateType) => ServiceState = stateType;
    }
}
