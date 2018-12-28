using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace RubbishApi.Controllers
{
    [Route("api/problem")]
    [ApiController]
    public class ProblemController : ControllerBase
    {
        private static int _executionCount = 0;

        [HttpGet("errorsoften")]
        public ActionResult<string> ErrorsOften()
        {
            _executionCount++;
            if (_executionCount % 2 == 0)
            {
                return BadRequest("Something was wrong!");
            }

            return Guid.NewGuid().ToString();
        }

        [HttpGet("slow")]
        public ActionResult<string> Slow()
        {
            Task.Delay(TimeSpan.FromSeconds(10)).Wait();

            return Guid.NewGuid().ToString();
        }
    }
}
