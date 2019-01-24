using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using PollyHelpers;
using TestHarnessApi;

namespace ResiliencyTests
{
    [TestClass]
    public class PollyCacheExampleTests
    {

        private static string _url = "/api/cache";
        private static TestServer _server;
        private static HttpClient _client;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenDataExistsInCache_ReturnsCachedValue()
        {
            var cachedCall = new PollyCacheExample<string>();

            string result = await cachedCall.ExecuteAsync(async() => await _client.GetStringAsync(_url), "CacheKey1"); // typically cache key would be dynamic
            string result2 = await cachedCall.ExecuteAsync(async () => await _client.GetStringAsync(_url), "CacheKey1");

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
            Assert.AreEqual(result, result2);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenDataDoesNotExistInCache_ReturnsDifferentValue()
        {
            var cachedCall = new PollyCacheExample<string>();

            string result = await cachedCall.ExecuteAsync(async () => await _client.GetStringAsync(_url), "CacheKey1"); // typically cache key would be dynamic
            string result2 = await cachedCall.ExecuteAsync(async () => await _client.GetStringAsync(_url), "CacheKey2");

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result2));
            Assert.AreNotEqual(result, result2);
        }
    }
}
