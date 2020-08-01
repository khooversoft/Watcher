using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestRestApiServer.Services
{
    public enum ServiceStateType
    {
        Stopped,
        Running,
        Ready,
        Failed,
    }
}
