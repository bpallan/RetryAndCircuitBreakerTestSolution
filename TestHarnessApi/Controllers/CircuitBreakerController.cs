using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using PollyHelpers;

namespace TestHarnessApi.Controllers
{
    [Route("api/circuitbreaker")]
    [ApiController]
    public class CircuitBreakerController : ControllerBase
    {
        private static readonly HttpClient _client = new HttpClient();
        private static readonly PollySimpleCircuitBreakerExample _simpleCircuitBreaker = new PollySimpleCircuitBreakerExample(1, TimeSpan.FromSeconds(30));

        public CircuitBreakerController()
        {
            
        }

        [HttpGet("broken")]
        public ActionResult<string> Broken()
        {
            return BadRequest("Something was wrong!");
        }

        [HttpGet("success")]
        public ActionResult<string> Success()
        {
            return Ok(Guid.NewGuid().ToString());
        }
    }
}