using System;
using System.Threading.Tasks;
using Polly;

namespace PollyHelpers
{
    public class PollyRetryExample
    {
        public static async Task ExecuteAsync(int numberOfRetries, TimeSpan delay, Func<Task> action)
        {
            var retryPolicy = Policy
                .Handle<Exception>() // use HttpRequestException or call .HandleTransientHttpError if you only care about http errors
                .WaitAndRetryAsync(numberOfRetries, i => delay);

            await retryPolicy.ExecuteAsync(async () => { await action(); });
        }
    }
}
