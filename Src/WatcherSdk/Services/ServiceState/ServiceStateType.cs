using System.Net;
using System.Runtime.CompilerServices;

namespace WatcherSdk.Services.ServiceState
{
    public enum ServiceStateType
    {
        Stopped,
        Running,
        Ready,
        Failed,
    }

    public static class ServiceStateTypeExtensions
    {
        public static HttpStatusCode ToHttpStatusCodeForReady(this ServiceStateType subject) => subject switch
        {
            ServiceStateType.Running => HttpStatusCode.ServiceUnavailable,
            ServiceStateType.Stopped => HttpStatusCode.ServiceUnavailable,

            ServiceStateType.Ready => HttpStatusCode.OK,

            _ => HttpStatusCode.InternalServerError,
        };

        public static HttpStatusCode ToHttpStatusCodeForRunning(this ServiceStateType subject) => subject switch
        {
            ServiceStateType.Stopped => HttpStatusCode.ServiceUnavailable,

            ServiceStateType.Running => HttpStatusCode.OK,
            ServiceStateType.Ready => HttpStatusCode.OK,

            _ => HttpStatusCode.InternalServerError,
        };

        public static bool TestReady(this HttpStatusCode subject) => subject switch
        {
            HttpStatusCode.OK => true,

            _ => false,
        };

        public static bool TestRunning(this HttpStatusCode subject) => subject switch
        {
            HttpStatusCode.OK => true,

            _ => false,
        };
    }
}
