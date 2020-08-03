using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Rest;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using WatcherSdk.Services.State;

namespace WatcherSdk.Probe
{
    public class RestProbe : IProbe
    {
        private readonly string _agentId;
        private readonly TargetRecord _targetRecord;
        private readonly IRecordContainer<TraceRecord> _traceContainer;
        private readonly HttpClient _httpClient;
        private readonly ILogger<RestProbe> _logger;

        public RestProbe(string agentId, TargetRecord targetRecord, HttpClient httpClient, IRecordContainer<TraceRecord> traceContainer, ILogger<RestProbe> logger)
        {
            agentId.VerifyNotNull(nameof(agentId));
            targetRecord.VerifyNotNull(nameof(targetRecord));
            httpClient.VerifyNotNull(nameof(httpClient));
            traceContainer.VerifyNotNull(nameof(traceContainer));
            logger.VerifyNotNull(nameof(logger));

            _agentId = agentId;
            _targetRecord = targetRecord;
            _traceContainer = traceContainer;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<bool> Ping(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Ping)}: {_targetRecord}");

            try
            {
                if (await TestReady(token)) return false;
                if (await TestRunning(token)) return false;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Ping)}: {_targetRecord}");

                var trace = _targetRecord.CreateErrorTrace(_agentId, HttpStatusCode.InternalServerError, null, ex.ToString());
                await _traceContainer.Set(trace, token);
                return false;
            }
        }

        private Task<bool> TestReady(CancellationToken token)
        {
            return InvokeUrl(_targetRecord.ReadyUrl!, x => x.TestReady(), token);
        }

        private async Task<bool> TestRunning(CancellationToken token)
        {
            if (_targetRecord.RunningUrl.IsEmpty()) return true;

            return await InvokeUrl(_targetRecord.RunningUrl!, x => x.TestRunning(), token);
        }

        private async Task<bool> InvokeUrl(string url, Func<HttpStatusCode, bool> testState, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            var request = new HttpRequestMessage(HttpMethod.Get, _targetRecord.ReadyUrl);
            HttpResponseMessage message = await _httpClient.SendAsync(request, token);

            sw.Stop();
            long ms = sw.ElapsedMilliseconds;

            string? body = await message.Content.ReadAsStringAsync();

            _logger.LogInformation($"{nameof(Ping)}: {_targetRecord}, StatusCode={message.StatusCode}, ms={ms}");
            var trace = _targetRecord.CreateOkTrace(_agentId, message.StatusCode, body, ms);

            return testState(message.StatusCode);
        }
    }
}
