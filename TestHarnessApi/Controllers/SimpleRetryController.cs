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
    [Route("api/simpleretry")]
    [ApiController]
    public class SimpleRetryController : ControllerBase
    {
        private static readonly HttpClient _client = new HttpClient();

        [HttpGet]
        public ActionResult<string> Get()
        {
            string response = "";

            SimpleRetryHelper.Execute(numberOfRetries: 1, delayMs: TimeSpan.FromMilliseconds(1000), action: () => { response = _client.GetStringAsync("http://localhost:16481/api/problem/temperror").Result; });            

            return Ok(response);
        }
    }
}