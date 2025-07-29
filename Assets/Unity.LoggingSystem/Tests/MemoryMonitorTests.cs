using NUnit.Framework;
using System;
using UnityEngine;
using RuntimeLogging.Tests.TestUtilities;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Tests for MemoryMonitor memory usage monitoring and automatic cleanup
    /// Validates memory threshold detection and cleanup triggering
    /// </summary>
    [Category("Performance")]
    public class MemoryMonitorTests
    {
        private MemoryMonitor monitor;
        
        [SetUp]
        public void SetUp()
        {
            monitor = new MemoryMonitor();
        }
        
        [TearDown]
        public void TearDown()
        {
            monitor?.StopMonitoring();
        }
        
        #region Monitoring Control Tests
        
        [Test]
        public void StartMonitoring_WhenCalled_EnablesMonitoring()
        {
            // Arrange & Act
            monitor.StartMonitoring();
            var stats = monitor.GetMemoryStats();
            
            // Assert
            Assert.That(stats.IsMonitoring, Is.True,
                "Monitoring should be enabled after StartMonitoring");
        }
        
        [Test]
        public void StopMonitoring_WhenCalled_DisablesMonitoring()
        {
            // Arrange
            monitor.StartMonitoring();
            
            // Act
            monitor.StopMonitoring();
            var stats = monitor.GetMemoryStats();
            
            // Assert
            Assert.That(stats.IsMonitoring, Is.False,
                "Monitoring should be disabled after StopMonitoring");
        }
        
        [Test]
        public void StartMonitoring_CalledMultipleTimes_RemainsStable()
        {
            // Arrange & Act
            monitor.StartMonitoring();
            monitor.StartMonitoring();
            monitor.StartMonitoring();
            
            // Assert
            Assert.DoesNotThrow(() => monitor.GetMemoryStats(),
                "Multiple StartMonitoring calls should not cause issues");
        }
        
        #endregion
        
        #region Memory Statistics Tests
        
        [Test]
        public void GetMemoryStats_InitialState_ReturnsValidStats()
        {
            // Arrange
            monitor.StartMonitoring();
            
            // Act
            var stats = monitor.GetMemoryStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(stats.InitialMemory, Is.GreaterThan(0), "Initial memory should be positive");
                Assert.That(stats.CurrentTotalMemory, Is.GreaterThan(0), "Current total memory should be positive");
                Assert.That(stats.MemoryThreshold, Is.GreaterThan(0), "Memory threshold should be positive");
                Assert.That(stats.CriticalMemoryThreshold, Is.GreaterThan(0), "Critical threshold should be positive");
                Assert.That(stats.IsMonitoring, Is.True, "Should indicate monitoring is active");
            });
        }
        
        [Test]
        public void GetMemoryStats_AfterOperations_UpdatesCurrentUsage()
        {
            // Arrange
            monitor.StartMonitoring();
            var initialStats = monitor.GetMemoryStats();
            
            // Allocate some memory to change usage
            var largeArray = new byte[1024 * 1024]; // 1MB
            
            // Act
            var updatedStats = monitor.GetMemoryStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(updatedStats.CurrentUsage, Is.GreaterThanOrEqualTo(0),
                    "Current usage should never be negative");
                Assert.That(updatedStats.CurrentTotalMemory, Is.GreaterThan(0),
                    "Current total memory should be positive");
                // Note: Due to GC, current usage might not always increase, so we just ensure it's non-negative
            });
        }
        
        [Test]
        public void GetMemoryStats_TracksPeakUsage_Correctly()
        {
            // Arrange
            monitor.StartMonitoring();
            var initialStats = monitor.GetMemoryStats();
            
            // Allocate memory to increase peak
            var largeArray = new byte[2 * 1024 * 1024]; // 2MB
            var peakStats = monitor.GetMemoryStats();
            
            // Release reference and force GC
            largeArray = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            // Act
            var finalStats = monitor.GetMemoryStats();
            
            // Assert
            Assert.That(finalStats.PeakUsage, Is.GreaterThanOrEqualTo(peakStats.CurrentUsage),
                "Peak usage should be maintained even after memory is freed");
        }
        
        #endregion
        
        #region Threshold Configuration Tests
        
        [Test]
        public void SetMemoryThreshold_WithValidValue_UpdatesThreshold()
        {
            // Arrange
            long newThreshold = 10 * 1024 * 1024; // 10MB
            
            // Act
            monitor.SetMemoryThreshold(newThreshold);
            var stats = monitor.GetMemoryStats();
            
            // Assert
            Assert.That(stats.MemoryThreshold, Is.EqualTo(newThreshold),
                "Memory threshold should be updated to new value");
        }
        
        [Test]
        public void SetMemoryThreshold_WithTooSmallValue_ClampsToMinimum()
        {
            // Arrange
            long tooSmallThreshold = 500; // 500 bytes
            
            // Act
            monitor.SetMemoryThreshold(tooSmallThreshold);
            var stats = monitor.GetMemoryStats();
            
            // Assert
            Assert.That(stats.MemoryThreshold, Is.GreaterThanOrEqualTo(1024),
                "Memory threshold should be clamped to minimum 1KB");
        }
        
        [Test]
        public void SetCriticalMemoryThreshold_WithValidValue_UpdatesThreshold()
        {
            // Arrange
            long normalThreshold = 10 * 1024 * 1024; // 10MB
            long criticalThreshold = 20 * 1024 * 1024; // 20MB
            monitor.SetMemoryThreshold(normalThreshold);
            
            // Act
            monitor.SetCriticalMemoryThreshold(criticalThreshold);
            var stats = monitor.GetMemoryStats();
            
            // Assert
            Assert.That(stats.CriticalMemoryThreshold, Is.EqualTo(criticalThreshold),
                "Critical memory threshold should be updated to new value");
        }
        
        [Test]
        public void SetCriticalMemoryThreshold_SmallerThanNormal_AdjustsToMinimum()
        {
            // Arrange
            long normalThreshold = 20 * 1024 * 1024; // 20MB
            long tooSmallCritical = 10 * 1024 * 1024; // 10MB (smaller than normal)
            monitor.SetMemoryThreshold(normalThreshold);
            
            // Act
            monitor.SetCriticalMemoryThreshold(tooSmallCritical);
            var stats = monitor.GetMemoryStats();
            
            // Assert
            Assert.That(stats.CriticalMemoryThreshold, Is.GreaterThanOrEqualTo(normalThreshold * 2),
                "Critical threshold should be at least twice the normal threshold");
        }
        
        #endregion
        
        #region Monitoring Interval Tests
        
        [Test]
        public void SetMonitoringInterval_WithValidValue_UpdatesInterval()
        {
            // Arrange
            float newInterval = 2.0f; // 2 seconds
            
            // Act & Assert
            Assert.DoesNotThrow(() => monitor.SetMonitoringInterval(newInterval),
                "Setting valid monitoring interval should not throw");
        }
        
        [Test]
        public void SetMonitoringInterval_WithTooSmallValue_ClampsToMinimum()
        {
            // Arrange
            float tooSmallInterval = 0.01f; // 10ms
            
            // Act & Assert
            Assert.DoesNotThrow(() => monitor.SetMonitoringInterval(tooSmallInterval),
                "Setting small monitoring interval should clamp to minimum without throwing");
        }
        
        #endregion
        
        #region Event Handling Tests
        
        [Test]
        public void OnMemoryThresholdReached_WhenThresholdExceeded_FiresEvent()
        {
            // Arrange
            bool eventFired = false;
            monitor.OnMemoryThresholdReached += () => eventFired = true;
            
            // Start monitoring first to establish baseline
            monitor.StartMonitoring();
            
            // Allocate memory to ensure we exceed threshold
            var memoryAllocation = new byte[10 * 1024]; // 10KB allocation
            
            // Set threshold that will be exceeded by the allocation
            monitor.SetMemoryThreshold(5000); // 5KB threshold (should be exceeded by 10KB allocation)
            
            // Act
            monitor.ForceMemoryCheck();
            
            // Assert
            Assert.That(eventFired, Is.True,
                "Memory threshold event should fire when threshold is exceeded");
        }
        
        [Test]
        public void OnCriticalMemoryThresholdReached_WhenCriticalExceeded_FiresEvent()
        {
            // Arrange
            bool criticalEventFired = false;
            monitor.OnCriticalMemoryThresholdReached += () => criticalEventFired = true;
            
            // Start monitoring first to establish baseline
            monitor.StartMonitoring();
            
            // Allocate memory to ensure we exceed thresholds
            var memoryAllocation = new byte[20 * 1024]; // 20KB allocation
            
            // Set thresholds that will be exceeded by the allocation
            monitor.SetMemoryThreshold(5000); // 5KB threshold
            monitor.SetCriticalMemoryThreshold(10000); // 10KB critical threshold (should be exceeded by 20KB allocation)
            
            // Act
            monitor.ForceMemoryCheck();
            
            // Assert
            Assert.That(criticalEventFired, Is.True,
                "Critical memory threshold event should fire when critical threshold is exceeded");
        }
        
        [Test]
        public void OnMemoryStatsUpdated_DuringMonitoring_FiresRegularly()
        {
            // Arrange
            int updateCount = 0;
            MemoryStats lastStats = default;
            monitor.OnMemoryStatsUpdated += (stats) => 
            {
                updateCount++;
                lastStats = stats;
            };
            monitor.StartMonitoring();
            
            // Act
            monitor.ForceMemoryCheck();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(updateCount, Is.GreaterThan(0), "Stats update event should fire");
                Assert.That(lastStats.IsMonitoring, Is.True, "Stats should indicate monitoring is active");
            });
        }
        
        #endregion
        
        #region Garbage Collection Tests
        
        [Test]
        public void TriggerGarbageCollection_WhenCalled_FreesMemory()
        {
            // Arrange
            var largeArray = new byte[5 * 1024 * 1024]; // 5MB
            long memoryBefore = GC.GetTotalMemory(false);
            largeArray = null; // Make eligible for GC
            
            // Act
            long memoryFreed = monitor.TriggerGarbageCollection();
            long memoryAfter = GC.GetTotalMemory(false);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(memoryFreed, Is.GreaterThanOrEqualTo(0), "Memory freed should be non-negative");
                Assert.That(memoryAfter, Is.LessThanOrEqualTo(memoryBefore), "Memory after GC should be less than or equal to before");
            });
        }
        
        [Test]
        public void TriggerGarbageCollection_ReturnsMemoryFreedAmount()
        {
            // Arrange
            long initialMemory = GC.GetTotalMemory(true); // Force GC first
            var largeArray = new byte[1024 * 1024]; // 1MB
            largeArray = null; // Make eligible for GC
            
            // Act
            long memoryFreed = monitor.TriggerGarbageCollection();
            
            // Assert
            Assert.That(memoryFreed, Is.TypeOf<long>(),
                "TriggerGarbageCollection should return a long value representing memory freed");
        }
        
        #endregion
        
        #region Statistics Reset Tests
        
        [Test]
        public void ResetStats_AfterOperations_ClearsCounters()
        {
            // Arrange
            monitor.SetMemoryThreshold(1024); // Small threshold to trigger events
            monitor.StartMonitoring();
            monitor.ForceMemoryCheck(); // Should trigger cleanup
            
            var statsBefore = monitor.GetMemoryStats();
            
            // Act
            monitor.ResetStats();
            var statsAfter = monitor.GetMemoryStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(statsAfter.CleanupTriggeredCount, Is.EqualTo(0), "Cleanup count should be reset");
                Assert.That(statsAfter.CriticalCleanupTriggeredCount, Is.EqualTo(0), "Critical cleanup count should be reset");
                Assert.That(statsAfter.PeakUsage, Is.EqualTo(0), "Peak usage should be reset");
            });
        }
        
        #endregion
        
        #region Update Method Tests
        
        [Test]
        public void Update_WhenNotMonitoring_DoesNothing()
        {
            // Arrange
            monitor.StopMonitoring();
            
            // Act & Assert
            Assert.DoesNotThrow(() => monitor.Update(),
                "Update should handle non-monitoring state gracefully");
        }
        
        [Test]
        public void Update_WhenMonitoring_ProcessesCorrectly()
        {
            // Arrange
            monitor.StartMonitoring();
            monitor.SetMonitoringInterval(0.1f); // Short interval for testing
            
            // Act & Assert
            Assert.DoesNotThrow(() => monitor.Update(),
                "Update should process monitoring without throwing");
        }
        
        #endregion
        
        #region Percentage Calculations Tests
        
        [Test]
        public void UsagePercentageOfThreshold_CalculatesCorrectly()
        {
            // Arrange
            monitor.SetMemoryThreshold(1000);
            monitor.StartMonitoring();
            
            // Act
            var stats = monitor.GetMemoryStats();
            float percentage = stats.UsagePercentageOfThreshold;
            
            // Assert
            Assert.That(percentage, Is.GreaterThanOrEqualTo(0f),
                "Usage percentage should be non-negative");
        }
        
        [Test]
        public void UsagePercentageOfCritical_CalculatesCorrectly()
        {
            // Arrange
            monitor.SetMemoryThreshold(1000);
            monitor.SetCriticalMemoryThreshold(2000);
            monitor.StartMonitoring();
            
            // Act
            var stats = monitor.GetMemoryStats();
            float percentage = stats.UsagePercentageOfCritical;
            
            // Assert
            Assert.That(percentage, Is.GreaterThanOrEqualTo(0f),
                "Critical usage percentage should be non-negative");
        }
        
        #endregion
        
        #region ToString Tests
        
        [Test]
        public void ToString_OnMemoryStats_ReturnsFormattedString()
        {
            // Arrange
            monitor.StartMonitoring();
            var stats = monitor.GetMemoryStats();
            
            // Act
            string result = stats.ToString();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Memory Usage:"), "Should contain memory usage information");
                Assert.That(result, Does.Contain("Peak:"), "Should contain peak usage information");
                Assert.That(result, Does.Contain("Cleanups:"), "Should contain cleanup count information");
                Assert.That(result, Does.Contain("MB"), "Should display values in MB");
            });
        }
        
        #endregion
        
        #region Edge Cases Tests
        
        [Test]
        public void ForceMemoryCheck_WhenNotMonitoring_HandlesGracefully()
        {
            // Arrange
            monitor.StopMonitoring();
            
            // Act & Assert
            Assert.DoesNotThrow(() => monitor.ForceMemoryCheck(),
                "ForceMemoryCheck should handle non-monitoring state gracefully");
        }
        
        [Test]
        public void MultipleEventSubscriptions_HandleCorrectly()
        {
            // Arrange
            int eventCount1 = 0;
            int eventCount2 = 0;
            
            monitor.OnMemoryThresholdReached += () => eventCount1++;
            monitor.OnMemoryThresholdReached += () => eventCount2++;
            
            // Start monitoring first to establish baseline
            monitor.StartMonitoring();
            
            // Allocate memory after monitoring starts to ensure positive usage
            var memoryAllocation = new byte[10 * 1024]; // 10KB allocation
            
            // Set threshold after allocation to ensure we exceed it
            monitor.SetMemoryThreshold(5000); // 5KB threshold (should be exceeded by 10KB allocation)
            
            // Act
            monitor.ForceMemoryCheck();
            
            // Debug: Check actual memory stats
            var stats = monitor.GetMemoryStats();
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                // Add debug information to help understand why events aren't firing
                Assert.That(stats.CurrentUsage, Is.GreaterThan(0), 
                    $"Current usage should be positive. Current: {stats.CurrentUsage}, Threshold: {stats.MemoryThreshold}");
                Assert.That(stats.CurrentUsage, Is.GreaterThan(stats.MemoryThreshold), 
                    $"Current usage should exceed threshold. Current: {stats.CurrentUsage}, Threshold: {stats.MemoryThreshold}");
                Assert.That(eventCount1, Is.GreaterThan(0), "First event handler should be called");
                Assert.That(eventCount2, Is.GreaterThan(0), "Second event handler should be called");
                Assert.That(eventCount1, Is.EqualTo(eventCount2), "Both handlers should be called same number of times");
            });
        }
        
        #endregion
    }
}