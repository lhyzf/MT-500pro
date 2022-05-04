using Microsoft.AspNetCore.Mvc;
using UPSMonitor.Server.Services;
using UPSMonitor.Shared;

namespace UPSMonitor.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UPSController : ControllerBase
    {
        private readonly ILogger<UPSController> _logger;
        private readonly UPSService _upsService;

        public UPSController(ILogger<UPSController> logger, UPSService upsService)
        {
            _logger = logger;
            _upsService = upsService;
        }

        [HttpGet("info")]
        public UPSInfo? GetInfo()
        {
            return _upsService.Info;
        }

        [HttpGet("status")]
        public UPSStatus? GetStatus()
        {
            return _upsService.Status;
        }
    }
}
