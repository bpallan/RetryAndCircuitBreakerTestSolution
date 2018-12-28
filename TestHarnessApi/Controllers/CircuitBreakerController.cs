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

        [HttpGet("simple")]
        public async Task<ActionResult<string>> Simple()
        {
            string response = "";

            try
            {
                await _simpleCircuitBreaker.ExecuteAsync(
                    action: async () =>
                    {
                        response = await _client.GetStringAsync("http://localhost:16481/api/problem/errorsoften");
                    }
                );
            }
            catch (BrokenCircuitException)
            {
                response = "Circuit is broken!!!";
            }            

            return Ok(response);
        }

        [HttpGet("fallback")]
        public async Task<ActionResult<string>> Fallback()
        {
            string response = "";

            await _simpleCircuitBreaker.ExecuteAsync(
                action: async () =>
                {
                    response = await _client.GetStringAsync("http://localhost:16481/api/problem/errorsoften");
                }
                , fallback: async () => { await Task.Run(() => response = "Circuit is broken fallback!!!"); }
            );

            return Ok(response);
        }
    }
}