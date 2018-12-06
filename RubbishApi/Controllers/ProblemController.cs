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

        // GET api/values/5
        [HttpGet("{problem}")]
        public ActionResult<string> Get(string problem)
        {
            _executionCount++;

            if (string.Equals(problem, "temperror", StringComparison.InvariantCultureIgnoreCase))
            {
                if (_executionCount % 2 == 0)
                {
                    return BadRequest("Something was wrong!");
                }
            }

            return Guid.NewGuid().ToString();
        }
    }
}
