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
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Timeout;
using Polly.Wrap;
using TestHarnessApi;

namespace ResiliencyTests
{
    [TestClass]
    public class PollyMultipleWrappersExampleTests
    {
        private static TestServer _server;
        private static HttpClient _client;

        /// <summary>
        /// Execute Multiple Policies using the following rules:
        /// 1. if call result is in cache, then return it from cache
        /// 2. entire call stack is limited to the overall timeout
        /// 3. failed calls will be retried x times
        /// 4. after x consecutive exceptions, circuit breaker will block calls to action for y seconds
        /// 5. action is limited to call timeout
        /// 6. if failure happens in stack and fallback defined, then execute fall back action
        /// 7. if failure happens in stack and no fallback defined, then throw exception back to caller (exception will be based on which policy, if any catches the failure)
        /// 8. return result of successful call
        /// </summary>
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        private static readonly Policy _cachePolicy =
            Policy.CacheAsync(new MemoryCacheProvider(_cache), TimeSpan.FromSeconds(60));

        private static readonly Policy _overallTimeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(2), TimeoutStrategy.Pessimistic);

        private static readonly Policy _retryPolicy = Policy.Handle<Exception>().RetryAsync(5);

        private static readonly Policy _circuitBreakerPolicy =
            Policy.Handle<Exception>().CircuitBreakerAsync(4, TimeSpan.FromSeconds(3));

        private static readonly Policy _callTimeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromMilliseconds(500), TimeoutStrategy.Pessimistic);

        private static readonly PolicyWrap _policyWrapper = Policy.WrapAsync(_cachePolicy, _overallTimeoutPolicy, _retryPolicy,
            _circuitBreakerPolicy, _callTimeoutPolicy);

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

            // cache
            string url = "/api/cache";
            string result = await _policyWrapper.ExecuteAsync(async context => await _client.GetStringAsync(url), new Context("CacheKey1")); // typically cache key would be dynamic
            string result2 = await _policyWrapper.ExecuteAsync(async context => await _client.GetStringAsync(url), new Context("CacheKey1"));

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
            Assert.AreEqual(result, result2);

            // inner timeout (500 ms), retry (5), outer timeout (this will exception as the inner timeouts will be retried enough times to trigger outer timeout)
            url = "/api/timeout/slow";
            bool timeout = false;

            try
            {
                await _policyWrapper.ExecuteAsync(async context => await _client.GetStringAsync(url), new Context("CacheKey2"));
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
                string result3 = await _policyWrapper.ExecuteAsync(async context => await _client.GetStringAsync(url), new Context("CacheKey3"));
            }
            catch (BrokenCircuitException)
            {
                circuitIsBroken = true;
            }

            Assert.IsTrue(circuitIsBroken);

            // lets try a fallback
            url = "/api/circuitbreaker/success";
            var fallbackWrapper = FallbackPolicy<string>
                .Handle<Exception>()
                .FallbackAsync(context => Task.Run(() => "default"))
                .WrapAsync(_policyWrapper);

            string result4 = await fallbackWrapper.ExecuteAsync(async context => await _client.GetStringAsync(url), new Context("CacheKey4"));

            Assert.AreEqual("default", result4);

            // let circuit breaker reset
            Task.Delay(3000).Wait();            
            string result5 = await fallbackWrapper.ExecuteAsync(async context => await _client.GetStringAsync(url), new Context("CacheKey5"));

            Assert.IsTrue(!string.IsNullOrWhiteSpace(result5));
            Assert.AreNotEqual("default", result5);
        }        
    }
}
