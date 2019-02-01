using System;
using Microsoft.AspNetCore.Mvc;

namespace TestHarnessApi.Controllers
{
    [Route("api/external")]
    [ApiController]
    public class FauxExternalApiController : ControllerBase
    {
        [HttpGet("success")]
        public ActionResult<string> Success()
        {
            return Ok(Guid.NewGuid().ToString());
        }

        private static int _executionCount = 0;

        /// <summary>
        /// throws an exception on every other call
        /// </summary>
        /// <returns></returns>
        [HttpGet("unreliable")]
        public ActionResult<string> Unreliable()
        {
            _executionCount++;
            if (_executionCount % 2 != 0)
            {
                throw new InvalidOperationException("Something went wrong!!!");
            }

            return Guid.NewGuid().ToString();
        }

        [HttpGet("broken")]
        public ActionResult<string> Broken()
        {
            throw new InvalidOperationException("Something went wrong!!!");
        }
    }
}
