using System;
using NUnit.Framework;
using UnityEngine;
using RuntimeLogging;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Unit tests for LogEntry struct validation
    /// </summary>
    public class LogEntryTests
    {
        [Test]
        public void LogEntry_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            string message = "Test message";
            LogLevel level = LogLevel.Warning;
            DateTime timestamp = DateTime.Now;
            string stackTrace = "Stack trace info";
            
            // Act
            LogEntry entry = new LogEntry(message, level, timestamp, stackTrace);
            
            // Assert
            Assert.AreEqual(message, entry.message);
            Assert.AreEqual(level, entry.level);
            Assert.AreEqual(timestamp, entry.timestamp);
            Assert.AreEqual(stackTrace, entry.stackTrace);
        }
        
        [Test]
        public void LogEntry_Constructor_HandlesNullMessage()
        {
            // Arrange
            string message = null;
            LogLevel level = LogLevel.Info;
            DateTime timestamp = DateTime.Now;
            
            // Act
            LogEntry entry = new LogEntry(message, level, timestamp);
            
            // Assert
            Assert.AreEqual(string.Empty, entry.message);
            Assert.AreEqual(level, entry.level);
            Assert.AreEqual(timestamp, entry.timestamp);
            Assert.AreEqual(string.Empty, entry.stackTrace);
        }
        
        [Test]
        public void LogEntry_FormattedMessage_ReturnsCorrectFormat()
        {
            // Arrange
            string message = "Test log message";
            LogLevel level = LogLevel.Error;
            DateTime timestamp = new DateTime(2023, 1, 1, 14, 30, 45);
            LogEntry entry = new LogEntry(message, level, timestamp);
            
            // Act
            string formatted = entry.FormattedMessage;
            
            // Assert
            Assert.AreEqual("[14:30:45][Error] Test log message", formatted);
        }
        
        [Test]
        public void LogEntry_GetDefaultLevelColor_ReturnsCorrectColors()
        {
            // Arrange & Act & Assert
            LogEntry infoEntry = new LogEntry("info", LogLevel.Info, DateTime.Now);
            Assert.AreEqual(Color.white, infoEntry.GetDefaultLevelColor());
            
            LogEntry warningEntry = new LogEntry("warning", LogLevel.Warning, DateTime.Now);
            Assert.AreEqual(Color.yellow, warningEntry.GetDefaultLevelColor());
            
            LogEntry errorEntry = new LogEntry("error", LogLevel.Error, DateTime.Now);
            Assert.AreEqual(Color.red, errorEntry.GetDefaultLevelColor());
        }
        
        [Test]
        public void LogEntry_GetLevelColor_WithNullConfig_ReturnsDefaultColors()
        {
            // Arrange
            LogEntry entry = new LogEntry("test", LogLevel.Warning, DateTime.Now);
            
            // Act
            Color color = entry.GetLevelColor(null);
            
            // Assert
            Assert.AreEqual(Color.yellow, color);
        }
        
        [Test]
        public void LogEntry_GetFormattedMessage_WithCustomTimestamp_ReturnsCorrectFormat()
        {
            // Arrange
            string message = "Custom timestamp test";
            LogLevel level = LogLevel.Info;
            DateTime timestamp = new DateTime(2023, 1, 1, 14, 30, 45);
            LogEntry entry = new LogEntry(message, level, timestamp);
            
            // Act
            string formatted = entry.GetFormattedMessage("yyyy-MM-dd HH:mm");
            
            // Assert
            Assert.AreEqual("[2023-01-01 14:30][Info] Custom timestamp test", formatted);
        }
        
        [Test]
        public void LogEntry_GetFormattedMessage_WithEmptyTimestampFormat_UsesDefault()
        {
            // Arrange
            string message = "Default timestamp test";
            LogLevel level = LogLevel.Info;
            DateTime timestamp = new DateTime(2023, 1, 1, 14, 30, 45);
            LogEntry entry = new LogEntry(message, level, timestamp);
            
            // Act
            string formatted = entry.GetFormattedMessage("");
            
            // Assert
            Assert.AreEqual("[14:30:45][Info] Default timestamp test", formatted);
        }
        
        [Test]
        public void LogEntry_GetRichTextMessage_WithConfig_ReturnsFormattedRichText()
        {
            // Arrange
            LogConfiguration config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.timestampFormat = "HH:mm:ss";
            string message = "Rich text test";
            LogLevel level = LogLevel.Warning;
            DateTime timestamp = new DateTime(2023, 1, 1, 14, 30, 45);
            LogEntry entry = new LogEntry(message, level, timestamp);
            
            // Act
            string richText = entry.GetRichTextMessage(config);
            
            // Assert
            Assert.IsTrue(richText.Contains("<color=#"));
            Assert.IsTrue(richText.Contains("[14:30:45][Warning] Rich text test"));
            Assert.IsTrue(richText.Contains("</color>"));
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(config);
        }
        
        [Test]
        public void LogEntry_GetRichTextMessage_WithHexCodes_ReturnsCorrectFormat()
        {
            // Arrange
            string message = "Hex color test";
            LogLevel level = LogLevel.Error;
            DateTime timestamp = new DateTime(2023, 1, 1, 14, 30, 45);
            LogEntry entry = new LogEntry(message, level, timestamp);
            
            // Act
            string richText = entry.GetRichTextMessage("#FFFFFF", "#FFFF00", "#FF0000");
            
            // Assert
            Assert.AreEqual("<color=#FF0000>[14:30:45][Error] Hex color test</color>", richText);
        }
        
        [Test]
        public void LogEntry_GetRichTextMessage_WithHexCodesAndCustomTimestamp_ReturnsCorrectFormat()
        {
            // Arrange
            string message = "Custom timestamp hex test";
            LogLevel level = LogLevel.Info;
            DateTime timestamp = new DateTime(2023, 1, 1, 14, 30, 45);
            LogEntry entry = new LogEntry(message, level, timestamp);
            
            // Act
            string richText = entry.GetRichTextMessage("#FFFFFF", "#FFFF00", "#FF0000", "yyyy-MM-dd HH:mm");
            
            // Assert
            Assert.AreEqual("<color=#FFFFFF>[2023-01-01 14:30][Info] Custom timestamp hex test</color>", richText);
        }
        
        [Test]
        public void LogEntry_GetRichTextMessage_HandlesHashInHexCode()
        {
            // Arrange
            string message = "Hash test";
            LogLevel level = LogLevel.Warning;
            DateTime timestamp = new DateTime(2023, 1, 1, 14, 30, 45);
            LogEntry entry = new LogEntry(message, level, timestamp);
            
            // Act - Test with hash prefix
            string richText = entry.GetRichTextMessage("#FFFFFF", "#FFFF00", "#FF0000");
            
            // Assert
            Assert.AreEqual("<color=#FFFF00>[14:30:45][Warning] Hash test</color>", richText);
        }
    }
}