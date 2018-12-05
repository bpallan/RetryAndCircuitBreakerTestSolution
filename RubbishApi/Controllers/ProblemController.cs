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
        private static int _errorCount = 0;

        // GET api/values/5
        [HttpGet("{problem}")]
        public async Task<ActionResult<string>> Get(string problem)
        {
            if (string.Equals(problem, "temperror", StringComparison.InvariantCultureIgnoreCase))
            {
                if (_errorCount % 2 == 0)
                {
                    _errorCount++;
                    return BadRequest("Something was wrong!");
                }
            }

            return Guid.NewGuid().ToString();
        }
    }
}
