using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExternalApiProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestHarnessApi.Controllers
{
    [Route("api/externalapiexample")]
    [ApiController]
    public class ExternalApiExampleController : ControllerBase
    {
        private readonly IExternalApiClient _client;
        public ExternalApiExampleController(IExternalApiClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            var result = await _client.GetSuccessfulResponse();

            return Ok(result);
        }
    }
}