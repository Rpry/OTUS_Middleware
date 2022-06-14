using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FunWithMiddleware.Controllers
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
      throw new Exception("Some error");
    }
  }
}