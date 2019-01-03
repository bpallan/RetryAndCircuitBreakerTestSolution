using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
        public void ExecuteAsync_WhenApiReturnsError_CircuitIsBrokenNoFallback()
        {
            // create circuit breaker (should be static in your application)
            PollySimpleCircuitBreakerExample circuitBreaker = new PollySimpleCircuitBreakerExample(numberOfFailures: 1, delay: TimeSpan.FromSeconds(3));

            // break circuit
            CallApi(circuitBreaker, "broken");

            // verify circuit breaker behavior
            string result = CallApi(circuitBreaker, "broken");

            Assert.AreEqual("Circuit is broken!!!", result);
        }

        [TestMethod]
        public void ExecuteAsync_WhenApiReturnsError_CircuitIsBrokenWithFallback()
        {
            // break circuit

            // verify fallback is called
        }

        [TestMethod]
        public void ExecuteAsync_WhenApiIsSuccessfulAgain_CircuitIsOpened()
        {
            // break circuit

            // verify circuit breaker is broken

            // wait

            // verify circuit is back online
        }

        private string CallApi(PollySimpleCircuitBreakerExample circuitBreaker, string action)
        {
            string response = "";

            try
            {
                circuitBreaker.ExecuteAsync(
                    action: async () =>
                    {
                        response = await _client.GetStringAsync($"{_url}/{action}");
                    }
                ).Wait();
            }
            catch (Exception ex) // if doing this async BrokenCircuitException will be your top level exception once circuit is broken
            {
                if (ex.InnerException is BrokenCircuitException)
                {
                    response = "Circuit is broken!!!";
                }
            }

            return response;
        }
    }
}
