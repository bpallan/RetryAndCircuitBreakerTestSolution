using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace PollyHelpers
{
    public class PollyCircuitBreakerHelper
    {
        private readonly Policy _circuitBreaker;

        public PollyCircuitBreakerHelper(int numberOfFailures, TimeSpan delay, Func<Task> fallback = null)
        {
            var circuitBreaker = Policy
                .Handle<Exception>() // use HttpRequestException or call .HandleTransientHttpError if you only care about http errors
                .CircuitBreakerAsync(numberOfFailures, delay);

            if (fallback != null)
            {
                var fallbackWrapper = Policy
                    .Handle<Exception>()
                    .FallbackAsync(async i => { await fallback(); })
                    .WrapAsync(circuitBreaker);

                _circuitBreaker = fallbackWrapper;
            }
            else
            {
                _circuitBreaker = circuitBreaker;
            }
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            await _circuitBreaker.ExecuteAsync(async () => { await action(); });
        }
    }
}
