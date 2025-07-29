using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using System;
using System.Linq;
using RuntimeLogging.Tests.TestUtilities;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Integration tests for cross-platform compatibility and TextMeshPro consistency
    /// Tests the complete logging system behavior across different platform scenarios
    /// </summary>
    [TestFixture]
    [Category("CrossPlatform")]
    public class CrossPlatformIntegrationTests
    {
        private GameObject testGameObject;
        private ScreenLogger screenLogger;
        private LogDisplay logDisplay;
        private TextMeshProUGUI textComponent;
        private LogConfiguration configuration;
        
        [SetUp]
        public void SetUp()
        {
            // Create test environment
            testGameObject = new GameObject("CrossPlatformTest");
            screenLogger = testGameObject.AddComponent<ScreenLogger>();
            logDisplay = testGameObject.AddComponent<LogDisplay>();
            textComponent = testGameObject.AddComponent<TextMeshProUGUI>();
            
            // Configure components
            logDisplay.SetTextComponent(textComponent);
            screenLogger.SetLogDisplay(logDisplay);
            
            // Create platform-optimized configuration
            configuration = PlatformCompatibility.GetPlatformOptimizedConfiguration();
            screenLogger.SetConfiguration(configuration);
            
            screenLogger.Initialize();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testGameObject);
            }
        }
        
        [Test]
        public void CompleteSystem_WithPlatformOptimizations_InitializesCorrectly()
        {
            // Arrange & Act
            screenLogger.ApplyPlatformOptimizations();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(screenLogger.IsInitialized(), Is.True, "ScreenLogger should be initialized");
                Assert.That(logDisplay.IsTextComponentValid(), Is.True, "LogDisplay should have valid TextMeshPro component");
                Assert.That(screenLogger.GetConfiguration(), Is.Not.Null, "Configuration should not be null");
            });
        }
        
        [Test]
        public void PlatformOptimizedConfiguration_AppliedToSystem_UsesCorrectSettings()
        {
            // Arrange & Act
            screenLogger.ApplyPlatformOptimizations();
            var appliedConfig = screenLogger.GetConfiguration();
            var platformSettings = PlatformCompatibility.GetPlatformPerformanceSettings();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(appliedConfig.maxLogCount, Is.EqualTo(platformSettings.MaxLogCount),
                    "MaxLogCount should match platform recommendations");
                
                if (PlatformCompatibility.IsMobilePlatform)
                {
                    Assert.That(appliedConfig.timestampFormat, Is.EqualTo("mm:ss"),
                        "Mobile platforms should use shorter timestamp format");
                }
                else
                {
                    Assert.That(appliedConfig.timestampFormat, Is.EqualTo("HH:mm:ss"),
                        "Desktop platforms should use full timestamp format");
                }
            });
        }
        
        [Test]
        public void TextMeshProComponent_WithPlatformOptimizations_ConfiguredCorrectly()
        {
            // Arrange & Act
            screenLogger.ApplyPlatformOptimizations();
            // UI settings are now configured in Unity Editor - test platform support only
            bool isSupported = PlatformCompatibility.IsTextMeshProSupported;
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(isSupported, Is.True, "TextMeshPro should be supported on this platform");
                Assert.That(textComponent, Is.Not.Null, "TextMeshPro component should exist");
                // UI properties are now configured in Unity Editor - no automatic modifications
            });
        }
        
        [Test]
        public void LoggingSystem_WithHighVolumeMessages_MaintainsPerformance()
        {
            // Arrange
            screenLogger.ApplyPlatformOptimizations();
            var startTime = DateTime.Now;
            const int messageCount = 100;
            
            // Act
            for (int i = 0; i < messageCount; i++)
            {
                screenLogger.Log($"Performance test message {i}");
                screenLogger.LogWarning($"Warning message {i}");
                screenLogger.LogError($"Error message {i}");
            }
            
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(duration.TotalSeconds, Is.LessThan(5.0), 
                    $"High volume logging should complete within 5 seconds, took {duration.TotalSeconds:F2}s");
                Assert.That(screenLogger.GetCurrentLogCount(), Is.GreaterThan(0), 
                    "Should have logged messages");
                Assert.That(screenLogger.GetCurrentLogCount(), Is.LessThanOrEqualTo(screenLogger.GetMaxLogCount()),
                    "Should respect maximum log count limits");
            });
        }
        
        [Test]
        public void CrossPlatformValidation_OnCurrentPlatform_PassesAllTests()
        {
            // Arrange & Act
            var validationResult = screenLogger.ValidateCrossPlatformCompatibility();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(validationResult, Is.Not.Null, "Validation result should not be null");
                Assert.That(validationResult.Platform, Is.EqualTo(Application.platform), 
                    "Validation should be for current platform");
                Assert.That(validationResult.CompatibilityReport.IsUnitySupportedVersion, Is.True,
                    "Current Unity version should be supported");
                Assert.That(validationResult.CompatibilityReport.IsTextMeshProSupported, Is.True,
                    "TextMeshPro should be supported");
            });
        }
        
        [Test]
        public void LoggerInterface_ConsistentBehavior_AcrossDifferentAdapters()
        {
            // Arrange
            var unityLogger = new UnityLogger();
            var compositeLogger = new CompositeLogger(unityLogger, screenLogger);
            
            // Ignore Unity console log messages for this test since we're only testing that methods don't throw
            LogAssert.ignoreFailingMessages = true;
            
            try
            {
                // Act & Assert - Test that all loggers handle the same operations consistently
                MultipleAssert.Multiple(() =>
                {
                    Assert.DoesNotThrow(() => unityLogger.Log("Test message"), 
                        "UnityLogger should handle Log without throwing");
                    Assert.DoesNotThrow(() => screenLogger.Log("Test message"), 
                        "ScreenLogger should handle Log without throwing");
                    Assert.DoesNotThrow(() => compositeLogger.Log("Test message"), 
                        "CompositeLogger should handle Log without throwing");
                    
                    Assert.DoesNotThrow(() => unityLogger.LogWarning("Warning message"), 
                        "UnityLogger should handle LogWarning without throwing");
                    Assert.DoesNotThrow(() => screenLogger.LogWarning("Warning message"), 
                        "ScreenLogger should handle LogWarning without throwing");
                    Assert.DoesNotThrow(() => compositeLogger.LogWarning("Warning message"), 
                        "CompositeLogger should handle LogWarning without throwing");
                    
                    Assert.DoesNotThrow(() => unityLogger.LogError("Error message"), 
                        "UnityLogger should handle LogError without throwing");
                    Assert.DoesNotThrow(() => screenLogger.LogError("Error message"), 
                        "ScreenLogger should handle LogError without throwing");
                    Assert.DoesNotThrow(() => compositeLogger.LogError("Error message"), 
                        "CompositeLogger should handle LogError without throwing");
                });
            }
            finally
            {
                // Reset log assertion behavior
                LogAssert.ignoreFailingMessages = false;
            }
        }
        
        [Test]
        public void TextMeshProRendering_WithRichTextMarkup_DisplaysCorrectly()
        {
            // Arrange
            screenLogger.ApplyPlatformOptimizations();
            var config = screenLogger.GetConfiguration();
            
            // Act
            screenLogger.Log("Info message");
            screenLogger.LogWarning("Warning message");
            screenLogger.LogError("Error message");
            
            // Force display update
            screenLogger.ForceDisplayUpdate();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(textComponent.richText, Is.True, "Rich text should be enabled");
                Assert.That(textComponent.text, Is.Not.Null.And.Not.Empty, "TextMeshPro should have content");
                
                // Verify rich text color markup is present
                var displayText = textComponent.text;
                Assert.That(displayText, Does.Contain("<color="), "Display should contain color markup");
                Assert.That(displayText, Does.Contain("</color>"), "Display should contain closing color tags");
            });
        }
        
        [Test]
        public void PlatformCompatibilityReport_ForCurrentEnvironment_ContainsValidData()
        {
            // Arrange & Act
            var report = screenLogger.GetPlatformCompatibilityReport();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(report.UnityVersion, Is.EqualTo(PlatformCompatibility.CurrentUnityVersion),
                    "Report should contain current Unity version");
                Assert.That(report.Platform, Is.EqualTo(Application.platform),
                    "Report should contain current platform");
                Assert.That(report.IsUnitySupportedVersion, Is.True,
                    "Current Unity version should be supported");
                Assert.That(report.IsTextMeshProSupported, Is.True,
                    "TextMeshPro should be supported");
                Assert.That(report.IsSupportedPlatform, Is.True,
                    "Current platform should be supported");
                Assert.That(report.GeneratedAt, Is.LessThanOrEqualTo(DateTime.Now),
                    "Report generation time should be recent");
            });
        }
        
        [Test]
        public void LogDisplay_PlatformValidation_ReturnsAccurateResults()
        {
            // Arrange & Act
            // UI validation is now handled by Unity Editor - test basic functionality
            bool isComponentValid = logDisplay.IsTextComponentValid();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(isComponentValid, Is.True, "TextMeshPro component should be valid");
                Assert.That(PlatformCompatibility.IsTextMeshProSupported, Is.True, "TextMeshPro should be supported");
                // UI validation is now handled by Unity Editor
            });
        }
        
        [Test]
        public void LogDisplay_PlatformRecommendations_ProvideUsefulGuidance()
        {
            // Arrange & Act
            // Platform recommendations are now handled by Unity Editor
            var report = PlatformCompatibility.GenerateCompatibilityReport();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(report.Platform, Is.EqualTo(Application.platform), 
                    "Report should contain current platform");
                Assert.That(report.IsTextMeshProSupported, Is.True,
                    "TextMeshPro should be supported on current platform");
            });
        }
        
        [Test]
        public void MemoryManagement_WithExtensiveLogging_RemainsStable()
        {
            // Arrange
            screenLogger.ApplyPlatformOptimizations();
            long initialMemory = GC.GetTotalMemory(false);
            
            // Act - Generate extensive logging
            for (int i = 0; i < 200; i++)
            {
                screenLogger.Log($"Memory test message {i}: " + new string('X', 50));
            }
            
            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long finalMemory = GC.GetTotalMemory(false);
            long memoryIncrease = finalMemory - initialMemory;
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(memoryIncrease, Is.LessThan(2 * 1024 * 1024), // Less than 2MB
                    $"Memory increase should be reasonable: {memoryIncrease / 1024}KB");
                Assert.That(screenLogger.GetCurrentLogCount(), Is.LessThanOrEqualTo(screenLogger.GetMaxLogCount()),
                    "Log count should respect maximum limits");
            });
        }
        
        [Test]
        public void ConfigurationChanges_AppliedAtRuntime_TakeEffectImmediately()
        {
            // Arrange
            screenLogger.ApplyPlatformOptimizations();
            var originalMaxCount = screenLogger.GetMaxLogCount();
            var newMaxCount = originalMaxCount / 2;
            
            // Act
            screenLogger.UpdateMaxLogCount(newMaxCount);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(screenLogger.GetMaxLogCount(), Is.EqualTo(newMaxCount),
                    "Max log count should be updated immediately");
                Assert.That(screenLogger.GetConfiguration().maxLogCount, Is.EqualTo(newMaxCount),
                    "Configuration should reflect the change");
            });
        }
        
        [Test]
        public void LogLevelColors_WithPlatformConfiguration_DisplayCorrectly()
        {
            // Arrange
            screenLogger.ApplyPlatformOptimizations();
            var config = screenLogger.GetConfiguration();
            
            // Act
            screenLogger.UpdateLogLevelColor(LogLevel.Info, "#00FF00");
            screenLogger.UpdateLogLevelColor(LogLevel.Warning, "#FFFF00");
            screenLogger.UpdateLogLevelColor(LogLevel.Error, "#FF0000");
            
            screenLogger.Log("Info message");
            screenLogger.LogWarning("Warning message");
            screenLogger.LogError("Error message");
            
            screenLogger.ForceDisplayUpdate();
            
            // Assert
            var displayText = textComponent.text;
            MultipleAssert.Multiple(() =>
            {
                Assert.That(displayText, Does.Contain("#00FF00"), "Should contain info color");
                Assert.That(displayText, Does.Contain("#FFFF00"), "Should contain warning color");
                Assert.That(displayText, Does.Contain("#FF0000"), "Should contain error color");
            });
        }
        
        [Test]
        public void SystemIntegration_WithLogManager_WorksCorrectly()
        {
            // Arrange
            var originalLogger = LogManager.GetLogger();
            
            try
            {
                // Act
                LogManager.SetLogger(screenLogger);
                var retrievedLogger = LogManager.GetLogger();
                
                // Test logging through LogManager
                retrievedLogger.Log("LogManager test message");
                
                // Assert
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(retrievedLogger, Is.EqualTo(screenLogger),
                        "LogManager should return the set ScreenLogger");
                    Assert.That(screenLogger.GetCurrentLogCount(), Is.GreaterThan(0),
                        "ScreenLogger should have received the message through LogManager");
                });
            }
            finally
            {
                // Restore original logger
                if (originalLogger != null)
                {
                    LogManager.SetLogger(originalLogger);
                }
            }
        }
        
        [Test]
        public void ErrorHandling_WithInvalidOperations_HandlesGracefully()
        {
            // Arrange & Act & Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => screenLogger.Log(null), 
                    "Should handle null log message gracefully");
                Assert.DoesNotThrow(() => screenLogger.LogWarning(""), 
                    "Should handle empty warning message gracefully");
                Assert.DoesNotThrow(() => screenLogger.LogError(new string('X', 10000)), 
                    "Should handle very long error message gracefully");
                Assert.DoesNotThrow(() => screenLogger.UpdateMaxLogCount(-1), 
                    "Should handle invalid max count gracefully");
                Assert.DoesNotThrow(() => screenLogger.UpdateTimestampFormat(null), 
                    "Should handle null timestamp format gracefully");
            });
        }
    }
}