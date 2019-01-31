using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestHarnessApi.Controllers
{
    [Route("api/circuitbreaker")]
    [ApiController]
    public class CircuitBreakerController : ControllerBase
    {       
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