using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Tools.Rest;
using WatcherSdk.Records;
using WatcherSdk.Repository;

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

            var sw = Stopwatch.StartNew();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _targetRecord.Url);
                HttpResponseMessage message = await _httpClient.SendAsync(request, token);

                sw.Stop();
                long ms = sw.ElapsedMilliseconds;

                string? body = await message.Content.ReadAsStringAsync();

                _logger.LogInformation($"{nameof(Ping)}: {_targetRecord}, StatusCode={message.StatusCode}, ms={ms}");
                var trace = _targetRecord.CreateOkTrace(_agentId, message.StatusCode, body, ms);

                return message.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(Ping)}: {_targetRecord}");

                var trace = _targetRecord.CreateErrorTrace(_agentId, HttpStatusCode.InternalServerError, null, ex.ToString());
                await _traceContainer.Set(trace, token);
                return false;
            }
            finally
            {
                sw.Stop();
            }
        }
    }
}
