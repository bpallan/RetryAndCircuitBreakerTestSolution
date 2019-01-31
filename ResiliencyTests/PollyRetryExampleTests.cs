using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using TestHarnessApi;

namespace ResiliencyTests
{
    [TestClass]
    public class PollyRetryExampleTests
    {
        private static string _url = "/api/retry";
        private static TestServer _server;
        private static HttpClient _client;
        

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ExecuteAsync_WhenRetriesZero_ThrowsException()
        {
            // call twice to ensure at least 1 call errors
            await CallApi(retries: 0);
            await CallApi(retries: 0);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenRetriesGreaterThanZero_ReturnsResult()
        {
            // call twice to ensure at least 1 call errors (and is handled by retry)
            var response = await CallApi(retries: 1); 
            await CallApi(retries: 1);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(response));
        }

        private static async Task <string> CallApi(int retries)
        {
            string response = await Policy
                .Handle<Exception>() // use HttpRequestException or call .HandleTransientHttpError if you only care about http errors
                .RetryAsync(retries)
                .ExecuteAsync(async context => await _client.GetStringAsync(_url), new Context());

            return response;
        }
    }    
}
