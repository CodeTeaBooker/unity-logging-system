using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace RuntimeLogging
{
    /// <summary>
    /// Performance profiling tools and benchmarks for TextMeshPro integration validation
    /// Provides comprehensive performance monitoring and analysis for the logging system
    /// </summary>
    public class LoggingPerformanceProfiler
    {
        private readonly Dictionary<string, PerformanceMetric> _metrics = new Dictionary<string, PerformanceMetric>();
        private readonly List<PerformanceSample> _samples = new List<PerformanceSample>();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
        private int _maxSamples = 1000;
        private bool _isEnabled = true;
        private float _sampleInterval = 0.1f; // 100ms
        private float _lastSampleTime = 0f;
        
        /// <summary>
        /// Starts profiling a specific operation
        /// </summary>
        /// <param name="operationName">Name of the operation being profiled</param>
        public void StartProfiling(string operationName)
        {
            if (!_isEnabled)
                return;
                
            if (!_metrics.ContainsKey(operationName))
            {
                _metrics[operationName] = new PerformanceMetric(operationName);
            }
            
            _metrics[operationName].StartTiming();
        }
        
        /// <summary>
        /// Stops profiling a specific operation
        /// </summary>
        /// <param name="operationName">Name of the operation being profiled</param>
        public void StopProfiling(string operationName)
        {
            if (!_isEnabled || !_metrics.ContainsKey(operationName))
                return;
                
            _metrics[operationName].StopTiming();
        }
        
        /// <summary>
        /// Records a custom performance measurement
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="durationMs">Duration in milliseconds</param>
        /// <param name="additionalData">Additional data to record</param>
        public void RecordMeasurement(string operationName, float durationMs, object additionalData = null)
        {
            if (!_isEnabled)
                return;
                
            if (!_metrics.ContainsKey(operationName))
            {
                _metrics[operationName] = new PerformanceMetric(operationName);
            }
            
            _metrics[operationName].RecordMeasurement(durationMs, additionalData);
        }
        
        /// <summary>
        /// Profiles a function execution and returns the result
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="function">Function to profile</param>
        /// <returns>Result of the function execution</returns>
        public T ProfileFunction<T>(string operationName, Func<T> function)
        {
            if (!_isEnabled)
                return function();
                
            StartProfiling(operationName);
            try
            {
                return function();
            }
            finally
            {
                StopProfiling(operationName);
            }
        }
        
        /// <summary>
        /// Profiles an action execution
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <param name="action">Action to profile</param>
        public void ProfileAction(string operationName, Action action)
        {
            if (!_isEnabled)
            {
                action();
                return;
            }
                
            StartProfiling(operationName);
            try
            {
                action();
            }
            finally
            {
                StopProfiling(operationName);
            }
        }
        
        /// <summary>
        /// Updates the profiler - should be called regularly (e.g., from Update)
        /// </summary>
        public void Update()
        {
            if (!_isEnabled)
                return;
                
            if (Time.unscaledTime - _lastSampleTime >= _sampleInterval)
            {
                CollectPerformanceSample();
                _lastSampleTime = Time.unscaledTime;
            }
        }
        
        /// <summary>
        /// Gets performance statistics for a specific operation
        /// </summary>
        /// <param name="operationName">Name of the operation</param>
        /// <returns>Performance statistics or null if operation not found</returns>
        public PerformanceStats GetStats(string operationName)
        {
            if (_metrics.ContainsKey(operationName))
            {
                return _metrics[operationName].GetStats();
            }
            return null;
        }
        
        /// <summary>
        /// Gets performance statistics for all operations
        /// </summary>
        /// <returns>Dictionary of all performance statistics</returns>
        public Dictionary<string, PerformanceStats> GetAllStats()
        {
            var allStats = new Dictionary<string, PerformanceStats>();
            foreach (var kvp in _metrics)
            {
                allStats[kvp.Key] = kvp.Value.GetStats();
            }
            return allStats;
        }
        
        /// <summary>
        /// Gets recent performance samples
        /// </summary>
        /// <param name="count">Number of recent samples to return</param>
        /// <returns>List of recent performance samples</returns>
        public List<PerformanceSample> GetRecentSamples(int count = 100)
        {
            int startIndex = Mathf.Max(0, _samples.Count - count);
            return _samples.GetRange(startIndex, _samples.Count - startIndex);
        }
        
        /// <summary>
        /// Runs a comprehensive benchmark of the logging system
        /// </summary>
        /// <param name="config">Benchmark configuration</param>
        /// <returns>Benchmark results</returns>
        public BenchmarkResults RunBenchmark(BenchmarkConfig config)
        {
            var results = new BenchmarkResults
            {
                Config = config,
                StartTime = DateTime.Now
            };
            
            // Benchmark log entry creation
            results.LogEntryCreationTime = BenchmarkLogEntryCreation(config.LogEntryCount);
            
            // Benchmark text formatting
            results.TextFormattingTime = BenchmarkTextFormatting(config.LogEntryCount);
            
            // Benchmark TextMeshPro updates
            results.TextMeshProUpdateTime = BenchmarkTextMeshProUpdates(config.LogEntryCount);
            
            // Benchmark memory usage
            results.MemoryUsage = BenchmarkMemoryUsage(config.LogEntryCount);
            
            results.EndTime = DateTime.Now;
            results.TotalTime = results.EndTime - results.StartTime;
            
            return results;
        }
        
        /// <summary>
        /// Enables or disables profiling
        /// </summary>
        /// <param name="enabled">Whether profiling should be enabled</param>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }
        
        /// <summary>
        /// Sets the maximum number of samples to keep
        /// </summary>
        /// <param name="maxSamples">Maximum number of samples</param>
        public void SetMaxSamples(int maxSamples)
        {
            _maxSamples = Mathf.Max(100, maxSamples);
            
            // Trim samples if necessary
            while (_samples.Count > _maxSamples)
            {
                _samples.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Sets the sample collection interval
        /// </summary>
        /// <param name="intervalSeconds">Interval in seconds</param>
        public void SetSampleInterval(float intervalSeconds)
        {
            _sampleInterval = Mathf.Max(0.01f, intervalSeconds);
        }
        
        /// <summary>
        /// Clears all performance data
        /// </summary>
        public void Clear()
        {
            _metrics.Clear();
            _samples.Clear();
        }
        
        /// <summary>
        /// Resets statistics for all operations
        /// </summary>
        public void ResetStats()
        {
            foreach (var metric in _metrics.Values)
            {
                metric.Reset();
            }
        }
        
        /// <summary>
        /// Generates a performance report
        /// </summary>
        /// <returns>Formatted performance report string</returns>
        public string GenerateReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Logging Performance Report ===");
            report.AppendLine($"Generated: {DateTime.Now}");
            report.AppendLine($"Profiling Enabled: {_isEnabled}");
            report.AppendLine($"Total Operations: {_metrics.Count}");
            report.AppendLine($"Total Samples: {_samples.Count}");
            report.AppendLine();
            
            foreach (var kvp in _metrics)
            {
                var stats = kvp.Value.GetStats();
                report.AppendLine($"Operation: {kvp.Key}");
                report.AppendLine($"  Executions: {stats.ExecutionCount}");
                report.AppendLine($"  Average Time: {stats.AverageTimeMs:F2}ms");
                report.AppendLine($"  Min Time: {stats.MinTimeMs:F2}ms");
                report.AppendLine($"  Max Time: {stats.MaxTimeMs:F2}ms");
                report.AppendLine($"  Total Time: {stats.TotalTimeMs:F2}ms");
                report.AppendLine();
            }
            
            return report.ToString();
        }
        
        private void CollectPerformanceSample()
        {
            var sample = new PerformanceSample
            {
                Timestamp = Time.unscaledTime,
                FrameRate = 1.0f / Time.unscaledDeltaTime,
                MemoryUsage = GC.GetTotalMemory(false),
                ActiveMetrics = _metrics.Count
            };
            
            _samples.Add(sample);
            
            // Trim samples if we exceed the limit
            while (_samples.Count > _maxSamples)
            {
                _samples.RemoveAt(0);
            }
        }
        
        private float BenchmarkLogEntryCreation(int count)
        {
            return ProfileFunction("LogEntryCreation", () =>
            {
                var stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < count; i++)
                {
                    var entry = new LogEntry($"Benchmark message {i}", LogLevel.Info, DateTime.Now);
                }
                stopwatch.Stop();
                return (float)stopwatch.ElapsedMilliseconds;
            });
        }
        
        private float BenchmarkTextFormatting(int count)
        {
            return ProfileFunction("TextFormatting", () =>
            {
                var entries = new List<LogEntry>();
                for (int i = 0; i < count; i++)
                {
                    entries.Add(new LogEntry($"Benchmark message {i}", LogLevel.Info, DateTime.Now));
                }
                
                var stopwatch = Stopwatch.StartNew();
                foreach (var entry in entries)
                {
                    var formatted = entry.GetFormattedMessage("HH:mm:ss");
                }
                stopwatch.Stop();
                return (float)stopwatch.ElapsedMilliseconds;
            });
        }
        
        private float BenchmarkTextMeshProUpdates(int count)
        {
            return ProfileFunction("TextMeshProUpdates", () =>
            {
                // This would require a TextMeshPro component to test properly
                // For now, simulate the text processing time
                var stopwatch = Stopwatch.StartNew();
                var text = "";
                for (int i = 0; i < count; i++)
                {
                    text += $"Benchmark line {i}\n";
                }
                stopwatch.Stop();
                return (float)stopwatch.ElapsedMilliseconds;
            });
        }
        
        private long BenchmarkMemoryUsage(int count)
        {
            long initialMemory = GC.GetTotalMemory(true);
            
            var entries = new List<LogEntry>();
            for (int i = 0; i < count; i++)
            {
                entries.Add(new LogEntry($"Memory benchmark message {i}", LogLevel.Info, DateTime.Now));
            }
            
            long finalMemory = GC.GetTotalMemory(false);
            return finalMemory - initialMemory;
        }
    }
    
    /// <summary>
    /// Individual performance metric tracking
    /// </summary>
    public class PerformanceMetric
    {
        private readonly string _name;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly List<float> _measurements = new List<float>();
        
        private int _executionCount = 0;
        private float _totalTime = 0f;
        private float _minTime = float.MaxValue;
        private float _maxTime = float.MinValue;
        
        public PerformanceMetric(string name)
        {
            _name = name;
        }
        
        public void StartTiming()
        {
            _stopwatch.Restart();
        }
        
        public void StopTiming()
        {
            _stopwatch.Stop();
            float elapsedMs = (float)_stopwatch.ElapsedMilliseconds;
            RecordMeasurement(elapsedMs);
        }
        
        public void RecordMeasurement(float durationMs, object additionalData = null)
        {
            _measurements.Add(durationMs);
            _executionCount++;
            _totalTime += durationMs;
            _minTime = Mathf.Min(_minTime, durationMs);
            _maxTime = Mathf.Max(_maxTime, durationMs);
            
            // Keep only recent measurements to prevent memory growth
            if (_measurements.Count > 1000)
            {
                _measurements.RemoveAt(0);
            }
        }
        
        public PerformanceStats GetStats()
        {
            return new PerformanceStats
            {
                Name = _name,
                ExecutionCount = _executionCount,
                TotalTimeMs = _totalTime,
                AverageTimeMs = _executionCount > 0 ? _totalTime / _executionCount : 0f,
                MinTimeMs = _minTime == float.MaxValue ? 0f : _minTime,
                MaxTimeMs = _maxTime == float.MinValue ? 0f : _maxTime,
                RecentMeasurements = new List<float>(_measurements)
            };
        }
        
        public void Reset()
        {
            _measurements.Clear();
            _executionCount = 0;
            _totalTime = 0f;
            _minTime = float.MaxValue;
            _maxTime = float.MinValue;
        }
    }
    
    /// <summary>
    /// Performance statistics for an operation
    /// </summary>
    public class PerformanceStats
    {
        public string Name;
        public int ExecutionCount;
        public float TotalTimeMs;
        public float AverageTimeMs;
        public float MinTimeMs;
        public float MaxTimeMs;
        public List<float> RecentMeasurements;
    }
    
    /// <summary>
    /// Performance sample collected over time
    /// </summary>
    public struct PerformanceSample
    {
        public float Timestamp;
        public float FrameRate;
        public long MemoryUsage;
        public int ActiveMetrics;
    }
    
    /// <summary>
    /// Configuration for benchmark tests
    /// </summary>
    public struct BenchmarkConfig
    {
        public int LogEntryCount;
        public bool IncludeMemoryTest;
        public bool IncludeTextMeshProTest;
        public bool IncludeStressTest;
    }
    
    /// <summary>
    /// Results from benchmark tests
    /// </summary>
    public struct BenchmarkResults
    {
        public BenchmarkConfig Config;
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan TotalTime;
        public float LogEntryCreationTime;
        public float TextFormattingTime;
        public float TextMeshProUpdateTime;
        public long MemoryUsage;
    }
}