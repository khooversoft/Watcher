using System.Net;
using System.Runtime.CompilerServices;

namespace WatcherSdk.Services.State
{
    public enum RunningState
    {
        Stopped,
        Running,
        Ready,
        Failed,
    }

    public static class ServiceStateTypeExtensions
    {
        public static HttpStatusCode ToHttpStatusCodeForReady(this RunningState subject) => subject switch
        {
            RunningState.Running => HttpStatusCode.ServiceUnavailable,
            RunningState.Stopped => HttpStatusCode.ServiceUnavailable,

            RunningState.Ready => HttpStatusCode.OK,

            _ => HttpStatusCode.InternalServerError,
        };

        public static HttpStatusCode ToHttpStatusCodeForRunning(this RunningState subject) => subject switch
        {
            RunningState.Stopped => HttpStatusCode.ServiceUnavailable,

            RunningState.Running => HttpStatusCode.OK,
            RunningState.Ready => HttpStatusCode.OK,

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
