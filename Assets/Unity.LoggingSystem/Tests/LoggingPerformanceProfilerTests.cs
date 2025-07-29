using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RuntimeLogging.Tests.TestUtilities;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Tests for LoggingPerformanceProfiler performance monitoring and benchmarking
    /// Validates profiling functionality and performance measurement accuracy
    /// </summary>
    [Category("Performance")]
    public class LoggingPerformanceProfilerTests
    {
        private LoggingPerformanceProfiler profiler;
        
        [SetUp]
        public void SetUp()
        {
            profiler = new LoggingPerformanceProfiler();
        }
        
        [TearDown]
        public void TearDown()
        {
            profiler?.Clear();
        }
        
        #region Basic Profiling Tests
        
        [Test]
        public void StartProfiling_WithValidOperation_InitializesMetric()
        {
            // Arrange
            string operationName = "TestOperation";
            
            // Act
            profiler.StartProfiling(operationName);
            profiler.StopProfiling(operationName);
            var stats = profiler.GetStats(operationName);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats, Is.Not.Null, "Stats should be created for the operation");
                Assert.That(stats.Name, Is.EqualTo(operationName), "Stats should have correct operation name");
                Assert.That(stats.ExecutionCount, Is.EqualTo(1), "Should track execution count");
            });
        }
        
        [Test]
        public void StopProfiling_WithoutStart_HandlesGracefully()
        {
            // Arrange
            string operationName = "NonExistentOperation";
            
            // Act & Assert
            Assert.DoesNotThrow(() => profiler.StopProfiling(operationName),
                "Should handle stopping non-existent profiling gracefully");
        }
        
        [Test]
        public void StartProfiling_WhenDisabled_DoesNothing()
        {
            // Arrange
            profiler.SetEnabled(false);
            string operationName = "DisabledOperation";
            
            // Act
            profiler.StartProfiling(operationName);
            profiler.StopProfiling(operationName);
            var stats = profiler.GetStats(operationName);
            
            // Assert
            Assert.That(stats, Is.Null,
                "Should not create stats when profiling is disabled");
        }
        
        #endregion
        
        #region Measurement Recording Tests
        
        [Test]
        public void RecordMeasurement_WithValidData_StoresCorrectly()
        {
            // Arrange
            string operationName = "MeasurementTest";
            float duration = 15.5f;
            
            // Act
            profiler.RecordMeasurement(operationName, duration);
            var stats = profiler.GetStats(operationName);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats, Is.Not.Null, "Stats should be created");
                Assert.That(stats.ExecutionCount, Is.EqualTo(1), "Should record execution");
                Assert.That(stats.TotalTimeMs, Is.EqualTo(duration), "Should record correct duration");
                Assert.That(stats.AverageTimeMs, Is.EqualTo(duration), "Average should equal single measurement");
            });
        }
        
        [Test]
        public void RecordMeasurement_MultipleTimes_CalculatesAverageCorrectly()
        {
            // Arrange
            string operationName = "AverageTest";
            float[] durations = { 10f, 20f, 30f };
            float expectedAverage = 20f;
            
            // Act
            foreach (float duration in durations)
            {
                profiler.RecordMeasurement(operationName, duration);
            }
            var stats = profiler.GetStats(operationName);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.ExecutionCount, Is.EqualTo(durations.Length), "Should count all executions");
                Assert.That(stats.AverageTimeMs, Is.EqualTo(expectedAverage), "Should calculate correct average");
                Assert.That(stats.TotalTimeMs, Is.EqualTo(60f), "Should sum total time correctly");
            });
        }
        
        [Test]
        public void RecordMeasurement_TracksMinMaxValues()
        {
            // Arrange
            string operationName = "MinMaxTest";
            float[] durations = { 25f, 5f, 45f, 15f };
            
            // Act
            foreach (float duration in durations)
            {
                profiler.RecordMeasurement(operationName, duration);
            }
            var stats = profiler.GetStats(operationName);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.MinTimeMs, Is.EqualTo(5f), "Should track minimum time");
                Assert.That(stats.MaxTimeMs, Is.EqualTo(45f), "Should track maximum time");
            });
        }
        
        #endregion
        
        #region Function Profiling Tests
        
        [Test]
        public void ProfileFunction_WithReturnValue_ReturnsCorrectResult()
        {
            // Arrange
            string operationName = "FunctionTest";
            int expectedResult = 42;
            
            // Act
            int result = profiler.ProfileFunction(operationName, () => expectedResult);
            var stats = profiler.GetStats(operationName);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expectedResult), "Should return function result");
                Assert.That(stats, Is.Not.Null, "Should create profiling stats");
                Assert.That(stats.ExecutionCount, Is.EqualTo(1), "Should record execution");
            });
        }
        
        [Test]
        public void ProfileFunction_WhenDisabled_SkipsProfiling()
        {
            // Arrange
            profiler.SetEnabled(false);
            string operationName = "DisabledFunction";
            int expectedResult = 123;
            
            // Act
            int result = profiler.ProfileFunction(operationName, () => expectedResult);
            var stats = profiler.GetStats(operationName);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expectedResult), "Should still return function result");
                Assert.That(stats, Is.Null, "Should not create stats when disabled");
            });
        }
        
        [Test]
        public void ProfileAction_ExecutesActionCorrectly()
        {
            // Arrange
            string operationName = "ActionTest";
            bool actionExecuted = false;
            
            // Act
            profiler.ProfileAction(operationName, () => actionExecuted = true);
            var stats = profiler.GetStats(operationName);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(actionExecuted, Is.True, "Action should be executed");
                Assert.That(stats, Is.Not.Null, "Should create profiling stats");
                Assert.That(stats.ExecutionCount, Is.EqualTo(1), "Should record execution");
            });
        }
        
        #endregion
        
        #region Statistics Management Tests
        
        [Test]
        public void GetAllStats_WithMultipleOperations_ReturnsAllStats()
        {
            // Arrange
            string[] operations = { "Op1", "Op2", "Op3" };
            foreach (string op in operations)
            {
                profiler.RecordMeasurement(op, 10f);
            }
            
            // Act
            var allStats = profiler.GetAllStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(allStats.Count, Is.EqualTo(operations.Length), "Should return all operation stats");
                foreach (string op in operations)
                {
                    Assert.That(allStats.ContainsKey(op), Is.True, $"Should contain stats for {op}");
                }
            });
        }
        
        [Test]
        public void GetStats_ForNonExistentOperation_ReturnsNull()
        {
            // Arrange
            string nonExistentOperation = "DoesNotExist";
            
            // Act
            var stats = profiler.GetStats(nonExistentOperation);
            
            // Assert
            Assert.That(stats, Is.Null,
                "Should return null for non-existent operation");
        }
        
        [Test]
        public void Clear_RemovesAllMetrics()
        {
            // Arrange
            profiler.RecordMeasurement("Op1", 10f);
            profiler.RecordMeasurement("Op2", 20f);
            
            // Act
            profiler.Clear();
            var allStats = profiler.GetAllStats();
            
            // Assert
            Assert.That(allStats.Count, Is.EqualTo(0),
                "Should remove all metrics after clear");
        }
        
        [Test]
        public void ResetStats_ClearsCountersButKeepsOperations()
        {
            // Arrange
            string operationName = "ResetTest";
            profiler.RecordMeasurement(operationName, 10f);
            profiler.RecordMeasurement(operationName, 20f);
            
            // Act
            profiler.ResetStats();
            var stats = profiler.GetStats(operationName);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats, Is.Not.Null, "Operation should still exist");
                Assert.That(stats.ExecutionCount, Is.EqualTo(0), "Execution count should be reset");
                Assert.That(stats.TotalTimeMs, Is.EqualTo(0f), "Total time should be reset");
            });
        }
        
        #endregion
        
        #region Sample Collection Tests
        
        [Test]
        public void Update_WhenEnabled_CollectsSamples()
        {
            // Arrange
            profiler.SetSampleInterval(0.01f); // Very short interval for testing
            
            // Act
            profiler.Update();
            System.Threading.Thread.Sleep(20); // Wait for interval
            profiler.Update();
            
            var samples = profiler.GetRecentSamples(10);
            
            // Assert
            Assert.That(samples.Count, Is.GreaterThan(0),
                "Should collect performance samples");
        }
        
        [Test]
        public void SetMaxSamples_LimitsStoredSamples()
        {
            // Arrange
            int maxSamples = 5;
            profiler.SetMaxSamples(maxSamples);
            profiler.SetSampleInterval(0.001f); // Very short interval
            
            // Act - Force many sample collections
            for (int i = 0; i < 20; i++)
            {
                profiler.Update();
                System.Threading.Thread.Sleep(2);
            }
            
            var samples = profiler.GetRecentSamples(100);
            
            // Assert
            Assert.That(samples.Count, Is.LessThanOrEqualTo(maxSamples),
                "Should not exceed maximum sample count");
        }
        
        [Test]
        public void GetRecentSamples_WithCount_ReturnsCorrectNumber()
        {
            // Arrange
            profiler.SetSampleInterval(0.001f);
            for (int i = 0; i < 10; i++)
            {
                profiler.Update();
                System.Threading.Thread.Sleep(2);
            }
            
            // Act
            var samples = profiler.GetRecentSamples(3);
            
            // Assert
            Assert.That(samples.Count, Is.LessThanOrEqualTo(3),
                "Should return requested number of recent samples");
        }
        
        #endregion
        
        #region Benchmark Tests
        
        [Test]
        public void RunBenchmark_WithValidConfig_ReturnsResults()
        {
            // Arrange
            var config = new BenchmarkConfig
            {
                LogEntryCount = 100,
                IncludeMemoryTest = true,
                IncludeTextMeshProTest = true,
                IncludeStressTest = false
            };
            
            // Act
            var results = profiler.RunBenchmark(config);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(results.Config.LogEntryCount, Is.EqualTo(config.LogEntryCount), "Should preserve config");
                Assert.That(results.StartTime, Is.LessThanOrEqualTo(results.EndTime), "End time should be after start time");
                Assert.That(results.LogEntryCreationTime, Is.GreaterThanOrEqualTo(0f), "Should measure log entry creation time");
                Assert.That(results.TextFormattingTime, Is.GreaterThanOrEqualTo(0f), "Should measure text formatting time");
                Assert.That(results.MemoryUsage, Is.GreaterThanOrEqualTo(0), "Should measure memory usage");
            });
        }
        
        [Test]
        public void RunBenchmark_WithZeroEntries_HandlesGracefully()
        {
            // Arrange
            var config = new BenchmarkConfig
            {
                LogEntryCount = 0,
                IncludeMemoryTest = false,
                IncludeTextMeshProTest = false,
                IncludeStressTest = false
            };
            
            // Act & Assert
            Assert.DoesNotThrow(() => profiler.RunBenchmark(config),
                "Should handle zero entries gracefully");
        }
        
        #endregion
        
        #region Configuration Tests
        
        [Test]
        public void SetEnabled_False_DisablesProfiling()
        {
            // Arrange
            profiler.SetEnabled(false);
            
            // Act
            profiler.RecordMeasurement("TestOp", 10f);
            var stats = profiler.GetStats("TestOp");
            
            // Assert
            Assert.That(stats, Is.Null,
                "Should not record measurements when disabled");
        }
        
        [Test]
        public void SetEnabled_True_EnablesProfiling()
        {
            // Arrange
            profiler.SetEnabled(false);
            profiler.SetEnabled(true);
            
            // Act
            profiler.RecordMeasurement("TestOp", 10f);
            var stats = profiler.GetStats("TestOp");
            
            // Assert
            Assert.That(stats, Is.Not.Null,
                "Should record measurements when re-enabled");
        }
        
        [Test]
        public void SetSampleInterval_WithValidValue_UpdatesInterval()
        {
            // Arrange
            float newInterval = 0.5f;
            
            // Act & Assert
            Assert.DoesNotThrow(() => profiler.SetSampleInterval(newInterval),
                "Should accept valid sample interval");
        }
        
        [Test]
        public void SetSampleInterval_WithTooSmallValue_ClampsToMinimum()
        {
            // Arrange
            float tooSmallInterval = 0.001f;
            
            // Act & Assert
            Assert.DoesNotThrow(() => profiler.SetSampleInterval(tooSmallInterval),
                "Should clamp small interval to minimum without throwing");
        }
        
        #endregion
        
        #region Report Generation Tests
        
        [Test]
        public void GenerateReport_WithData_ReturnsFormattedReport()
        {
            // Arrange
            profiler.RecordMeasurement("TestOp1", 10f);
            profiler.RecordMeasurement("TestOp2", 20f);
            
            // Act
            string report = profiler.GenerateReport();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(report, Does.Contain("Logging Performance Report"), "Should contain report header");
                Assert.That(report, Does.Contain("TestOp1"), "Should contain operation names");
                Assert.That(report, Does.Contain("TestOp2"), "Should contain operation names");
                Assert.That(report, Does.Contain("Executions:"), "Should contain execution information");
                Assert.That(report, Does.Contain("Average Time:"), "Should contain timing information");
            });
        }
        
        [Test]
        public void GenerateReport_WithNoData_ReturnsBasicReport()
        {
            // Arrange & Act
            string report = profiler.GenerateReport();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(report, Does.Contain("Logging Performance Report"), "Should contain report header");
                Assert.That(report, Does.Contain("Total Operations: 0"), "Should indicate no operations");
            });
        }
        
        #endregion
        
        #region Edge Cases Tests
        
        [Test]
        public void ProfileFunction_WithException_PropagatesException()
        {
            // Arrange
            string operationName = "ExceptionTest";
            var expectedException = new InvalidOperationException("Test exception");
            
            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() =>
                profiler.ProfileFunction<object>(operationName, () => throw expectedException),
                "Should propagate function exceptions");
            
            Assert.That(actualException.Message, Is.EqualTo(expectedException.Message),
                "Should preserve exception details");
        }
        
        [Test]
        public void ProfileAction_WithException_PropagatesException()
        {
            // Arrange
            string operationName = "ActionExceptionTest";
            var expectedException = new ArgumentException("Action exception");
            
            // Act & Assert
            var actualException = Assert.Throws<ArgumentException>(() =>
                profiler.ProfileAction(operationName, () => throw expectedException),
                "Should propagate action exceptions");
            
            Assert.That(actualException.Message, Is.EqualTo(expectedException.Message),
                "Should preserve exception details");
        }
        
        [Test]
        public void RecordMeasurement_WithNegativeDuration_HandlesCorrectly()
        {
            // Arrange
            string operationName = "NegativeTest";
            float negativeDuration = -5f;
            
            // Act
            profiler.RecordMeasurement(operationName, negativeDuration);
            var stats = profiler.GetStats(operationName);
            
            // Assert
            Assert.That(stats, Is.Not.Null,
                "Should handle negative duration without throwing");
        }
        
        [Test]
        public void Update_WhenNotEnabled_DoesNotCollectSamples()
        {
            // Arrange
            profiler.SetEnabled(false);
            profiler.SetSampleInterval(0.001f);
            
            // Act
            for (int i = 0; i < 10; i++)
            {
                profiler.Update();
                System.Threading.Thread.Sleep(2);
            }
            
            var samples = profiler.GetRecentSamples(100);
            
            // Assert
            Assert.That(samples.Count, Is.EqualTo(0),
                "Should not collect samples when disabled");
        }
        
        #endregion
    }
}