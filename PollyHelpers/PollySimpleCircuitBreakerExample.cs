using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace PollyHelpers
{
    /// <summary>
    /// Important:  You must have 1 of these per circuit you want to manage.  Different actions passed through the same circuit breaker instance will share the same circuit.  
    /// for example, if api call 1 and api call 2 are independent, they should use different instances of this class to manage their circuits
    /// </summary>
    public class PollySimpleCircuitBreakerExample
    {
        private readonly Policy _circuitBreakerPolicy;

        public PollySimpleCircuitBreakerExample(int numberOfFailures, TimeSpan delay, Func<Task> fallback = null) // w/out fallback will throw a BrokenCircuitException while circuit is broken
        {
            Policy innerPolicy = Policy
                .Handle<Exception>() // use HttpRequestException or call .HandleTransientHttpError if you only care about http errors
                .CircuitBreakerAsync(numberOfFailures, delay);

            if (fallback != null)
            {
                _circuitBreakerPolicy = Policy
                    .Handle<Exception>()
                    .FallbackAsync(async i => { await fallback(); })
                    .WrapAsync(innerPolicy);
            }
            else
            {
                _circuitBreakerPolicy = innerPolicy;
            }
        }

        public async Task ExecuteAsync(Func<Task> action, Func<Task> fallback = null) 
        {
            await _circuitBreakerPolicy.ExecuteAsync(async () => { await action(); });           
        }
    }
}
