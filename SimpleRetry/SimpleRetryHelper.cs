using System;
using System.Threading.Tasks;

namespace SimpleRetry
{
    public static class SimpleRetryHelper
    {
        public static void Execute(int numberOfRetries, TimeSpan delayMs, Action action)
        {
            var attempts = 0;

            do
            {
                try
                {
                    attempts++;
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    if (attempts > numberOfRetries)
                    {
                        throw;
                    }

                    Task.Delay(delayMs).Wait();
                }

            } while (true);
        }
    }
}
