using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly.CircuitBreaker;
using PollyHelpers;
using TestHarnessApi;

namespace ResiliencyTests
{
    [TestClass]
    public class PollySimpleCircuitBreakerExampleTests
    {
        private static string _url = "/api/circuitbreaker";
        private static TestServer _server;
        private static HttpClient _client;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenApiReturnsError_CircuitIsBrokenNoFallback()
        {
            // create circuit breaker (should be static in your application)
            PollySimpleCircuitBreakerExample circuitBreaker = new PollySimpleCircuitBreakerExample(numberOfFailures: 1, delay: TimeSpan.FromSeconds(3));

            // break circuit
            await CallApi(circuitBreaker, "broken");

            // verify circuit breaker behavior
            string result = await CallApi(circuitBreaker, "broken");

            Assert.AreEqual("Circuit is broken!!!", result);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenApiReturnsError_CircuitIsBrokenWithFallback()
        {
            // create circuit breaker (should be static in your application)
            string fallbackresult = "";
            PollySimpleCircuitBreakerExample circuitBreaker = new PollySimpleCircuitBreakerExample(numberOfFailures: 1, delay: TimeSpan.FromSeconds(3), fallback: async() => await Task.Run(() => fallbackresult = "default"));

            // break circuit
            await CallApi(circuitBreaker, "broken");

            // verify fallback is called.            
            await CallApi(circuitBreaker, "broken");

            Assert.AreEqual("default", fallbackresult);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenApiIsSuccessfulAgain_CircuitIsOpened()
        {
            // create circuit breaker (should be static in your application)
            PollySimpleCircuitBreakerExample circuitBreaker = new PollySimpleCircuitBreakerExample(numberOfFailures: 1, delay: TimeSpan.FromSeconds(3));

            // break circuit
            await CallApi(circuitBreaker, "broken");

            // verify circuit breaker is broken
            string result = await CallApi(circuitBreaker, "success");
            Assert.AreEqual("Circuit is broken!!!", result);
            Assert.AreEqual(CircuitState.Open, circuitBreaker.CircuitBreakerState);

            // wait
            await Task.Delay(3000);

            // verify circuit is back online
            result = await CallApi(circuitBreaker, "success");
            Assert.AreEqual(CircuitState.Closed, circuitBreaker.CircuitBreakerState);
            Assert.AreNotEqual("Circuit is broken!!!", result);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
        }

        private async Task<string> CallApi(PollySimpleCircuitBreakerExample circuitBreaker, string action)
        {
            string response = "";

            try
            {
                await circuitBreaker.ExecuteAsync(
                    action: async () => { response = await _client.GetStringAsync($"{_url}/{action}"); }
                );
            }            
            catch (BrokenCircuitException) // if calling this syncronous BrokenCircuitException will be your inner exception once circuit is broken
            {
                response = "Circuit is broken!!!";
            }
            catch (Exception)
            {
                response = "Exception from api call!!!";
            }

            return response;
        }
    }
}
