using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PollyHelpers;

namespace TestHarnessApi.Controllers
{
    [Route("api/circuitbreaker")]
    [ApiController]
    public class CircuitBreakerController : ControllerBase
    {
        private static readonly HttpClient _client = new HttpClient();
        private static readonly PollySimpleCircuitBreakerExample _simpleCircuitBreaker = new PollySimpleCircuitBreakerExample(1, TimeSpan.FromSeconds(10));

        public CircuitBreakerController()
        {
            
        }

        [HttpGet("simple")]
        public async Task<ActionResult<string>> Simple()
        {
            string response = "";

            await _simpleCircuitBreaker.ExecuteAsync(
                action: async () =>
                {
                    response = await _client.GetStringAsync("http://localhost:16481/api/problem/temperror");
                },
                fallback: async () => { await Task.Run(() => response = "Circuit is broken!!!"); });

            return Ok(response);
        }
    }
}