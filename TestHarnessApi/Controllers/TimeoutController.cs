using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PollyHelpers;
using SimpleRetry;

namespace TestHarnessApi.Controllers
{
    [Route("api/timeout")]
    [ApiController]
    public class TimeoutController : ControllerBase
    {
        private static readonly HttpClient _client = new HttpClient();

        [HttpGet("{timeout?}")]
        public async Task<ActionResult<string>> Get(int timeout = 5)
        {
            string response = "";

            await PollyTimeoutExample.ExecuteAsync(timeout: TimeSpan.FromSeconds(timeout), action: async () => { response = await _client.GetStringAsync("http://localhost:16481/api/problem/slow"); });

            return Ok(response);
        }
    }
}