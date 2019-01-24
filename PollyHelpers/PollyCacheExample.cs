using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching.Memory;

namespace PollyHelpers
{
    public class PollyCacheExample<T>
    {
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        private static readonly Policy _cachePolicy =
            Policy.CacheAsync(new MemoryCacheProvider(_cache), TimeSpan.FromSeconds(60));

        public async Task<T> ExecuteAsync(Func<Task<T>> action, string cacheKey)
        {
            return await _cachePolicy.ExecuteAsync(context => action(), new Context(cacheKey));
        }
    }
}
