using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ExternalApi.Controllers
{
    [Route("api/external")]
    [ApiController]
    public class ExternalController : ControllerBase
    {
        [HttpGet("success")]
        public ActionResult<string> Success()
        {
            return Ok(Guid.NewGuid().ToString());
        }
    }
}
