using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Extensions;
using WatcherAgent.Models;
using System.Net;
using WatcherAgent.Services.AgentStatus;

namespace WatcherAgent.Controllers
{
    [Route("ping")]
    [ApiController]
    internal class PingController : ControllerBase
    {
        private readonly IAgentStatusService _stateService;
        private readonly ILogger<PingController> _logger;

        public PingController(IAgentStatusService stateService, ILogger<PingController> logger)
        {
            _stateService = stateService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogTrace($"{nameof(Get)}: Ping, current state: {_stateService.ServiceState}");

            var result = new PingResultModel
            {
                Status = _stateService.ServiceState.ToString()
            };

            switch (_stateService.ServiceState)
            {
                case ServiceStatusState.Stopped:
                    return StatusCode((int)HttpStatusCode.ServiceUnavailable, result);

                case ServiceStatusState.Running:
                case ServiceStatusState.Ready:
                    return Ok(result);

                case ServiceStatusState.Failed:
                default:
                    return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
        }

        [HttpGet("running")]
        public IActionResult GetRunning()
        {
            _logger.LogTrace($"{nameof(GetRunning)}: Ping running, current state: {_stateService.ServiceState}");

            var result = new PingResultModel
            {
                Status = _stateService.ServiceState.ToString()
            };

            switch (_stateService.ServiceState)
            {
                case ServiceStatusState.Stopped:
                    return StatusCode((int)HttpStatusCode.ServiceUnavailable, result);

                case ServiceStatusState.Ready:
                case ServiceStatusState.Running:
                    return Ok(result);

                case ServiceStatusState.Failed:
                default:
                    return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
        }

        [HttpGet("ready")]
        public IActionResult GetReady()
        {
            _logger.LogTrace($"{nameof(GetRunning)}: Ping ready, current state: {_stateService.ServiceState}");

            var result = new PingResultModel
            {
                Status = _stateService.ServiceState.ToString()
            };

            switch (_stateService.ServiceState)
            {
                case ServiceStatusState.Running:
                case ServiceStatusState.Stopped:
                    return StatusCode((int)HttpStatusCode.ServiceUnavailable, result);

                case ServiceStatusState.Ready:
                    return Ok(result);

                case ServiceStatusState.Failed:
                default:
                    return StatusCode((int)HttpStatusCode.InternalServerError, result);
            }
        }
    }
}
