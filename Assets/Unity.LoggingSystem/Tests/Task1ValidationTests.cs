using System;
using NUnit.Framework;
using UnityEngine;
using RuntimeLogging;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Validation tests specifically for Task 1 requirements
    /// </summary>
    public class Task1ValidationTests
    {
        [Test]
        public void Task1_ILogger_InterfaceHasRequiredMethods()
        {
            // Verify ILogger interface has the required methods
            var interfaceType = typeof(ILogger);
            
            // Check Log method
            var logMethod = interfaceType.GetMethod("Log", new[] { typeof(string) });
            Assert.IsNotNull(logMethod, "ILogger should have Log(string) method");
            
            // Check LogWarning method
            var logWarningMethod = interfaceType.GetMethod("LogWarning", new[] { typeof(string) });
            Assert.IsNotNull(logWarningMethod, "ILogger should have LogWarning(string) method");
            
            // Check LogError method
            var logErrorMethod = interfaceType.GetMethod("LogError", new[] { typeof(string) });
            Assert.IsNotNull(logErrorMethod, "ILogger should have LogError(string) method");
        }
        
        [Test]
        public void Task1_LogLevel_EnumHasRequiredValues()
        {
            // Verify LogLevel enum has Info, Warning, Error
            Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.Info));
            Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.Warning));
            Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.Error));
            
            // Verify enum values
            Assert.AreEqual(0, (int)LogLevel.Info);
            Assert.AreEqual(1, (int)LogLevel.Warning);
            Assert.AreEqual(2, (int)LogLevel.Error);
        }
        
        [Test]
        public void Task1_LogEntry_HasRequiredFields()
        {
            // Create a LogEntry and verify it has required fields
            var timestamp = DateTime.Now;
            var entry = new LogEntry("test message", LogLevel.Info, timestamp);
            
            Assert.AreEqual("test message", entry.message);
            Assert.AreEqual(LogLevel.Info, entry.level);
            Assert.AreEqual(timestamp, entry.timestamp);
        }
        
        [Test]
        public void Task1_LogEntry_HasRichTextFormattingMethods()
        {
            // Verify LogEntry has rich text formatting methods
            var entry = new LogEntry("test", LogLevel.Info, DateTime.Now);
            var entryType = typeof(LogEntry);
            
            // Check GetRichTextMessage with config
            var richTextMethod1 = entryType.GetMethod("GetRichTextMessage", new[] { typeof(LogConfiguration) });
            Assert.IsNotNull(richTextMethod1, "LogEntry should have GetRichTextMessage(LogConfiguration) method");
            
            // Check GetRichTextMessage with hex codes
            var richTextMethod2 = entryType.GetMethod("GetRichTextMessage", new[] { typeof(string), typeof(string), typeof(string), typeof(string) });
            Assert.IsNotNull(richTextMethod2, "LogEntry should have GetRichTextMessage with hex color parameters");
        }
        
        [Test]
        public void Task1_LogConfiguration_HasDefaultSettings()
        {
            // Create LogConfiguration and verify default settings
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            
            // Verify default values as specified in task
            Assert.AreEqual(100, config.maxLogCount, "Default max count should be 100");
            Assert.AreEqual("HH:mm:ss", config.timestampFormat, "Default timestamp format should be 'HH:mm:ss'");
            
            // Verify hex color codes exist
            Assert.AreEqual("#FFFFFF", config.infoColorHex);
            Assert.AreEqual("#FFFF00", config.warningColorHex);
            Assert.AreEqual("#FF0000", config.errorColorHex);
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(config);
        }
        
        [Test]
        public void Task1_LogConfiguration_IsScriptableObject()
        {
            // Verify LogConfiguration inherits from ScriptableObject
            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(typeof(LogConfiguration)));
            
            // Verify it can be created
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            Assert.IsNotNull(config);
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(config);
        }
        
        [Test]
        public void Task1_LogEntry_RichTextFormatting_WorksCorrectly()
        {
            // Test rich text formatting functionality
            var timestamp = new DateTime(2023, 1, 1, 14, 30, 45);
            var entry = new LogEntry("Test message", LogLevel.Warning, timestamp);
            
            // Test with hex codes
            var richText = entry.GetRichTextMessage("#FFFFFF", "#FFFF00", "#FF0000");
            
            Assert.IsTrue(richText.Contains("<color=#FFFF00>"));
            Assert.IsTrue(richText.Contains("[14:30:45][Warning] Test message"));
            Assert.IsTrue(richText.Contains("</color>"));
        }
        
        [Test]
        public void Task1_LogConfiguration_HexColorValidation_Works()
        {
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            
            // Test invalid hex color correction
            config.infoColorHex = "invalid";
            config.ValidateSettings();
            Assert.AreEqual("#FFFFFF", config.infoColorHex);
            
            // Test hex color without # gets corrected
            config.warningColorHex = "AABBCC";
            config.ValidateSettings();
            Assert.AreEqual("#AABBCC", config.warningColorHex);
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(config);
        }
    }
}