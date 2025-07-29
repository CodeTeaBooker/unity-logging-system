using NUnit.Framework;
using UnityEngine;
using TMPro;
using RuntimeLogging.Tests.TestUtilities;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Tests for TextMeshProOptimizer text optimization and performance features
    /// Validates text truncation strategies and performance optimizations
    /// </summary>
    [Category("Performance")]
    public class TextMeshProOptimizerTests
    {
        private TextMeshProOptimizer optimizer;
        private GameObject testObject;
        private TextMeshProUGUI textComponent;
        
        [SetUp]
        public void SetUp()
        {
            optimizer = new TextMeshProOptimizer();
            
            // Create test TextMeshPro component
            testObject = new GameObject("Test TextMeshPro");
            textComponent = testObject.AddComponent<TextMeshProUGUI>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testObject);
            }
        }
        
        #region Basic Optimization Tests
        
        [Test]
        public void OptimizeTextForDisplay_WithNullText_ReturnsEmpty()
        {
            // Arrange
            string nullText = null;
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(nullText);
            
            // Assert
            Assert.That(result, Is.EqualTo(string.Empty),
                "Null text should return empty string");
        }
        
        [Test]
        public void OptimizeTextForDisplay_WithEmptyText_ReturnsEmpty()
        {
            // Arrange
            string emptyText = string.Empty;
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(emptyText);
            
            // Assert
            Assert.That(result, Is.EqualTo(string.Empty),
                "Empty text should return empty string");
        }
        
        [Test]
        public void OptimizeTextForDisplay_WithShortText_ReturnsUnchanged()
        {
            // Arrange
            string shortText = "Short test message";
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(shortText);
            
            // Assert
            Assert.That(result, Is.EqualTo(shortText),
                "Short text should remain unchanged");
        }
        
        [Test]
        public void OptimizeTextForDisplay_WithLongText_TruncatesCorrectly()
        {
            // Arrange
            optimizer.SetMaxCharacterLimit(100);
            string longText = new string('A', 200); // 200 characters
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(longText);
            
            // Assert
            Assert.That(result.Length, Is.LessThanOrEqualTo(100),
                "Long text should be truncated to character limit");
        }
        
        #endregion
        
        #region Character Limit Tests
        
        [Test]
        public void SetMaxCharacterLimit_WithValidValue_UpdatesLimit()
        {
            // Arrange
            int newLimit = 5000;
            
            // Act
            optimizer.SetMaxCharacterLimit(newLimit);
            var stats = optimizer.GetStats();
            
            // Assert
            Assert.That(stats.MaxCharacterLimit, Is.EqualTo(newLimit),
                "Character limit should be updated");
        }
        
        [Test]
        public void SetMaxCharacterLimit_WithTooSmallValue_ClampsToMinimum()
        {
            // Arrange
            int tooSmallLimit = 5;
            
            // Act
            optimizer.SetMaxCharacterLimit(tooSmallLimit);
            var stats = optimizer.GetStats();
            
            // Assert
            Assert.That(stats.MaxCharacterLimit, Is.GreaterThanOrEqualTo(10),
                "Character limit should be clamped to minimum");
        }
        
        [Test]
        public void OptimizeTextForDisplay_ExceedsCharacterLimit_TruncatesText()
        {
            // Arrange
            optimizer.SetMaxCharacterLimit(50);
            string longText = "This is a very long text that exceeds the character limit and should be truncated";
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(longText);
            
            // Assert
            Assert.That(result.Length, Is.LessThanOrEqualTo(50),
                "Text should be truncated to character limit");
        }
        
        #endregion
        
        #region Line Limit Tests
        
        [Test]
        public void SetMaxLineLimit_WithValidValue_UpdatesLimit()
        {
            // Arrange
            int newLimit = 50;
            
            // Act
            optimizer.SetMaxLineLimit(newLimit);
            var stats = optimizer.GetStats();
            
            // Assert
            Assert.That(stats.MaxLineLimit, Is.EqualTo(newLimit),
                "Line limit should be updated");
        }
        
        [Test]
        public void SetMaxLineLimit_WithTooSmallValue_ClampsToMinimum()
        {
            // Arrange
            int tooSmallLimit = 0;
            
            // Act
            optimizer.SetMaxLineLimit(tooSmallLimit);
            var stats = optimizer.GetStats();
            
            // Assert
            Assert.That(stats.MaxLineLimit, Is.GreaterThanOrEqualTo(1),
                "Line limit should be clamped to minimum");
        }
        
        [Test]
        public void OptimizeTextForDisplay_ExceedsLineLimit_TruncatesLines()
        {
            // Arrange
            optimizer.SetMaxLineLimit(3);
            string multiLineText = "Line 1\nLine 2\nLine 3\nLine 4\nLine 5";
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(multiLineText);
            string[] lines = result.Split('\n');
            
            // Assert
            Assert.That(lines.Length, Is.LessThanOrEqualTo(3),
                "Text should be truncated to line limit");
        }
        
        #endregion
        
        #region Truncation Strategy Tests
        
        [Test]
        public void SetTruncationStrategy_RemoveOldest_KeepsNewestContent()
        {
            // Arrange
            optimizer.SetTruncationStrategy(TruncationStrategy.RemoveOldest);
            optimizer.SetMaxLineLimit(2);
            string multiLineText = "Old Line 1\nOld Line 2\nNew Line 1\nNew Line 2";
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(multiLineText);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("New Line"), "Should contain newest content");
                Assert.That(result.Split('\n').Length, Is.LessThanOrEqualTo(2), "Should respect line limit");
            });
        }
        
        [Test]
        public void SetTruncationStrategy_RemoveNewest_KeepsOldestContent()
        {
            // Arrange
            optimizer.SetTruncationStrategy(TruncationStrategy.RemoveNewest);
            optimizer.SetMaxLineLimit(2);
            string multiLineText = "Old Line 1\nOld Line 2\nNew Line 1\nNew Line 2";
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(multiLineText);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Old Line"), "Should contain oldest content");
                Assert.That(result.Split('\n').Length, Is.LessThanOrEqualTo(2), "Should respect line limit");
            });
        }
        
        [Test]
        public void SetTruncationStrategy_RemoveMiddle_KeepsStartAndEnd()
        {
            // Arrange
            optimizer.SetTruncationStrategy(TruncationStrategy.RemoveMiddle);
            optimizer.SetMaxLineLimit(4);
            string multiLineText = "Start 1\nStart 2\nMiddle 1\nMiddle 2\nMiddle 3\nEnd 1\nEnd 2";
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(multiLineText);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Start"), "Should contain start content");
                Assert.That(result, Does.Contain("End"), "Should contain end content");
                Assert.That(result, Does.Contain("truncated"), "Should indicate truncation");
            });
        }
        
        #endregion
        
        #region Truncation Ratio Tests
        
        [Test]
        public void SetTruncationRatio_WithValidValue_UpdatesRatio()
        {
            // Arrange
            float newRatio = 0.6f;
            
            // Act
            optimizer.SetTruncationRatio(newRatio);
            var stats = optimizer.GetStats();
            
            // Assert
            Assert.That(stats.TruncationRatio, Is.EqualTo(newRatio),
                "Truncation ratio should be updated");
        }
        
        [Test]
        public void SetTruncationRatio_WithInvalidValue_ClampsToValidRange()
        {
            // Arrange
            float invalidRatio = 1.5f; // Greater than 1.0
            
            // Act
            optimizer.SetTruncationRatio(invalidRatio);
            var stats = optimizer.GetStats();
            
            // Assert
            Assert.That(stats.TruncationRatio, Is.InRange(0f, 1f),
                "Truncation ratio should be clamped to valid range");
        }
        
        [Test]
        public void OptimizeTextForDisplay_WithCustomRatio_TruncatesCorrectly()
        {
            // Arrange
            optimizer.SetMaxCharacterLimit(100);
            optimizer.SetTruncationRatio(0.5f); // Keep 50%
            string longText = new string('A', 200);
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(longText);
            
            // Assert
            Assert.That(result.Length, Is.LessThanOrEqualTo(50), // 50% of 100
                "Text should be truncated according to ratio");
        }
        
        #endregion
        
        #region Performance Timing Tests
        
        [Test]
        public void SetTargetFrameTime_WithValidValue_UpdatesSettings()
        {
            // Arrange
            float newFrameTime = 0.033f; // 30 FPS
            
            // Act
            optimizer.SetTargetFrameTime(newFrameTime);
            var stats = optimizer.GetStats();
            
            // Assert
            Assert.That(stats.TargetFrameTimeMs, Is.EqualTo(newFrameTime * 1000f),
                "Target frame time should be updated");
        }
        
        [Test]
        public void SetTargetFrameTime_WithTooSmallValue_ClampsToMinimum()
        {
            // Arrange
            float tooSmallFrameTime = 0.0001f; // 0.1ms
            
            // Act
            optimizer.SetTargetFrameTime(tooSmallFrameTime);
            var stats = optimizer.GetStats();
            
            // Assert
            Assert.That(stats.TargetFrameTimeMs, Is.GreaterThanOrEqualTo(1f),
                "Target frame time should be clamped to minimum");
        }
        
        [Test]
        public void OptimizeTextForDisplay_TracksProcessingTime()
        {
            // Arrange
            string testText = "Test message for timing";
            
            // Act
            optimizer.OptimizeTextForDisplay(testText);
            var stats = optimizer.GetStats();
            
            // Assert
            Assert.That(stats.LastProcessingTimeMs, Is.GreaterThanOrEqualTo(0f),
                "Processing time should be tracked");
        }
        
        #endregion
        
        #region Incremental Optimization Tests
        
        [Test]
        public void OptimizeTextIncremental_WithShortText_CompletesImmediately()
        {
            // Arrange
            string shortText = "Short text";
            
            // Act
            var result = optimizer.OptimizeTextIncremental(shortText);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IsComplete, Is.True, "Short text optimization should complete immediately");
                Assert.That(result.OptimizedText, Is.EqualTo(shortText), "Short text should remain unchanged");
            });
        }
        
        [Test]
        public void OptimizeTextIncremental_WithNullText_ReturnsEmptyComplete()
        {
            // Arrange
            string nullText = null;
            
            // Act
            var result = optimizer.OptimizeTextIncremental(nullText);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IsComplete, Is.True, "Null text should complete immediately");
                Assert.That(result.OptimizedText, Is.EqualTo(string.Empty), "Null text should return empty");
            });
        }
        
        [Test]
        public void OptimizeTextIncremental_WithLongText_ProcessesCorrectly()
        {
            // Arrange
            optimizer.SetMaxCharacterLimit(100);
            string longText = new string('A', 200);
            
            // Act
            var result = optimizer.OptimizeTextIncremental(longText, 10); // 10ms limit
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.OptimizedText, Is.Not.Null, "Should return optimized text");
                Assert.That(result.OptimizedText.Length, Is.LessThanOrEqualTo(100), "Should respect character limit");
            });
        }
        
        #endregion
        
        #region TextMeshPro Integration Tests
        
        [Test]
        public void OptimizeTextForDisplay_WithTextMeshProComponent_AppliesOptimizations()
        {
            // Arrange
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            optimizer.SetMaxCharacterLimit(100);
            string longText = new string('A', 200);
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(longText, textComponent);
            
            // Assert
            Assert.That(result.Length, Is.LessThanOrEqualTo(100),
                "Should apply TextMeshPro-specific optimizations");
        }
        
        [Test]
        public void OptimizeTextForDisplay_WithNoWrapTextMeshPro_ProcessesNormally()
        {
            // Arrange
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            string testText = "Test message";
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(testText, textComponent);
            
            // Assert
            Assert.That(result, Is.EqualTo(testText),
                "Should process normally for no-wrap TextMeshPro");
        }
        
        #endregion
        
        #region Statistics Tests
        
        [Test]
        public void GetStats_InitialState_ReturnsDefaultValues()
        {
            // Arrange & Act
            var stats = optimizer.GetStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.TruncationCount, Is.EqualTo(0), "Initial truncation count should be zero");
                Assert.That(stats.OptimizationCount, Is.EqualTo(0), "Initial optimization count should be zero");
                Assert.That(stats.LastProcessingTimeMs, Is.EqualTo(0f), "Initial processing time should be zero");
                Assert.That(stats.MaxCharacterLimit, Is.GreaterThan(0), "Should have default character limit");
                Assert.That(stats.MaxLineLimit, Is.GreaterThan(0), "Should have default line limit");
            });
        }
        
        [Test]
        public void GetStats_AfterOptimizations_UpdatesCounts()
        {
            // Arrange
            optimizer.SetMaxCharacterLimit(50);
            string longText = new string('A', 100);
            
            // Act
            optimizer.OptimizeTextForDisplay(longText);
            optimizer.OptimizeTextForDisplay("Short");
            var stats = optimizer.GetStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.OptimizationCount, Is.EqualTo(2), "Should count all optimizations");
                Assert.That(stats.TruncationCount, Is.EqualTo(1), "Should count truncations");
                Assert.That(stats.LastProcessingTimeMs, Is.GreaterThanOrEqualTo(0f), "Should track processing time");
            });
        }
        
        [Test]
        public void ResetStats_AfterOperations_ClearsCounters()
        {
            // Arrange
            optimizer.SetMaxCharacterLimit(50);
            optimizer.OptimizeTextForDisplay(new string('A', 100));
            
            // Act
            optimizer.ResetStats();
            var stats = optimizer.GetStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.TruncationCount, Is.EqualTo(0), "Truncation count should be reset");
                Assert.That(stats.OptimizationCount, Is.EqualTo(0), "Optimization count should be reset");
                Assert.That(stats.LastProcessingTimeMs, Is.EqualTo(0f), "Processing time should be reset");
            });
        }
        
        [Test]
        public void ToString_OnStats_ReturnsFormattedString()
        {
            // Arrange
            optimizer.OptimizeTextForDisplay("Test");
            var stats = optimizer.GetStats();
            
            // Act
            string result = stats.ToString();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Optimizer:"), "Should contain optimizer information");
                Assert.That(result, Does.Contain("optimizations"), "Should contain optimization count");
                Assert.That(result, Does.Contain("truncations"), "Should contain truncation count");
                Assert.That(result, Does.Contain("Limits:"), "Should contain limit information");
            });
        }
        
        #endregion
        
        #region Edge Cases Tests
        
        [Test]
        public void OptimizeTextForDisplay_WithOnlyNewlines_HandlesCorrectly()
        {
            // Arrange
            string newlineText = "\n\n\n\n\n";
            optimizer.SetMaxLineLimit(2);
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(newlineText);
            
            // Assert
            Assert.That(result.Split('\n').Length, Is.LessThanOrEqualTo(2),
                "Should handle newline-only text correctly");
        }
        
        [Test]
        public void OptimizeTextForDisplay_WithMixedContent_ProcessesCorrectly()
        {
            // Arrange
            string mixedText = "Normal text\n\nEmpty line above\nAnother line";
            optimizer.SetMaxLineLimit(2);
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(mixedText);
            
            // Assert
            Assert.That(result, Is.Not.Null.And.Not.Empty,
                "Should handle mixed content correctly");
        }
        
        [Test]
        public void OptimizeTextForDisplay_WithVeryLongSingleLine_TruncatesCorrectly()
        {
            // Arrange
            string veryLongLine = new string('X', 10000);
            optimizer.SetMaxCharacterLimit(100);
            
            // Act
            string result = optimizer.OptimizeTextForDisplay(veryLongLine);
            
            // Assert
            Assert.That(result.Length, Is.LessThanOrEqualTo(100),
                "Should truncate very long single line");
        }
        
        #endregion
    }
}