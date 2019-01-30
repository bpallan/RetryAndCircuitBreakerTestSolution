using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using TestHarnessApi;

namespace ResiliencyTests
{
    [TestClass]
    public class PollySimpleCircuitBreakerExampleTests
    {
        private static string _url = "/api/circuitbreaker";
        private static TestServer _server;
        private static HttpClient _client;

        // policy
        // Important:  You must have 1 of these per circuit you want to manage.  Different actions passed through the same circuit breaker instance will share the same circuit.  
        // for example, if api call 1 and api call 2 are independent, they should use different instances of this class to manage their circuits        
        private static readonly CircuitBreakerPolicy _circuitBreaker = Policy
            .Handle<Exception>() // use HttpRequestException or call .HandleTransientHttpError if you only care about http errors
            .CircuitBreakerAsync(1, TimeSpan.FromSeconds(3));

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenApiReturnsError_CircuitIsBrokenNoFallback()
        {
            // break circuit
            await CallApi("broken");

            // verify circuit breaker behavior
            string result = await CallApi("broken");

            Assert.AreEqual("Circuit is broken!!!", result);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenApiReturnsError_CircuitIsBrokenWithFallback()
        {
            // break circuit
            await CallApi("broken");

            // verify fallback is called.            
            string result = await CallApiWithFallback("broken", async() => await Task.Run(() => "default"));

            Assert.AreEqual("default", result);
        }

        [TestMethod]
        public async Task ExecuteAsync_WhenApiIsSuccessfulAgain_CircuitIsOpened()
        {
            // break circuit
            await CallApi("broken");

            // verify circuit breaker is broken
            string result = await CallApi("success");
            Assert.AreEqual("Circuit is broken!!!", result);
            Assert.AreEqual(CircuitState.Open, _circuitBreaker.CircuitState);

            // wait
            await Task.Delay(3000);

            // verify circuit is back online
            result = await CallApi("success");
            Assert.AreEqual(CircuitState.Closed, _circuitBreaker.CircuitState);
            Assert.AreNotEqual("Circuit is broken!!!", result);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(result));
        }

        private async Task<string> CallApi(string action)
        {
            string response = "";

            try
            {
                await _circuitBreaker.ExecuteAsync(
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

        private async Task<string> CallApiWithFallback(string action, Func<Task<string>> fallback)
        {
            var fallbackWrapper = FallbackPolicy<string>
                .Handle<Exception>()
                .FallbackAsync(context => fallback())
                .WrapAsync(_circuitBreaker);

            string response = await fallbackWrapper.ExecuteAsync(context => _client.GetStringAsync($"{_url}/{action}"), new Context()); 

            return response;
        }
    }
}
