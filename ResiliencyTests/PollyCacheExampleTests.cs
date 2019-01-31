using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.Caching.Memory;
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

        // cache policty
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        private static readonly Policy _cachePolicy =
            Policy.CacheAsync(new MemoryCacheProvider(_cache), TimeSpan.FromSeconds(60));

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenDataExistsInCache_ReturnsCachedValue()
        {
            string result = await _cachePolicy.ExecuteAsync(async context => await _client.GetStringAsync(_url), new Context("CacheKey1")); // typically cache key would be dynamic
            string result2 = await _cachePolicy.ExecuteAsync(async context => await _client.GetStringAsync(_url), new Context("CacheKey1"));

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
            Assert.AreEqual(result, result2);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenDataDoesNotExistInCache_ReturnsDifferentValue()
        {
            string result = await _cachePolicy.ExecuteAsync(async context => await _client.GetStringAsync(_url), new Context("CacheKey1")); // typically cache key would be dynamic
            string result2 = await _cachePolicy.ExecuteAsync(async context => await _client.GetStringAsync(_url), new Context("CacheKey2"));

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result2));
            Assert.AreNotEqual(result, result2);
        }
    }
}
