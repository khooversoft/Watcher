using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestRestApiServer.Models;
using TestRestApiServer.Services;
using Toolbox.Extensions;
using WatcherSdk.Services.ServiceState;

namespace TestRestApiServer.Controllers
{
    [Route("ping")]
    [ApiController]
    public class PingController : ControllerBase
    {
        private readonly IStateService _stateService;
        private readonly ILogger<PingController> _logger;

        public PingController(IStateService stateService, ILogger<PingController> logger)
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

            return StatusCode((int)_stateService.ServiceState.ToHttpStatusCodeForReady(), result);
        }

        [HttpGet("ready")]
        public IActionResult GetReady()
        {
            _logger.LogTrace($"{nameof(GetRunning)}: Ping ready, current state: {_stateService.ServiceState}");

            var result = new PingResultModel
            {
                Status = _stateService.ServiceState.ToString()
            };

            return StatusCode((int)_stateService.ServiceState.ToHttpStatusCodeForReady(), result);
        }

        [HttpGet("running")]
        public IActionResult GetRunning()
        {
            _logger.LogTrace($"{nameof(GetRunning)}: Ping running, current state: {_stateService.ServiceState}");

            var result = new PingResultModel
            {
                Status = _stateService.ServiceState.ToString()
            };

            return StatusCode((int)_stateService.ServiceState.ToHttpStatusCodeForRunning(), result);
        }

        [HttpPost("state/{newState}")]
        public IActionResult SetState(string newState)
        {
            if (newState.IsEmpty()) return BadRequest();
            if (!Enum.TryParse(typeof(ServiceStateType), newState, true, out object? newStateTypeObject)) return BadRequest();

            ServiceStateType newStateType = (ServiceStateType)newStateTypeObject!;

            _logger.LogTrace($"{nameof(SetState)}: current state={_stateService.ServiceState}, new state={newStateType}");
            _stateService.SetState(newStateType);

            var result = new PingResultModel
            {
                Status = _stateService.ServiceState.ToString()
            };

            return Ok(result);
        }
    }
}
