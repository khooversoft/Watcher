using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Toolbox.Extensions;
using Toolbox.Tools;
using WatcherSdk.Probe;
using WatcherSdk.Records;
using WatcherSdk.Repository;

namespace WatcherSdk.Agent
{
    public class AgentHostBuilder
    {
        public AgentHostBuilder()
        {
        }

        public string? AgentId { get; set; }

        public IList<TargetRecord> Targets { get; set; } = new List<TargetRecord>();

        public IRecordContainer<TraceRecord>? TraceContainer { get; set; }

        public IProbeFactory? ProbeFactory { get; set; }

        public ILoggerFactory? LoggerFactory { get; set; }

        public AgentHostBuilder SetAgentId(string agentId)
        {
            AgentId = agentId;
            return this;
        }

        public AgentHostBuilder AddTarget(params TargetRecord[] targetRecords)
        {
            Targets.ForEach(x => Targets.Add(x));
            return this;
        }

        public AgentHostBuilder SetTraceContainer(IRecordContainer<TraceRecord> traceRecord)
        {
            TraceContainer = traceRecord;
            return this;
        }

        public AgentHostBuilder SetProbeFactory(IProbeFactory probeFactory)
        {
            ProbeFactory = probeFactory;
            return this;
        }
        public AgentHostBuilder SetLoggerFactory(ILoggerFactory loggingFactory)
        {
            LoggerFactory = loggingFactory;
            return this;
        }

        public IAgentHost Build()
        {
            AgentId.VerifyNotNull(nameof(AgentId));
            ProbeFactory.VerifyNotNull(nameof(ProbeFactory));
            TraceContainer.VerifyNotNull(nameof(TraceContainer));
            LoggerFactory.VerifyNotNull(nameof(LoggerFactory));

            var monitoringJobs = Targets
                .VerifyNotNull(nameof(Targets))
                .VerifyAssert(x => x.Count > 0, "No targets")
                .Select(x => (Target: x, Probe: ProbeFactory.Create(x, AgentId, TraceContainer)))
                .Select(x => new MonitoringJob(x.Target, x.Probe, LoggerFactory.CreateLogger<MonitoringJob>()))
                .ToArray();

            return new AgentHost(monitoringJobs, LoggerFactory.CreateLogger<AgentHost>());
        }
    }
}
