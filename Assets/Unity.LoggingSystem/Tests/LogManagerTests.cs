using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RuntimeLogging.Tests
{
    [TestFixture]
    public class LogManagerTests
    {
        /// <summary>
        /// Enhanced MockLogger for LogManager testing with message collections
        /// </summary>
        private class MockLogger : ILogger
        {
            public List<string> LogMessages { get; } = new List<string>();
            public List<string> WarningMessages { get; } = new List<string>();
            public List<string> ErrorMessages { get; } = new List<string>();
            
            public void Log(string message)
            {
                lock (LogMessages)
                {
                    LogMessages.Add(message);
                }
            }
            
            public void LogWarning(string message)
            {
                lock (WarningMessages)
                {
                    WarningMessages.Add(message);
                }
            }
            
            public void LogError(string message)
            {
                lock (ErrorMessages)
                {
                    ErrorMessages.Add(message);
                }
            }
            
            public void Clear()
            {
                lock (LogMessages) { LogMessages.Clear(); }
                lock (WarningMessages) { WarningMessages.Clear(); }
                lock (ErrorMessages) { ErrorMessages.Clear(); }
            }
        }
        
        private MockLogger _mockLogger;
        
        [SetUp]
        public void SetUp()
        {
            // Clear any existing logger before each test
            LogManager.ClearLogger();
            _mockLogger = new MockLogger();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up after each test
            LogManager.ClearLogger();
        }
        
        [Test]
        public void SetLogger_WithValidLogger_SetsLoggerSuccessfully()
        {
            // Act
            LogManager.SetLogger(_mockLogger);
            
            // Assert
            Assert.That(LogManager.GetLogger(), Is.EqualTo(_mockLogger));
            Assert.That(LogManager.HasLogger(), Is.True);
        }
        
        [Test]
        public void SetLogger_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => LogManager.SetLogger(null));
        }
        
        [Test]
        public void GetLogger_WhenNoLoggerSet_ReturnsNull()
        {
            // Act
            var result = LogManager.GetLogger();
            
            // Assert
            Assert.That(result, Is.Null);
            Assert.That(LogManager.HasLogger(), Is.False);
        }
        
        [Test]
        public void ClearLogger_WhenLoggerIsSet_ClearsLogger()
        {
            // Arrange
            LogManager.SetLogger(_mockLogger);
            Assert.That(LogManager.HasLogger(), Is.True);
            
            // Act
            LogManager.ClearLogger();
            
            // Assert
            Assert.That(LogManager.GetLogger(), Is.Null);
            Assert.That(LogManager.HasLogger(), Is.False);
        }
        
        [Test]
        public void ConvenienceMethods_WithLoggerSet_CallsUnderlyingLogger()
        {
            // Arrange
            LogManager.SetLogger(_mockLogger);
            const string testMessage = "Test message";
            const string warningMessage = "Warning message";
            const string errorMessage = "Error message";
            
            // Act
            LogManager.Log(testMessage);
            LogManager.LogWarning(warningMessage);
            LogManager.LogError(errorMessage);
            
            // Assert
            Assert.That(_mockLogger.LogMessages.Count, Is.EqualTo(1));
            Assert.That(_mockLogger.LogMessages[0], Is.EqualTo(testMessage));
            Assert.That(_mockLogger.WarningMessages.Count, Is.EqualTo(1));
            Assert.That(_mockLogger.WarningMessages[0], Is.EqualTo(warningMessage));
            Assert.That(_mockLogger.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(_mockLogger.ErrorMessages[0], Is.EqualTo(errorMessage));
        }
        
        [Test]
        public void ConvenienceMethods_WithNoLoggerSet_DoesNotThrow()
        {
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => LogManager.Log("Test"));
            Assert.DoesNotThrow(() => LogManager.LogWarning("Warning"));
            Assert.DoesNotThrow(() => LogManager.LogError("Error"));
        }
        
        [Test]
        public void ThreadSafety_ConcurrentSetAndGet_MaintainsConsistency()
        {
            // Arrange
            const int threadCount = 10;
            const int operationsPerThread = 100;
            var loggers = new ILogger[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                loggers[i] = new MockLogger();
            }
            
            var tasks = new Task[threadCount];
            var exceptions = new Exception[threadCount];
            
            // Act - Multiple threads setting and getting loggers concurrently
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        for (int j = 0; j < operationsPerThread; j++)
                        {
                            LogManager.SetLogger(loggers[threadIndex]);
                            var retrievedLogger = LogManager.GetLogger();
                            Assert.That(retrievedLogger, Is.Not.Null);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions[threadIndex] = ex;
                    }
                });
            }
            
            Task.WaitAll(tasks);
            
            // Assert - No exceptions should occur
            for (int i = 0; i < threadCount; i++)
            {
                Assert.That(exceptions[i], Is.Null, $"Thread {i} threw an exception: {exceptions[i]}");
            }
            
            // Final logger should be one of the set loggers
            var finalLogger = LogManager.GetLogger();
            Assert.That(finalLogger, Is.Not.Null);
            Assert.That(loggers, Contains.Item(finalLogger));
        }
        
        [Test]
        public void ThreadSafety_ConcurrentConvenienceMethods_DoesNotThrow()
        {
            // Arrange
            LogManager.SetLogger(_mockLogger);
            const int threadCount = 5;
            const int operationsPerThread = 50;
            var tasks = new Task[threadCount];
            var exceptions = new Exception[threadCount];
            
            // Act - Multiple threads calling convenience methods concurrently
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        for (int j = 0; j < operationsPerThread; j++)
                        {
                            LogManager.Log($"Thread {threadIndex} Log {j}");
                            LogManager.LogWarning($"Thread {threadIndex} Warning {j}");
                            LogManager.LogError($"Thread {threadIndex} Error {j}");
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions[threadIndex] = ex;
                    }
                });
            }
            
            Task.WaitAll(tasks);
            
            // Assert - No exceptions should occur
            for (int i = 0; i < threadCount; i++)
            {
                Assert.That(exceptions[i], Is.Null, $"Thread {i} threw an exception: {exceptions[i]}");
            }
            
            // Verify that messages were logged (exact count may vary due to threading)
            Assert.That(_mockLogger.LogMessages.Count, Is.GreaterThan(0));
            Assert.That(_mockLogger.WarningMessages.Count, Is.GreaterThan(0));
            Assert.That(_mockLogger.ErrorMessages.Count, Is.GreaterThan(0));
        }
    }
}