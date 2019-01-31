using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestHarnessApi.Controllers
{
    [Route("api/timeout")]
    [ApiController]
    public class TimeoutController : ControllerBase
    {
        private static readonly HttpClient _client = new HttpClient();

        [HttpGet("slow")]
        public async Task<ActionResult<string>> Slow()
        {
            await Task.Delay(1000);

            return Ok(Guid.NewGuid().ToString());
        }

        [HttpGet("fast")]
        public async Task<ActionResult<string>> Fast()
        {
            await Task.Delay(50);

            return Ok(Guid.NewGuid().ToString());
        }
    }
}