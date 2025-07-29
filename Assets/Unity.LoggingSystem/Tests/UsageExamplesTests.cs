using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using RuntimeLogging.Tests.TestUtilities;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Usage examples and documentation tests showing ILogger integration with LogDisplay
    /// Provides practical examples and validates usage patterns described in documentation
    /// Requirements: Add code documentation and usage examples showing ILogger integration with LogDisplay
    /// </summary>
    [TestFixture]
    public class UsageExamplesTests
    {
        private GameObject _testGameObject;
        private ScreenLogger _screenLogger;
        private LogDisplay _logDisplay;
        private TextMeshProUGUI _textComponent;
        private LogConfiguration _configuration;
        
        [SetUp]
        public void SetUp()
        {
            // Create test environment
            _testGameObject = new GameObject("UsageExampleTest");
            _screenLogger = _testGameObject.AddComponent<ScreenLogger>();
            _logDisplay = _testGameObject.AddComponent<LogDisplay>();
            _textComponent = _testGameObject.AddComponent<TextMeshProUGUI>();
            
            // Create configuration
            _configuration = ScriptableObject.CreateInstance<LogConfiguration>();
            _configuration.maxLogCount = 100;
            _configuration.timestampFormat = "HH:mm:ss";
            _configuration.infoColorHex = "#FFFFFF";
            _configuration.warningColorHex = "#FFFF00";
            _configuration.errorColorHex = "#FF0000";
            
            // Configure components
            _logDisplay.SetTextComponent(_textComponent);
            _screenLogger.SetLogDisplay(_logDisplay);
            _screenLogger.SetConfiguration(_configuration);
            _screenLogger.Initialize();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (_testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_testGameObject);
            }
            
            if (_configuration != null)
            {
                UnityEngine.Object.DestroyImmediate(_configuration);
            }
        }
        
        /// <summary>
        /// Usage Example 1: Basic ILogger Interface Usage
        /// Demonstrates the fundamental ILogger interface methods
        /// </summary>
        [Test]
        public void UsageExample_BasicILoggerInterface()
        {
            UnityEngine.Debug.Log("=== USAGE EXAMPLE: Basic ILogger Interface ===");
            
            // Example: Basic logging through ILogger interface
            ILogger logger = _screenLogger;
            
            // Log different message types
            logger.Log("Application started successfully");
            logger.LogWarning("Configuration file not found, using defaults");
            logger.LogError("Failed to connect to server");
            
            // Force display update for testing
            _screenLogger.ForceDisplayUpdate();
            
            // Verify the example works
            string displayText = _textComponent.text;
            MultipleAssert.Multiple(() =>
            {
                Assert.That(displayText, Does.Contain("Application started successfully"),
                    "Basic Log() method should work through ILogger interface");
                Assert.That(displayText, Does.Contain("Configuration file not found"),
                    "LogWarning() method should work through ILogger interface");
                Assert.That(displayText, Does.Contain("Failed to connect to server"),
                    "LogError() method should work through ILogger interface");
            });
            
            UnityEngine.Debug.Log("✓ Basic ILogger interface usage example validated");
        }
        
        /// <summary>
        /// Usage Example 2: LogManager Global Access Pattern
        /// Demonstrates how to use LogManager for global logging access
        /// </summary>
        [Test]
        public void UsageExample_LogManagerGlobalAccess()
        {
            UnityEngine.Debug.Log("=== USAGE EXAMPLE: LogManager Global Access ===");
            
            var originalLogger = LogManager.GetLogger();
            
            try
            {
                // Example: Setting up global logger
                LogManager.SetLogger(_screenLogger);
                
                // Example: Using LogManager from anywhere in the application
                LogManager.Log("Global log message from anywhere");
                LogManager.LogWarning("Global warning from any component");
                LogManager.LogError("Global error from any system");
                
                // Example: Checking if logger is available
                if (LogManager.HasLogger())
                {
                    LogManager.Log("Logger is available and ready");
                }
                
                // Force display update for testing
                _screenLogger.ForceDisplayUpdate();
                
                // Verify the example works
                string displayText = _textComponent.text;
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(displayText, Does.Contain("Global log message"),
                        "LogManager.Log() should work correctly");
                    Assert.That(displayText, Does.Contain("Global warning"),
                        "LogManager.LogWarning() should work correctly");
                    Assert.That(displayText, Does.Contain("Global error"),
                        "LogManager.LogError() should work correctly");
                    Assert.That(displayText, Does.Contain("Logger is available"),
                        "LogManager.HasLogger() check should work correctly");
                });
            }
            finally
            {
                // Restore original logger
                if (originalLogger != null)
                {
                    LogManager.SetLogger(originalLogger);
                }
                else
                {
                    LogManager.ClearLogger();
                }
            }
            
            UnityEngine.Debug.Log("✓ LogManager global access example validated");
        }
        
        /// <summary>
        /// Usage Example 3: CompositeLogger for Multiple Outputs
        /// Demonstrates using CompositeLogger to output to multiple targets
        /// </summary>
        [Test]
        public void UsageExample_CompositeLoggerMultipleOutputs()
        {
            UnityEngine.Debug.Log("=== USAGE EXAMPLE: CompositeLogger Multiple Outputs ===");
            
            // Example: Creating multiple logger targets
            var unityLogger = new UnityLogger();
            var screenLogger = _screenLogger;
            
            // Example: Combining loggers with CompositeLogger
            var compositeLogger = new CompositeLogger(unityLogger, screenLogger);
            
            // Temporarily ignore Unity console messages for cleaner test output
            LogAssert.ignoreFailingMessages = true;
            
            try
            {
                // Example: Logging to multiple targets simultaneously
                compositeLogger.Log("This message goes to both Unity Console and Screen");
                compositeLogger.LogWarning("This warning appears in both outputs");
                compositeLogger.LogError("This error is logged to all configured targets");
                
                // Force display update for testing
                _screenLogger.ForceDisplayUpdate();
                
                // Verify the screen logger received the messages
                string displayText = _textComponent.text;
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(displayText, Does.Contain("both Unity Console and Screen"),
                        "CompositeLogger should route messages to ScreenLogger");
                    Assert.That(displayText, Does.Contain("both outputs"),
                        "CompositeLogger should route warnings to ScreenLogger");
                    Assert.That(displayText, Does.Contain("all configured targets"),
                        "CompositeLogger should route errors to ScreenLogger");
                });
                
                // Example: Adding logger at runtime
                var additionalLogger = new UnityLogger();
                var expandedComposite = new CompositeLogger(compositeLogger, additionalLogger);
                expandedComposite.Log("Message to expanded composite logger");
                
                _screenLogger.ForceDisplayUpdate();
                Assert.That(_textComponent.text, Does.Contain("expanded composite logger"),
                    "Runtime logger addition should work correctly");
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
            
            UnityEngine.Debug.Log("✓ CompositeLogger multiple outputs example validated");
        }
        
        /// <summary>
        /// Usage Example 4: Configuration and Customization
        /// Demonstrates how to configure and customize the logging display
        /// </summary>
        [Test]
        public void UsageExample_ConfigurationAndCustomization()
        {
            UnityEngine.Debug.Log("=== USAGE EXAMPLE: Configuration and Customization ===");
            
            // Example: Creating custom configuration
            var customConfig = ScriptableObject.CreateInstance<LogConfiguration>();
            customConfig.maxLogCount = 50;
            customConfig.timestampFormat = "yyyy-MM-dd HH:mm:ss";
            customConfig.infoColorHex = "#00FF00";    // Green
            customConfig.warningColorHex = "#FFA500"; // Orange
            customConfig.errorColorHex = "#FF4444";   // Light red
            
            try
            {
                // Example: Applying custom configuration
                _screenLogger.SetConfiguration(customConfig);
                
                // Example: Runtime configuration changes
                _screenLogger.UpdateMaxLogCount(25);
                _screenLogger.UpdateTimestampFormat("mm:ss");
                _screenLogger.UpdateLogLevelColor(LogLevel.Info, "#0080FF"); // Blue
                
                // Example: Logging with custom configuration
                _screenLogger.Log("Info message with custom blue color");
                _screenLogger.LogWarning("Warning with custom orange color");
                _screenLogger.LogError("Error with custom light red color");
                
                _screenLogger.ForceDisplayUpdate();
                
                // Verify configuration changes
                string displayText = _textComponent.text;
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(_screenLogger.GetMaxLogCount(), Is.EqualTo(25),
                        "Runtime max count change should be applied");
                    Assert.That(displayText, Does.Contain("#0080FF"),
                        "Custom info color should be applied");
                    Assert.That(displayText, Does.Contain("#FFA500"),
                        "Custom warning color should be applied");
                    Assert.That(displayText, Does.Contain("#FF4444"),
                        "Custom error color should be applied");
                    Assert.That(displayText, Does.Match(@"\[\d{2}:\d{2}\]"),
                        "Custom timestamp format should be applied");
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(customConfig);
            }
            
            UnityEngine.Debug.Log("✓ Configuration and customization example validated");
        }
        
        /// <summary>
        /// Usage Example 5: Display Control and Management
        /// Demonstrates how to control the display visibility and content
        /// </summary>
        [Test]
        public void UsageExample_DisplayControlAndManagement()
        {
            UnityEngine.Debug.Log("=== USAGE EXAMPLE: Display Control and Management ===");
            
            // Example: Basic display control
            _screenLogger.Log("Initial message");
            _screenLogger.ForceDisplayUpdate();
            
            // Example: Hiding the display
            _screenLogger.Hide();
            Assert.That(_testGameObject.activeInHierarchy, Is.False,
                "Hide() should make display invisible");
            
            // Example: Logging while hidden (messages are buffered)
            _screenLogger.Log("Message logged while hidden");
            _screenLogger.LogWarning("Warning logged while hidden");
            
            // Example: Showing the display (buffered messages appear)
            _screenLogger.Show();
            _screenLogger.ForceDisplayUpdate();
            Assert.That(_testGameObject.activeInHierarchy, Is.True,
                "Show() should make display visible");
            
            string displayText = _textComponent.text;
            Assert.That(displayText, Does.Contain("Message logged while hidden"),
                "Messages logged while hidden should appear when shown");
            
            // Example: Clearing all messages
            _screenLogger.Clear();
            _screenLogger.ForceDisplayUpdate();
            Assert.That(_textComponent.text, Is.EqualTo(string.Empty),
                "Clear() should remove all messages");
            
            // Example: Checking current state
            int currentLogCount = _screenLogger.GetCurrentLogCount();
            int maxLogCount = _screenLogger.GetMaxLogCount();
            bool isVisible = _testGameObject.activeInHierarchy;
            
            UnityEngine.Debug.Log($"Current log count: {currentLogCount}");
            UnityEngine.Debug.Log($"Max log count: {maxLogCount}");
            UnityEngine.Debug.Log($"Display visible: {isVisible}");
            
            Assert.That(currentLogCount, Is.EqualTo(0), "Log count should be 0 after clear");
            Assert.That(isVisible, Is.True, "Display should be visible");
            
            UnityEngine.Debug.Log("✓ Display control and management example validated");
        }
        
        /// <summary>
        /// Usage Example 6: Performance Optimization Patterns
        /// Demonstrates best practices for performance-conscious logging
        /// </summary>
        [UnityTest]
        public IEnumerator UsageExample_PerformanceOptimizationPatterns()
        {
            UnityEngine.Debug.Log("=== USAGE EXAMPLE: Performance Optimization Patterns ===");
            
            // Clear any existing logs first
            _screenLogger.Clear();
            _screenLogger.ForceDisplayUpdate();
            
            // Example: Conditional logging for performance
            bool debugMode = true;
            if (debugMode)
            {
                _screenLogger.Log("Debug information (only when debug mode is enabled)");
            }
            
            // Example: Batched logging for high-frequency scenarios
            var messageBatch = new List<string>
            {
                "Batch message 1",
                "Batch message 2",
                "Batch message 3"
            };
            
            foreach (string message in messageBatch)
            {
                _screenLogger.Log(message);
            }
            
            // Example: Using appropriate log levels
            _screenLogger.Log("Regular operation completed");           // Frequent, low priority
            _screenLogger.LogWarning("Performance threshold exceeded"); // Occasional, medium priority
            _screenLogger.LogError("Critical system failure");         // Rare, high priority
            
            // Force update to ensure all messages are displayed
            _screenLogger.ForceDisplayUpdate();
            
            // Example: Simple throttled logging demonstration
            _screenLogger.Log("Frame update log 0");
            yield return new WaitForSeconds(0.1f);
            _screenLogger.Log("Frame update log 1");
            yield return new WaitForSeconds(0.1f);
            _screenLogger.Log("Frame update log 2");
            
            _screenLogger.ForceDisplayUpdate();
            
            // Verify performance patterns work correctly
            string displayText = _textComponent.text;
            UnityEngine.Debug.Log($"Display text content: {displayText}");
            
            MultipleAssert.Multiple(() =>
            {
                Assert.That(displayText, Does.Contain("Debug information"),
                    "Conditional logging should work");
                Assert.That(displayText, Does.Contain("Batch message"),
                    "Batched logging should work");
                Assert.That(displayText, Does.Contain("Frame update log"),
                    "Throttled logging should work");
                
                // Verify we have frame update logs (accounting for potential test state persistence)
                int actualFrameLogCount = CountOccurrences(displayText, "Frame update log");
                Assert.That(actualFrameLogCount, Is.GreaterThan(0),
                    "Should have frame update logs");
                Assert.That(actualFrameLogCount, Is.LessThanOrEqualTo(6),
                    "Should not have more than 6 frame update logs (3 original + 3 potential duplicates)");
                
                // Verify specific frame update logs exist
                Assert.That(displayText, Does.Contain("Frame update log 0"),
                    "Should contain frame update log 0");
                Assert.That(displayText, Does.Contain("Frame update log 1"),
                    "Should contain frame update log 1");
                Assert.That(displayText, Does.Contain("Frame update log 2"),
                    "Should contain frame update log 2");
            });
            
            UnityEngine.Debug.Log("✓ Performance optimization patterns example validated");
        }
        
        /// <summary>
        /// Usage Example 7: Error Handling and Robustness
        /// Demonstrates proper error handling patterns with the logging system
        /// </summary>
        [Test]
        public void UsageExample_ErrorHandlingAndRobustness()
        {
            UnityEngine.Debug.Log("=== USAGE EXAMPLE: Error Handling and Robustness ===");
            
            // Example: Safe logging with null checks
            string potentiallyNullMessage = null;
            if (!string.IsNullOrEmpty(potentiallyNullMessage))
            {
                _screenLogger.Log(potentiallyNullMessage);
            }
            else
            {
                _screenLogger.LogWarning("Attempted to log null or empty message");
            }
            
            // Example: Exception handling in logging context
            try
            {
                // Simulate some operation that might fail
                throw new InvalidOperationException("Simulated error for demonstration");
            }
            catch (Exception ex)
            {
                _screenLogger.LogError($"Operation failed: {ex.Message}");
                _screenLogger.Log($"Stack trace: {ex.StackTrace}");
            }
            
            // Example: Graceful degradation when logger is unavailable
            ILogger logger = LogManager.GetLogger();
            if (logger != null)
            {
                logger.Log("Logger is available, logging normally");
            }
            else
            {
                // Fallback to Unity's built-in logging
                Debug.Log("Logger unavailable, using Unity Debug.Log as fallback");
            }
            
            // Example: Validating system state before logging
            if (_screenLogger.IsInitialized())
            {
                _screenLogger.Log("System is properly initialized");
            }
            else
            {
                _screenLogger.Initialize();
                _screenLogger.Log("System was re-initialized");
            }
            
            _screenLogger.ForceDisplayUpdate();
            
            // Verify error handling works correctly
            string displayText = _textComponent.text;
            MultipleAssert.Multiple(() =>
            {
                Assert.That(displayText, Does.Contain("Attempted to log null"),
                    "Null message handling should work");
                Assert.That(displayText, Does.Contain("Operation failed: Simulated error"),
                    "Exception logging should work");
                Assert.That(displayText, Does.Contain("Stack trace:"),
                    "Stack trace logging should work");
                Assert.That(displayText, Does.Contain("properly initialized"),
                    "Initialization check should work");
            });
            
            UnityEngine.Debug.Log("✓ Error handling and robustness example validated");
        }
        
        /// <summary>
        /// Usage Example 8: Integration with Game Systems
        /// Demonstrates how to integrate logging with typical game systems
        /// </summary>
        [Test]
        public void UsageExample_IntegrationWithGameSystems()
        {
            UnityEngine.Debug.Log("=== USAGE EXAMPLE: Integration with Game Systems ===");
            
            // Example: Player system integration
            SimulatePlayerSystem();
            
            // Example: Game state management integration
            SimulateGameStateSystem();
            
            // Example: Network system integration
            SimulateNetworkSystem();
            
            // Example: Resource management integration
            SimulateResourceSystem();
            
            _screenLogger.ForceDisplayUpdate();
            
            // Verify game system integration works
            string displayText = _textComponent.text;
            MultipleAssert.Multiple(() =>
            {
                Assert.That(displayText, Does.Contain("Player spawned"),
                    "Player system integration should work");
                Assert.That(displayText, Does.Contain("Game state changed"),
                    "Game state system integration should work");
                Assert.That(displayText, Does.Contain("Network connection"),
                    "Network system integration should work");
                Assert.That(displayText, Does.Contain("Resource loaded"),
                    "Resource system integration should work");
            });
            
            UnityEngine.Debug.Log("✓ Game systems integration example validated");
        }
        
        #region Helper Methods for Usage Examples
        
        private void SimulatePlayerSystem()
        {
            // Example: Player system logging
            _screenLogger.Log("Player spawned at position (0, 0, 0)");
            _screenLogger.Log("Player health: 100/100");
            _screenLogger.LogWarning("Player health below 25%");
            _screenLogger.LogError("Player died");
        }
        
        private void SimulateGameStateSystem()
        {
            // Example: Game state logging
            _screenLogger.Log("Game state changed: MainMenu -> Playing");
            _screenLogger.Log("Level loaded: Level_01");
            _screenLogger.LogWarning("Game paused due to focus loss");
            _screenLogger.Log("Game resumed");
        }
        
        private void SimulateNetworkSystem()
        {
            // Example: Network system logging
            _screenLogger.Log("Network connection established");
            _screenLogger.Log("Received player data from server");
            _screenLogger.LogWarning("Network latency high: 250ms");
            _screenLogger.LogError("Network connection lost");
        }
        
        private void SimulateResourceSystem()
        {
            // Example: Resource system logging
            _screenLogger.Log("Resource loaded: PlayerTexture.png");
            _screenLogger.Log("Audio clip loaded: BackgroundMusic.ogg");
            _screenLogger.LogWarning("Resource cache nearly full: 85%");
            _screenLogger.LogError("Failed to load resource: MissingTexture.png");
        }
        
        private int CountOccurrences(string text, string substring)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(substring, index)) != -1)
            {
                count++;
                index += substring.Length;
            }
            return count;
        }
        
        #endregion
        
        /// <summary>
        /// Generates comprehensive usage documentation
        /// </summary>
        [Test]
        public void GenerateUsageDocumentation()
        {
            UnityEngine.Debug.Log("=== USAGE DOCUMENTATION GENERATOR ===");
            
            var documentation = new System.Text.StringBuilder();
            documentation.AppendLine("# Runtime Logging Panel - Usage Guide");
            documentation.AppendLine("=====================================");
            documentation.AppendLine();
            documentation.AppendLine("## Overview");
            documentation.AppendLine("The Runtime Logging Panel provides a unified ILogger interface for cross-platform");
            documentation.AppendLine("logging with real-time TextMeshPro display capabilities.");
            documentation.AppendLine();
            
            documentation.AppendLine("## Basic Usage");
            documentation.AppendLine("```csharp");
            documentation.AppendLine("// Get logger instance");
            documentation.AppendLine("ILogger logger = screenLogger; // or LogManager.GetLogger()");
            documentation.AppendLine();
            documentation.AppendLine("// Log different message types");
            documentation.AppendLine("logger.Log(\"Information message\");");
            documentation.AppendLine("logger.LogWarning(\"Warning message\");");
            documentation.AppendLine("logger.LogError(\"Error message\");");
            documentation.AppendLine("```");
            documentation.AppendLine();
            
            documentation.AppendLine("## Global Access with LogManager");
            documentation.AppendLine("```csharp");
            documentation.AppendLine("// Set up global logger");
            documentation.AppendLine("LogManager.SetLogger(screenLogger);");
            documentation.AppendLine();
            documentation.AppendLine("// Use from anywhere in your application");
            documentation.AppendLine("LogManager.Log(\"Global message\");");
            documentation.AppendLine("LogManager.LogWarning(\"Global warning\");");
            documentation.AppendLine("LogManager.LogError(\"Global error\");");
            documentation.AppendLine("```");
            documentation.AppendLine();
            
            documentation.AppendLine("## Multiple Output Targets");
            documentation.AppendLine("```csharp");
            documentation.AppendLine("// Create composite logger for multiple outputs");
            documentation.AppendLine("var unityLogger = new UnityLogger();");
            documentation.AppendLine("var screenLogger = GetComponent<ScreenLogger>();");
            documentation.AppendLine("var compositeLogger = new CompositeLogger(unityLogger, screenLogger);");
            documentation.AppendLine();
            documentation.AppendLine("// Messages go to both Unity Console and Screen");
            documentation.AppendLine("compositeLogger.Log(\"Message to multiple targets\");");
            documentation.AppendLine("```");
            documentation.AppendLine();
            
            documentation.AppendLine("## Configuration and Customization");
            documentation.AppendLine("```csharp");
            documentation.AppendLine("// Runtime configuration changes");
            documentation.AppendLine("screenLogger.UpdateMaxLogCount(50);");
            documentation.AppendLine("screenLogger.UpdateTimestampFormat(\"mm:ss\");");
            documentation.AppendLine("screenLogger.UpdateLogLevelColor(LogLevel.Info, \"#00FF00\");");
            documentation.AppendLine("```");
            documentation.AppendLine();
            
            documentation.AppendLine("## Display Control");
            documentation.AppendLine("```csharp");
            documentation.AppendLine("// Control display visibility");
            documentation.AppendLine("screenLogger.Show();");
            documentation.AppendLine("screenLogger.Hide();");
            documentation.AppendLine("screenLogger.Clear();");
            documentation.AppendLine();
            documentation.AppendLine("// Check current state");
            documentation.AppendLine("int logCount = screenLogger.GetCurrentLogCount();");
            documentation.AppendLine("int maxCount = screenLogger.GetMaxLogCount();");
            documentation.AppendLine("```");
            documentation.AppendLine();
            
            documentation.AppendLine("## Performance Best Practices");
            documentation.AppendLine("```csharp");
            documentation.AppendLine("// Conditional logging");
            documentation.AppendLine("if (debugMode)");
            documentation.AppendLine("{");
            documentation.AppendLine("    logger.Log(\"Debug information\");");
            documentation.AppendLine("}");
            documentation.AppendLine();
            documentation.AppendLine("// Throttled logging in update loops");
            documentation.AppendLine("if (Time.time - lastLogTime > logInterval)");
            documentation.AppendLine("{");
            documentation.AppendLine("    logger.Log(\"Periodic update\");");
            documentation.AppendLine("    lastLogTime = Time.time;");
            documentation.AppendLine("}");
            documentation.AppendLine("```");
            documentation.AppendLine();
            
            documentation.AppendLine("## Error Handling");
            documentation.AppendLine("```csharp");
            documentation.AppendLine("// Safe logging with null checks");
            documentation.AppendLine("if (!string.IsNullOrEmpty(message))");
            documentation.AppendLine("{");
            documentation.AppendLine("    logger.Log(message);");
            documentation.AppendLine("}");
            documentation.AppendLine();
            documentation.AppendLine("// Exception handling");
            documentation.AppendLine("try");
            documentation.AppendLine("{");
            documentation.AppendLine("    // Some operation");
            documentation.AppendLine("}");
            documentation.AppendLine("catch (Exception ex)");
            documentation.AppendLine("{");
            documentation.AppendLine("    logger.LogError($\"Operation failed: {ex.Message}\");");
            documentation.AppendLine("}");
            documentation.AppendLine("```");
            
            string docContent = documentation.ToString();
            UnityEngine.Debug.Log(docContent);
            
            // Save documentation to file
            try
            {
                string fileName = $"RuntimeLoggingUsageGuide_{DateTime.Now:yyyyMMdd_HHmmss}.md";
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
                System.IO.File.WriteAllText(filePath, docContent);
                UnityEngine.Debug.Log($"Usage documentation saved to: {filePath}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Could not save documentation to file: {ex.Message}");
            }
            
            Assert.Pass("Usage documentation generated successfully");
        }
    }
}