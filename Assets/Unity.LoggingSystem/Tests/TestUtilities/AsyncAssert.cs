using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuntimeLogging.Tests.TestUtilities
{
    public static class AsyncAssert
    {
        public static async Task<TException> ThrowsAsync<TException>(
            Func<Task> asyncAction,
            string expectedMessageContains = null,
            int timeoutMilliseconds = 5000)
            where TException : Exception
        {
            try
            {
                using var cts = new CancellationTokenSource(timeoutMilliseconds);
                var task = asyncAction();
                var completed = await Task.WhenAny(task, Task.Delay(timeoutMilliseconds, cts.Token));

                if (completed != task)
                    throw new TimeoutException($"Async operation timed out ({timeoutMilliseconds} ms)");

                await task;
                Assert.Fail($"Expected {typeof(TException).Name} but method completed successfully");
                return null;
            }
            catch (TException ex)
            {
                if (!string.IsNullOrEmpty(expectedMessageContains) &&
                    !ex.Message.Contains(expectedMessageContains))
                {
                    Assert.Fail($"Exception message should contain '{expectedMessageContains}', " +
                                $"but was: {ex.Message}");
                }
                return ex;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }
    }
}