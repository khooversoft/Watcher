using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Toolbox.Tools;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace WatcherSdk.Probe
{
    public class ProbeFactory : IProbeFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerFactory _loggerFactory;

        public ProbeFactory(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            httpClientFactory.VerifyNotNull(nameof(httpClientFactory));
            loggerFactory.VerifyNotNull(nameof(loggerFactory));

            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;
        }

        public IProbe Create(TargetRecord targetRecord, string agentId, IRecordContainer<TraceRecord>? traceContainer)
        {
            targetRecord.VerifyNotNull(nameof(targetRecord));
            agentId.VerifyNotEmpty(nameof(agentId));
            traceContainer.VerifyNotNull(nameof(traceContainer));

            if (!Enum.TryParse(typeof(ProbeType), targetRecord.TargetType, true, out object probeTypeObj))
            {
                throw new InvalidExpressionException($"Unknown target type {targetRecord.TargetType}");
            }

            return (ProbeType)probeTypeObj switch
            {
                ProbeType.REST => new RestProbe(agentId!, targetRecord, _httpClientFactory.CreateClient(), traceContainer, _loggerFactory.CreateLogger<RestProbe>()),

                _ => throw new InvalidOperationException($"Unsupported target type, probeType={probeTypeObj}"),
            };
        }
    }
}
