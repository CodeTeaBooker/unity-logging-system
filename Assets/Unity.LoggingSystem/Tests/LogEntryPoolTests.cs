using NUnit.Framework;
using System;
using RuntimeLogging.Tests.TestUtilities;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Tests for LogEntryPool object pooling functionality
    /// Validates efficient log entry reuse and garbage collection reduction
    /// </summary>
    [Category("Performance")]
    public class LogEntryPoolTests
    {
        private LogEntryPool pool;
        
        [SetUp]
        public void SetUp()
        {
            pool = new LogEntryPool();
        }
        
        [TearDown]
        public void TearDown()
        {
            pool?.Clear();
        }
        
        #region Get Method Tests
        
        [Test]
        public void Get_WithValidParameters_ReturnsLogEntry()
        {
            // Arrange
            string message = "Test message";
            LogLevel level = LogLevel.Info;
            DateTime timestamp = DateTime.Now;
            
            // Act
            var result = pool.Get(message, level, timestamp);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.message, Is.EqualTo(message), "Message should match input");
                Assert.That(result.level, Is.EqualTo(level), "Level should match input");
                Assert.That(result.timestamp, Is.EqualTo(timestamp), "Timestamp should match input");
            });
        }
        
        [Test]
        public void Get_WithStackTrace_ReturnsLogEntryWithStackTrace()
        {
            // Arrange
            string message = "Test message";
            LogLevel level = LogLevel.Error;
            DateTime timestamp = DateTime.Now;
            string stackTrace = "Stack trace info";
            
            // Act
            var result = pool.Get(message, level, timestamp, stackTrace);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.message, Is.EqualTo(message), "Message should match input");
                Assert.That(result.level, Is.EqualTo(level), "Level should match input");
                Assert.That(result.timestamp, Is.EqualTo(timestamp), "Timestamp should match input");
                Assert.That(result.stackTrace, Is.EqualTo(stackTrace), "Stack trace should match input");
            });
        }
        
        [Test]
        public void Get_FromEmptyPool_CreatesNewEntry()
        {
            // Arrange
            string message = "New entry";
            LogLevel level = LogLevel.Warning;
            DateTime timestamp = DateTime.Now;
            
            // Act
            var result = pool.Get(message, level, timestamp);
            var stats = pool.GetStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.message, Is.EqualTo(message), "Should create new entry with correct message");
                Assert.That(stats.TotalAllocations, Is.EqualTo(1), "Should track allocation");
                Assert.That(stats.TotalReuses, Is.EqualTo(0), "Should not count as reuse");
            });
        }
        
        [Test]
        public void Get_FromPoolWithEntries_ReusesEntry()
        {
            // Arrange
            var entry1 = pool.Get("First", LogLevel.Info, DateTime.Now);
            pool.Return(entry1);
            
            string message = "Reused entry";
            LogLevel level = LogLevel.Error;
            DateTime timestamp = DateTime.Now;
            
            // Act
            var result = pool.Get(message, level, timestamp);
            var stats = pool.GetStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.message, Is.EqualTo(message), "Should reuse entry with new message");
                Assert.That(result.level, Is.EqualTo(level), "Should reuse entry with new level");
                Assert.That(stats.TotalReuses, Is.EqualTo(1), "Should count as reuse");
            });
        }
        
        #endregion
        
        #region Return Method Tests
        
        [Test]
        public void Return_WithValidEntry_AddsToPool()
        {
            // Arrange
            var entry = pool.Get("Test", LogLevel.Info, DateTime.Now);
            int initialPoolSize = pool.GetCurrentPoolSize();
            
            // Act
            pool.Return(entry);
            
            // Assert
            Assert.That(pool.GetCurrentPoolSize(), Is.EqualTo(initialPoolSize + 1),
                "Pool size should increase after returning entry");
        }
        
        [Test]
        public void Return_WhenPoolAtCapacity_DoesNotExceedLimit()
        {
            // Arrange
            pool.SetMaxPoolSize(2);
            var entry1 = pool.Get("Test1", LogLevel.Info, DateTime.Now);
            var entry2 = pool.Get("Test2", LogLevel.Info, DateTime.Now);
            var entry3 = pool.Get("Test3", LogLevel.Info, DateTime.Now);
            
            // Act
            pool.Return(entry1);
            pool.Return(entry2);
            pool.Return(entry3); // This should not increase pool size beyond limit
            
            // Assert
            Assert.That(pool.GetCurrentPoolSize(), Is.LessThanOrEqualTo(2),
                "Pool should not exceed maximum size");
        }
        
        [Test]
        public void Return_MultipleEntries_MaintainsPoolIntegrity()
        {
            // Arrange
            var entries = new LogEntry[5];
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = pool.Get($"Test {i}", LogLevel.Info, DateTime.Now);
            }
            
            // Act
            foreach (var entry in entries)
            {
                pool.Return(entry);
            }
            
            // Assert
            Assert.That(pool.GetCurrentPoolSize(), Is.EqualTo(entries.Length),
                "Pool should contain all returned entries");
        }
        
        #endregion
        
        #region Pool Size Management Tests
        
        [Test]
        public void SetMaxPoolSize_WithValidSize_UpdatesLimit()
        {
            // Arrange
            int newMaxSize = 50;
            
            // Act
            pool.SetMaxPoolSize(newMaxSize);
            
            // Assert
            Assert.That(pool.GetMaxPoolSize(), Is.EqualTo(newMaxSize),
                "Max pool size should be updated");
        }
        
        [Test]
        public void SetMaxPoolSize_WithTooSmallValue_ClampsToMinimum()
        {
            // Arrange
            int tooSmallSize = 0;
            
            // Act
            pool.SetMaxPoolSize(tooSmallSize);
            
            // Assert
            Assert.That(pool.GetMaxPoolSize(), Is.GreaterThanOrEqualTo(1),
                "Pool size should be clamped to minimum value");
        }
        
        [Test]
        public void SetMaxPoolSize_SmallerThanCurrent_TrimsPool()
        {
            // Arrange
            pool.SetMaxPoolSize(10);
            for (int i = 0; i < 10; i++)
            {
                var entry = pool.Get($"Test {i}", LogLevel.Info, DateTime.Now);
                pool.Return(entry);
            }
            
            // Act
            pool.SetMaxPoolSize(5);
            
            // Assert
            Assert.That(pool.GetCurrentPoolSize(), Is.LessThanOrEqualTo(5),
                "Pool should be trimmed to new maximum size");
        }
        
        #endregion
        
        #region Statistics Tests
        
        [Test]
        public void GetStats_InitialState_ReturnsZeroStats()
        {
            // Arrange & Act
            var stats = pool.GetStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.CurrentPoolSize, Is.EqualTo(0), "Initial pool size should be zero");
                Assert.That(stats.TotalAllocations, Is.EqualTo(0), "Initial allocations should be zero");
                Assert.That(stats.TotalReuses, Is.EqualTo(0), "Initial reuses should be zero");
                Assert.That(stats.ReuseRatio, Is.EqualTo(0f), "Initial reuse ratio should be zero");
            });
        }
        
        [Test]
        public void GetStats_AfterOperations_ReturnsAccurateStats()
        {
            // Arrange
            var entry1 = pool.Get("Test1", LogLevel.Info, DateTime.Now);
            var entry2 = pool.Get("Test2", LogLevel.Info, DateTime.Now);
            pool.Return(entry1);
            var entry3 = pool.Get("Test3", LogLevel.Info, DateTime.Now); // Should reuse entry1
            
            // Act
            var stats = pool.GetStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.TotalAllocations, Is.EqualTo(2), "Should track new allocations");
                Assert.That(stats.TotalReuses, Is.EqualTo(1), "Should track reuses");
                Assert.That(stats.ReuseRatio, Is.GreaterThan(0f), "Reuse ratio should be calculated");
            });
        }
        
        [Test]
        public void GetStats_ReuseRatioCalculation_IsAccurate()
        {
            // Arrange
            var entry1 = pool.Get("Test1", LogLevel.Info, DateTime.Now); // Allocation 1
            pool.Return(entry1);
            var entry2 = pool.Get("Test2", LogLevel.Info, DateTime.Now); // Reuse 1
            var entry3 = pool.Get("Test3", LogLevel.Info, DateTime.Now); // Allocation 2
            
            // Act
            var stats = pool.GetStats();
            float expectedRatio = 1f / 3f; // 1 reuse out of 3 total operations
            
            // Assert
            Assert.That(stats.ReuseRatio, Is.EqualTo(expectedRatio).Within(0.01f),
                $"Reuse ratio should be {expectedRatio:F2}");
        }
        
        #endregion
        
        #region Clear and Reset Tests
        
        [Test]
        public void Clear_WithEntriesInPool_EmptiesPool()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                var entry = pool.Get($"Test {i}", LogLevel.Info, DateTime.Now);
                pool.Return(entry);
            }
            
            // Act
            pool.Clear();
            
            // Assert
            Assert.That(pool.GetCurrentPoolSize(), Is.EqualTo(0),
                "Pool should be empty after clear");
        }
        
        [Test]
        public void ResetStats_AfterOperations_ClearsStatistics()
        {
            // Arrange
            var entry = pool.Get("Test", LogLevel.Info, DateTime.Now);
            pool.Return(entry);
            pool.Get("Test2", LogLevel.Info, DateTime.Now);
            
            // Act
            pool.ResetStats();
            var stats = pool.GetStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.TotalAllocations, Is.EqualTo(0), "Allocations should be reset");
                Assert.That(stats.TotalReuses, Is.EqualTo(0), "Reuses should be reset");
                Assert.That(stats.ReuseRatio, Is.EqualTo(0f), "Reuse ratio should be reset");
            });
        }
        
        #endregion
        
        #region Thread Safety Tests
        
        [Test]
        public void ConcurrentOperations_MultipleThreads_MaintainsIntegrity()
        {
            // Arrange
            const int operationCount = 100;
            var entries = new LogEntry[operationCount];
            
            // Act - Simulate concurrent get operations
            System.Threading.Tasks.Parallel.For(0, operationCount, i =>
            {
                entries[i] = pool.Get($"Concurrent test {i}", LogLevel.Info, DateTime.Now);
            });
            
            // Return all entries
            System.Threading.Tasks.Parallel.For(0, operationCount, i =>
            {
                pool.Return(entries[i]);
            });
            
            // Assert
            var stats = pool.GetStats();
            Assert.That(stats.TotalAllocations + stats.TotalReuses, Is.EqualTo(operationCount),
                "Total operations should match expected count");
        }
        
        #endregion
        
        #region Edge Cases Tests
        
        [Test]
        public void Get_WithNullMessage_HandlesGracefully()
        {
            // Arrange
            string nullMessage = null;
            LogLevel level = LogLevel.Info;
            DateTime timestamp = DateTime.Now;
            
            // Act & Assert
            Assert.DoesNotThrow(() => pool.Get(nullMessage, level, timestamp),
                "Pool should handle null message gracefully");
        }
        
        [Test]
        public void Get_WithEmptyMessage_HandlesGracefully()
        {
            // Arrange
            string emptyMessage = string.Empty;
            LogLevel level = LogLevel.Info;
            DateTime timestamp = DateTime.Now;
            
            // Act
            var result = pool.Get(emptyMessage, level, timestamp);
            
            // Assert
            Assert.That(result.message, Is.EqualTo(emptyMessage),
                "Pool should handle empty message correctly");
        }
        
        [Test]
        public void ToString_OnStats_ReturnsFormattedString()
        {
            // Arrange
            var entry = pool.Get("Test", LogLevel.Info, DateTime.Now);
            pool.Return(entry);
            pool.Get("Test2", LogLevel.Info, DateTime.Now);
            
            // Act
            var stats = pool.GetStats();
            string result = stats.ToString();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Pool:"), "Should contain pool information");
                Assert.That(result, Does.Contain("Allocations:"), "Should contain allocation information");
                Assert.That(result, Does.Contain("Reuses:"), "Should contain reuse information");
                Assert.That(result, Does.Contain("Reuse Ratio:"), "Should contain reuse ratio information");
            });
        }
        
        #endregion
    }
}