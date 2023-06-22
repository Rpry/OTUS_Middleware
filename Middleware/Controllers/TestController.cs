using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Middleware.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("time")]
        public string Get()
        {
            return DateTime.Now.ToString("f");
        }

        [HttpGet("error")]
        public async Task<int> ThrowTask()
        {
            throw new Exception("NULL reference exception");
        }
    }
}