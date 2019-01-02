﻿using System;
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
    [Route("api/retry")]
    [ApiController]
    public class RetryController : ControllerBase
    {
        private static int _executionCount = 0;

        /// <summary>
        /// throws an exception on every other call
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<string> Get()
        {
            _executionCount++;
            if (_executionCount % 2 != 0)
            {
                return BadRequest("Something was wrong!");
            }

            return Guid.NewGuid().ToString();
        }

        private static readonly HttpClient _client = new HttpClient();

        [HttpGet("simple/{retries?}")]
        public async Task<ActionResult<string>> SimpleAsync(int retries = 0)
        {
            string response = "";

            await SimpleRetryExample.ExecuteAsync(numberOfRetries: retries, delay: TimeSpan.FromMilliseconds(1000), action: async () => { response = await _client.GetStringAsync("http://localhost:16481/api/problem/errorsoften"); });

            return Ok(response);
        }

        [HttpGet("polly/{retries?}")]
        public async Task<ActionResult<string>> Polly(int retries = 0)
        {
            string response = "";

            await PollyRetryExample.ExecuteAsync(numberOfRetries: retries, delay: TimeSpan.FromMilliseconds(1000), action: async () => { response = await _client.GetStringAsync("http://localhost:16481/api/problem/errorsoften"); });

            return Ok(response);
        }
    }
}