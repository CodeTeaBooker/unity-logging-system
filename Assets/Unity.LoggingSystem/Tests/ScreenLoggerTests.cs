using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RuntimeLogging.Tests.TestUtilities;
using TMPro;
using System.Collections;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Comprehensive tests for ScreenLogger class
    /// Tests ILogger interface implementation, LogDataManager integration, LogDisplay integration,
    /// component lifecycle management, and performance optimization
    /// </summary>
    [TestFixture]
    public class ScreenLoggerTests
    {
        private GameObject testGameObject;
        private ScreenLogger screenLogger;
        private LogConfiguration testConfiguration;
        
        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with ScreenLogger component
            testGameObject = new GameObject("TestScreenLogger");
            screenLogger = testGameObject.AddComponent<ScreenLogger>();
            
            // Create test configuration
            testConfiguration = ScriptableObject.CreateInstance<LogConfiguration>();
            testConfiguration.ResetToDefaults();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            
            if (testConfiguration != null)
            {
                Object.DestroyImmediate(testConfiguration);
            }
        }
        
        #region ILogger Interface Implementation Tests
        
        [Test]
        public void Log_WithValidMessage_AddsInfoLogEntry()
        {
            // Arrange
            screenLogger.Initialize();
            var testMessage = "Test info message";
            var initialCount = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.Log(testMessage);
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(initialCount + 1),
                "Log count should increase by 1 after adding a log entry");
        }
        
        [Test]
        public void LogWarning_WithValidMessage_AddsWarningLogEntry()
        {
            // Arrange
            screenLogger.Initialize();
            var testMessage = "Test warning message";
            var initialCount = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.LogWarning(testMessage);
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(initialCount + 1),
                "Log count should increase by 1 after adding a warning entry");
        }
        
        [Test]
        public void LogError_WithValidMessage_AddsErrorLogEntry()
        {
            // Arrange
            screenLogger.Initialize();
            var testMessage = "Test error message";
            var initialCount = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.LogError(testMessage);
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(initialCount + 1),
                "Log count should increase by 1 after adding an error entry");
        }
        
        [Test]
        public void Log_WithNullMessage_DoesNotAddLogEntry()
        {
            // Arrange
            screenLogger.Initialize();
            var initialCount = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.Log(null);
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(initialCount),
                "Log count should not change when logging null message");
        }
        
        [Test]
        public void Log_WithEmptyMessage_DoesNotAddLogEntry()
        {
            // Arrange
            screenLogger.Initialize();
            var initialCount = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.Log("");
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(initialCount),
                "Log count should not change when logging empty message");
        }
        
        #endregion
        
        #region Component Lifecycle Management Tests
        
        [Test]
        public void Initialize_WhenCalled_SetsInitializedToTrue()
        {
            // Arrange
            Assert.That(screenLogger.IsInitialized(), Is.False,
                "ScreenLogger should not be initialized before Initialize() is called");
            
            // Act
            screenLogger.Initialize();
            
            // Assert
            Assert.That(screenLogger.IsInitialized(), Is.True,
                "ScreenLogger should be initialized after Initialize() is called");
        }
        
        [Test]
        public void IsEnabled_AfterAwake_ReturnsTrue()
        {
            // Arrange & Act
            screenLogger.Initialize();
            
            // Assert
            Assert.That(screenLogger.IsEnabled(), Is.True,
                "ScreenLogger should be enabled by default after initialization");
        }
        
        [Test]
        public void SetEnabled_WithFalse_DisablesLogger()
        {
            // Arrange
            screenLogger.Initialize();
            Assert.That(screenLogger.IsEnabled(), Is.True, "Logger should start enabled");
            
            // Act
            screenLogger.SetEnabled(false);
            
            // Assert
            Assert.That(screenLogger.IsEnabled(), Is.False,
                "Logger should be disabled after SetEnabled(false)");
        }
        
        [Test]
        public void SetEnabled_WithTrue_EnablesLogger()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetEnabled(false);
            Assert.That(screenLogger.IsEnabled(), Is.False, "Logger should be disabled");
            
            // Act
            screenLogger.SetEnabled(true);
            
            // Assert
            Assert.That(screenLogger.IsEnabled(), Is.True,
                "Logger should be enabled after SetEnabled(true)");
        }
        
        #endregion
        
        #region Performance Optimization Tests
        
        [Test]
        public void Log_WhenDisabled_DoesNotAddLogEntry()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetEnabled(false);
            var initialCount = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.Log("Test message");
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(initialCount),
                "Disabled logger should not add log entries for performance optimization");
        }
        
        [Test]
        public void LogWarning_WhenDisabled_DoesNotAddLogEntry()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetEnabled(false);
            var initialCount = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.LogWarning("Test warning");
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(initialCount),
                "Disabled logger should not add warning entries for performance optimization");
        }
        
        [Test]
        public void LogError_WhenDisabled_DoesNotAddLogEntry()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetEnabled(false);
            var initialCount = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.LogError("Test error");
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(initialCount),
                "Disabled logger should not add error entries for performance optimization");
        }
        
        [Test]
        public void GetPerformanceStats_WhenCalled_ReturnsValidStats()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.Log("Test message");
            
            // Act
            var stats = screenLogger.GetPerformanceStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.IsInitialized, Is.True, "Stats should show logger is initialized");
                Assert.That(stats.IsEnabled, Is.True, "Stats should show logger is enabled");
                Assert.That(stats.CurrentLogCount, Is.GreaterThan(0), "Stats should show current log count");
                Assert.That(stats.MaxLogCount, Is.GreaterThan(0), "Stats should show max log count");
            });
        }
        
        #endregion
        
        #region Public API Methods Tests
        
        [Test]
        public void Show_WhenCalled_ShowsLogDisplay()
        {
            // Arrange
            screenLogger.Initialize();
            var logDisplay = screenLogger.GetLogDisplay();
            
            // Act
            screenLogger.Show();
            
            // Assert
            Assert.That(logDisplay.gameObject.activeInHierarchy, Is.True,
                "LogDisplay should be active after Show() is called");
        }
        
        [Test]
        public void Hide_WhenCalled_HidesLogDisplay()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.Show();
            var logDisplay = screenLogger.GetLogDisplay();
            
            // Act
            screenLogger.Hide();
            
            // Assert
            Assert.That(logDisplay.gameObject.activeInHierarchy, Is.False,
                "LogDisplay should be inactive after Hide() is called");
        }
        
        [Test]
        public void Clear_WhenCalled_ClearsAllLogs()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.Log("Test message 1");
            screenLogger.Log("Test message 2");
            Assert.That(screenLogger.GetCurrentLogCount(), Is.GreaterThan(0), "Should have logs before clearing");
            
            // Act
            screenLogger.Clear();
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(0),
                "All logs should be cleared after Clear() is called");
        }
        
        [Test]
        public void SetMaxLogCount_WithValidCount_UpdatesMaxLogCount()
        {
            // Arrange
            screenLogger.Initialize();
            var newMaxCount = 50;
            
            // Act
            screenLogger.SetMaxLogCount(newMaxCount);
            
            // Assert
            Assert.That(screenLogger.GetMaxLogCount(), Is.EqualTo(newMaxCount),
                $"Max log count should be updated to {newMaxCount}");
        }
        
        [Test]
        public void SetMaxLogCount_WithZero_ClampsToMinimumValue()
        {
            // Arrange
            screenLogger.Initialize();
            
            // Act
            screenLogger.SetMaxLogCount(0);
            
            // Assert
            Assert.That(screenLogger.GetMaxLogCount(), Is.GreaterThan(0),
                "Max log count should be clamped to minimum value when set to 0");
        }
        
        [Test]
        public void SetMaxLogCount_WithNegativeValue_ClampsToMinimumValue()
        {
            // Arrange
            screenLogger.Initialize();
            
            // Act
            screenLogger.SetMaxLogCount(-10);
            
            // Assert
            Assert.That(screenLogger.GetMaxLogCount(), Is.GreaterThan(0),
                "Max log count should be clamped to minimum value when set to negative");
        }
        
        #endregion
        
        #region Configuration Management Tests
        
        [Test]
        public void SetConfiguration_WithValidConfig_UpdatesConfiguration()
        {
            // Arrange
            screenLogger.Initialize();
            var newConfig = ScriptableObject.CreateInstance<LogConfiguration>();
            newConfig.maxLogCount = 75;
            
            // Act
            screenLogger.SetConfiguration(newConfig);
            
            // Assert
            Assert.That(screenLogger.GetConfiguration(), Is.EqualTo(newConfig),
                "Configuration should be updated to the new config");
            
            // Cleanup
            Object.DestroyImmediate(newConfig);
        }
        
        [Test]
        public void SetConfiguration_WithNull_DoesNotThrow()
        {
            // Arrange
            screenLogger.Initialize();
            
            // Act & Assert
            Assert.DoesNotThrow(() => screenLogger.SetConfiguration(null),
                "Setting null configuration should not throw exception");
        }
        
        [Test]
        public void GetConfiguration_WhenCalled_ReturnsCurrentConfiguration()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            
            // Act
            var result = screenLogger.GetConfiguration();
            
            // Assert
            Assert.That(result, Is.EqualTo(testConfiguration),
                "GetConfiguration should return the currently set configuration");
        }
        
        #endregion
        
        #region LogDisplay Integration Tests
        
        [Test]
        public void SetLogDisplay_WithValidDisplay_UpdatesLogDisplay()
        {
            // Arrange
            screenLogger.Initialize();
            var newDisplayObject = new GameObject("NewLogDisplay");
            var newDisplay = newDisplayObject.AddComponent<LogDisplay>();
            
            // Act
            screenLogger.SetLogDisplay(newDisplay);
            
            // Assert
            Assert.That(screenLogger.GetLogDisplay(), Is.EqualTo(newDisplay),
                "LogDisplay should be updated to the new display component");
            
            // Cleanup
            Object.DestroyImmediate(newDisplayObject);
        }
        
        [Test]
        public void SetLogDisplay_WithNull_DoesNotThrow()
        {
            // Arrange
            screenLogger.Initialize();
            
            // Act & Assert
            Assert.DoesNotThrow(() => screenLogger.SetLogDisplay(null),
                "Setting null LogDisplay should not throw exception");
        }
        
        [Test]
        public void GetLogDisplay_WhenCalled_ReturnsCurrentLogDisplay()
        {
            // Arrange
            screenLogger.Initialize();
            var logDisplay = screenLogger.GetLogDisplay();
            
            // Act & Assert
            Assert.That(logDisplay, Is.Not.Null,
                "GetLogDisplay should return a valid LogDisplay component");
        }
        
        [Test]
        public void ForceDisplayUpdate_WhenCalled_DoesNotThrow()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.Log("Test message");
            
            // Act & Assert
            Assert.DoesNotThrow(() => screenLogger.ForceDisplayUpdate(),
                "ForceDisplayUpdate should execute without throwing");
        }
        
        #endregion
        
        #region Event System Tests
        
        [Test]
        public void OnLoggerStateChanged_WhenEnabled_FiresEvent()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetEnabled(false); // First disable to ensure state change
            bool eventFired = false;
            bool eventState = false;
            
            screenLogger.OnLoggerStateChanged += (enabled) =>
            {
                eventFired = true;
                eventState = enabled;
            };
            
            // Act
            screenLogger.SetEnabled(true);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(eventFired, Is.True, "OnLoggerStateChanged event should fire when state changes");
                Assert.That(eventState, Is.True, "Event should indicate logger is enabled");
            });
        }
        
        [Test]
        public void OnLoggerStateChanged_WhenDisabled_FiresEvent()
        {
            // Arrange
            screenLogger.Initialize();
            bool eventFired = false;
            bool eventState = true;
            
            screenLogger.OnLoggerStateChanged += (enabled) =>
            {
                eventFired = true;
                eventState = enabled;
            };
            
            // Act
            screenLogger.SetEnabled(false);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(eventFired, Is.True, "OnLoggerStateChanged event should fire when state changes");
                Assert.That(eventState, Is.False, "Event should indicate logger is disabled");
            });
        }
        
        [Test]
        public void OnLogsCleared_WhenClearCalled_FiresEvent()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.Log("Test message");
            bool eventFired = false;
            
            screenLogger.OnLogsCleared += () =>
            {
                eventFired = true;
            };
            
            // Act
            screenLogger.Clear();
            
            // Assert
            Assert.That(eventFired, Is.True,
                "OnLogsCleared event should fire when Clear() is called");
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void ScreenLogger_WithMultipleLogTypes_HandlesAllCorrectly()
        {
            // Arrange
            screenLogger.Initialize();
            var initialCount = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.Log("Info message");
            screenLogger.LogWarning("Warning message");
            screenLogger.LogError("Error message");
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(initialCount + 3),
                "All three log types should be added correctly");
        }
        
        [Test]
        public void ScreenLogger_WithMaxLogCountExceeded_RemovesOldestLogs()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetMaxLogCount(2);
            
            // Act
            screenLogger.Log("Message 1");
            screenLogger.Log("Message 2");
            screenLogger.Log("Message 3"); // Should remove Message 1
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(2),
                "Log count should not exceed max log count");
        }
        
        [Test]
        public void ScreenLogger_ShowHideCycle_MaintainsLogData()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.Log("Test message");
            var logCountAfterLogging = screenLogger.GetCurrentLogCount();
            
            // Act
            screenLogger.Hide();
            screenLogger.Show();
            
            // Assert
            Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(logCountAfterLogging),
                "Log data should be maintained through show/hide cycle");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void Log_BeforeInitialization_InitializesAutomatically()
        {
            // Arrange
            Assert.That(screenLogger.IsInitialized(), Is.False, "Logger should not be initialized initially");
            
            // Act
            screenLogger.Log("Test message");
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(screenLogger.IsInitialized(), Is.True, "Logger should auto-initialize when logging");
                Assert.That(screenLogger.GetCurrentLogCount(), Is.EqualTo(1), "Log should be added after auto-initialization");
            });
        }
        
        [Test]
        public void Show_BeforeInitialization_InitializesAutomatically()
        {
            // Arrange
            Assert.That(screenLogger.IsInitialized(), Is.False, "Logger should not be initialized initially");
            
            // Act
            screenLogger.Show();
            
            // Assert
            Assert.That(screenLogger.IsInitialized(), Is.True,
                "Logger should auto-initialize when Show() is called");
        }
        
        [Test]
        public void Clear_BeforeInitialization_InitializesAutomatically()
        {
            // Arrange
            Assert.That(screenLogger.IsInitialized(), Is.False, "Logger should not be initialized initially");
            
            // Act
            screenLogger.Clear();
            
            // Assert
            Assert.That(screenLogger.IsInitialized(), Is.True,
                "Logger should auto-initialize when Clear() is called");
        }
        
        #endregion
        
        #region Configuration Persistence and Runtime Settings Tests
        
        
        [Test]
        public void ScreenLogger_ApplyConfigurationChanges_CallsConfigurationApply()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            
            // Act
            Assert.DoesNotThrow(() => screenLogger.ApplyConfigurationChanges(),
                "ApplyConfigurationChanges should execute without throwing");
        }
        
        [Test]
        public void ScreenLogger_UpdateLogLevelColor_UpdatesConfigurationColor()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            string newInfoColor = "#123456";
            string newWarningColor = "#789ABC";
            string newErrorColor = "#DEF012";
            
            // Act
            screenLogger.UpdateLogLevelColor(LogLevel.Info, newInfoColor);
            screenLogger.UpdateLogLevelColor(LogLevel.Warning, newWarningColor);
            screenLogger.UpdateLogLevelColor(LogLevel.Error, newErrorColor);
            
            // Assert
            var config = screenLogger.GetConfiguration();
            Assert.That(config.infoColorHex, Is.EqualTo(newInfoColor),
                "Info color should be updated in configuration");
            Assert.That(config.warningColorHex, Is.EqualTo(newWarningColor),
                "Warning color should be updated in configuration");
            Assert.That(config.errorColorHex, Is.EqualTo(newErrorColor),
                "Error color should be updated in configuration");
        }
        
        [Test]
        public void ScreenLogger_UpdateLogLevelColor_WithInvalidHex_ValidatesColor()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            string originalColor = testConfiguration.infoColorHex;
            
            // Act
            screenLogger.UpdateLogLevelColor(LogLevel.Info, "INVALID_COLOR");
            
            // Assert
            var config = screenLogger.GetConfiguration();
            Assert.That(config.infoColorHex, Is.Not.EqualTo("INVALID_COLOR"),
                "Invalid color should be validated and corrected");
            Assert.That(config.infoColorHex, Does.StartWith("#"),
                "Color should be in valid hex format");
        }
        
        [Test]
        public void ScreenLogger_UpdateTimestampFormat_UpdatesConfigurationFormat()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            string newFormat = "yyyy-MM-dd HH:mm:ss.fff";
            
            // Act
            screenLogger.UpdateTimestampFormat(newFormat);
            
            // Assert
            var config = screenLogger.GetConfiguration();
            Assert.That(config.timestampFormat, Is.EqualTo(newFormat),
                "Timestamp format should be updated in configuration");
        }
        
        [Test]
        public void ScreenLogger_UpdateTimestampFormat_WithEmptyString_UsesDefault()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            
            // Act
            screenLogger.UpdateTimestampFormat("");
            
            // Assert
            var config = screenLogger.GetConfiguration();
            Assert.That(config.timestampFormat, Is.EqualTo("HH:mm:ss"),
                "Empty timestamp format should use default");
        }
        
        [Test]
        public void ScreenLogger_UpdateMaxLogCount_UpdatesConfigurationAndDataManager()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            int newMaxCount = 250;
            
            // Act
            screenLogger.UpdateMaxLogCount(newMaxCount);
            
            // Assert
            var config = screenLogger.GetConfiguration();
            Assert.That(config.maxLogCount, Is.EqualTo(newMaxCount),
                "Max log count should be updated in configuration");
            Assert.That(screenLogger.GetMaxLogCount(), Is.EqualTo(newMaxCount),
                "Max log count should be updated in data manager");
        }
        
        [Test]
        public void ScreenLogger_UpdateMaxLogCount_ClampsToValidRange()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            
            // Act - Test below minimum
            screenLogger.UpdateMaxLogCount(-10);
            
            // Assert
            var config = screenLogger.GetConfiguration();
            Assert.That(config.maxLogCount, Is.EqualTo(1),
                "Max log count should be clamped to minimum value");
            
            // Act - Test above maximum
            screenLogger.UpdateMaxLogCount(2000);
            
            // Assert
            Assert.That(config.maxLogCount, Is.EqualTo(1000),
                "Max log count should be clamped to maximum value");
        }
        
        [Test]
        public void ScreenLogger_ResetConfigurationToDefaults_RestoresDefaultValues()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            
            // Modify configuration
            screenLogger.UpdateMaxLogCount(500);
            screenLogger.UpdateTimestampFormat("yyyy-MM-dd");
            screenLogger.UpdateLogLevelColor(LogLevel.Info, "#123456");
            
            // Act
            screenLogger.ResetConfigurationToDefaults();
            
            // Assert
            var config = screenLogger.GetConfiguration();
            Assert.That(config.maxLogCount, Is.EqualTo(100),
                "Max log count should be reset to default");
            Assert.That(config.timestampFormat, Is.EqualTo("HH:mm:ss"),
                "Timestamp format should be reset to default");
            Assert.That(config.infoColorHex, Is.EqualTo("#FFFFFF"),
                "Info color should be reset to default");
        }
        
        [Test]
        public void ScreenLogger_ConfigurationChanges_ApplyImmediatelyToDisplay()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            
            // Add a log entry
            screenLogger.Log("Test message");
            
            // Act - Change configuration
            screenLogger.UpdateLogLevelColor(LogLevel.Info, "#FF0000");
            
            // Assert - This test verifies the method doesn't throw and configuration is applied
            Assert.DoesNotThrow(() => screenLogger.ForceDisplayUpdate(),
                "Display update should work with new configuration");
            
            var config = screenLogger.GetConfiguration();
            Assert.That(config.infoColorHex, Is.EqualTo("#FF0000"),
                "Configuration change should be applied");
        }
        
        [Test]
        public void ScreenLogger_ConfigurationMethods_WithNullConfiguration_HandleGracefully()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(null);
            
            // Act & Assert - All methods should handle null configuration gracefully
            Assert.DoesNotThrow(() => screenLogger.ApplyConfigurationChanges(),
                "ApplyConfigurationChanges should handle null configuration");
            Assert.DoesNotThrow(() => screenLogger.UpdateLogLevelColor(LogLevel.Info, "#FF0000"),
                "UpdateLogLevelColor should handle null configuration");
            Assert.DoesNotThrow(() => screenLogger.UpdateTimestampFormat("HH:mm:ss"),
                "UpdateTimestampFormat should handle null configuration");
            Assert.DoesNotThrow(() => screenLogger.UpdateMaxLogCount(100),
                "UpdateMaxLogCount should handle null configuration");
            Assert.DoesNotThrow(() => screenLogger.ResetConfigurationToDefaults(),
                "ResetConfigurationToDefaults should handle null configuration");
        }
        
        [Test]
        public void ScreenLogger_ConfigurationEventHandling_UpdatesDisplayOnConfigurationChange()
        {
            // Arrange
            screenLogger.Initialize();
            screenLogger.SetConfiguration(testConfiguration);
            screenLogger.Log("Test message before config change");
            
            // Act - Trigger configuration change event
            testConfiguration.maxLogCount = 200;
            testConfiguration.ApplyConfigurationChanges();
            
            // Assert - Verify the logger handles the configuration change
            Assert.DoesNotThrow(() => screenLogger.ForceDisplayUpdate(),
                "Display should update successfully after configuration change");
            
            // Verify configuration was applied
            Assert.That(screenLogger.GetMaxLogCount(), Is.EqualTo(200),
                "Configuration change should be applied to data manager");
        }
        
        #endregion
    }
}