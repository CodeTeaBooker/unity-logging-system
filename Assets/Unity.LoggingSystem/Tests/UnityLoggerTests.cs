using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RuntimeLogging.Tests
{
    [TestFixture]
    public class UnityLoggerTests
    {
        private UnityLogger _unityLogger;
        
        [SetUp]
        public void SetUp()
        {
            _unityLogger = new UnityLogger();
        }
        
        [Test]
        public void Log_WithValidMessage_OutputsToUnityConsole()
        {
            // Arrange
            const string testMessage = "Test info message";
            
            // Act & Assert - Should not throw and should output to Unity console
            Assert.DoesNotThrow(() => _unityLogger.Log(testMessage));
            
            // Note: We can't easily verify Unity Debug.Log output in unit tests,
            // but we can verify the method doesn't throw exceptions
            LogAssert.Expect(LogType.Log, testMessage);
        }
        
        [Test]
        public void LogWarning_WithValidMessage_OutputsToUnityConsole()
        {
            // Arrange
            const string testMessage = "Test warning message";
            
            // Act & Assert - Should not throw and should output to Unity console
            Assert.DoesNotThrow(() => _unityLogger.LogWarning(testMessage));
            
            LogAssert.Expect(LogType.Warning, testMessage);
        }
        
        [Test]
        public void LogError_WithValidMessage_OutputsToUnityConsole()
        {
            // Arrange
            const string testMessage = "Test error message";
            
            // Act & Assert - Should not throw and should output to Unity console
            Assert.DoesNotThrow(() => _unityLogger.LogError(testMessage));
            
            LogAssert.Expect(LogType.Error, testMessage);
        }
        
        [Test]
        public void Log_WithNullMessage_HandlesGracefully()
        {
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => _unityLogger.Log(null));
            
            // Should output "(null)" to indicate null was passed
            LogAssert.Expect(LogType.Log, "(null)");
        }
        
        [Test]
        public void LogWarning_WithNullMessage_HandlesGracefully()
        {
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => _unityLogger.LogWarning(null));
            
            // Should output "(null)" to indicate null was passed
            LogAssert.Expect(LogType.Warning, "(null)");
        }
        
        [Test]
        public void LogError_WithNullMessage_HandlesGracefully()
        {
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => _unityLogger.LogError(null));
            
            // Should output "(null)" to indicate null was passed
            LogAssert.Expect(LogType.Error, "(null)");
        }
        
        [Test]
        public void Log_WithEmptyMessage_HandlesGracefully()
        {
            // Arrange
            const string emptyMessage = "";
            
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => _unityLogger.Log(emptyMessage));
            
            LogAssert.Expect(LogType.Log, emptyMessage);
        }
        
        [Test]
        public void LogWarning_WithEmptyMessage_HandlesGracefully()
        {
            // Arrange
            const string emptyMessage = "";
            
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => _unityLogger.LogWarning(emptyMessage));
            
            LogAssert.Expect(LogType.Warning, emptyMessage);
        }
        
        [Test]
        public void LogError_WithEmptyMessage_HandlesGracefully()
        {
            // Arrange
            const string emptyMessage = "";
            
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => _unityLogger.LogError(emptyMessage));
            
            LogAssert.Expect(LogType.Error, emptyMessage);
        }
        
        [Test]
        public void UnityLogger_ImplementsILoggerInterface()
        {
            // Act - Should be able to cast to ILogger interface
            ILogger logger = _unityLogger;
            
            // Assert - Interface methods should be available
            Assert.That(logger, Is.Not.Null);
            Assert.DoesNotThrow(() => logger.Log("Interface test"));
            Assert.DoesNotThrow(() => logger.LogWarning("Interface warning test"));
            Assert.DoesNotThrow(() => logger.LogError("Interface error test"));
            
            LogAssert.Expect(LogType.Log, "Interface test");
            LogAssert.Expect(LogType.Warning, "Interface warning test");
            LogAssert.Expect(LogType.Error, "Interface error test");
        }
        
        [Test]
        public void UnityLogger_WithLongMessage_HandlesCorrectly()
        {
            // Arrange
            string longMessage = new string('A', 1000); // 1000 character message
            
            // Act & Assert - Should handle long messages without issues
            Assert.DoesNotThrow(() => _unityLogger.Log(longMessage));
            Assert.DoesNotThrow(() => _unityLogger.LogWarning(longMessage));
            Assert.DoesNotThrow(() => _unityLogger.LogError(longMessage));
            
            LogAssert.Expect(LogType.Log, longMessage);
            LogAssert.Expect(LogType.Warning, longMessage);
            LogAssert.Expect(LogType.Error, longMessage);
        }
        
        [Test]
        public void UnityLogger_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            const string specialMessage = "Test with special chars: \n\t\r\"'\\";
            
            // Act & Assert - Should handle special characters without issues
            Assert.DoesNotThrow(() => _unityLogger.Log(specialMessage));
            Assert.DoesNotThrow(() => _unityLogger.LogWarning(specialMessage));
            Assert.DoesNotThrow(() => _unityLogger.LogError(specialMessage));
            
            LogAssert.Expect(LogType.Log, specialMessage);
            LogAssert.Expect(LogType.Warning, specialMessage);
            LogAssert.Expect(LogType.Error, specialMessage);
        }
    }
}