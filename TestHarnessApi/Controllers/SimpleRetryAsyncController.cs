using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleRetry;

namespace TestHarnessApi.Controllers
{
    [Route("api/simpleretryasync")]
    [ApiController]
    public class SimpleRetryAsyncController : ControllerBase
    {
        private static readonly HttpClient _client = new HttpClient();

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            return await Get(retries: 0);
        }

        [HttpGet("{retries}")]
        public async Task<ActionResult<string>> Get(int retries)
        {
            string response = "";

            await SimpleRetryHelper.ExecuteAsync(numberOfRetries: retries, delayMs: TimeSpan.FromMilliseconds(1000), action: async () => { response = await _client.GetStringAsync("http://localhost:16481/api/problem/temperror"); });

            return Ok(response);
        }
    }
}