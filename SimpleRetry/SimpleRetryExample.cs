﻿using System;
using System.Threading.Tasks;

namespace SimpleRetry
{
    public static class SimpleRetryExample
    {
        public static async Task ExecuteAsync(int numberOfRetries, TimeSpan delay, Func<Task> action)
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

                    await Task.Delay(delay);
                }

            } while (true);
        }
    }
}
