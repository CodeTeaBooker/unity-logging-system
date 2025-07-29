using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using RuntimeLogging.Tests.TestUtilities;
using UnityEngine.TestTools;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Comprehensive tests for CompositeLogger class covering composite behavior and error handling
    /// Tests Requirements: 8.4, 9.1, 5.2
    /// </summary>
    [TestFixture]
    public class CompositeLoggerTests
    {
        private MockLogger mockLogger1;
        private MockLogger mockLogger2;
        private MockLogger mockLogger3;
        private CompositeLogger compositeLogger;
        
        [SetUp]
        public void SetUp()
        {
            mockLogger1 = new MockLogger("Logger1");
            mockLogger2 = new MockLogger("Logger2");
            mockLogger3 = new MockLogger("Logger3");
        }
        
        [TearDown]
        public void TearDown()
        {
            compositeLogger = null;
            mockLogger1 = null;
            mockLogger2 = null;
            mockLogger3 = null;
        }
        
        #region Constructor Tests
        
        [Test]
        public void Constructor_WithNoLoggers_CreatesEmptyComposite()
        {
            // Arrange & Act
            compositeLogger = new CompositeLogger();
            
            // Assert
            Assert.That(compositeLogger, Is.Not.Null, "Constructor should create valid instance");
            Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(0), "Empty constructor should create composite with no loggers");
        }
        
        [Test]
        public void Constructor_WithNullArray_CreatesEmptyComposite()
        {
            // Arrange
            ILogger[] nullArray = null;
            
            // Act
            compositeLogger = new CompositeLogger(nullArray);
            
            // Assert
            Assert.That(compositeLogger, Is.Not.Null, "Constructor should handle null array gracefully");
            Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(0), "Null array should result in empty composite");
        }
        
        [Test]
        public void Constructor_WithValidLoggers_AddsAllLoggers()
        {
            // Arrange & Act
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2, mockLogger3);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(3), "Should add all provided loggers");
                Assert.That(compositeLogger.ContainsLogger(mockLogger1), Is.True, "Should contain first logger");
                Assert.That(compositeLogger.ContainsLogger(mockLogger2), Is.True, "Should contain second logger");
                Assert.That(compositeLogger.ContainsLogger(mockLogger3), Is.True, "Should contain third logger");
            });
        }
        
        [Test]
        public void Constructor_WithNullLoggersInArray_SkipsNullLoggers()
        {
            // Arrange & Act
            compositeLogger = new CompositeLogger(mockLogger1, null, mockLogger2, null);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(2), "Should skip null loggers");
                Assert.That(compositeLogger.ContainsLogger(mockLogger1), Is.True, "Should contain first valid logger");
                Assert.That(compositeLogger.ContainsLogger(mockLogger2), Is.True, "Should contain second valid logger");
            });
        }
        
        [Test]
        public void Constructor_WithEnumerableLoggers_AddsAllLoggers()
        {
            // Arrange
            var loggerList = new List<ILogger> { mockLogger1, mockLogger2 };
            
            // Act
            compositeLogger = new CompositeLogger(loggerList);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(2), "Should add all loggers from enumerable");
                Assert.That(compositeLogger.ContainsLogger(mockLogger1), Is.True, "Should contain first logger from enumerable");
                Assert.That(compositeLogger.ContainsLogger(mockLogger2), Is.True, "Should contain second logger from enumerable");
            });
        }
        
        #endregion
        
        #region Logging Method Tests
        
        [Test]
        public void Log_WithMultipleLoggers_CallsAllLoggers()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2, mockLogger3);
            var testMessage = "Test info message";
            
            // Act
            compositeLogger.Log(testMessage);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(mockLogger1.LogMessages.Count, Is.EqualTo(1), "First logger should receive log call");
                Assert.That(mockLogger1.LogMessages[0], Is.EqualTo(testMessage), "First logger should receive correct message");
                Assert.That(mockLogger2.LogMessages.Count, Is.EqualTo(1), "Second logger should receive log call");
                Assert.That(mockLogger2.LogMessages[0], Is.EqualTo(testMessage), "Second logger should receive correct message");
                Assert.That(mockLogger3.LogMessages.Count, Is.EqualTo(1), "Third logger should receive log call");
                Assert.That(mockLogger3.LogMessages[0], Is.EqualTo(testMessage), "Third logger should receive correct message");
            });
        }
        
        [Test]
        public void LogWarning_WithMultipleLoggers_CallsAllLoggers()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2);
            var testMessage = "Test warning message";
            
            // Act
            compositeLogger.LogWarning(testMessage);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(mockLogger1.WarningMessages.Count, Is.EqualTo(1), "First logger should receive warning call");
                Assert.That(mockLogger1.WarningMessages[0], Is.EqualTo(testMessage), "First logger should receive correct warning message");
                Assert.That(mockLogger2.WarningMessages.Count, Is.EqualTo(1), "Second logger should receive warning call");
                Assert.That(mockLogger2.WarningMessages[0], Is.EqualTo(testMessage), "Second logger should receive correct warning message");
            });
        }
        
        [Test]
        public void LogError_WithMultipleLoggers_CallsAllLoggers()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2);
            var testMessage = "Test error message";
            
            // Act
            compositeLogger.LogError(testMessage);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(mockLogger1.ErrorMessages.Count, Is.EqualTo(1), "First logger should receive error call");
                Assert.That(mockLogger1.ErrorMessages[0], Is.EqualTo(testMessage), "First logger should receive correct error message");
                Assert.That(mockLogger2.ErrorMessages.Count, Is.EqualTo(1), "Second logger should receive error call");
                Assert.That(mockLogger2.ErrorMessages[0], Is.EqualTo(testMessage), "Second logger should receive correct error message");
            });
        }
        
        [Test]
        public void Log_WithNoLoggers_DoesNotThrow()
        {
            // Arrange
            compositeLogger = new CompositeLogger();
            var testMessage = "Test message";
            
            // Act & Assert
            Assert.DoesNotThrow(() => compositeLogger.Log(testMessage),
                "Logging with no loggers should not throw exception");
        }
        
        #endregion
        
        #region Error Isolation Tests
        
        [Test]
        public void Log_WithOneFailingLogger_ContinuesWithOtherLoggers()
        {
            // Arrange
            var failingLogger = new FailingMockLogger("FailingLogger");
            compositeLogger = new CompositeLogger(mockLogger1, failingLogger, mockLogger2);
            var testMessage = "Test message";
            
            bool errorEventFired = false;
            ILogger failedLogger = null;
            Exception capturedException = null;
            
            compositeLogger.OnLoggerFailed += (logger, ex) =>
            {
                errorEventFired = true;
                failedLogger = logger;
                capturedException = ex;
            };
            
            // Expect the Unity error log that CompositeLogger will generate
            LogAssert.Expect(UnityEngine.LogType.Error, "CompositeLogger: Logger FailingMockLogger failed with error: FailingMockLogger FailingLogger failed on Log");
            
            // Act
            compositeLogger.Log(testMessage);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(errorEventFired, Is.True, "Error event should be fired when logger fails");
                Assert.That(failedLogger, Is.EqualTo(failingLogger), "Failed logger should be reported in event");
                Assert.That(capturedException, Is.Not.Null, "Exception should be captured and reported");
                Assert.That(mockLogger1.LogMessages.Count, Is.EqualTo(1), "First logger should still receive message despite other logger failure");
                Assert.That(mockLogger2.LogMessages.Count, Is.EqualTo(1), "Third logger should still receive message despite other logger failure");
                Assert.That(mockLogger1.LogMessages[0], Is.EqualTo(testMessage), "First logger should receive correct message");
                Assert.That(mockLogger2.LogMessages[0], Is.EqualTo(testMessage), "Third logger should receive correct message");
            });
        }
        
        [Test]
        public void LogWarning_WithFailingLogger_IsolatesError()
        {
            // Arrange
            var failingLogger = new FailingMockLogger("FailingLogger");
            compositeLogger = new CompositeLogger(mockLogger1, failingLogger);
            var testMessage = "Test warning";
            
            // Expect the Unity error log that CompositeLogger will generate
            LogAssert.Expect(UnityEngine.LogType.Error, "CompositeLogger: Logger FailingMockLogger failed with error: FailingMockLogger FailingLogger failed on LogWarning");
            
            // Act & Assert
            Assert.DoesNotThrow(() => compositeLogger.LogWarning(testMessage),
                "Composite logger should isolate failing logger errors");
            Assert.That(mockLogger1.WarningMessages.Count, Is.EqualTo(1), 
                "Working logger should still receive message despite other logger failure");
        }
        
        [Test]
        public void LogError_WithFailingLogger_IsolatesError()
        {
            // Arrange
            var failingLogger = new FailingMockLogger("FailingLogger");
            compositeLogger = new CompositeLogger(mockLogger1, failingLogger);
            var testMessage = "Test error";
            
            // Expect the Unity error log that CompositeLogger will generate
            LogAssert.Expect(UnityEngine.LogType.Error, "CompositeLogger: Logger FailingMockLogger failed with error: FailingMockLogger FailingLogger failed on LogError");
            
            // Act & Assert
            Assert.DoesNotThrow(() => compositeLogger.LogError(testMessage),
                "Composite logger should isolate failing logger errors");
            Assert.That(mockLogger1.ErrorMessages.Count, Is.EqualTo(1), 
                "Working logger should still receive message despite other logger failure");
        }
        
        #endregion
        
        #region Runtime Logger Management Tests
        
        [Test]
        public void AddLogger_WithValidLogger_AddsSuccessfully()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1);
            bool addEventFired = false;
            ILogger addedLogger = null;
            
            compositeLogger.OnLoggerAdded += (logger) =>
            {
                addEventFired = true;
                addedLogger = logger;
            };
            
            // Act
            var result = compositeLogger.AddLogger(mockLogger2);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.True, "AddLogger should return true for successful addition");
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(2), "Logger count should increase after adding");
                Assert.That(compositeLogger.ContainsLogger(mockLogger2), Is.True, "Composite should contain newly added logger");
                Assert.That(addEventFired, Is.True, "OnLoggerAdded event should be fired");
                Assert.That(addedLogger, Is.EqualTo(mockLogger2), "Event should report correct added logger");
            });
        }
        
        [Test]
        public void AddLogger_WithNullLogger_ReturnsFalse()
        {
            // Arrange
            compositeLogger = new CompositeLogger();
            
            // Act
            var result = compositeLogger.AddLogger(null);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.False, "AddLogger should return false for null logger");
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(0), "Logger count should remain unchanged");
            });
        }
        
        [Test]
        public void AddLogger_WithDuplicateLogger_ReturnsFalse()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1);
            
            // Act
            var result = compositeLogger.AddLogger(mockLogger1);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.False, "AddLogger should return false for duplicate logger");
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(1), "Logger count should remain unchanged for duplicate");
            });
        }
        
        [Test]
        public void RemoveLogger_WithExistingLogger_RemovesSuccessfully()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2);
            bool removeEventFired = false;
            ILogger removedLogger = null;
            
            compositeLogger.OnLoggerRemoved += (logger) =>
            {
                removeEventFired = true;
                removedLogger = logger;
            };
            
            // Act
            var result = compositeLogger.RemoveLogger(mockLogger1);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.True, "RemoveLogger should return true for successful removal");
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(1), "Logger count should decrease after removal");
                Assert.That(compositeLogger.ContainsLogger(mockLogger1), Is.False, "Composite should not contain removed logger");
                Assert.That(compositeLogger.ContainsLogger(mockLogger2), Is.True, "Composite should still contain other loggers");
                Assert.That(removeEventFired, Is.True, "OnLoggerRemoved event should be fired");
                Assert.That(removedLogger, Is.EqualTo(mockLogger1), "Event should report correct removed logger");
            });
        }
        
        [Test]
        public void RemoveLogger_WithNonExistentLogger_ReturnsFalse()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1);
            
            // Act
            var result = compositeLogger.RemoveLogger(mockLogger2);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.False, "RemoveLogger should return false for non-existent logger");
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(1), "Logger count should remain unchanged");
            });
        }
        
        [Test]
        public void RemoveLogger_WithNullLogger_ReturnsFalse()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1);
            
            // Act
            var result = compositeLogger.RemoveLogger(null);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.False, "RemoveLogger should return false for null logger");
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(1), "Logger count should remain unchanged");
            });
        }
        
        #endregion
        
        #region Type-Based Logger Management Tests
        
        [Test]
        public void RemoveLoggersOfType_WithExistingType_RemovesAllOfType()
        {
            // Arrange
            var unityLogger1 = new UnityLogger();
            var unityLogger2 = new UnityLogger();
            compositeLogger = new CompositeLogger(mockLogger1, unityLogger1, mockLogger2, unityLogger2);
            
            // Act
            var removedCount = compositeLogger.RemoveLoggersOfType<UnityLogger>();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(removedCount, Is.EqualTo(2), "Should remove all loggers of specified type");
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(2), "Should only remove loggers of specified type");
                Assert.That(compositeLogger.ContainsLoggerOfType<UnityLogger>(), Is.False, "Should not contain any loggers of removed type");
                Assert.That(compositeLogger.ContainsLogger(mockLogger1), Is.True, "Should preserve loggers of other types");
                Assert.That(compositeLogger.ContainsLogger(mockLogger2), Is.True, "Should preserve loggers of other types");
            });
        }
        
        [Test]
        public void RemoveLoggersOfType_WithNonExistentType_ReturnsZero()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2);
            
            // Act
            var removedCount = compositeLogger.RemoveLoggersOfType<UnityLogger>();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(removedCount, Is.EqualTo(0), "Should return zero when no loggers of type exist");
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(2), "Should not affect existing loggers");
            });
        }
        
        [Test]
        public void ContainsLoggerOfType_WithExistingType_ReturnsTrue()
        {
            // Arrange
            var unityLogger = new UnityLogger();
            compositeLogger = new CompositeLogger(mockLogger1, unityLogger);
            
            // Act
            var result = compositeLogger.ContainsLoggerOfType<UnityLogger>();
            
            // Assert
            Assert.That(result, Is.True, "Should return true when logger of specified type exists");
        }
        
        [Test]
        public void ContainsLoggerOfType_WithNonExistentType_ReturnsFalse()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2);
            
            // Act
            var result = compositeLogger.ContainsLoggerOfType<UnityLogger>();
            
            // Assert
            Assert.That(result, Is.False, "Should return false when no logger of specified type exists");
        }
        
        [Test]
        public void GetLoggersOfType_WithExistingType_ReturnsAllOfType()
        {
            // Arrange
            var unityLogger1 = new UnityLogger();
            var unityLogger2 = new UnityLogger();
            compositeLogger = new CompositeLogger(mockLogger1, unityLogger1, unityLogger2);
            
            // Act
            var unityLoggers = compositeLogger.GetLoggersOfType<UnityLogger>();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(unityLoggers.Count, Is.EqualTo(2), "Should return all loggers of specified type");
                Assert.That(unityLoggers.Contains(unityLogger1), Is.True, "Should contain first logger of type");
                Assert.That(unityLoggers.Contains(unityLogger2), Is.True, "Should contain second logger of type");
            });
        }
        
        [Test]
        public void GetLoggersOfType_WithNonExistentType_ReturnsEmptyList()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2);
            
            // Act
            var unityLoggers = compositeLogger.GetLoggersOfType<UnityLogger>();
            
            // Assert
            Assert.That(unityLoggers.Count, Is.EqualTo(0), "Should return empty list when no loggers of type exist");
        }
        
        #endregion
        
        #region Collection Management Tests
        
        [Test]
        public void ClearLoggers_WithMultipleLoggers_RemovesAllLoggers()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2, mockLogger3);
            var removedLoggers = new List<ILogger>();
            
            compositeLogger.OnLoggerRemoved += (logger) => removedLoggers.Add(logger);
            
            // Act
            compositeLogger.ClearLoggers();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(0), "Should remove all loggers");
                Assert.That(removedLoggers.Count, Is.EqualTo(3), "Should fire removal event for each logger");
                Assert.That(removedLoggers.Contains(mockLogger1), Is.True, "Should report removal of first logger");
                Assert.That(removedLoggers.Contains(mockLogger2), Is.True, "Should report removal of second logger");
                Assert.That(removedLoggers.Contains(mockLogger3), Is.True, "Should report removal of third logger");
            });
        }
        
        [Test]
        public void GetLoggers_WithMultipleLoggers_ReturnsReadOnlyList()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2);
            
            // Act
            var loggers = compositeLogger.GetLoggers();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(loggers.Count, Is.EqualTo(2), "Should return all loggers");
                Assert.That(loggers.Contains(mockLogger1), Is.True, "Should contain first logger");
                Assert.That(loggers.Contains(mockLogger2), Is.True, "Should contain second logger");
                Assert.That(loggers, Is.InstanceOf<IReadOnlyList<ILogger>>(), "Should return read-only collection");
            });
        }
        
        [Test]
        public void ContainsLogger_WithExistingLogger_ReturnsTrue()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2);
            
            // Act
            var result = compositeLogger.ContainsLogger(mockLogger1);
            
            // Assert
            Assert.That(result, Is.True, "Should return true for existing logger");
        }
        
        [Test]
        public void ContainsLogger_WithNonExistentLogger_ReturnsFalse()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1);
            
            // Act
            var result = compositeLogger.ContainsLogger(mockLogger2);
            
            // Assert
            Assert.That(result, Is.False, "Should return false for non-existent logger");
        }
        
        #endregion
        
        #region Performance Statistics Tests
        
        [Test]
        public void GetPerformanceStats_WithMultipleLoggerTypes_ReturnsCorrectStats()
        {
            // Arrange
            var unityLogger1 = new UnityLogger();
            var unityLogger2 = new UnityLogger();
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2, unityLogger1, unityLogger2);
            
            // Act
            var stats = compositeLogger.GetPerformanceStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.TotalLoggerCount, Is.EqualTo(4), "Should report correct total logger count");
                Assert.That(stats.LoggerTypes.ContainsKey("MockLogger"), Is.True, "Should track MockLogger type");
                Assert.That(stats.LoggerTypes.ContainsKey("UnityLogger"), Is.True, "Should track UnityLogger type");
                Assert.That(stats.LoggerTypes["MockLogger"], Is.EqualTo(2), "Should count MockLogger instances correctly");
                Assert.That(stats.LoggerTypes["UnityLogger"], Is.EqualTo(2), "Should count UnityLogger instances correctly");
            });
        }
        
        [Test]
        public void GetPerformanceStats_WithEmptyComposite_ReturnsZeroStats()
        {
            // Arrange
            compositeLogger = new CompositeLogger();
            
            // Act
            var stats = compositeLogger.GetPerformanceStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.TotalLoggerCount, Is.EqualTo(0), "Should report zero loggers");
                Assert.That(stats.LoggerTypes.Count, Is.EqualTo(0), "Should have empty logger types dictionary");
            });
        }
        
        #endregion
        
        #region Thread Safety Tests
        
        [Test]
        public void AddLogger_ConcurrentAccess_ThreadSafe()
        {
            // Arrange
            compositeLogger = new CompositeLogger();
            var loggers = new List<ILogger>();
            for (int i = 0; i < 10; i++)
            {
                loggers.Add(new MockLogger($"Logger{i}"));
            }
            
            // Act
            var tasks = loggers.Select(logger => 
                System.Threading.Tasks.Task.Run(() => compositeLogger.AddLogger(logger))
            ).ToArray();
            
            System.Threading.Tasks.Task.WaitAll(tasks);
            
            // Assert
            Assert.That(compositeLogger.GetLoggerCount(), Is.EqualTo(10), 
                "All loggers should be added safely in concurrent scenario");
        }
        
        [Test]
        public void Log_ConcurrentAccess_ThreadSafe()
        {
            // Arrange
            compositeLogger = new CompositeLogger(mockLogger1, mockLogger2);
            var testMessage = "Concurrent test message";
            
            // Act
            var tasks = Enumerable.Range(0, 10).Select(_ => 
                System.Threading.Tasks.Task.Run(() => compositeLogger.Log(testMessage))
            ).ToArray();
            
            System.Threading.Tasks.Task.WaitAll(tasks);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(mockLogger1.LogMessages.Count, Is.EqualTo(10), 
                    "First logger should receive all concurrent messages");
                Assert.That(mockLogger2.LogMessages.Count, Is.EqualTo(10), 
                    "Second logger should receive all concurrent messages");
            });
        }
        
        #endregion
        
        #region Mock Logger Classes
        
        /// <summary>
        /// Mock logger implementation for testing purposes
        /// </summary>
        private class MockLogger : ILogger
        {
            private readonly object lockObject = new object();
            public string Name { get; }
            public List<string> LogMessages { get; } = new List<string>();
            public List<string> WarningMessages { get; } = new List<string>();
            public List<string> ErrorMessages { get; } = new List<string>();
            
            public MockLogger(string name)
            {
                Name = name;
            }
            
            public void Log(string message)
            {
                lock (lockObject)
                {
                    LogMessages.Add(message);
                }
            }
            
            public void LogWarning(string message)
            {
                lock (lockObject)
                {
                    WarningMessages.Add(message);
                }
            }
            
            public void LogError(string message)
            {
                lock (lockObject)
                {
                    ErrorMessages.Add(message);
                }
            }
        }
        
        /// <summary>
        /// Mock logger that always throws exceptions for testing error isolation
        /// </summary>
        private class FailingMockLogger : ILogger
        {
            public string Name { get; }
            
            public FailingMockLogger(string name)
            {
                Name = name;
            }
            
            public void Log(string message)
            {
                throw new InvalidOperationException($"FailingMockLogger {Name} failed on Log");
            }
            
            public void LogWarning(string message)
            {
                throw new InvalidOperationException($"FailingMockLogger {Name} failed on LogWarning");
            }
            
            public void LogError(string message)
            {
                throw new InvalidOperationException($"FailingMockLogger {Name} failed on LogError");
            }
        }
        
        #endregion
    }
}