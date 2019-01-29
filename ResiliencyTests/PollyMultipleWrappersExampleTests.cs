using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly.CircuitBreaker;
using Polly.Timeout;
using PollyHelpers;
using TestHarnessApi;

namespace ResiliencyTests
{
    [TestClass]
    public class PollyMultipleWrappersExampleTests
    {
        private static TestServer _server;
        private static HttpClient _client;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        // difficult to test as policies in helpers are static and each test effects other tests... so running through scenerios in this 1 execution
        [TestMethod]
        public async Task ExecuteAsync_MultiplePolicies_WorkAsExpected()
        {

            var multiPolicy = new PollyMultipleWrappersExample<string>();

            // cache
            string url = "/api/cache";
            string result = await multiPolicy.ExecuteAsync(async () => await _client.GetStringAsync(url), "CacheKey1"); // typically cache key would be dynamic
            string result2 = await multiPolicy.ExecuteAsync(async () => await _client.GetStringAsync(url), "CacheKey1");

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
            Assert.AreEqual(result, result2);

            // inner timeout (500 ms), retry (5), outer timeout (this will exception as the inner timeouts will be retried enough times to trigger outer timeout)
            url = "/api/timeout/slow";
            bool timeout = false;

            try
            {
                await multiPolicy.ExecuteAsync(async () => await _client.GetStringAsync(url), "CacheKey2");
            }
            catch (TimeoutRejectedException)
            {
                timeout = true;
            }

            Assert.IsTrue(timeout);

            // trying this again will trip circuit breaker before we get a chance to hit the outer timeout
            bool circuitIsBroken = false;

            try
            {
                string result3 = await multiPolicy.ExecuteAsync(async () => await _client.GetStringAsync(url), "CacheKey3");
            }
            catch (BrokenCircuitException)
            {
                circuitIsBroken = true;
            }
            
            Assert.IsTrue(circuitIsBroken);

            // lets try the fallback
            string result4 = await multiPolicy.ExecuteAsync(async () => await _client.GetStringAsync(url), "CacheKey3", () => Task.Run(() => "default"));

            Assert.AreEqual("default", result4);
        }        
    }
}
