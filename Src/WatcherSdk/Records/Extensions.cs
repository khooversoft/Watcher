using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using WatcherSdk.Repository;

namespace WatcherSdk.Records
{
    public static class Extensions
    {
        public static TraceRecord CreateOkTrace(this TargetRecord targetRecord, string agentId, HttpStatusCode httpStatusCode, string? body, long ms)
        {
            return new TraceRecord
            {
                TargetId = targetRecord.Id,
                AgentId = agentId,
                Url = targetRecord.Url,
                HttpStatusCode = httpStatusCode,
                Body = body,
                TargetState = Models.TargetState.Ok,
                ProbeMs = ms,
            };
        }

        public static TraceRecord CreateErrorTrace(this TargetRecord targetRecord, string agentId, HttpStatusCode httpStatusCode, string? body, string? ex)
        {
            return new TraceRecord
            {
                TargetId = targetRecord.Id,
                AgentId = agentId,
                Url = targetRecord.Url,
                HttpStatusCode = httpStatusCode,
                Body = body,
                TargetState = Models.TargetState.Error,
                Exception = ex,
            };
        }
    }
}
