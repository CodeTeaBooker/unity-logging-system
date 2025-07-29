using NUnit.Framework;
using RuntimeLogging;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Unit tests for LogLevel enum validation
    /// </summary>
    public class LogLevelTests
    {
        [Test]
        public void LogLevel_HasCorrectValues()
        {
            // Assert that all expected enum values exist
            Assert.IsTrue(System.Enum.IsDefined(typeof(LogLevel), LogLevel.Info));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LogLevel), LogLevel.Warning));
            Assert.IsTrue(System.Enum.IsDefined(typeof(LogLevel), LogLevel.Error));
        }
        
        [Test]
        public void LogLevel_HasCorrectIntegerValues()
        {
            // Assert that enum values have expected integer representations
            Assert.AreEqual(0, (int)LogLevel.Info);
            Assert.AreEqual(1, (int)LogLevel.Warning);
            Assert.AreEqual(2, (int)LogLevel.Error);
        }
        
        [Test]
        public void LogLevel_CanBeConvertedToString()
        {
            // Assert that enum values can be converted to strings
            Assert.AreEqual("Info", LogLevel.Info.ToString());
            Assert.AreEqual("Warning", LogLevel.Warning.ToString());
            Assert.AreEqual("Error", LogLevel.Error.ToString());
        }
        
        [Test]
        public void LogLevel_CanBeUsedInSwitchStatement()
        {
            // Test that LogLevel can be used in switch statements
            string result = GetLogLevelDescription(LogLevel.Warning);
            Assert.AreEqual("Warning level", result);
            
            result = GetLogLevelDescription(LogLevel.Info);
            Assert.AreEqual("Info level", result);
            
            result = GetLogLevelDescription(LogLevel.Error);
            Assert.AreEqual("Error level", result);
        }
        
        [Test]
        public void LogLevel_CanBeCompared()
        {
            // Test that LogLevel values can be compared
            Assert.IsTrue(LogLevel.Info < LogLevel.Warning);
            Assert.IsTrue(LogLevel.Warning < LogLevel.Error);
            Assert.IsTrue(LogLevel.Info < LogLevel.Error);
            
            Assert.AreEqual(LogLevel.Info, LogLevel.Info);
            Assert.AreNotEqual(LogLevel.Info, LogLevel.Warning);
        }
        
        private string GetLogLevelDescription(LogLevel level)
        {
            return level switch
            {
                LogLevel.Info => "Info level",
                LogLevel.Warning => "Warning level",
                LogLevel.Error => "Error level",
                _ => "Unknown level"
            };
        }
    }
}