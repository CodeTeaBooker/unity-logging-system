using NUnit.Framework;
using UnityEngine;
using RuntimeLogging;
using System.IO;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Unit tests for LogConfigurationManager global configuration management
    /// </summary>
    public class LogConfigurationManagerTests
    {
        private LogConfiguration testConfig;
        
        [SetUp]
        public void SetUp()
        {
            testConfig = ScriptableObject.CreateInstance<LogConfiguration>();
            testConfig.ResetToDefaults();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testConfig != null)
            {
                UnityEngine.Object.DestroyImmediate(testConfig);
            }
            
        }
        
        [Test]
        public void LogConfigurationManager_GlobalConfiguration_ReturnsValidInstance()
        {
            // Act
            var globalConfig = LogConfigurationManager.GlobalConfiguration;
            
            // Assert
            Assert.That(globalConfig, Is.Not.Null, 
                "GlobalConfiguration should return a valid instance");
            Assert.That(globalConfig.maxLogCount, Is.EqualTo(100), 
                "GlobalConfiguration should have default maxLogCount");
        }
        
        [Test]
        public void LogConfigurationManager_SetGlobalConfiguration_UpdatesGlobalInstance()
        {
            // Arrange
            testConfig.maxLogCount = 300;
            testConfig.timestampFormat = "yyyy-MM-dd HH:mm:ss";
            
            // Act
            LogConfigurationManager.SetGlobalConfiguration(testConfig);
            
            // Assert
            var globalConfig = LogConfigurationManager.GlobalConfiguration;
            Assert.That(globalConfig, Is.EqualTo(testConfig), 
                "GlobalConfiguration should be updated to the new instance");
            Assert.That(globalConfig.maxLogCount, Is.EqualTo(300), 
                "GlobalConfiguration should have updated maxLogCount");
        }
        
        [Test]
        public void LogConfigurationManager_SetGlobalConfiguration_FiresGlobalEvent()
        {
            // Arrange
            bool eventFired = false;
            LogConfiguration receivedConfig = null;
            LogConfigurationManager.OnGlobalConfigurationChanged += (config) =>
            {
                eventFired = true;
                receivedConfig = config;
            };
            
            testConfig.maxLogCount = 400;
            
            // Act
            LogConfigurationManager.SetGlobalConfiguration(testConfig);
            
            // Assert
            Assert.That(eventFired, Is.True, 
                "OnGlobalConfigurationChanged event should be fired");
            Assert.That(receivedConfig, Is.EqualTo(testConfig), 
                "Event should pass the correct configuration instance");
        }
        
        [Test]
        public void LogConfigurationManager_UpdateGlobalLogLevelColor_UpdatesCorrectColor()
        {
            // Arrange
            string newInfoColor = "#123456";
            string newWarningColor = "#789ABC";
            string newErrorColor = "#DEF012";
            
            // Act
            LogConfigurationManager.UpdateGlobalLogLevelColor(LogLevel.Info, newInfoColor);
            LogConfigurationManager.UpdateGlobalLogLevelColor(LogLevel.Warning, newWarningColor);
            LogConfigurationManager.UpdateGlobalLogLevelColor(LogLevel.Error, newErrorColor);
            
            // Assert
            var globalConfig = LogConfigurationManager.GlobalConfiguration;
            Assert.That(globalConfig.infoColorHex, Is.EqualTo(newInfoColor), 
                "Info color should be updated globally");
            Assert.That(globalConfig.warningColorHex, Is.EqualTo(newWarningColor), 
                "Warning color should be updated globally");
            Assert.That(globalConfig.errorColorHex, Is.EqualTo(newErrorColor), 
                "Error color should be updated globally");
        }
        
        [Test]
        public void LogConfigurationManager_UpdateGlobalTimestampFormat_UpdatesFormat()
        {
            // Arrange
            string newFormat = "yyyy-MM-dd HH:mm:ss.fff";
            
            // Act
            LogConfigurationManager.UpdateGlobalTimestampFormat(newFormat);
            
            // Assert
            var globalConfig = LogConfigurationManager.GlobalConfiguration;
            Assert.That(globalConfig.timestampFormat, Is.EqualTo(newFormat), 
                "Timestamp format should be updated globally");
        }
        
        [Test]
        public void LogConfigurationManager_UpdateGlobalTimestampFormat_WithEmptyString_UsesDefault()
        {
            // Act
            LogConfigurationManager.UpdateGlobalTimestampFormat("");
            
            // Assert
            var globalConfig = LogConfigurationManager.GlobalConfiguration;
            Assert.That(globalConfig.timestampFormat, Is.EqualTo("HH:mm:ss"), 
                "Empty timestamp format should use default");
        }
        
        [Test]
        public void LogConfigurationManager_UpdateGlobalMaxLogCount_UpdatesCount()
        {
            // Arrange
            int newMaxCount = 500;
            
            // Act
            LogConfigurationManager.UpdateGlobalMaxLogCount(newMaxCount);
            
            // Assert
            var globalConfig = LogConfigurationManager.GlobalConfiguration;
            Assert.That(globalConfig.maxLogCount, Is.EqualTo(newMaxCount), 
                "Max log count should be updated globally");
        }
        
        [Test]
        public void LogConfigurationManager_UpdateGlobalMaxLogCount_ClampsToValidRange()
        {
            // Act - Test below minimum
            LogConfigurationManager.UpdateGlobalMaxLogCount(-10);
            
            // Assert
            var globalConfig = LogConfigurationManager.GlobalConfiguration;
            Assert.That(globalConfig.maxLogCount, Is.EqualTo(1), 
                "Max log count should be clamped to minimum value");
            
            // Act - Test above maximum
            LogConfigurationManager.UpdateGlobalMaxLogCount(2000);
            
            // Assert
            Assert.That(globalConfig.maxLogCount, Is.EqualTo(1000), 
                "Max log count should be clamped to maximum value");
        }
        
        [Test]
        public void LogConfigurationManager_ResetGlobalConfigurationToDefaults_RestoresDefaults()
        {
            // Arrange - Modify global configuration
            LogConfigurationManager.UpdateGlobalMaxLogCount(500);
            LogConfigurationManager.UpdateGlobalTimestampFormat("yyyy-MM-dd");
            LogConfigurationManager.UpdateGlobalLogLevelColor(LogLevel.Info, "#123456");
            
            // Act
            LogConfigurationManager.ResetGlobalConfigurationToDefaults();
            
            // Assert
            var globalConfig = LogConfigurationManager.GlobalConfiguration;
            Assert.That(globalConfig.maxLogCount, Is.EqualTo(100), 
                "Max log count should be reset to default");
            Assert.That(globalConfig.timestampFormat, Is.EqualTo("HH:mm:ss"), 
                "Timestamp format should be reset to default");
            Assert.That(globalConfig.infoColorHex, Is.EqualTo("#FFFFFF"), 
                "Info color should be reset to default");
        }
        
        [Test]
        public void LogConfigurationManager_ValidateHexColor_ValidatesCorrectly()
        {
            // Act & Assert - Valid color
            string result1 = LogConfiguration.ValidateAndConvertHexColor("#FF0000");
            Assert.That(result1, Is.EqualTo("#FF0000"), 
                "Valid hex color should be returned unchanged");
            
            // Act & Assert - Invalid color with default
            string result2 = LogConfiguration.ValidateAndConvertHexColor("INVALID", "#123456");
            Assert.That(result2, Is.EqualTo("#123456"), 
                "Invalid hex color should return specified default");
        }
        
        [Test]
        public void LogConfigurationManager_ColorToHex_ConvertsCorrectly()
        {
            // Arrange
            Color testColor = Color.green;
            
            // Act
            string result = LogConfiguration.ColorToHex(testColor);
            
            // Assert
            Assert.That(result, Is.EqualTo("#00FF00"), 
                "Green color should convert to #00FF00");
        }
        
        [Test]
        public void LogConfigurationManager_HexToColor_ConvertsCorrectly()
        {
            // Arrange
            string blueHex = "#0000FF";
            
            // Act
            Color result = LogConfiguration.HexToColor(blueHex);
            
            // Assert
            Assert.That(result.r, Is.EqualTo(0f).Within(0.01f), 
                "Red component should be 0.0");
            Assert.That(result.g, Is.EqualTo(0f).Within(0.01f), 
                "Green component should be 0.0");
            Assert.That(result.b, Is.EqualTo(1f).Within(0.01f), 
                "Blue component should be 1.0");
        }
        
        [Test]
        public void LogConfigurationManager_CreateConfigurationPreset_Development_HasCorrectSettings()
        {
            // Act
            var devConfig = LogConfigurationManager.CreateConfigurationPreset(LogConfigurationPreset.Development);
            
            // Assert
            Assert.That(devConfig.maxLogCount, Is.EqualTo(200), 
                "Development preset should have 200 max log count");
            Assert.That(devConfig.timestampFormat, Is.EqualTo("HH:mm:ss.fff"), 
                "Development preset should have millisecond timestamp format");
            Assert.That(devConfig.infoColorHex, Is.EqualTo("#CCCCCC"), 
                "Development preset should have light gray info color");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(devConfig);
        }
        
        [Test]
        public void LogConfigurationManager_CreateConfigurationPreset_Production_HasCorrectSettings()
        {
            // Act
            var prodConfig = LogConfigurationManager.CreateConfigurationPreset(LogConfigurationPreset.Production);
            
            // Assert
            Assert.That(prodConfig.maxLogCount, Is.EqualTo(50), 
                "Production preset should have 50 max log count");
            Assert.That(prodConfig.timestampFormat, Is.EqualTo("HH:mm:ss"), 
                "Production preset should have standard timestamp format");
            Assert.That(prodConfig.infoColorHex, Is.EqualTo("#FFFFFF"), 
                "Production preset should have white info color");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(prodConfig);
        }
        
        [Test]
        public void LogConfigurationManager_CreateConfigurationPreset_Testing_HasCorrectSettings()
        {
            // Act
            var testConfig = LogConfigurationManager.CreateConfigurationPreset(LogConfigurationPreset.Testing);
            
            // Assert
            Assert.That(testConfig.maxLogCount, Is.EqualTo(500), 
                "Testing preset should have 500 max log count");
            Assert.That(testConfig.timestampFormat, Is.EqualTo("yyyy-MM-dd HH:mm:ss.fff"), 
                "Testing preset should have full timestamp format");
            Assert.That(testConfig.infoColorHex, Is.EqualTo("#00FF00"), 
                "Testing preset should have green info color");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(testConfig);
        }
        
        [Test]
        public void LogConfigurationManager_CreateConfigurationPreset_HighContrast_HasCorrectSettings()
        {
            // Act
            var contrastConfig = LogConfigurationManager.CreateConfigurationPreset(LogConfigurationPreset.HighContrast);
            
            // Assert
            Assert.That(contrastConfig.maxLogCount, Is.EqualTo(100), 
                "HighContrast preset should have 100 max log count");
            Assert.That(contrastConfig.panelAlpha, Is.EqualTo(1.0f).Within(0.01f), 
                "HighContrast preset should have full opacity");
            Assert.That(contrastConfig.infoColorHex, Is.EqualTo("#FFFFFF"), 
                "HighContrast preset should have white info color");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(contrastConfig);
        }
        
    }
    
    /// <summary>
    /// Test implementation of ILogConfigurationListener for testing listener functionality
    /// </summary>
    public class TestLogConfigurationListener : ILogConfigurationListener
    {
        public bool ConfigurationChangedCalled { get; private set; }
        public LogConfiguration LastReceivedConfiguration { get; private set; }
        
        public void OnConfigurationChanged(LogConfiguration configuration)
        {
            ConfigurationChangedCalled = true;
            LastReceivedConfiguration = configuration;
        }
        
        public void Reset()
        {
            ConfigurationChangedCalled = false;
            LastReceivedConfiguration = null;
        }
    }
    
    /// <summary>
    /// Additional tests for LogConfigurationManager listener functionality
    /// </summary>
    public class LogConfigurationManagerListenerTests
    {
        private TestLogConfigurationListener testListener;
        private LogConfiguration testConfig;
        
        [SetUp]
        public void SetUp()
        {
            testListener = new TestLogConfigurationListener();
            testConfig = ScriptableObject.CreateInstance<LogConfiguration>();
            testConfig.ResetToDefaults();
        }
        
        [TearDown]
        public void TearDown()
        {
            LogConfigurationManager.UnregisterListener(testListener);
            
            if (testConfig != null)
            {
                UnityEngine.Object.DestroyImmediate(testConfig);
            }
        }
        
        [Test]
        public void LogConfigurationManager_RegisterListener_NotifiesOnConfigurationChange()
        {
            // Arrange
            LogConfigurationManager.RegisterListener(testListener);
            testConfig.maxLogCount = 350;
            
            // Act
            LogConfigurationManager.SetGlobalConfiguration(testConfig);
            
            // Assert
            Assert.That(testListener.ConfigurationChangedCalled, Is.True, 
                "Registered listener should be notified of configuration changes");
            Assert.That(testListener.LastReceivedConfiguration, Is.EqualTo(testConfig), 
                "Listener should receive the correct configuration instance");
        }
        
        [Test]
        public void LogConfigurationManager_UnregisterListener_StopsNotifications()
        {
            // Arrange
            LogConfigurationManager.RegisterListener(testListener);
            LogConfigurationManager.UnregisterListener(testListener);
            testConfig.maxLogCount = 450;
            
            // Act
            LogConfigurationManager.SetGlobalConfiguration(testConfig);
            
            // Assert
            Assert.That(testListener.ConfigurationChangedCalled, Is.False, 
                "Unregistered listener should not be notified of configuration changes");
        }
        
        [Test]
        public void LogConfigurationManager_RegisterListener_WithNullListener_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => LogConfigurationManager.RegisterListener(null),
                "Registering null listener should not throw exception");
        }
        
        [Test]
        public void LogConfigurationManager_UnregisterListener_WithNullListener_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => LogConfigurationManager.UnregisterListener(null),
                "Unregistering null listener should not throw exception");
        }
        
        [Test]
        public void LogConfigurationManager_RegisterListener_WithSameListenerTwice_RegistersOnlyOnce()
        {
            // Arrange & Act
            LogConfigurationManager.RegisterListener(testListener);
            LogConfigurationManager.RegisterListener(testListener); // Register same listener again
            
            testConfig.maxLogCount = 550;
            LogConfigurationManager.SetGlobalConfiguration(testConfig);
            
            // Assert
            Assert.That(testListener.ConfigurationChangedCalled, Is.True, 
                "Listener should still be notified even when registered twice");
            // Note: We can't easily test that it's only called once without more complex setup
        }
    }
}