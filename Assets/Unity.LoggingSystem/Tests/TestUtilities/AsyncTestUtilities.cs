using System;
using System.Collections;
using System.Threading.Tasks;

namespace RuntimeLogging.Tests.TestUtilities
{
    public static class AsyncTestUtilities
    {
        public static IEnumerator RunAsyncTest(Func<Task> testMethod, int timeoutMilliseconds = 5000)
        {
            var task = testMethod();
            var start = DateTime.UtcNow;

            while (!task.IsCompleted)
            {
                if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMilliseconds)
                    throw new TimeoutException($"Async test timed out ({timeoutMilliseconds} ms)");
                yield return null;
            }

            if (task.IsFaulted)
                throw task.Exception?.GetBaseException() ?? new Exception("Async task failed");
        }
    }
}