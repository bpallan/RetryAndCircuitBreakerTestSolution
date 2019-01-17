using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExternalApiProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;

namespace TestHarnessApi.Controllers
{
    [Route("api/externalapiexample")]
    [ApiController]
    public class ExternalApiExampleController : ControllerBase
    {
        private readonly IExternalApiClient _client;
        public ExternalApiExampleController(IExternalApiClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            var result = await _client.GetSuccessfulResponse();

            return Ok(result);
        }

        [HttpGet("retry")]
        public async Task<ActionResult<string>> Retry()
        {
            var result = await _client.GetUnreliableResponse();

            return Ok(result);
        }

        [HttpGet("circuitbreaker")]
        public async Task<ActionResult<string>> CircuitBreaker()
        {
            try
            {
                var result = await _client.GetFailureResponse();

                return Ok(result);
            }
            catch (BrokenCircuitException)
            {
                return Ok("Circuit is broken.  Should do fallback behavior.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }            
        }
    }
}