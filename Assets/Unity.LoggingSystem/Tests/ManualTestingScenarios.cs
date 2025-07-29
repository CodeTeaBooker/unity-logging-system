using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Manual testing scenarios for TextMeshPro display validation
    /// Provides structured test cases for manual verification of visual and functional aspects
    /// Requirements: Manual testing scenarios for TextMeshPro display validation
    /// </summary>
    [TestFixture]
    [Category("Manual")]
    public class ManualTestingScenarios
    {
        private GameObject _testGameObject;
        private ScreenLogger _screenLogger;
        private LogDisplay _logDisplay;
        private TextMeshProUGUI _textComponent;
        private LogConfiguration _configuration;
        
        [SetUp]
        public void SetUp()
        {
            // Create test environment for manual testing
            _testGameObject = new GameObject("ManualTestingScenario");
            _screenLogger = _testGameObject.AddComponent<ScreenLogger>();
            _logDisplay = _testGameObject.AddComponent<LogDisplay>();
            _textComponent = _testGameObject.AddComponent<TextMeshProUGUI>();
            
            // Create configuration with distinct colors for manual verification
            _configuration = ScriptableObject.CreateInstance<LogConfiguration>();
            _configuration.maxLogCount = 50;
            _configuration.timestampFormat = "HH:mm:ss";
            _configuration.infoColorHex = "#00FF00";    // Bright green for easy identification
            _configuration.warningColorHex = "#FFAA00"; // Orange for warnings
            _configuration.errorColorHex = "#FF0000";   // Red for errors
            
            // Configure components
            _logDisplay.SetTextComponent(_textComponent);
            _screenLogger.SetLogDisplay(_logDisplay);
            _screenLogger.SetConfiguration(_configuration);
            _screenLogger.Initialize();
            
            // Position for visibility during manual testing
            _testGameObject.transform.position = Vector3.zero;
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
        /// Manual Test Scenario 1: Basic TextMeshPro Display Functionality
        /// Verify that logs appear correctly in TextMeshPro with proper formatting
        /// </summary>
        [Test]
        [Category("ManualVerification")]
        public void ManualTest_BasicDisplayFunctionality()
        {
            // Arrange
            UnityEngine.Debug.Log("=== MANUAL TEST: Basic Display Functionality ===");
            UnityEngine.Debug.Log("INSTRUCTIONS: Verify the following in the TextMeshPro component:");
            UnityEngine.Debug.Log("1. Three messages appear with different colors");
            UnityEngine.Debug.Log("2. Timestamps are formatted as HH:mm:ss");
            UnityEngine.Debug.Log("3. Log levels are clearly indicated [Info], [Warning], [Error]");
            UnityEngine.Debug.Log("4. Messages appear in chronological order");
            
            // Act
            _screenLogger.Log("This is an INFO message - should appear in GREEN");
            _screenLogger.LogWarning("This is a WARNING message - should appear in ORANGE");
            _screenLogger.LogError("This is an ERROR message - should appear in RED");
            _screenLogger.ForceDisplayUpdate();
            
            // Manual verification points
            string displayText = _textComponent.text;
            UnityEngine.Debug.Log("Current TextMeshPro content:");
            UnityEngine.Debug.Log(displayText);
            
            // Automated checks to support manual verification
            Assert.That(displayText, Does.Contain("INFO message"), "Info message should be present");
            Assert.That(displayText, Does.Contain("WARNING message"), "Warning message should be present");
            Assert.That(displayText, Does.Contain("ERROR message"), "Error message should be present");
            Assert.That(displayText, Does.Contain("<color=#00FF00>"), "Should contain green color markup");
            Assert.That(displayText, Does.Contain("<color=#FFAA00>"), "Should contain orange color markup");
            Assert.That(displayText, Does.Contain("<color=#FF0000>"), "Should contain red color markup");
            
            UnityEngine.Debug.Log("=== MANUAL VERIFICATION REQUIRED ===");
            UnityEngine.Debug.Log("Please verify visually that colors and formatting appear correctly");
        }
        
        /// <summary>
        /// Manual Test Scenario 2: High Volume Display Performance
        /// Verify TextMeshPro performance with many log entries
        /// </summary>
        [UnityTest]
        [Category("ManualVerification")]
        public IEnumerator ManualTest_HighVolumeDisplayPerformance()
        {
            UnityEngine.Debug.Log("=== MANUAL TEST: High Volume Display Performance ===");
            UnityEngine.Debug.Log("INSTRUCTIONS: Monitor the following during test execution:");
            UnityEngine.Debug.Log("1. TextMeshPro updates smoothly without stuttering");
            UnityEngine.Debug.Log("2. Frame rate remains stable");
            UnityEngine.Debug.Log("3. Memory usage doesn't grow excessively");
            UnityEngine.Debug.Log("4. Oldest messages are removed when limit is reached");
            
            const int totalLogs = 200;
            const int batchSize = 10;
            
            for (int batch = 0; batch < totalLogs / batchSize; batch++)
            {
                // Add batch of logs
                for (int i = 0; i < batchSize; i++)
                {
                    int logIndex = batch * batchSize + i;
                    LogLevel level = (LogLevel)(logIndex % 3);
                    string message = $"High volume test message {logIndex} - batch {batch}";
                    
                    switch (level)
                    {
                        case LogLevel.Info:
                            _screenLogger.Log(message);
                            break;
                        case LogLevel.Warning:
                            _screenLogger.LogWarning(message);
                            break;
                        case LogLevel.Error:
                            _screenLogger.LogError(message);
                            break;
                    }
                }
                
                _screenLogger.ForceDisplayUpdate();
                
                // Log progress for manual monitoring
                if (batch % 5 == 0)
                {
                    UnityEngine.Debug.Log($"Progress: {batch * batchSize}/{totalLogs} logs processed");
                    UnityEngine.Debug.Log($"Current log count: {_screenLogger.GetCurrentLogCount()}");
                    UnityEngine.Debug.Log($"TextMeshPro text length: {_textComponent.text.Length} characters");
                }
                
                yield return new WaitForSeconds(0.1f); // Allow visual monitoring
            }
            
            // Final verification
            Assert.That(_screenLogger.GetCurrentLogCount(), Is.LessThanOrEqualTo(_configuration.maxLogCount),
                "Log count should be managed within limits");
            
            UnityEngine.Debug.Log("=== MANUAL VERIFICATION REQUIRED ===");
            UnityEngine.Debug.Log("Please verify that performance remained smooth throughout the test");
            UnityEngine.Debug.Log($"Final log count: {_screenLogger.GetCurrentLogCount()}");
            UnityEngine.Debug.Log($"Final text length: {_textComponent.text.Length}");
        }
        
        /// <summary>
        /// Manual Test Scenario 3: Display Control and Visibility
        /// Verify show/hide functionality and visual feedback
        /// </summary>
        [UnityTest]
        [Category("ManualVerification")]
        public IEnumerator ManualTest_DisplayControlAndVisibility()
        {
            UnityEngine.Debug.Log("=== MANUAL TEST: Display Control and Visibility ===");
            UnityEngine.Debug.Log("INSTRUCTIONS: Observe the following visual changes:");
            UnityEngine.Debug.Log("1. Display starts visible with initial messages");
            UnityEngine.Debug.Log("2. Display becomes hidden (GameObject inactive)");
            UnityEngine.Debug.Log("3. Display becomes visible again with accumulated messages");
            UnityEngine.Debug.Log("4. Display clears all content");
            
            // Initial state - visible with messages
            _screenLogger.Log("Initial message - should be visible");
            _screenLogger.LogWarning("Initial warning - should be visible");
            _screenLogger.ForceDisplayUpdate();
            
            UnityEngine.Debug.Log("Phase 1: Display should be VISIBLE with 2 messages");
            Assert.That(_testGameObject.activeInHierarchy, Is.True, "Display should be initially visible");
            yield return new WaitForSeconds(2f);
            
            // Hide display
            _screenLogger.Hide();
            UnityEngine.Debug.Log("Phase 2: Display should be HIDDEN");
            Assert.That(_testGameObject.activeInHierarchy, Is.False, "Display should be hidden");
            
            // Add messages while hidden
            _screenLogger.Log("Message while hidden - should appear when shown");
            _screenLogger.LogError("Error while hidden - should appear when shown");
            yield return new WaitForSeconds(2f);
            
            // Show display again
            _screenLogger.Show();
            _screenLogger.ForceDisplayUpdate();
            UnityEngine.Debug.Log("Phase 3: Display should be VISIBLE with 4 accumulated messages");
            Assert.That(_testGameObject.activeInHierarchy, Is.True, "Display should be visible again");
            
            string displayText = _textComponent.text;
            Assert.That(displayText, Does.Contain("Message while hidden"), 
                "Messages logged while hidden should appear");
            Assert.That(displayText, Does.Contain("Error while hidden"), 
                "Messages logged while hidden should appear");
            yield return new WaitForSeconds(2f);
            
            // Clear display
            _screenLogger.Clear();
            _screenLogger.ForceDisplayUpdate();
            UnityEngine.Debug.Log("Phase 4: Display should be EMPTY");
            Assert.That(_textComponent.text, Is.EqualTo(string.Empty), "Display should be cleared");
            
            UnityEngine.Debug.Log("=== MANUAL VERIFICATION REQUIRED ===");
            UnityEngine.Debug.Log("Please verify that all visual state changes occurred as expected");
        }
        
        /// <summary>
        /// Manual Test Scenario 4: Rich Text Formatting and Colors
        /// Verify TextMeshPro rich text rendering with different log levels
        /// </summary>
        [Test]
        [Category("ManualVerification")]
        public void ManualTest_RichTextFormattingAndColors()
        {
            UnityEngine.Debug.Log("=== MANUAL TEST: Rich Text Formatting and Colors ===");
            UnityEngine.Debug.Log("INSTRUCTIONS: Verify the following visual elements:");
            UnityEngine.Debug.Log("1. Each log level has distinct color (Green=Info, Orange=Warning, Red=Error)");
            UnityEngine.Debug.Log("2. Timestamps are properly formatted and visible");
            UnityEngine.Debug.Log("3. Log level indicators are clearly readable");
            UnityEngine.Debug.Log("4. Text wrapping works correctly for long messages");
            
            // Test different message lengths and content types
            _screenLogger.Log("Short info message");
            _screenLogger.LogWarning("Medium length warning message with some additional content to test wrapping");
            _screenLogger.LogError("Very long error message with extensive content that should demonstrate how TextMeshPro handles longer text content and whether it wraps properly within the display boundaries while maintaining readability and proper formatting");
            
            // Test special characters and formatting
            _screenLogger.Log("Message with numbers: 12345 and symbols: !@#$%^&*()");
            _screenLogger.LogWarning("Message with\nnewlines\nand\ttabs");
            _screenLogger.LogError("Message with \"quotes\" and 'apostrophes' and [brackets]");
            
            _screenLogger.ForceDisplayUpdate();
            
            // Display raw markup for manual verification
            string displayText = _textComponent.text;
            UnityEngine.Debug.Log("Raw TextMeshPro markup:");
            UnityEngine.Debug.Log(displayText);
            
            // Automated checks
            Assert.That(displayText, Does.Contain("<color="), "Should contain color markup");
            Assert.That(displayText, Does.Contain("</color>"), "Should contain closing color tags");
            Assert.That(displayText, Does.Match(@"\[\d{2}:\d{2}:\d{2}\]"), "Should contain timestamps");
            Assert.That(displayText, Does.Contain("[Info]"), "Should contain info level indicator");
            Assert.That(displayText, Does.Contain("[Warning]"), "Should contain warning level indicator");
            Assert.That(displayText, Does.Contain("[Error]"), "Should contain error level indicator");
            
            UnityEngine.Debug.Log("=== MANUAL VERIFICATION REQUIRED ===");
            UnityEngine.Debug.Log("Please verify that colors, formatting, and special characters render correctly");
        }
        
        /// <summary>
        /// Manual Test Scenario 5: Configuration Changes at Runtime
        /// Verify that configuration changes are applied immediately and visibly
        /// </summary>
        [UnityTest]
        [Category("ManualVerification")]
        public IEnumerator ManualTest_RuntimeConfigurationChanges()
        {
            UnityEngine.Debug.Log("=== MANUAL TEST: Runtime Configuration Changes ===");
            UnityEngine.Debug.Log("INSTRUCTIONS: Observe the following configuration changes:");
            UnityEngine.Debug.Log("1. Initial messages with default colors");
            UnityEngine.Debug.Log("2. Color changes applied to new messages");
            UnityEngine.Debug.Log("3. Timestamp format changes");
            UnityEngine.Debug.Log("4. Max log count changes affecting display");
            
            // Initial messages with default configuration
            _screenLogger.Log("Initial info message - GREEN");
            _screenLogger.LogWarning("Initial warning message - ORANGE");
            _screenLogger.LogError("Initial error message - RED");
            _screenLogger.ForceDisplayUpdate();
            
            UnityEngine.Debug.Log("Phase 1: Initial messages with default colors");
            yield return new WaitForSeconds(2f);
            
            // Change colors at runtime
            _screenLogger.UpdateLogLevelColor(LogLevel.Info, "#0080FF");    // Blue
            _screenLogger.UpdateLogLevelColor(LogLevel.Warning, "#FF00FF"); // Magenta
            _screenLogger.UpdateLogLevelColor(LogLevel.Error, "#FFFF00");   // Yellow
            
            _screenLogger.Log("New info message - should be BLUE");
            _screenLogger.LogWarning("New warning message - should be MAGENTA");
            _screenLogger.LogError("New error message - should be YELLOW");
            _screenLogger.ForceDisplayUpdate();
            
            UnityEngine.Debug.Log("Phase 2: New messages with changed colors");
            yield return new WaitForSeconds(2f);
            
            // Change timestamp format
            _screenLogger.UpdateTimestampFormat("mm:ss");
            _screenLogger.Log("Message with short timestamp format");
            _screenLogger.ForceDisplayUpdate();
            
            UnityEngine.Debug.Log("Phase 3: Message with shortened timestamp format");
            yield return new WaitForSeconds(2f);
            
            // Change max log count to trigger truncation
            _screenLogger.UpdateMaxLogCount(3);
            _screenLogger.Log("Message that should trigger truncation");
            _screenLogger.ForceDisplayUpdate();
            
            UnityEngine.Debug.Log("Phase 4: Truncation should occur (only 3 most recent messages visible)");
            Assert.That(_screenLogger.GetCurrentLogCount(), Is.LessThanOrEqualTo(3),
                "Log count should be limited to 3");
            
            string finalDisplay = _textComponent.text;
            UnityEngine.Debug.Log("Final display content:");
            UnityEngine.Debug.Log(finalDisplay);
            
            UnityEngine.Debug.Log("=== MANUAL VERIFICATION REQUIRED ===");
            UnityEngine.Debug.Log("Please verify that all configuration changes were applied visually");
        }
        
        /// <summary>
        /// Manual Test Scenario 6: Error Handling and Recovery
        /// Verify system behavior under error conditions
        /// </summary>
        [Test]
        [Category("ManualVerification")]
        public void ManualTest_ErrorHandlingAndRecovery()
        {
            UnityEngine.Debug.Log("=== MANUAL TEST: Error Handling and Recovery ===");
            UnityEngine.Debug.Log("INSTRUCTIONS: Verify system remains stable during error conditions:");
            UnityEngine.Debug.Log("1. System handles null and empty messages gracefully");
            UnityEngine.Debug.Log("2. System handles invalid configuration values");
            UnityEngine.Debug.Log("3. System recovers and continues normal operation");
            UnityEngine.Debug.Log("4. No exceptions or crashes occur");
            
            // Test normal operation first
            _screenLogger.Log("Normal operation before error tests");
            _screenLogger.ForceDisplayUpdate();
            
            // Test error conditions
            try
            {
                // Null and empty message handling
                _screenLogger.Log(null);
                _screenLogger.LogWarning("");
                _screenLogger.LogError("   "); // Whitespace only
                
                // Invalid configuration values
                _screenLogger.UpdateMaxLogCount(-10);
                _screenLogger.UpdateMaxLogCount(0);
                _screenLogger.UpdateTimestampFormat(null);
                _screenLogger.UpdateTimestampFormat("");
                _screenLogger.UpdateLogLevelColor(LogLevel.Info, "invalid-color");
                _screenLogger.UpdateLogLevelColor(LogLevel.Warning, null);
                
                // Very long message
                string veryLongMessage = new string('X', 50000);
                _screenLogger.LogError(veryLongMessage);
                
                // Rapid successive calls
                for (int i = 0; i < 100; i++)
                {
                    _screenLogger.Log($"Rapid message {i}");
                }
                
                _screenLogger.ForceDisplayUpdate();
                
                UnityEngine.Debug.Log("All error conditions handled without exceptions");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Unexpected exception during error handling test: {ex}");
                Assert.Fail($"System should handle error conditions gracefully, but threw: {ex.Message}");
            }
            
            // Test recovery
            _screenLogger.Log("Recovery test - system should still work normally");
            _screenLogger.LogWarning("Recovery warning - should appear correctly");
            _screenLogger.LogError("Recovery error - should appear correctly");
            _screenLogger.ForceDisplayUpdate();
            
            string displayText = _textComponent.text;
            Assert.That(displayText, Does.Contain("Recovery test"), "System should recover normal operation");
            Assert.That(displayText, Does.Contain("Recovery warning"), "System should recover normal operation");
            Assert.That(displayText, Does.Contain("Recovery error"), "System should recover normal operation");
            
            UnityEngine.Debug.Log("=== MANUAL VERIFICATION REQUIRED ===");
            UnityEngine.Debug.Log("Please verify that no visual glitches or errors occurred during testing");
            UnityEngine.Debug.Log("Final display content:");
            UnityEngine.Debug.Log(displayText);
        }
        
        /// <summary>
        /// Manual Test Scenario 7: TextMeshPro Component Integration
        /// Verify proper integration with TextMeshPro component features
        /// </summary>
        [Test]
        [Category("ManualVerification")]
        public void ManualTest_TextMeshProComponentIntegration()
        {
            UnityEngine.Debug.Log("=== MANUAL TEST: TextMeshPro Component Integration ===");
            UnityEngine.Debug.Log("INSTRUCTIONS: Verify TextMeshPro-specific features:");
            UnityEngine.Debug.Log("1. Rich text markup renders correctly");
            UnityEngine.Debug.Log("2. Text fits within component boundaries");
            UnityEngine.Debug.Log("3. Font rendering is clear and readable");
            UnityEngine.Debug.Log("4. Component properties are respected");
            
            // Configure TextMeshPro component properties for testing
            _textComponent.fontSize = 14;
            _textComponent.fontStyle = FontStyles.Normal;
            _textComponent.alignment = TextAlignmentOptions.TopLeft;
            _textComponent.textWrappingMode = TMPro.TextWrappingModes.Normal;
            _textComponent.overflowMode = TextOverflowModes.Truncate;
            
            // Test various content types
            _screenLogger.Log("Testing font size and basic rendering");
            _screenLogger.LogWarning("Testing word wrapping with a longer message that should wrap to multiple lines within the TextMeshPro component boundaries");
            _screenLogger.LogError("Testing rich text with <b>bold</b>, <i>italic</i>, and <color=#00FFFF>custom colors</color>");
            
            // Test alignment and overflow
            _screenLogger.Log("Short");
            _screenLogger.Log("Medium length message");
            _screenLogger.Log("Very long message that tests the overflow behavior and truncation settings of the TextMeshPro component to ensure it handles excessive content appropriately");
            
            _screenLogger.ForceDisplayUpdate();
            
            // Verify component state
            Assert.That(_textComponent.text, Is.Not.Null.And.Not.Empty, 
                "TextMeshPro should contain content");
            Assert.That(_textComponent.fontSize, Is.EqualTo(14), 
                "Font size should be maintained");
            Assert.That(_textComponent.textWrappingMode, Is.EqualTo(TMPro.TextWrappingModes.Normal), 
                "Word wrapping should be enabled");
            
            UnityEngine.Debug.Log("TextMeshPro component properties:");
            UnityEngine.Debug.Log($"Font Size: {_textComponent.fontSize}");
            UnityEngine.Debug.Log($"Font Style: {_textComponent.fontStyle}");
            UnityEngine.Debug.Log($"Alignment: {_textComponent.alignment}");
            UnityEngine.Debug.Log($"Text Wrapping Mode: {_textComponent.textWrappingMode}");
            UnityEngine.Debug.Log($"Overflow Mode: {_textComponent.overflowMode}");
            UnityEngine.Debug.Log($"Text Length: {_textComponent.text.Length}");
            
            UnityEngine.Debug.Log("=== MANUAL VERIFICATION REQUIRED ===");
            UnityEngine.Debug.Log("Please verify that TextMeshPro rendering appears correct and professional");
        }
        
        /// <summary>
        /// Generates a comprehensive manual testing report
        /// </summary>
        [Test]
        [Category("ManualVerification")]
        public void GenerateManualTestingReport()
        {
            UnityEngine.Debug.Log("=== MANUAL TESTING REPORT GENERATOR ===");
            UnityEngine.Debug.Log("This test generates a comprehensive report for manual testing validation");
            
            var report = new System.Text.StringBuilder();
            report.AppendLine("RUNTIME LOGGING PANEL - MANUAL TESTING CHECKLIST");
            report.AppendLine("================================================");
            report.AppendLine($"Generated: {DateTime.Now}");
            report.AppendLine($"Unity Version: {Application.unityVersion}");
            report.AppendLine($"Platform: {Application.platform}");
            report.AppendLine();
            
            report.AppendLine("VISUAL VERIFICATION CHECKLIST:");
            report.AppendLine("□ Log messages appear in correct colors (Info=Green, Warning=Orange, Error=Red)");
            report.AppendLine("□ Timestamps are formatted correctly and consistently");
            report.AppendLine("□ Log level indicators [Info], [Warning], [Error] are clearly visible");
            report.AppendLine("□ Messages appear in chronological order");
            report.AppendLine("□ Rich text formatting renders correctly");
            report.AppendLine("□ Long messages wrap or truncate appropriately");
            report.AppendLine("□ Special characters and symbols display correctly");
            report.AppendLine();
            
            report.AppendLine("FUNCTIONAL VERIFICATION CHECKLIST:");
            report.AppendLine("□ Show/Hide functionality works correctly");
            report.AppendLine("□ Clear functionality removes all messages");
            report.AppendLine("□ Messages logged while hidden appear when shown");
            report.AppendLine("□ Configuration changes apply immediately");
            report.AppendLine("□ Max log count limit is enforced");
            report.AppendLine("□ System handles error conditions gracefully");
            report.AppendLine("□ Performance remains smooth under high load");
            report.AppendLine();
            
            report.AppendLine("PERFORMANCE VERIFICATION CHECKLIST:");
            report.AppendLine("□ No stuttering or frame drops during log updates");
            report.AppendLine("□ Memory usage remains reasonable");
            report.AppendLine("□ TextMeshPro updates are smooth and responsive");
            report.AppendLine("□ System remains stable during stress testing");
            report.AppendLine();
            
            report.AppendLine("INTEGRATION VERIFICATION CHECKLIST:");
            report.AppendLine("□ LogManager integration works correctly");
            report.AppendLine("□ CompositeLogger routes messages properly");
            report.AppendLine("□ Cross-platform compatibility is maintained");
            report.AppendLine("□ TextMeshPro component integration is seamless");
            report.AppendLine();
            
            report.AppendLine("NOTES:");
            report.AppendLine("- Run each manual test scenario individually");
            report.AppendLine("- Verify both automated assertions and visual appearance");
            report.AppendLine("- Test on target platforms and devices");
            report.AppendLine("- Document any issues or unexpected behavior");
            
            string reportContent = report.ToString();
            UnityEngine.Debug.Log(reportContent);
            
            // Save report to file if possible
            try
            {
                string fileName = $"ManualTestingReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
                System.IO.File.WriteAllText(filePath, reportContent);
                UnityEngine.Debug.Log($"Manual testing report saved to: {filePath}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Could not save report to file: {ex.Message}");
            }
            
            Assert.Pass("Manual testing report generated successfully");
        }
    }
}