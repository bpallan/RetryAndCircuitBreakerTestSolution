using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace PollyHelpers
{
    public class PollySimpleCircuitBreakerExample
    {
        private readonly Policy _circuitBreaker;

        public PollySimpleCircuitBreakerExample(int numberOfFailures, TimeSpan delay)
        {
            _circuitBreaker = Policy
                .Handle<Exception>() // use HttpRequestException or call .HandleTransientHttpError if you only care about http errors
                .CircuitBreakerAsync(numberOfFailures, delay);            
        }

        public async Task ExecuteAsync(Func<Task> action, Func<Task> fallback = null) // w/out fallback will throw a BrokenCircuitException while circuit is broken
        {
            if (fallback != null)
            {
                var fallbackWrapper = Policy
                    .Handle<Exception>()
                    .FallbackAsync(async i => { await fallback(); })
                    .WrapAsync(_circuitBreaker);

                await fallbackWrapper.ExecuteAsync(async () => { await action(); });
            }
            else
            {
                await _circuitBreaker.ExecuteAsync(async () => { await action(); });
            }            
        }
    }
}
