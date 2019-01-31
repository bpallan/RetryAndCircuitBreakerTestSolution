using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Timeout;
using TestHarnessApi;

namespace ResiliencyTests
{
    [TestClass]
    public class PollyTimeoutExampleTests
    {
        private static string _url = "/api/timeout";
        private static TestServer _server;
        private static HttpClient _client;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenResponseIsLessThanTimeout_ReturnsResult()
        {
            string response = await CallApi("fast", 500);

            Assert.IsFalse(string.IsNullOrWhiteSpace(response));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutRejectedException))]
        public async Task ExecuteAsync_WhenResponseIsGreaterThanTimeout_ThrowsException()
        {
            string response = await CallApi("slow", 500);
        }

        private static async Task<string> CallApi(string action, int milliseconds)
        {
            string response = await Policy.TimeoutAsync(TimeSpan.FromMilliseconds(milliseconds), TimeoutStrategy.Pessimistic)
                .ExecuteAsync(async context => await _client.GetStringAsync($"{_url}/{action}"), new Context());

            return response;
        }
    }
}
