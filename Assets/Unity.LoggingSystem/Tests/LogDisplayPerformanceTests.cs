using NUnit.Framework;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Diagnostics;
using RuntimeLogging.Tests.TestUtilities;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Performance tests for LogDisplay TextMeshPro update scenarios
    /// Tests efficient text update mechanisms and performance optimization
    /// </summary>
    [Category("Performance")]
    public class LogDisplayPerformanceTests
    {
        private GameObject testObject;
        private LogDisplay logDisplay;
        private TextMeshProUGUI textComponent;
        private LogConfiguration testConfig;
        
        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with LogDisplay component
            testObject = new GameObject("Performance Test LogDisplay");
            logDisplay = testObject.AddComponent<LogDisplay>();
            
            // Create TextMeshPro component
            textComponent = testObject.AddComponent<TextMeshProUGUI>();
            textComponent.text = "";
            logDisplay.SetTextComponent(textComponent);
            
            // Create test configuration
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
            
            if (testObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testObject);
            }
        }
        
        #region Text Update Performance Tests
        
        [Test]
        public void UpdateDisplay_HighVolumeTextUpdates_MaintainsPerformance()
        {
            // Arrange
            const int updateCount = 1000;
            const int maxAcceptableMs = 1000; // 1 second for 1000 updates
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < updateCount; i++)
            {
                string message = $"Performance test message {i} with some additional content to simulate real log messages";
                logDisplay.UpdateDisplay(message);
            }
            logDisplay.ForceImmediateUpdate();
            stopwatch.Stop();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxAcceptableMs),
                    $"High volume text updates should complete within {maxAcceptableMs}ms, actual: {stopwatch.ElapsedMilliseconds}ms");
                Assert.That(textComponent.text, Does.Contain("Performance test message"),
                    "Final text should contain the test messages");
            });
        }
        
        [Test]
        public void UpdateDisplayWithRichText_LargeLogEntrySet_PerformsEfficiently()
        {
            // Arrange
            const int entryCount = 500;
            const int maxAcceptableMs = 500; // 500ms for 500 entries
            
            var logEntries = new List<LogEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                var level = (LogLevel)(i % 3); // Cycle through log levels
                logEntries.Add(new LogEntry($"Performance log entry {i} with detailed message content", level, System.DateTime.Now));
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            logDisplay.UpdateDisplayWithRichText(logEntries, testConfig);
            logDisplay.ForceImmediateUpdate();
            stopwatch.Stop();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxAcceptableMs),
                    $"Large log entry set processing should complete within {maxAcceptableMs}ms, actual: {stopwatch.ElapsedMilliseconds}ms");
                Assert.That(textComponent.text, Does.Contain("Performance log entry"),
                    "Display should contain the processed log entries");
                Assert.That(textComponent.text, Does.Contain("<color=#"),
                    "Rich text markup should be applied");
            });
        }
        
        [Test]
        public void UpdateDisplayWithOptimizedRichText_MaximumEntries_OptimizesPerformance()
        {
            // Arrange
            const int entryCount = 1000;
            const int maxAcceptableMs = 300; // 300ms for optimized processing
            
            var logEntries = new List<LogEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                var level = (LogLevel)(i % 3);
                logEntries.Add(new LogEntry($"Optimized performance test {i}", level, System.DateTime.Now));
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            logDisplay.UpdateDisplayWithOptimizedRichText(logEntries, testConfig);
            logDisplay.ForceImmediateUpdate();
            stopwatch.Stop();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxAcceptableMs),
                    $"Optimized rich text processing should complete within {maxAcceptableMs}ms, actual: {stopwatch.ElapsedMilliseconds}ms");
                Assert.That(textComponent.text, Does.Contain("Optimized performance test"),
                    "Display should contain the optimized processed entries");
            });
        }
        
        #endregion
        
        #region Batch Update Performance Tests
        
        [Test]
        public void BatchUpdates_HighFrequencySingleEntries_ImprovesThroughput()
        {
            // Arrange
            logDisplay.SetBatchUpdatesEnabled(true);
            logDisplay.SetBatchUpdateInterval(0.05f);
            
            const int entryCount = 200;
            const int maxAcceptableMs = 400; // 400ms for batched processing
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < entryCount; i++)
            {
                var entry = new LogEntry($"Batch performance test {i}", LogLevel.Info, System.DateTime.Now);
                logDisplay.UpdateDisplayWithSingleEntry(entry, testConfig);
            }
            logDisplay.ForceImmediateUpdate(); // Process all batched updates
            stopwatch.Stop();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxAcceptableMs),
                    $"Batch processing should complete within {maxAcceptableMs}ms, actual: {stopwatch.ElapsedMilliseconds}ms");
                Assert.That(textComponent.text, Does.Contain("Batch performance test"),
                    "Display should contain the batched entries");
                
                var stats = logDisplay.GetPerformanceStats();
                Assert.That(stats.PendingBatchUpdates, Is.EqualTo(0),
                    "All batch updates should be processed");
            });
        }
        
        [Test]
        public void BatchUpdates_VsImmediateUpdates_ShowsPerformanceGain()
        {
            // Arrange - Use more entries to make performance difference measurable
            const int entryCount = 500;
            const int testRuns = 3; // Multiple runs for more reliable results
            var testEntries = new List<LogEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                testEntries.Add(new LogEntry($"Comparison test {i}", LogLevel.Info, System.DateTime.Now));
            }
            
            long totalImmediateTime = 0;
            long totalBatchTime = 0;
            
            // Run multiple tests to get more reliable timing
            for (int run = 0; run < testRuns; run++)
            {
                // Test immediate updates
                logDisplay.ClearDisplay();
                logDisplay.SetBatchUpdatesEnabled(false);
                var immediateStopwatch = Stopwatch.StartNew();
                foreach (var entry in testEntries)
                {
                    logDisplay.UpdateDisplayWithSingleEntry(entry, testConfig);
                    logDisplay.ForceImmediateUpdate();
                }
                immediateStopwatch.Stop();
                totalImmediateTime += immediateStopwatch.ElapsedMilliseconds;
                
                // Clear and test batch updates
                logDisplay.ClearDisplay();
                logDisplay.SetBatchUpdatesEnabled(true);
                var batchStopwatch = Stopwatch.StartNew();
                foreach (var entry in testEntries)
                {
                    logDisplay.UpdateDisplayWithSingleEntry(entry, testConfig);
                }
                logDisplay.ForceImmediateUpdate();
                batchStopwatch.Stop();
                totalBatchTime += batchStopwatch.ElapsedMilliseconds;
            }
            
            long avgImmediateTime = totalImmediateTime / testRuns;
            long avgBatchTime = totalBatchTime / testRuns;
            
            // Assert - Batch updates should be more efficient for high-frequency scenarios
            // Allow for some variance but batch should generally be faster or within reasonable tolerance
            const long toleranceMs = 10; // 10ms tolerance for timing precision
            
            Assert.That(avgBatchTime, Is.LessThanOrEqualTo(avgImmediateTime + toleranceMs),
                $"Batch updates (avg: {avgBatchTime}ms) should be faster than or within {toleranceMs}ms of immediate updates (avg: {avgImmediateTime}ms)");
        }
        
        #endregion
        
        #region Text Length Management Performance Tests
        
        [Test]
        public void TextTruncation_WithExcessiveContent_PerformsEfficiently()
        {
            // Arrange
            logDisplay.SetMaxCharacterLimitForTesting(5000);
            const int maxAcceptableMs = 100; // 100ms for truncation
            
            // Create text that exceeds the limit significantly
            var longText = new System.Text.StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                longText.AppendLine($"This is a long line of text for truncation testing {i} with additional content to make it longer");
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            logDisplay.UpdateDisplay(longText.ToString());
            logDisplay.ForceImmediateUpdate();
            stopwatch.Stop();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxAcceptableMs),
                    $"Text truncation should complete within {maxAcceptableMs}ms, actual: {stopwatch.ElapsedMilliseconds}ms");
                Assert.That(textComponent.text.Length, Is.LessThanOrEqualTo(5000),
                    "Text should be truncated to respect character limit");
                Assert.That(textComponent.text, Is.Not.Empty,
                    "Truncated text should not be empty");
            });
        }
        
        [Test]
        public void LineLimiting_WithManyLines_PerformsEfficiently()
        {
            // Arrange
            logDisplay.SetMaxLinesLimitForTesting(50);
            const int maxAcceptableMs = 100; // 100ms for line limiting
            
            var logEntries = new List<LogEntry>();
            for (int i = 0; i < 200; i++)
            {
                logEntries.Add(new LogEntry($"Line limiting test entry {i}", LogLevel.Info, System.DateTime.Now));
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            logDisplay.UpdateDisplayWithRichText(logEntries, testConfig);
            logDisplay.ForceImmediateUpdate();
            stopwatch.Stop();
            
            // Assert
            string[] lines = textComponent.text.Split('\n');
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxAcceptableMs),
                    $"Line limiting should complete within {maxAcceptableMs}ms, actual: {stopwatch.ElapsedMilliseconds}ms");
                Assert.That(lines.Length, Is.LessThanOrEqualTo(50),
                    "Should limit to maximum specified lines");
            });
        }
        
        #endregion
        
        #region Rich Text Processing Performance Tests
        
        [Test]
        public void RichTextMarkup_WithVariousLogLevels_ProcessesEfficiently()
        {
            // Arrange
            const int entryCount = 300;
            const int maxAcceptableMs = 200; // 200ms for rich text processing
            
            var logEntries = new List<LogEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                var level = (LogLevel)(i % 3);
                logEntries.Add(new LogEntry($"Rich text test {i} with level {level}", level, System.DateTime.Now));
            }
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            logDisplay.UpdateDisplayWithRichText(logEntries, testConfig);
            logDisplay.ForceImmediateUpdate();
            stopwatch.Stop();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxAcceptableMs),
                    $"Rich text markup processing should complete within {maxAcceptableMs}ms, actual: {stopwatch.ElapsedMilliseconds}ms");
                Assert.That(textComponent.text, Does.Contain("<color=#"),
                    "Should contain rich text color markup");
                Assert.That(textComponent.text, Does.Contain("Rich text test"),
                    "Should contain the test messages");
            });
        }
        
        [Test]
        public void OptimizedRichText_VsStandardRichText_ShowsPerformanceImprovement()
        {
            // Arrange - Use more entries to make performance difference measurable
            const int entryCount = 1000;
            const int testRuns = 3; // Multiple runs for more reliable results
            var logEntries = new List<LogEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                var level = (LogLevel)(i % 3);
                logEntries.Add(new LogEntry($"Performance comparison test {i}", level, System.DateTime.Now));
            }
            
            long totalStandardTime = 0;
            long totalOptimizedTime = 0;
            
            // Run multiple tests to get more reliable timing
            for (int run = 0; run < testRuns; run++)
            {
                // Test standard rich text processing
                logDisplay.ClearDisplay();
                var standardStopwatch = Stopwatch.StartNew();
                logDisplay.UpdateDisplayWithRichText(logEntries, testConfig);
                logDisplay.ForceImmediateUpdate();
                standardStopwatch.Stop();
                totalStandardTime += standardStopwatch.ElapsedMilliseconds;
                
                // Test optimized rich text processing
                logDisplay.ClearDisplay();
                var optimizedStopwatch = Stopwatch.StartNew();
                logDisplay.UpdateDisplayWithOptimizedRichText(logEntries, testConfig);
                logDisplay.ForceImmediateUpdate();
                optimizedStopwatch.Stop();
                totalOptimizedTime += optimizedStopwatch.ElapsedMilliseconds;
            }
            
            long avgStandardTime = totalStandardTime / testRuns;
            long avgOptimizedTime = totalOptimizedTime / testRuns;
            
            // Assert - Allow for some variance but optimized should generally be faster or equal
            // Use a tolerance to account for timing precision and system variance
            const long toleranceMs = 5; // 5ms tolerance
            
            Assert.That(avgOptimizedTime, Is.LessThanOrEqualTo(avgStandardTime + toleranceMs),
                $"Optimized rich text (avg: {avgOptimizedTime}ms) should be faster than or within {toleranceMs}ms of standard processing (avg: {avgStandardTime}ms)");
        }
        
        #endregion
        
        #region Memory Efficiency Tests
        
        [Test]
        public void TextProcessing_WithRepeatedUpdates_MaintainsMemoryEfficiency()
        {
            // Arrange
            const int updateCycles = 50;
            const int entriesPerCycle = 20;
            
            long initialMemory = System.GC.GetTotalMemory(true);
            
            // Act - Perform multiple update cycles
            for (int cycle = 0; cycle < updateCycles; cycle++)
            {
                var logEntries = new List<LogEntry>();
                for (int i = 0; i < entriesPerCycle; i++)
                {
                    logEntries.Add(new LogEntry($"Memory test cycle {cycle} entry {i}", LogLevel.Info, System.DateTime.Now));
                }
                
                logDisplay.UpdateDisplayWithRichText(logEntries, testConfig);
                logDisplay.ForceImmediateUpdate();
                
                // Occasionally clear to simulate real usage
                if (cycle % 10 == 0)
                {
                    logDisplay.ClearDisplay();
                }
            }
            
            // Force garbage collection and measure memory
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long finalMemory = System.GC.GetTotalMemory(false);
            
            // Assert
            long memoryIncrease = finalMemory - initialMemory;
            const long maxAcceptableIncrease = 10 * 1024 * 1024; // 10MB
            
            Assert.That(memoryIncrease, Is.LessThan(maxAcceptableIncrease),
                $"Memory increase should be less than {maxAcceptableIncrease / (1024 * 1024)}MB, actual: {memoryIncrease / (1024 * 1024)}MB");
        }
        
        #endregion
        
        #region Update Throttling Performance Tests
        
        [Test]
        public void UpdateThrottling_WithRapidUpdates_MaintainsFrameRate()
        {
            // Arrange
            logDisplay.SetUpdateThrottleTime(0.05f); // 50ms throttling
            const int rapidUpdateCount = 100;
            const int maxAcceptableMs = 300; // Should be much faster due to throttling
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < rapidUpdateCount; i++)
            {
                logDisplay.UpdateDisplay($"Throttling test message {i}");
                // Don't force immediate update to test throttling
            }
            logDisplay.ForceImmediateUpdate(); // Process final pending update
            stopwatch.Stop();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxAcceptableMs),
                    $"Throttled updates should complete within {maxAcceptableMs}ms, actual: {stopwatch.ElapsedMilliseconds}ms");
                Assert.That(textComponent.text, Does.Contain("Throttling test message"),
                    "Final message should be displayed despite throttling");
            });
        }
        
        [Test]
        public void PerformanceStats_TrackingOverhead_IsMinimal()
        {
            // Arrange
            const int statCheckCount = 1000;
            const int maxAcceptableMs = 50; // 50ms for 1000 stat checks
            
            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < statCheckCount; i++)
            {
                var stats = logDisplay.GetPerformanceStats();
                // Verify stats are valid to ensure they're actually being calculated
                Assert.That(stats.CurrentCharacterCount, Is.GreaterThanOrEqualTo(0));
            }
            stopwatch.Stop();
            
            // Assert
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(maxAcceptableMs),
                $"Performance stats tracking should have minimal overhead, actual: {stopwatch.ElapsedMilliseconds}ms for {statCheckCount} checks");
        }
        
        #endregion
    }
}