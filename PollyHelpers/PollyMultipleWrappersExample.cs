using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching.Memory;
using Polly.Wrap;

namespace PollyHelpers
{
    // see: https://github.com/App-vNext/Polly/wiki/PolicyWrap#ordering-the-available-policy-types-in-a-wrap

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
    public class PollyMultipleWrappersExample<T>
    {
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        private static readonly Policy _cachePolicy =
            Policy.CacheAsync(new MemoryCacheProvider(_cache), TimeSpan.FromSeconds(60));

        private static readonly Policy _overallTimeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(5));

        private static readonly Policy _retryPolicy = Policy.Handle<Exception>().RetryAsync(10);

        private static readonly Policy _circuitBreakerPolicy =
            Policy.Handle<Exception>().CircuitBreakerAsync(4, TimeSpan.FromSeconds(60));

        private static readonly Policy _callTimeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromMilliseconds(500));

        private static readonly PolicyWrap _policyWrapper = Policy.WrapAsync(_cachePolicy, _overallTimeoutPolicy, _retryPolicy,
            _circuitBreakerPolicy, _callTimeoutPolicy);

        public async Task<T> ExecuteAsync(Func<Task<T>> action, string cacheKey, Func<Task<T>> fallback = null)
        {
            if (fallback != null)
            {
                var fallbackWrapper = Policy
                    .Handle<Exception>()
                    .FallbackAsync(context => fallback())
                    .WrapAsync(_policyWrapper);

                return await fallbackWrapper.ExecuteAsync(context => action(), new Context(cacheKey));
            }
            else
            {
                return await _policyWrapper.ExecuteAsync(context => action(), new Context(cacheKey));
            }
        }
    }
}
