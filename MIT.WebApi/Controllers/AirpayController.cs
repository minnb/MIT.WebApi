using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MIT.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AirpayController : ControllerBase
    {
        private readonly ILogger<AirpayController> _logger;
        public AirpayController(ILogger<AirpayController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return "OK";
        }
    }
}
