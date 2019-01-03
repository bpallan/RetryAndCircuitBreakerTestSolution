using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PollyHelpers;
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
        public void ExecuteAsync_WhenRetriesZero_ThrowsException()
        {
            try
            {
                // call twice to ensure at least 1 call errors
                CallApi(retries: 0);
                CallApi(retries: 0);
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [TestMethod]
        public void ExecuteAsync_WhenRetriesGreaterThanZero_ReturnsResult()
        {
            // call twice to ensure at least 1 call errors (and is handled by retry)
            var response = CallApi(retries: 1); CallApi(retries: 1);
            CallApi(retries: 1);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(response));
        }

        private static string CallApi(int retries)
        {
            string response = "";

            PollyRetryExample.ExecuteAsync(numberOfRetries: retries, delay: TimeSpan.FromMilliseconds(1000), action: async () => { response = await _client.GetStringAsync(_url); }).Wait();

            return response;
        }
    }    
}
