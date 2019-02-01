using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TestHarnessApi.Controllers
{
    [Route("api/cache")]
    [ApiController]    
    public class CacheController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
