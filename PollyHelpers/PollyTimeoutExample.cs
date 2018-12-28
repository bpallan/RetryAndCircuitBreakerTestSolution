using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;

namespace PollyHelpers
{
    public class PollyTimeoutExample
    {
        public static async Task ExecuteAsync(TimeSpan timeout, Func<Task> action)
        {
            var timeoutPolicy = Policy
                    .TimeoutAsync(timeout, TimeoutStrategy.Pessimistic);

            await timeoutPolicy.ExecuteAsync(async () => { await action(); });
        }
    }
}
