using NUnit.Framework;
using RuntimeLogging;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Unit tests for ILogger interface definition validation
    /// </summary>
    public class ILoggerInterfaceTests
    {
        /// <summary>
        /// Mock implementation of ILogger for testing interface compliance
        /// </summary>
        private class MockLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public string LastWarningMessage { get; private set; }
            public string LastErrorMessage { get; private set; }
            
            public int LogCallCount { get; private set; }
            public int LogWarningCallCount { get; private set; }
            public int LogErrorCallCount { get; private set; }
            
            public void Log(string message)
            {
                LastLogMessage = message;
                LogCallCount++;
            }
            
            public void LogWarning(string message)
            {
                LastWarningMessage = message;
                LogWarningCallCount++;
            }
            
            public void LogError(string message)
            {
                LastErrorMessage = message;
                LogErrorCallCount++;
            }
            
            public void Reset()
            {
                LastLogMessage = null;
                LastWarningMessage = null;
                LastErrorMessage = null;
                LogCallCount = 0;
                LogWarningCallCount = 0;
                LogErrorCallCount = 0;
            }
        }
        
        private MockLogger mockLogger;
        
        [SetUp]
        public void SetUp()
        {
            mockLogger = new MockLogger();
        }
        
        [Test]
        public void ILogger_Log_AcceptsStringMessage()
        {
            // Arrange
            string testMessage = "Test info message";
            
            // Act
            mockLogger.Log(testMessage);
            
            // Assert
            Assert.AreEqual(testMessage, mockLogger.LastLogMessage);
            Assert.AreEqual(1, mockLogger.LogCallCount);
        }
        
        [Test]
        public void ILogger_LogWarning_AcceptsStringMessage()
        {
            // Arrange
            string testMessage = "Test warning message";
            
            // Act
            mockLogger.LogWarning(testMessage);
            
            // Assert
            Assert.AreEqual(testMessage, mockLogger.LastWarningMessage);
            Assert.AreEqual(1, mockLogger.LogWarningCallCount);
        }
        
        [Test]
        public void ILogger_LogError_AcceptsStringMessage()
        {
            // Arrange
            string testMessage = "Test error message";
            
            // Act
            mockLogger.LogError(testMessage);
            
            // Assert
            Assert.AreEqual(testMessage, mockLogger.LastErrorMessage);
            Assert.AreEqual(1, mockLogger.LogErrorCallCount);
        }
        
        [Test]
        public void ILogger_AllMethods_HandleNullMessages()
        {
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => mockLogger.Log(null));
            Assert.DoesNotThrow(() => mockLogger.LogWarning(null));
            Assert.DoesNotThrow(() => mockLogger.LogError(null));
        }
        
        [Test]
        public void ILogger_AllMethods_HandleEmptyMessages()
        {
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => mockLogger.Log(""));
            Assert.DoesNotThrow(() => mockLogger.LogWarning(""));
            Assert.DoesNotThrow(() => mockLogger.LogError(""));
            
            Assert.AreEqual("", mockLogger.LastLogMessage);
            Assert.AreEqual("", mockLogger.LastWarningMessage);
            Assert.AreEqual("", mockLogger.LastErrorMessage);
        }
        
        [Test]
        public void ILogger_MultipleCallsToSameMethod_IncrementCountCorrectly()
        {
            // Act
            mockLogger.Log("Message 1");
            mockLogger.Log("Message 2");
            mockLogger.Log("Message 3");
            
            // Assert
            Assert.AreEqual(3, mockLogger.LogCallCount);
            Assert.AreEqual("Message 3", mockLogger.LastLogMessage); // Should contain last message
        }
        
        [Test]
        public void ILogger_DifferentMethods_AreIndependent()
        {
            // Act
            mockLogger.Log("Info message");
            mockLogger.LogWarning("Warning message");
            mockLogger.LogError("Error message");
            
            // Assert
            Assert.AreEqual(1, mockLogger.LogCallCount);
            Assert.AreEqual(1, mockLogger.LogWarningCallCount);
            Assert.AreEqual(1, mockLogger.LogErrorCallCount);
            
            Assert.AreEqual("Info message", mockLogger.LastLogMessage);
            Assert.AreEqual("Warning message", mockLogger.LastWarningMessage);
            Assert.AreEqual("Error message", mockLogger.LastErrorMessage);
        }
        
        [Test]
        public void ILogger_Interface_CanBeUsedPolymorphically()
        {
            // Arrange
            ILogger logger = mockLogger;
            
            // Act & Assert - Should compile and work through interface
            Assert.DoesNotThrow(() => logger.Log("Polymorphic log"));
            Assert.DoesNotThrow(() => logger.LogWarning("Polymorphic warning"));
            Assert.DoesNotThrow(() => logger.LogError("Polymorphic error"));
            
            Assert.AreEqual("Polymorphic log", mockLogger.LastLogMessage);
            Assert.AreEqual("Polymorphic warning", mockLogger.LastWarningMessage);
            Assert.AreEqual("Polymorphic error", mockLogger.LastErrorMessage);
        }
    }
}