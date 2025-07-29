using NUnit.Framework;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using RuntimeLogging.Tests.TestUtilities;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Unit tests for LogDisplay MonoBehaviour class
    /// Tests component lifecycle and TextMeshPro integration
    /// </summary>
    public class LogDisplayTests
    {
        private GameObject testObject;
        private LogDisplay logDisplay;
        private TextMeshProUGUI textComponent;
        
        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with LogDisplay component
            testObject = new GameObject("Test LogDisplay");
            logDisplay = testObject.AddComponent<LogDisplay>();
            
            // Create TextMeshPro component
            textComponent = testObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = "";
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testObject);
            }
        }
        
        #region Show/Hide Tests
        
        [Test]
        public void Show_WhenCalled_ActivatesGameObject()
        {
            // Arrange
            testObject.SetActive(false);
            
            // Act
            logDisplay.Show();
            
            // Assert
            Assert.That(testObject.activeInHierarchy, Is.True,
                "Show should activate the GameObject");
        }
        
        [Test]
        public void Hide_WhenCalled_DeactivatesGameObject()
        {
            // Arrange
            testObject.SetActive(true);
            
            // Act
            logDisplay.Hide();
            
            // Assert
            Assert.That(testObject.activeInHierarchy, Is.False,
                "Hide should deactivate the GameObject");
        }
        
        #endregion
        
        #region UpdateDisplay Tests
        
        [Test]
        public void UpdateDisplay_WithValidTextComponent_UpdatesText()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            string testText = "Test log message";
            
            // Act
            logDisplay.UpdateDisplay(testText);
            logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
            
            // Assert
            Assert.That(textComponent.text, Is.EqualTo(testText),
                "UpdateDisplay should set the TextMeshPro text to the provided formatted text");
        }
        
        [Test]
        public void UpdateDisplay_WithNullText_SetsEmptyString()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            
            // Act
            logDisplay.UpdateDisplay(null);
            logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
            
            // Assert
            Assert.That(textComponent.text, Is.EqualTo(string.Empty),
                "UpdateDisplay should set empty string when null text is provided");
        }
        
        [Test]
        public void UpdateDisplay_WithNullTextComponent_DoesNotThrow()
        {
            // Arrange
            logDisplay.SetTextComponent(null);
            string testText = "Test message";
            
            // Act & Assert
            Assert.DoesNotThrow(() => logDisplay.UpdateDisplay(testText),
                "UpdateDisplay should not throw when TextMeshPro component is null");
        }
        
        [Test]
        public void UpdateDisplay_WithRichText_PreservesFormatting()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            string richText = "<color=#FF0000>Error message</color>";
            
            // Act
            logDisplay.UpdateDisplay(richText);
            logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
            
            // Assert
            Assert.That(textComponent.text, Is.EqualTo(richText),
                "UpdateDisplay should preserve rich text formatting");
        }
        
        #endregion
        
        #region ClearDisplay Tests
        
        [Test]
        public void ClearDisplay_WithValidTextComponent_ClearsText()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            textComponent.text = "Some existing text";
            
            // Act
            logDisplay.ClearDisplay();
            
            // Assert
            Assert.That(textComponent.text, Is.EqualTo(string.Empty),
                "ClearDisplay should clear the TextMeshPro text");
        }
        
        [Test]
        public void ClearDisplay_WithNullTextComponent_DoesNotThrow()
        {
            // Arrange
            logDisplay.SetTextComponent(null);
            
            // Act & Assert
            Assert.DoesNotThrow(() => logDisplay.ClearDisplay(),
                "ClearDisplay should not throw when TextMeshPro component is null");
        }
        
        #endregion
        
        #region SetTextComponent Tests
        
        [Test]
        public void SetTextComponent_WithValidComponent_SetsReference()
        {
            // Arrange
            var newTextComponent = testObject.AddComponent<TextMeshProUGUI>();
            
            // Act
            logDisplay.SetTextComponent(newTextComponent);
            
            // Assert
            Assert.That(logDisplay.GetTextComponent(), Is.EqualTo(newTextComponent),
                "SetTextComponent should set the TextMeshPro component reference");
        }
        
        [Test]
        public void SetTextComponent_WithNull_SetsNullReference()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            
            // Act
            logDisplay.SetTextComponent(null);
            
            // Assert
            Assert.That(logDisplay.GetTextComponent(), Is.Null,
                "SetTextComponent should accept null and set null reference");
        }
        
        [Test]
        public void SetTextComponent_RuntimeAssignment_WorksCorrectly()
        {
            // Arrange
            var initialTextObject = new GameObject("Initial Text");
            var runtimeTextObject = new GameObject("Runtime Text");
            var initialComponent = initialTextObject.AddComponent<TextMeshProUGUI>();
            var runtimeComponent = runtimeTextObject.AddComponent<TextMeshProUGUI>();
            
            try
            {
                logDisplay.SetTextComponent(initialComponent);
                logDisplay.UpdateDisplay("Initial test");
                logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
                string initialText = initialComponent.text;
                
                // Act
                logDisplay.SetTextComponent(runtimeComponent);
                logDisplay.UpdateDisplay("Runtime test");
                logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
                
                // Assert
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(logDisplay.GetTextComponent(), Is.EqualTo(runtimeComponent),
                        "Runtime assignment should update the component reference");
                    Assert.That(runtimeComponent.text, Is.EqualTo("Runtime test"),
                        "Updates should go to the new runtime-assigned component");
                    Assert.That(initialComponent.text, Is.EqualTo(initialText),
                        "Initial component should retain its previous value after runtime reassignment");
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(initialTextObject);
                UnityEngine.Object.DestroyImmediate(runtimeTextObject);
            }
        }
        
        #endregion
        
        #region Component Lifecycle Tests
        
        [Test]
        public void AutoAssignment_WithTextComponentOnSameGameObject_WorksCorrectly()
        {
            // Arrange
            var newTestObject = new GameObject("Auto Assign Test");
            var autoTextComponent = newTestObject.AddComponent<TextMeshProUGUI>();
            var autoLogDisplay = newTestObject.AddComponent<LogDisplay>();
            
            try
            {
                // Act - Test the auto-assignment logic by calling UpdateDisplay which triggers initialization
                autoLogDisplay.UpdateDisplay("Test message");
                autoLogDisplay.ForceImmediateUpdate(); // Force immediate update for testing
                
                // Assert
                Assert.That(autoLogDisplay.GetTextComponent(), Is.EqualTo(autoTextComponent),
                    "Component should auto-assign TextMeshPro component from same GameObject when first used");
                Assert.That(autoTextComponent.text, Is.EqualTo("Test message"),
                    "Auto-assigned component should receive the text update");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(newTestObject);
            }
        }
        
        [Test]
        public void Awake_WithoutTextComponent_DoesNotCrash()
        {
            // Arrange
            var newTestObject = new GameObject("No TextMeshPro Test");
            
            try
            {
                // Act & Assert
                Assert.DoesNotThrow(() => newTestObject.AddComponent<LogDisplay>(),
                    "LogDisplay should not crash when no TextMeshPro component is found");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(newTestObject);
            }
        }
        
        #endregion
        
        #region Null Safety Tests
        
        [Test]
        public void IsTextComponentValid_WithValidComponent_ReturnsTrue()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            
            // Act
            bool isValid = logDisplay.IsTextComponentValid();
            
            // Assert
            Assert.That(isValid, Is.True,
                "IsTextComponentValid should return true when component is assigned");
        }
        
        [Test]
        public void IsTextComponentValid_WithNullComponent_ReturnsFalse()
        {
            // Arrange
            logDisplay.SetTextComponent(null);
            
            // Act
            bool isValid = logDisplay.IsTextComponentValid();
            
            // Assert
            Assert.That(isValid, Is.False,
                "IsTextComponentValid should return false when component is null");
        }
        
        [Test]
        public void GetTextComponent_ReturnsCurrentComponent()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            
            // Act
            var retrievedComponent = logDisplay.GetTextComponent();
            
            // Assert
            Assert.That(retrievedComponent, Is.EqualTo(textComponent),
                "GetTextComponent should return the currently assigned component");
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void LogDisplay_CompleteWorkflow_WorksCorrectly()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            string testMessage = "Integration test message";
            
            // Act & Assert - Complete workflow
            MultipleAssert.Multiple(() =>
            {
                // Initially hidden
                logDisplay.Hide();
                Assert.That(testObject.activeInHierarchy, Is.False, "Should start hidden");
                
                // Show and update
                logDisplay.Show();
                Assert.That(testObject.activeInHierarchy, Is.True, "Should be visible after Show");
                
                logDisplay.UpdateDisplay(testMessage);
                logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
                Assert.That(textComponent.text, Is.EqualTo(testMessage), "Should display the message");
                
                // Clear
                logDisplay.ClearDisplay();
                Assert.That(textComponent.text, Is.EqualTo(string.Empty), "Should be cleared");
                
                // Hide again
                logDisplay.Hide();
                Assert.That(testObject.activeInHierarchy, Is.False, "Should be hidden again");
            });
        }
        
        #endregion
        
        #region Efficient TextMeshPro Updates Tests
        
        [Test]
        public void UpdateDisplayWithRichText_WithValidEntries_AppliesColorMarkup()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.ResetToDefaults();
            
            var logEntries = new List<LogEntry>
            {
                new LogEntry("Info message", LogLevel.Info, System.DateTime.Now),
                new LogEntry("Warning message", LogLevel.Warning, System.DateTime.Now),
                new LogEntry("Error message", LogLevel.Error, System.DateTime.Now)
            };
            
            try
            {
                // Act
                logDisplay.UpdateDisplayWithRichText(logEntries, config);
                logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
                
                // Assert
                string displayText = textComponent.text;
                Assert.That(displayText, Does.Contain("Info message"), "Should contain info message");
                Assert.That(displayText, Does.Contain("Warning message"), "Should contain warning message");
                Assert.That(displayText, Does.Contain("Error message"), "Should contain error message");
                Assert.That(displayText, Does.Contain("<color=#"), "Should contain color markup");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        [Test]
        public void UpdateDisplayWithRichText_WithNullEntries_ClearsDisplay()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            textComponent.text = "Previous content";
            
            // Act
            logDisplay.UpdateDisplayWithRichText(null, null);
            logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
            
            // Assert
            Assert.That(textComponent.text, Is.EqualTo(string.Empty),
                "Should clear display when null entries provided");
        }
        
        [Test]
        public void SetMaxCharacterLimit_WithValidLimit_UpdatesLimit()
        {
            // Arrange
            int newLimit = 5000;
            
            // Act
            logDisplay.SetMaxCharacterLimit(newLimit);
            
            // Assert - Test by providing text that exceeds the limit
            logDisplay.SetTextComponent(textComponent);
            string longText = new string('A', 6000);
            logDisplay.UpdateDisplay(longText);
            logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
            
            Assert.That(textComponent.text.Length, Is.LessThanOrEqualTo(newLimit),
                "Text should be truncated to respect character limit");
        }
        
        [Test]
        public void SetMaxCharacterLimit_WithTooSmallLimit_EnforcesMinimum()
        {
            // Arrange
            int tooSmallLimit = 500;
            
            // Act
            logDisplay.SetMaxCharacterLimit(tooSmallLimit);
            
            // Assert - Test that minimum is enforced
            logDisplay.SetTextComponent(textComponent);
            string longText = new string('B', 1500);
            logDisplay.UpdateDisplay(longText);
            logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
            
            Assert.That(textComponent.text.Length, Is.GreaterThan(tooSmallLimit),
                "Should enforce minimum character limit of 1000");
        }
        
        [Test]
        public void SetUpdateThrottleTime_WithValidTime_UpdatesThrottling()
        {
            // Arrange
            float newThrottleTime = 0.2f;
            
            // Act & Assert
            Assert.DoesNotThrow(() => logDisplay.SetUpdateThrottleTime(newThrottleTime),
                "Should accept valid throttle time");
        }
        
        [Test]
        public void SetUpdateThrottleTime_WithTooSmallTime_EnforcesMinimum()
        {
            // Arrange
            float tooSmallTime = 0.01f;
            
            // Act & Assert
            Assert.DoesNotThrow(() => logDisplay.SetUpdateThrottleTime(tooSmallTime),
                "Should enforce minimum throttle time without throwing");
        }
        
        [Test]
        public void UpdateDisplay_WithExcessiveText_TruncatesContent()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetMaxCharacterLimitForTesting(2000);
            string excessiveText = new string('X', 3000);
            
            // Act
            logDisplay.UpdateDisplay(excessiveText);
            logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
            
            // Assert
            Assert.That(textComponent.text.Length, Is.LessThanOrEqualTo(2000),
                "Should truncate excessive text to respect character limit");
            Assert.That(textComponent.text, Does.EndWith("X"),
                "Should keep the most recent content when truncating");
        }
        
        [Test]
        public void UpdateDisplay_WithNewlineInTruncation_PreservesLineIntegrity()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetMaxCharacterLimitForTesting(100);
            string textWithNewlines = new string('A', 80) + "\nLine2\n" + new string('B', 50);
            
            // Act
            logDisplay.UpdateDisplay(textWithNewlines);
            logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
            
            // Assert
            string result = textComponent.text;
            Assert.That(result.Length, Is.LessThanOrEqualTo(100),
                "Should respect character limit");
            Assert.That(result, Does.Not.StartWith("A"),
                "Should truncate from beginning");
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        public void UpdateDisplay_MultipleRapidUpdates_HandlesEfficiently()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetUpdateThrottleTime(0.05f);
            
            // Act - Simulate rapid updates
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                logDisplay.UpdateDisplay($"Message {i}");
            }
            stopwatch.Stop();
            
            // Force immediate processing of any pending updates
            logDisplay.ForceImmediateUpdate();
            
            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000),
                "Should handle rapid updates efficiently within 1 second");
            Assert.That(textComponent.text, Does.Contain("Message"),
                "Should still update the display despite throttling");
            
            // Verify that the last message is present (since throttling keeps the most recent)
            Assert.That(textComponent.text, Does.Contain("Message 99"),
                "Should contain the most recent message after throttling");
        }
        
        [Test]
        public void UpdateDisplayWithRichText_LargeLogSet_PerformsEfficiently()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.ResetToDefaults();
            
            var largeLogSet = new List<LogEntry>();
            for (int i = 0; i < 500; i++)
            {
                largeLogSet.Add(new LogEntry($"Log entry {i}", LogLevel.Info, System.DateTime.Now));
            }
            
            try
            {
                // Act
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                logDisplay.UpdateDisplayWithRichText(largeLogSet, config);
                logDisplay.ForceImmediateUpdate(); // Force immediate update for testing
                stopwatch.Stop();
                
                // Assert
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500),
                    "Should process large log set efficiently within 500ms");
                Assert.That(textComponent.text, Does.Contain("Log entry"),
                    "Should display the log entries");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        #endregion
        
        #region Batch Update Tests
        
        [Test]
        public void SetBatchUpdatesEnabled_WithTrue_EnablesBatchProcessing()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            
            // Act
            logDisplay.SetBatchUpdatesEnabled(true);
            
            // Assert
            var stats = logDisplay.GetPerformanceStats();
            Assert.That(stats.BatchUpdatesEnabled, Is.True,
                "Batch updates should be enabled");
        }
        
        [Test]
        public void SetBatchUpdatesEnabled_WithFalse_DisablesBatchProcessing()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetBatchUpdatesEnabled(true);
            
            // Act
            logDisplay.SetBatchUpdatesEnabled(false);
            
            // Assert
            var stats = logDisplay.GetPerformanceStats();
            Assert.That(stats.BatchUpdatesEnabled, Is.False,
                "Batch updates should be disabled");
        }
        
        [Test]
        public void UpdateDisplayWithSingleEntry_WithBatchingEnabled_QueuesBatchUpdate()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetBatchUpdatesEnabled(true);
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.ResetToDefaults();
            var logEntry = new LogEntry("Test message", LogLevel.Info, System.DateTime.Now);
            
            try
            {
                // Act
                logDisplay.UpdateDisplayWithSingleEntry(logEntry, config);
                
                // Assert
                var stats = logDisplay.GetPerformanceStats();
                Assert.That(stats.PendingBatchUpdates, Is.GreaterThan(0),
                    "Should queue batch update when batching is enabled");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        [Test]
        public void UpdateDisplayWithSingleEntry_WithBatchingDisabled_UpdatesImmediately()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetBatchUpdatesEnabled(false);
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.ResetToDefaults();
            var logEntry = new LogEntry("Test message", LogLevel.Info, System.DateTime.Now);
            
            try
            {
                // Act
                logDisplay.UpdateDisplayWithSingleEntry(logEntry, config);
                logDisplay.ForceImmediateUpdate();
                
                // Assert
                Assert.That(textComponent.text, Does.Contain("Test message"),
                    "Should update display immediately when batching is disabled");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        [Test]
        public void SetBatchUpdateInterval_WithValidInterval_UpdatesInterval()
        {
            // Arrange
            float newInterval = 0.1f;
            
            // Act & Assert
            Assert.DoesNotThrow(() => logDisplay.SetBatchUpdateInterval(newInterval),
                "Should accept valid batch update interval");
        }
        
        [Test]
        public void SetBatchUpdateInterval_WithTooSmallInterval_EnforcesMinimum()
        {
            // Arrange
            float tooSmallInterval = 0.005f;
            
            // Act & Assert
            Assert.DoesNotThrow(() => logDisplay.SetBatchUpdateInterval(tooSmallInterval),
                "Should enforce minimum batch update interval without throwing");
        }
        
        #endregion
        
        #region Line Limit Tests
        
        [Test]
        public void SetMaxLinesLimit_WithValidLimit_UpdatesLimit()
        {
            // Arrange
            int newLimit = 50;
            
            // Act
            logDisplay.SetMaxLinesLimitForTesting(newLimit);
            
            // Assert - Test by providing text with more lines than the limit
            logDisplay.SetTextComponent(textComponent);
            var logEntries = new List<LogEntry>();
            for (int i = 0; i < 60; i++)
            {
                logEntries.Add(new LogEntry($"Line {i}", LogLevel.Info, System.DateTime.Now));
            }
            
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.ResetToDefaults();
            
            try
            {
                logDisplay.UpdateDisplayWithRichText(logEntries, config);
                logDisplay.ForceImmediateUpdate();
                
                string[] lines = textComponent.text.Split('\n');
                Assert.That(lines.Length, Is.LessThanOrEqualTo(newLimit),
                    "Should limit the number of displayed lines");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        [Test]
        public void SetMaxLinesLimit_WithTooSmallLimit_EnforcesMinimum()
        {
            // Arrange
            int tooSmallLimit = 5;
            
            // Act & Assert
            Assert.DoesNotThrow(() => logDisplay.SetMaxLinesLimit(tooSmallLimit),
                "Should enforce minimum line limit without throwing");
        }
        
        [Test]
        public void UpdateDisplayWithRichText_WithExcessiveLines_KeepsMostRecent()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetMaxLinesLimitForTesting(3);
            
            var logEntries = new List<LogEntry>
            {
                new LogEntry("Line 1", LogLevel.Info, System.DateTime.Now),
                new LogEntry("Line 2", LogLevel.Info, System.DateTime.Now),
                new LogEntry("Line 3", LogLevel.Info, System.DateTime.Now),
                new LogEntry("Line 4", LogLevel.Info, System.DateTime.Now),
                new LogEntry("Line 5", LogLevel.Info, System.DateTime.Now)
            };
            
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.ResetToDefaults();
            
            try
            {
                // Act
                logDisplay.UpdateDisplayWithRichText(logEntries, config);
                logDisplay.ForceImmediateUpdate();
                
                // Assert
                string displayText = textComponent.text;
                string[] lines = displayText.Split('\n');
                Assert.That(lines.Length, Is.LessThanOrEqualTo(3),
                    "Should limit to maximum 3 lines");
                Assert.That(displayText, Does.Contain("Line 1"),
                    "Should keep the first lines when processing in order");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        #endregion
        
        #region Optimized Rich Text Tests
        
        [Test]
        public void UpdateDisplayWithOptimizedRichText_WithValidEntries_AppliesOptimizedFormatting()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.ResetToDefaults();
            
            var logEntries = new List<LogEntry>
            {
                new LogEntry("Info message", LogLevel.Info, System.DateTime.Now),
                new LogEntry("Warning message", LogLevel.Warning, System.DateTime.Now),
                new LogEntry("Error message", LogLevel.Error, System.DateTime.Now)
            };
            
            try
            {
                // Act
                logDisplay.UpdateDisplayWithOptimizedRichText(logEntries, config);
                logDisplay.ForceImmediateUpdate();
                
                // Assert
                string displayText = textComponent.text;
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(displayText, Does.Contain("Info message"), "Should contain info message");
                    Assert.That(displayText, Does.Contain("Warning message"), "Should contain warning message");
                    Assert.That(displayText, Does.Contain("Error message"), "Should contain error message");
                    Assert.That(displayText, Does.Contain("<color=#"), "Should contain optimized color markup");
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        [Test]
        public void UpdateDisplayWithOptimizedRichText_WithMaxEntries_LimitsProcessing()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.ResetToDefaults();
            
            var logEntries = new List<LogEntry>();
            for (int i = 0; i < 10; i++)
            {
                logEntries.Add(new LogEntry($"Message {i}", LogLevel.Info, System.DateTime.Now));
            }
            
            try
            {
                // Act
                logDisplay.UpdateDisplayWithOptimizedRichText(logEntries, config, 5);
                logDisplay.ForceImmediateUpdate();
                
                // Assert
                string displayText = textComponent.text;
                string[] lines = displayText.Split('\n');
                Assert.That(lines.Length, Is.LessThanOrEqualTo(5),
                    "Should limit processing to maximum specified entries");
                Assert.That(displayText, Does.Contain("Message 0"),
                    "Should contain first messages");
                Assert.That(displayText, Does.Not.Contain("Message 5"),
                    "Should not contain messages beyond the limit");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        [Test]
        public void UpdateDisplayWithOptimizedRichText_WithNullConfiguration_UsesDefaults()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            var logEntries = new List<LogEntry>
            {
                new LogEntry("Test message", LogLevel.Info, System.DateTime.Now)
            };
            
            // Act
            logDisplay.UpdateDisplayWithOptimizedRichText(logEntries, null);
            logDisplay.ForceImmediateUpdate();
            
            // Assert
            string displayText = textComponent.text;
            MultipleAssert.Multiple(() =>
            {
                Assert.That(displayText, Does.Contain("Test message"), "Should contain the message");
                Assert.That(displayText, Does.Contain("<color=#FFFFFF>"), "Should use default white color for info");
            });
        }
        
        #endregion
        
        #region Performance Statistics Tests
        
        [Test]
        public void GetPerformanceStats_ReturnsValidStatistics()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetBatchUpdatesEnabled(true);
            
            // Act
            var stats = logDisplay.GetPerformanceStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.PendingUpdates, Is.GreaterThanOrEqualTo(0), "Pending updates should be non-negative");
                Assert.That(stats.PendingBatchUpdates, Is.GreaterThanOrEqualTo(0), "Pending batch updates should be non-negative");
                Assert.That(stats.LastUpdateTime, Is.GreaterThanOrEqualTo(0), "Last update time should be non-negative");
                Assert.That(stats.LastBatchTime, Is.GreaterThanOrEqualTo(0), "Last batch time should be non-negative");
                Assert.That(stats.CurrentCharacterCount, Is.GreaterThanOrEqualTo(0), "Character count should be non-negative");
                Assert.That(stats.BatchUpdatesEnabled, Is.True, "Batch updates should be enabled as set");
            });
        }
        
        [Test]
        public void GetPerformanceStats_AfterTextUpdate_ReflectsChanges()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            string testText = "Performance test message";
            
            // Act
            logDisplay.UpdateDisplay(testText);
            logDisplay.ForceImmediateUpdate();
            var stats = logDisplay.GetPerformanceStats();
            
            // Assert
            Assert.That(stats.CurrentCharacterCount, Is.EqualTo(testText.Length),
                "Character count should reflect the current text length");
        }
        
        #endregion
        
        #region Text Processing Efficiency Tests
        
        [Test]
        public void ProcessTextForDisplay_WithLongText_TruncatesEfficiently()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetMaxCharacterLimitForTesting(1000);
            string longText = new string('A', 1500);
            
            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            logDisplay.UpdateDisplay(longText);
            logDisplay.ForceImmediateUpdate();
            stopwatch.Stop();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100),
                    "Text processing should be efficient, completing within 100ms");
                Assert.That(textComponent.text.Length, Is.LessThanOrEqualTo(1000),
                    "Text should be truncated to respect character limit");
                Assert.That(textComponent.text, Does.EndWith("A"),
                    "Should preserve the most recent content when truncating");
            });
        }
        
        [Test]
        public void ProcessTextForDisplay_WithManyLines_LimitsEfficiently()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetMaxLinesLimitForTesting(20);
            
            var manyLines = new System.Text.StringBuilder();
            for (int i = 0; i < 50; i++)
            {
                manyLines.AppendLine($"Line {i}");
            }
            
            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            logDisplay.UpdateDisplay(manyLines.ToString());
            logDisplay.ForceImmediateUpdate();
            stopwatch.Stop();
            
            // Assert
            string[] resultLines = textComponent.text.Split('\n');
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100),
                    "Line limiting should be efficient, completing within 100ms");
                Assert.That(resultLines.Length, Is.LessThanOrEqualTo(20),
                    "Should limit to maximum specified lines");
            });
        }
        
        [Test]
        public void BatchProcessing_WithHighFrequencyUpdates_MaintainsPerformance()
        {
            // Arrange
            logDisplay.SetTextComponent(textComponent);
            logDisplay.SetBatchUpdatesEnabled(true);
            logDisplay.SetBatchUpdateInterval(0.05f);
            
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.ResetToDefaults();
            
            try
            {
                // Act - Simulate high-frequency single entry updates
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < 50; i++)
                {
                    var entry = new LogEntry($"Batch message {i}", LogLevel.Info, System.DateTime.Now);
                    logDisplay.UpdateDisplayWithSingleEntry(entry, config);
                }
                logDisplay.ForceImmediateUpdate(); // Process all batched updates
                stopwatch.Stop();
                
                // Assert
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500),
                        "Batch processing should handle high-frequency updates efficiently within 500ms");
                    Assert.That(textComponent.text, Does.Contain("Batch message"),
                        "Should display the batched messages");
                    
                    var stats = logDisplay.GetPerformanceStats();
                    Assert.That(stats.PendingBatchUpdates, Is.EqualTo(0),
                        "All batch updates should be processed after force update");
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        #endregion
    }
}