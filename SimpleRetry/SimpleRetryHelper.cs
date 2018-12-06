using System;
using System.Threading.Tasks;

namespace SimpleRetry
{
    public static class SimpleRetryHelper
    {
        public static void Execute(int numberOfRetries, TimeSpan delayMs, Action action)
        {
            ExecuteAsync(numberOfRetries, delayMs, () => Task.Run(action)).Wait();
        }

        public static async Task ExecuteAsync(int numberOfRetries, TimeSpan delayMs, Func<Task> action)
        {
            var attempts = 0;

            do
            {
                try
                {
                    attempts++;
                    await action();
                    break;
                }
                catch (Exception)
                {
                    if (attempts > numberOfRetries)
                    {
                        throw;
                    }

                    await Task.Delay(delayMs);
                }

            } while (true);
        }
    }
}
