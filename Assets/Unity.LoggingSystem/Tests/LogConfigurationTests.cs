using NUnit.Framework;
using UnityEngine;
using RuntimeLogging;
using System;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Unit tests for LogConfiguration ScriptableObject validation
    /// </summary>
    public class LogConfigurationTests
    {
        private LogConfiguration config;
        
        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<LogConfiguration>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (config != null)
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        [Test]
        public void LogConfiguration_DefaultValues_AreSetCorrectly()
        {
            // Assert
            Assert.AreEqual(100, config.maxLogCount);
            Assert.AreEqual(true, config.autoScroll);
            Assert.AreEqual("HH:mm:ss", config.timestampFormat);
            Assert.AreEqual(Color.white, config.infoColor);
            Assert.AreEqual(Color.yellow, config.warningColor);
            Assert.AreEqual(Color.red, config.errorColor);
            Assert.AreEqual("#FFFFFF", config.infoColorHex);
            Assert.AreEqual("#FFFF00", config.warningColorHex);
            Assert.AreEqual("#FF0000", config.errorColorHex);
            Assert.AreEqual(0.8f, config.panelAlpha, 0.001f);
        }
        
        [Test]
        public void LogConfiguration_ValidateSettings_ClampsMaxLogCount()
        {
            // Arrange
            config.maxLogCount = 0; // Below minimum
            
            // Act
            config.ValidateSettings();
            
            // Assert
            Assert.AreEqual(1, config.maxLogCount);
            
            // Arrange
            config.maxLogCount = -5; // Negative value
            
            // Act
            config.ValidateSettings();
            
            // Assert
            Assert.AreEqual(1, config.maxLogCount);
            
            // Arrange
            config.maxLogCount = 2000; // Above maximum
            
            // Act
            config.ValidateSettings();
            
            // Assert
            Assert.AreEqual(1000, config.maxLogCount);
        }
        
        [Test]
        public void LogConfiguration_ValidateSettings_HandlesEmptyTimestampFormat()
        {
            // Arrange
            config.timestampFormat = "";
            
            // Act
            config.ValidateSettings();
            
            // Assert
            Assert.AreEqual("HH:mm:ss", config.timestampFormat);
            
            // Arrange
            config.timestampFormat = null;
            
            // Act
            config.ValidateSettings();
            
            // Assert
            Assert.AreEqual("HH:mm:ss", config.timestampFormat);
        }
        
        [Test]
        public void LogConfiguration_ValidateSettings_ClampsPanelAlpha()
        {
            // Arrange
            config.panelAlpha = -0.5f; // Below minimum
            
            // Act
            config.ValidateSettings();
            
            // Assert
            Assert.AreEqual(0f, config.panelAlpha, 0.001f);
            
            // Arrange
            config.panelAlpha = 1.5f; // Above maximum
            
            // Act
            config.ValidateSettings();
            
            // Assert
            Assert.AreEqual(1f, config.panelAlpha, 0.001f);
        }
        
        [Test]
        public void LogConfiguration_ResetToDefaults_RestoresAllValues()
        {
            // Arrange - modify all values
            config.maxLogCount = 50;
            config.autoScroll = false;
            config.timestampFormat = "yyyy-MM-dd";
            config.infoColor = Color.blue;
            config.warningColor = Color.green;
            config.errorColor = Color.magenta;
            config.infoColorHex = "#123456";
            config.warningColorHex = "#789ABC";
            config.errorColorHex = "#DEF012";
            config.panelAlpha = 0.5f;
            
            // Act
            config.ResetToDefaults();
            
            // Assert
            Assert.AreEqual(100, config.maxLogCount);
            Assert.AreEqual(true, config.autoScroll);
            Assert.AreEqual("HH:mm:ss", config.timestampFormat);
            Assert.AreEqual(Color.white, config.infoColor);
            Assert.AreEqual(Color.yellow, config.warningColor);
            Assert.AreEqual(Color.red, config.errorColor);
            Assert.AreEqual("#FFFFFF", config.infoColorHex);
            Assert.AreEqual("#FFFF00", config.warningColorHex);
            Assert.AreEqual("#FF0000", config.errorColorHex);
            Assert.AreEqual(0.8f, config.panelAlpha, 0.001f);
        }
        
        [Test]
        public void LogConfiguration_ValidMaxLogCountRange_IsAccepted()
        {
            // Arrange & Act & Assert - Test minimum boundary
            config.maxLogCount = 1; // Minimum valid value
            config.ValidateSettings();
            Assert.AreEqual(1, config.maxLogCount);
            
            // Arrange & Act & Assert - Test middle value
            config.maxLogCount = 50; // Valid middle value
            config.ValidateSettings();
            Assert.AreEqual(50, config.maxLogCount);
            
            // Arrange & Act & Assert - Test maximum boundary
            config.maxLogCount = 1000; // Maximum valid value
            config.ValidateSettings();
            Assert.AreEqual(1000, config.maxLogCount);
        }
        
        [Test]
        public void LogConfiguration_ValidPanelAlphaRange_IsAccepted()
        {
            // Arrange
            config.panelAlpha = 0.6f; // Valid value
            
            // Act
            config.ValidateSettings();
            
            // Assert
            Assert.AreEqual(0.6f, config.panelAlpha, 0.001f);
        }
        
        [Test]
        public void LogConfiguration_ValidateSettings_CorrectsInvalidHexColors()
        {
            // Arrange - Set invalid hex colors
            config.infoColorHex = "invalid";
            config.warningColorHex = "#GGG";
            config.errorColorHex = "";
            
            // Act
            config.ValidateSettings();
            
            // Assert - Should be reset to defaults
            Assert.AreEqual("#FFFFFF", config.infoColorHex);
            Assert.AreEqual("#FFFF00", config.warningColorHex);
            Assert.AreEqual("#FF0000", config.errorColorHex);
        }
        
        [Test]
        public void LogConfiguration_ValidateSettings_AddsHashToHexColors()
        {
            // Arrange - Set hex colors without hash
            config.infoColorHex = "AABBCC";
            config.warningColorHex = "DDEEFF";
            config.errorColorHex = "112233";
            
            // Act
            config.ValidateSettings();
            
            // Assert - Should add hash prefix
            Assert.AreEqual("#AABBCC", config.infoColorHex);
            Assert.AreEqual("#DDEEFF", config.warningColorHex);
            Assert.AreEqual("#112233", config.errorColorHex);
        }
        
        [Test]
        public void LogConfiguration_GetHexColorMethods_ReturnCorrectValues()
        {
            // Arrange
            config.infoColorHex = "#123456";
            config.warningColorHex = "#789ABC";
            config.errorColorHex = "#DEF012";
            
            // Act & Assert
            Assert.AreEqual("#123456", config.GetInfoColorHex());
            Assert.AreEqual("#789ABC", config.GetWarningColorHex());
            Assert.AreEqual("#DEF012", config.GetErrorColorHex());
        }
        
        
        [Test]
        public void LogConfiguration_ApplyConfigurationChanges_FiresEvent()
        {
            // Arrange
            bool eventFired = false;
            LogConfiguration receivedConfig = null;
            config.OnConfigurationChanged += (cfg) => 
            {
                eventFired = true;
                receivedConfig = cfg;
            };
            
            // Act
            config.ApplyConfigurationChanges();
            
            // Assert
            Assert.That(eventFired, Is.True, 
                "OnConfigurationChanged event should be fired");
            Assert.That(receivedConfig, Is.EqualTo(config), 
                "Event should pass the correct configuration instance");
        }
        
        [Test]
        public void LogConfiguration_ValidateAndConvertHexColor_WithValidColor_ReturnsValidated()
        {
            // Arrange
            string validHex = "#FF0000";
            
            // Act
            string result = LogConfiguration.ValidateAndConvertHexColor(validHex);
            
            // Assert
            Assert.That(result, Is.EqualTo("#FF0000"), 
                "Valid hex color should be returned unchanged");
        }
        
        [Test]
        public void LogConfiguration_ValidateAndConvertHexColor_WithoutHash_AddsHash()
        {
            // Arrange
            string hexWithoutHash = "FF0000";
            
            // Act
            string result = LogConfiguration.ValidateAndConvertHexColor(hexWithoutHash);
            
            // Assert
            Assert.That(result, Is.EqualTo("#FF0000"), 
                "Hex color without hash should have hash added");
        }
        
        [Test]
        public void LogConfiguration_ValidateAndConvertHexColor_WithInvalidColor_ReturnsDefault()
        {
            // Arrange
            string invalidHex = "INVALID";
            string defaultColor = "#123456";
            
            // Act
            string result = LogConfiguration.ValidateAndConvertHexColor(invalidHex, defaultColor);
            
            // Assert
            Assert.That(result, Is.EqualTo(defaultColor), 
                "Invalid hex color should return default color");
        }
        
        [Test]
        public void LogConfiguration_ValidateAndConvertHexColor_WithNullOrEmpty_ReturnsDefault()
        {
            // Act & Assert - Null input
            string result1 = LogConfiguration.ValidateAndConvertHexColor(null);
            Assert.That(result1, Is.EqualTo("#FFFFFF"), 
                "Null hex color should return default white");
            
            // Act & Assert - Empty input
            string result2 = LogConfiguration.ValidateAndConvertHexColor("");
            Assert.That(result2, Is.EqualTo("#FFFFFF"), 
                "Empty hex color should return default white");
        }
        
        [Test]
        public void LogConfiguration_ColorToHex_ConvertsCorrectly()
        {
            // Arrange
            Color redColor = Color.red;
            
            // Act
            string result = LogConfiguration.ColorToHex(redColor);
            
            // Assert
            Assert.That(result, Is.EqualTo("#FF0000"), 
                "Red color should convert to #FF0000");
        }
        
        [Test]
        public void LogConfiguration_HexToColor_ConvertsCorrectly()
        {
            // Arrange
            string redHex = "#FF0000";
            
            // Act
            Color result = LogConfiguration.HexToColor(redHex);
            
            // Assert
            Assert.That(result.r, Is.EqualTo(1f).Within(0.01f), 
                "Red component should be 1.0");
            Assert.That(result.g, Is.EqualTo(0f).Within(0.01f), 
                "Green component should be 0.0");
            Assert.That(result.b, Is.EqualTo(0f).Within(0.01f), 
                "Blue component should be 0.0");
        }
        
        [Test]
        public void LogConfiguration_HexToColor_WithInvalidHex_ReturnsDefault()
        {
            // Arrange
            string invalidHex = "INVALID";
            Color defaultColor = Color.blue;
            
            // Act
            Color result = LogConfiguration.HexToColor(invalidHex, defaultColor);
            
            // Assert
            Assert.That(result, Is.EqualTo(defaultColor), 
                "Invalid hex should return default color");
        }
        
        
    }
}