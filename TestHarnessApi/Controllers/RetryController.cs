using System;
using Microsoft.AspNetCore.Mvc;


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
    }
}