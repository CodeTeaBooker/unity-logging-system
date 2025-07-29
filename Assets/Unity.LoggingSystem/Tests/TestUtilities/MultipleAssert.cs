using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeLogging.Tests.TestUtilities
{
    public static class MultipleAssert
    {
        public static void Multiple(Action assertions)
        {
            if (assertions == null)
                throw new ArgumentNullException(nameof(assertions));

            var failures = new List<string>();
            try
            {
                assertions();
            }
            catch (AssertionException ex)
            {
                failures.Add(ex.Message);
            }

            if (failures.Count > 0)
            {
                var message = $"Multiple assertions failed ({failures.Count} failures):\n" +
                    string.Join("\n", failures.Select((f, i) => $"[{i + 1}] {f}"));
                Assert.Fail(message);
            }
        }
    }
}