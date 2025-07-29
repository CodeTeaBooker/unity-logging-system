using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace RuntimeLogging
{
    /// <summary>
    /// Manages log data storage, formatting, and memory optimization for ScreenLogger.
    /// Provides thread-safe circular buffer with configurable size limits and performance optimizations.
    /// </summary>
    public class LogDataManager : IDisposable
    {
        private readonly object _lockObject = new object();
        private readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();
        private readonly LogEntryPool _logEntryPool = new LogEntryPool();
        private readonly MemoryMonitor _memoryMonitor = new MemoryMonitor();
        private readonly TextMeshProOptimizer _textOptimizer = new TextMeshProOptimizer();
        
        private int _maxLogCount = 100;
        private LogConfiguration _configuration;
        private bool _performanceOptimizationsEnabled = true;
        private bool _disposed = false;
        
        /// <summary>
        /// Event fired when a new log entry is added
        /// </summary>
        public event Action<LogEntry> OnLogAdded;
        
        /// <summary>
        /// Event fired when all logs are cleared
        /// </summary>
        public event Action OnLogsCleared;
        
        /// <summary>
        /// Constructor with optional configuration
        /// </summary>
        /// <param name="configuration">Optional configuration for log formatting</param>
        public LogDataManager(LogConfiguration configuration = null)
        {
            _configuration = configuration;
            if (_configuration != null)
            {
                _maxLogCount = _configuration.maxLogCount;
            }
            
            // Initialize performance monitoring
            _memoryMonitor.StartMonitoring();
            _memoryMonitor.OnMemoryThresholdReached += HandleMemoryThresholdReached;
            _memoryMonitor.OnCriticalMemoryThresholdReached += HandleCriticalMemoryThresholdReached;
        }
        
        /// <summary>
        /// Adds a new log entry to the buffer with automatic cleanup and performance optimizations
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The log level</param>
        public void AddLog(string message, LogLevel level)
        {
            if (string.IsNullOrEmpty(message))
                return;
                
            LogEntry logEntry;
            
            if (_performanceOptimizationsEnabled)
            {
                // Use object pooling to reduce garbage collection
                logEntry = _logEntryPool.Get(message, level, DateTime.Now);
            }
            else
            {
                logEntry = new LogEntry(message, level, DateTime.Now);
            }
            
            lock (_lockObject)
            {
                // Add new log entry
                _logQueue.Enqueue(logEntry);
                
                // Remove oldest entries if we exceed the limit
                while (_logQueue.Count > _maxLogCount)
                {
                    var removedEntry = _logQueue.Dequeue();
                    
                    // Return removed entry to pool if optimizations are enabled
                    if (_performanceOptimizationsEnabled)
                    {
                        _logEntryPool.Return(removedEntry);
                    }
                }
            }
            
            // Update memory monitoring (only if not in concurrent context to avoid Unity thread issues)
            if (_performanceOptimizationsEnabled)
            {
                UpdateMemoryMonitoring();
            }
            
            // Fire event outside of lock to prevent deadlocks
            OnLogAdded?.Invoke(logEntry);
        }
        
        /// <summary>
        /// Clears all log entries from the buffer with performance optimizations
        /// </summary>
        public void ClearLogs()
        {
            lock (_lockObject)
            {
                // Return all entries to pool before clearing if optimizations are enabled
                if (_performanceOptimizationsEnabled)
                {
                    while (_logQueue.Count > 0)
                    {
                        var entry = _logQueue.Dequeue();
                        _logEntryPool.Return(entry);
                    }
                }
                else
                {
                    _logQueue.Clear();
                }
            }
            
            // Fire event outside of lock
            OnLogsCleared?.Invoke();
        }
        
        /// <summary>
        /// Gets a read-only copy of all current log entries
        /// </summary>
        /// <returns>Read-only list of log entries in chronological order</returns>
        public IReadOnlyList<LogEntry> GetLogs()
        {
            lock (_lockObject)
            {
                return _logQueue.ToArray();
            }
        }
        
        /// <summary>
        /// Sets the maximum number of log entries to store
        /// </summary>
        /// <param name="count">Maximum log count (clamped between 1 and 1000)</param>
        public void SetMaxLogCount(int count)
        {
            count = Mathf.Clamp(count, 1, 1000);
            
            lock (_lockObject)
            {
                _maxLogCount = count;
                
                // Remove excess entries if new limit is smaller
                while (_logQueue.Count > _maxLogCount)
                {
                    var removedEntry = _logQueue.Dequeue();
                    
                    // Return removed entry to pool if optimizations are enabled
                    if (_performanceOptimizationsEnabled)
                    {
                        _logEntryPool.Return(removedEntry);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets the current maximum log count
        /// </summary>
        public int GetMaxLogCount()
        {
            lock (_lockObject)
            {
                return _maxLogCount;
            }
        }
        
        /// <summary>
        /// Gets the current number of stored log entries
        /// </summary>
        public int GetLogCount()
        {
            lock (_lockObject)
            {
                return _logQueue.Count;
            }
        }
        
        /// <summary>
        /// Updates the configuration used for log formatting
        /// </summary>
        /// <param name="configuration">New configuration</param>
        public void SetConfiguration(LogConfiguration configuration)
        {
            _configuration = configuration;
            if (_configuration != null)
            {
                SetMaxLogCount(_configuration.maxLogCount);
            }
        }
        
        /// <summary>
        /// Gets the current configuration
        /// </summary>
        public LogConfiguration GetConfiguration()
        {
            return _configuration;
        }
        
        /// <summary>
        /// Gets formatted log entries with timestamps and level indicators
        /// </summary>
        /// <returns>Array of formatted log messages</returns>
        public string[] GetFormattedLogs()
        {
            var logs = GetLogs();
            var formattedLogs = new string[logs.Count];
            
            string timestampFormat = _configuration?.timestampFormat ?? "HH:mm:ss";
            
            for (int i = 0; i < logs.Count; i++)
            {
                formattedLogs[i] = logs[i].GetFormattedMessage(timestampFormat);
            }
            
            return formattedLogs;
        }
        
        /// <summary>
        /// Checks if the buffer is at capacity
        /// </summary>
        /// <returns>True if buffer is full</returns>
        public bool IsAtCapacity()
        {
            lock (_lockObject)
            {
                return _logQueue.Count >= _maxLogCount;
            }
        }
        
        /// <summary>
        /// Gets memory usage statistics for the log buffer
        /// </summary>
        /// <returns>Estimated memory usage in bytes</returns>
        public long GetEstimatedMemoryUsage()
        {
            lock (_lockObject)
            {
                long totalBytes = 0;
                
                foreach (var entry in _logQueue)
                {
                    // Rough estimation: string length * 2 (Unicode) + struct overhead
                    totalBytes += (entry.message?.Length ?? 0) * 2;
                    totalBytes += (entry.stackTrace?.Length ?? 0) * 2;
                    totalBytes += 32; // Approximate struct overhead
                }
                
                return totalBytes;
            }
        }
        
        /// <summary>
        /// Enables or disables performance optimizations
        /// </summary>
        /// <param name="enabled">Whether performance optimizations should be enabled</param>
        public void SetPerformanceOptimizationsEnabled(bool enabled)
        {
            _performanceOptimizationsEnabled = enabled;
            
            if (enabled)
            {
                _memoryMonitor.StartMonitoring();
            }
            else
            {
                _memoryMonitor.StopMonitoring();
            }
        }
        
        /// <summary>
        /// Gets performance statistics for the log data manager
        /// </summary>
        /// <returns>Performance statistics</returns>
        public LogDataManagerPerformanceStats GetPerformanceStats()
        {
            var memoryStats = _memoryMonitor.GetMemoryStats();
            var poolStats = _logEntryPool.GetStats();
            var optimizerStats = _textOptimizer.GetStats();
            
            return new LogDataManagerPerformanceStats
            {
                LogCount = GetLogCount(),
                MaxLogCount = GetMaxLogCount(),
                EstimatedMemoryUsage = GetEstimatedMemoryUsage(),
                MemoryStats = memoryStats,
                PoolStats = poolStats,
                OptimizerStats = optimizerStats,
                PerformanceOptimizationsEnabled = _performanceOptimizationsEnabled
            };
        }
        
        /// <summary>
        /// Configures memory monitoring thresholds
        /// </summary>
        /// <param name="memoryThreshold">Memory threshold in bytes</param>
        /// <param name="criticalThreshold">Critical memory threshold in bytes</param>
        public void ConfigureMemoryMonitoring(long memoryThreshold, long criticalThreshold)
        {
            _memoryMonitor.SetMemoryThreshold(memoryThreshold);
            _memoryMonitor.SetCriticalMemoryThreshold(criticalThreshold);
        }
        
        /// <summary>
        /// Configures text optimization settings
        /// </summary>
        /// <param name="maxCharacters">Maximum character limit</param>
        /// <param name="maxLines">Maximum line limit</param>
        /// <param name="truncationStrategy">Truncation strategy</param>
        public void ConfigureTextOptimization(int maxCharacters, int maxLines, TruncationStrategy truncationStrategy)
        {
            _textOptimizer.SetMaxCharacterLimit(maxCharacters);
            _textOptimizer.SetMaxLineLimit(maxLines);
            _textOptimizer.SetTruncationStrategy(truncationStrategy);
        }
        
        /// <summary>
        /// Forces garbage collection and cleanup
        /// </summary>
        /// <returns>Amount of memory freed in bytes</returns>
        public long ForceCleanup()
        {
            return _memoryMonitor.TriggerGarbageCollection();
        }
        
        private void HandleMemoryThresholdReached()
        {
            // Reduce log count to free memory
            lock (_lockObject)
            {
                int targetCount = Mathf.RoundToInt(_maxLogCount * 0.75f); // Keep 75% of logs
                while (_logQueue.Count > targetCount)
                {
                    var removedEntry = _logQueue.Dequeue();
                    if (_performanceOptimizationsEnabled)
                    {
                        _logEntryPool.Return(removedEntry);
                    }
                }
            }
        }
        
        private void HandleCriticalMemoryThresholdReached()
        {
            // Emergency cleanup - clear half the logs
            lock (_lockObject)
            {
                int targetCount = Mathf.RoundToInt(_maxLogCount * 0.5f); // Keep 50% of logs
                while (_logQueue.Count > targetCount)
                {
                    var removedEntry = _logQueue.Dequeue();
                    if (_performanceOptimizationsEnabled)
                    {
                        _logEntryPool.Return(removedEntry);
                    }
                }
            }
            
            // Force garbage collection
            _memoryMonitor.TriggerGarbageCollection();
        }
        
        /// <summary>
        /// Generates complete formatted display text for TextMeshPro with rich text markup and performance optimizations
        /// </summary>
        /// <returns>Complete formatted text ready for TextMeshPro display</returns>
        public string GetFormattedDisplayText()
        {
            var logs = GetLogs();
            if (logs.Count == 0)
                return string.Empty;
            
            var formattedLines = new string[logs.Count];
            
            if (_configuration != null)
            {
                // Use configuration for rich text formatting
                for (int i = 0; i < logs.Count; i++)
                {
                    formattedLines[i] = logs[i].GetRichTextMessage(
                        _configuration.GetInfoColorHex(),
                        _configuration.GetWarningColorHex(),
                        _configuration.GetErrorColorHex(),
                        _configuration.timestampFormat
                    );
                }
            }
            else
            {
                // Use default formatting without rich text
                for (int i = 0; i < logs.Count; i++)
                {
                    formattedLines[i] = logs[i].GetFormattedMessage("HH:mm:ss");
                }
            }
            
            string result = string.Join("\n", formattedLines);
            
            // Apply TextMeshPro optimizations if enabled
            if (_performanceOptimizationsEnabled)
            {
                result = _textOptimizer.OptimizeTextForDisplay(result);
            }
            
            return result;
        }
        
        /// <summary>
        /// Updates memory monitoring - should be called regularly from main thread
        /// </summary>
        public void UpdateMemoryMonitoring()
        {
            if (_performanceOptimizationsEnabled)
            {
                try
                {
                    _memoryMonitor.Update();
                }
                catch (UnityEngine.UnityException)
                {
                    // Skip memory monitoring update if called from background thread
                    // This is acceptable as memory monitoring will be updated on next main thread call
                }
            }
        }
        
        /// <summary>
        /// Gets the memory monitor instance for external monitoring
        /// </summary>
        public MemoryMonitor GetMemoryMonitor()
        {
            return _memoryMonitor;
        }
        
        /// <summary>
        /// Gets the text optimizer instance for external configuration
        /// </summary>
        public TextMeshProOptimizer GetTextOptimizer()
        {
            return _textOptimizer;
        }
        
        /// <summary>
        /// Gets the log entry pool instance for external monitoring
        /// </summary>
        public LogEntryPool GetLogEntryPool()
        {
            return _logEntryPool;
        }
        
        /// <summary>
        /// Disposes of resources and unsubscribes from events to prevent memory leaks
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        
        /// <summary>
        /// Protected dispose pattern implementation
        /// </summary>
        /// <param name="disposing">Whether disposing managed resources</param>
        protected void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Unsubscribe from memory monitor events
                if (_memoryMonitor != null)
                {
                    _memoryMonitor.OnMemoryThresholdReached -= HandleMemoryThresholdReached;
                    _memoryMonitor.OnCriticalMemoryThresholdReached -= HandleCriticalMemoryThresholdReached;
                    _memoryMonitor.StopMonitoring();
                }
                
                // Clear log queue and return entries to pool
                lock (_lockObject)
                {
                    while (_logQueue.Count > 0)
                    {
                        var entry = _logQueue.Dequeue();
                        _logEntryPool.Return(entry);
                    }
                }
                
                // Clear events
                OnLogAdded = null;
                OnLogsCleared = null;
                
                _disposed = true;
            }
        }
    }
    
    /// <summary>
    /// Performance statistics for LogDataManager
    /// </summary>
    public struct LogDataManagerPerformanceStats
    {
        public int LogCount;
        public int MaxLogCount;
        public long EstimatedMemoryUsage;
        public MemoryStats MemoryStats;
        public LogEntryPoolStats PoolStats;
        public TextOptimizerStats OptimizerStats;
        public bool PerformanceOptimizationsEnabled;
        
        public override string ToString()
        {
            return $"LogDataManager: {LogCount}/{MaxLogCount} logs, " +
                   $"Memory: {EstimatedMemoryUsage / 1024}KB, " +
                   $"Optimizations: {(PerformanceOptimizationsEnabled ? "ON" : "OFF")}";
        }
    }
}