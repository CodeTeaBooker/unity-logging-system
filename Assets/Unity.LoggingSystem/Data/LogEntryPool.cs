using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeLogging
{
    /// <summary>
    /// Object pool for LogEntry instances to reduce garbage collection
    /// Provides efficient reuse of log entry objects for high-frequency logging scenarios
    /// </summary>
    public class LogEntryPool
    {
        private readonly Stack<LogEntry> _pool = new Stack<LogEntry>();
        private readonly object _lockObject = new object();
        private int _maxPoolSize = 1000;
        private long _totalAllocations = 0;
        private long _totalReuses = 0;
        
        /// <summary>
        /// Gets a LogEntry from the pool or creates a new one if pool is empty
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="level">Log level</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="stackTrace">Optional stack trace</param>
        /// <returns>LogEntry instance ready for use</returns>
        public LogEntry Get(string message, LogLevel level, DateTime timestamp, string stackTrace = null)
        {
            LogEntry entry;
            
            lock (_lockObject)
            {
                if (_pool.Count > 0)
                {
                    entry = _pool.Pop();
                    _totalReuses++;
                }
                else
                {
                    entry = new LogEntry();
                    _totalAllocations++;
                }
            }
            
            // Initialize the entry with new values (reuse the existing object)
            entry.Initialize(message, level, timestamp, stackTrace);
            return entry;
        }
        
        /// <summary>
        /// Returns a LogEntry to the pool for reuse
        /// </summary>
        /// <param name="entry">LogEntry to return to pool</param>
        public void Return(LogEntry entry)
        {
            if (entry == null) return;
            
            lock (_lockObject)
            {
                if (_pool.Count < _maxPoolSize)
                {
                    // Completely reset the entry data before returning to pool
                    ResetLogEntry(entry);
                    _pool.Push(entry);
                }
                // If pool is full, just let the entry be garbage collected
            }
        }
        
        /// <summary>
        /// Completely resets a LogEntry to its default state
        /// </summary>
        /// <param name="entry">LogEntry to reset</param>
        private void ResetLogEntry(LogEntry entry)
        {
            if (entry == null) return;
            
            // Reset all fields to their default values
            entry.message = string.Empty;
            entry.level = LogLevel.Info;
            entry.timestamp = DateTime.MinValue;
            entry.stackTrace = string.Empty;
        }
        
        /// <summary>
        /// Sets the maximum pool size
        /// </summary>
        /// <param name="maxSize">Maximum number of entries to keep in pool</param>
        public void SetMaxPoolSize(int maxSize)
        {
            lock (_lockObject)
            {
                _maxPoolSize = Mathf.Max(1, maxSize);
                
                // Trim pool if it's now too large
                while (_pool.Count > _maxPoolSize)
                {
                    _pool.Pop();
                }
            }
        }
        
        /// <summary>
        /// Gets the current pool size
        /// </summary>
        public int GetCurrentPoolSize()
        {
            lock (_lockObject)
            {
                return _pool.Count;
            }
        }
        
        /// <summary>
        /// Gets the maximum pool size
        /// </summary>
        public int GetMaxPoolSize()
        {
            lock (_lockObject)
            {
                return _maxPoolSize;
            }
        }
        
        /// <summary>
        /// Gets pool efficiency statistics
        /// </summary>
        public LogEntryPoolStats GetStats()
        {
            lock (_lockObject)
            {
                return new LogEntryPoolStats
                {
                    CurrentPoolSize = _pool.Count,
                    MaxPoolSize = _maxPoolSize,
                    TotalAllocations = _totalAllocations,
                    TotalReuses = _totalReuses,
                    ReuseRatio = _totalAllocations > 0 ? (float)_totalReuses / (_totalAllocations + _totalReuses) : 0f
                };
            }
        }
        
        /// <summary>
        /// Clears the entire pool
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                // Reset all entries in the pool before clearing
                while (_pool.Count > 0)
                {
                    var entry = _pool.Pop();
                    ResetLogEntry(entry);
                }
                _pool.Clear();
            }
        }
        
        /// <summary>
        /// Resets statistics counters
        /// </summary>
        public void ResetStats()
        {
            lock (_lockObject)
            {
                _totalAllocations = 0;
                _totalReuses = 0;
            }
        }
    }
    
    /// <summary>
    /// Statistics for LogEntry pool performance monitoring
    /// </summary>
    public struct LogEntryPoolStats
    {
        public int CurrentPoolSize;
        public int MaxPoolSize;
        public long TotalAllocations;
        public long TotalReuses;
        public float ReuseRatio;
        
        public override string ToString()
        {
            return $"Pool: {CurrentPoolSize}/{MaxPoolSize}, Allocations: {TotalAllocations}, Reuses: {TotalReuses}, Reuse Ratio: {ReuseRatio:P1}";
        }
    }
}