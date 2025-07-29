using System;
using UnityEngine;

namespace RuntimeLogging
{
    /// <summary>
    /// Monitors memory usage for the logging system and provides automatic cleanup
    /// Tracks memory consumption and triggers cleanup when approaching limits
    /// </summary>
    public class MemoryMonitor
    {
        private long _memoryThreshold = 50 * 1024 * 1024; // 50MB default threshold
        private long _criticalMemoryThreshold = 100 * 1024 * 1024; // 100MB critical threshold
        private float _monitoringInterval = 1.0f; // Check every second
        private float _lastMonitorTime = 0f;
        private bool _isMonitoring = false;
        
        // Memory statistics
        private long _initialMemory = 0;
        private long _peakMemoryUsage = 0;
        private long _currentMemoryUsage = 0;
        private int _cleanupTriggeredCount = 0;
        private int _criticalCleanupTriggeredCount = 0;
        
        // Events for memory management
        public event Action OnMemoryThresholdReached;
        public event Action OnCriticalMemoryThresholdReached;
        public event Action<MemoryStats> OnMemoryStatsUpdated;
        
        /// <summary>
        /// Starts memory monitoring
        /// </summary>
        public void StartMonitoring()
        {
            if (!_isMonitoring)
            {
                _initialMemory = GC.GetTotalMemory(false);
                _isMonitoring = true;
                _lastMonitorTime = GetThreadSafeTime();
            }
        }
        
        /// <summary>
        /// Stops memory monitoring
        /// </summary>
        public void StopMonitoring()
        {
            _isMonitoring = false;
        }
        
        /// <summary>
        /// Updates memory monitoring - should be called regularly (e.g., from Update)
        /// </summary>
        public void Update()
        {
            if (!_isMonitoring)
                return;
                
            // Use DateTime for thread-safe timing instead of Unity's Time.unscaledTime
            float currentTime = GetThreadSafeTime();
            if (currentTime - _lastMonitorTime >= _monitoringInterval)
            {
                CheckMemoryUsage();
                _lastMonitorTime = currentTime;
            }
        }
        
        /// <summary>
        /// Gets thread-safe time that works in both main thread and background threads
        /// </summary>
        private float GetThreadSafeTime()
        {
            try
            {
                // Try to use Unity's time if we're on the main thread
                return Time.unscaledTime;
            }
            catch (UnityException)
            {
                // Fall back to DateTime-based timing for background threads
                return (float)(DateTime.UtcNow - DateTime.MinValue).TotalSeconds;
            }
        }
        
        /// <summary>
        /// Forces an immediate memory check
        /// </summary>
        public void ForceMemoryCheck()
        {
            if (_isMonitoring)
            {
                CheckMemoryUsage();
            }
        }
        
        /// <summary>
        /// Sets the memory threshold for triggering cleanup
        /// </summary>
        /// <param name="thresholdBytes">Threshold in bytes</param>
        public void SetMemoryThreshold(long thresholdBytes)
        {
            _memoryThreshold = Math.Max(1024, thresholdBytes); // Minimum 1KB for testing
        }
        
        /// <summary>
        /// Sets the critical memory threshold for emergency cleanup
        /// </summary>
        /// <param name="thresholdBytes">Critical threshold in bytes</param>
        public void SetCriticalMemoryThreshold(long thresholdBytes)
        {
            _criticalMemoryThreshold = Math.Max(_memoryThreshold * 2, thresholdBytes);
        }
        
        /// <summary>
        /// Sets the monitoring interval
        /// </summary>
        /// <param name="intervalSeconds">Interval in seconds</param>
        public void SetMonitoringInterval(float intervalSeconds)
        {
            _monitoringInterval = Mathf.Max(0.1f, intervalSeconds); // Minimum 100ms
        }
        
        /// <summary>
        /// Gets current memory statistics
        /// </summary>
        public MemoryStats GetMemoryStats()
        {
            long currentMemory = GC.GetTotalMemory(false);
            _currentMemoryUsage = Math.Max(0, currentMemory - _initialMemory); // Ensure non-negative
            
            if (_currentMemoryUsage > _peakMemoryUsage)
            {
                _peakMemoryUsage = _currentMemoryUsage;
            }
            
            return new MemoryStats
            {
                InitialMemory = _initialMemory,
                CurrentTotalMemory = currentMemory,
                CurrentUsage = _currentMemoryUsage,
                PeakUsage = _peakMemoryUsage,
                MemoryThreshold = _memoryThreshold,
                CriticalMemoryThreshold = _criticalMemoryThreshold,
                CleanupTriggeredCount = _cleanupTriggeredCount,
                CriticalCleanupTriggeredCount = _criticalCleanupTriggeredCount,
                IsMonitoring = _isMonitoring
            };
        }
        
        /// <summary>
        /// Triggers garbage collection and returns memory freed
        /// </summary>
        public long TriggerGarbageCollection()
        {
            long memoryBefore = GC.GetTotalMemory(false);
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            long memoryAfter = GC.GetTotalMemory(false);
            return memoryBefore - memoryAfter;
        }
        
        /// <summary>
        /// Resets memory statistics
        /// </summary>
        public void ResetStats()
        {
            _initialMemory = GC.GetTotalMemory(false);
            _peakMemoryUsage = 0;
            _currentMemoryUsage = 0;
            _cleanupTriggeredCount = 0;
            _criticalCleanupTriggeredCount = 0;
        }
        
        private void CheckMemoryUsage()
        {
            var stats = GetMemoryStats();
            
            // Trigger events based on memory usage
            if (stats.CurrentUsage >= _criticalMemoryThreshold)
            {
                _criticalCleanupTriggeredCount++;
                OnCriticalMemoryThresholdReached?.Invoke();
            }
            else if (stats.CurrentUsage >= _memoryThreshold)
            {
                _cleanupTriggeredCount++;
                OnMemoryThresholdReached?.Invoke();
            }
            
            // Always notify about stats update
            OnMemoryStatsUpdated?.Invoke(stats);
        }
    }
    
    /// <summary>
    /// Memory usage statistics
    /// </summary>
    public struct MemoryStats
    {
        public long InitialMemory;
        public long CurrentTotalMemory;
        public long CurrentUsage;
        public long PeakUsage;
        public long MemoryThreshold;
        public long CriticalMemoryThreshold;
        public int CleanupTriggeredCount;
        public int CriticalCleanupTriggeredCount;
        public bool IsMonitoring;
        
        public float UsagePercentageOfThreshold => MemoryThreshold > 0 ? (float)CurrentUsage / MemoryThreshold * 100f : 0f;
        public float UsagePercentageOfCritical => CriticalMemoryThreshold > 0 ? (float)CurrentUsage / CriticalMemoryThreshold * 100f : 0f;
        
        public override string ToString()
        {
            return $"Memory Usage: {CurrentUsage / (1024 * 1024)}MB / {MemoryThreshold / (1024 * 1024)}MB " +
                   $"({UsagePercentageOfThreshold:F1}%), Peak: {PeakUsage / (1024 * 1024)}MB, " +
                   $"Cleanups: {CleanupTriggeredCount}, Critical: {CriticalCleanupTriggeredCount}";
        }
    }
}