using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuntimeLogging
{
    /// <summary>
    /// Unified performance monitoring system for the logging framework
    /// Consolidates all performance statistics into a single monitoring solution
    /// </summary>
    public static class UnifiedPerformanceMonitor
    {
        private static readonly Dictionary<string, IPerformanceStats> _performanceStats = new Dictionary<string, IPerformanceStats>();
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Event fired when performance statistics are updated
        /// </summary>
        public static event Action<string, IPerformanceStats> OnPerformanceStatsUpdated;
        
        /// <summary>
        /// Registers a performance statistics provider
        /// </summary>
        /// <param name="stats">Performance statistics to register</param>
        public static void RegisterStats(IPerformanceStats stats)
        {
            if (stats == null) return;
            
            lock (_lock)
            {
                _performanceStats[stats.ComponentName] = stats;
                OnPerformanceStatsUpdated?.Invoke(stats.ComponentName, stats);
            }
        }
        
        /// <summary>
        /// Unregisters a performance statistics provider
        /// </summary>
        /// <param name="componentName">Name of the component to unregister</param>
        public static void UnregisterStats(string componentName)
        {
            if (string.IsNullOrEmpty(componentName)) return;
            
            lock (_lock)
            {
                _performanceStats.Remove(componentName);
            }
        }
        
        /// <summary>
        /// Gets performance statistics for a specific component
        /// </summary>
        /// <param name="componentName">Name of the component</param>
        /// <returns>Performance statistics or null if not found</returns>
        public static IPerformanceStats GetStats(string componentName)
        {
            if (string.IsNullOrEmpty(componentName)) return null;
            
            lock (_lock)
            {
                _performanceStats.TryGetValue(componentName, out IPerformanceStats stats);
                return stats;
            }
        }
        
        /// <summary>
        /// Gets all registered performance statistics
        /// </summary>
        /// <returns>Dictionary of all performance statistics</returns>
        public static Dictionary<string, IPerformanceStats> GetAllStats()
        {
            lock (_lock)
            {
                return new Dictionary<string, IPerformanceStats>(_performanceStats);
            }
        }
        
        /// <summary>
        /// Generates a comprehensive performance report
        /// </summary>
        /// <returns>Formatted performance report</returns>
        public static string GeneratePerformanceReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Unified Performance Report ===");
            report.AppendLine($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Total Monitored Components: {_performanceStats.Count}");
            report.AppendLine();
            
            lock (_lock)
            {
                if (_performanceStats.Count == 0)
                {
                    report.AppendLine("No performance statistics available.");
                    return report.ToString();
                }
                
                foreach (var kvp in _performanceStats.OrderBy(x => x.Key))
                {
                    report.AppendLine($"Component: {kvp.Key}");
                    report.AppendLine($"Status: {(kvp.Value.IsActive ? "Active" : "Inactive")}");
                    report.AppendLine($"Last Updated: {kvp.Value.CollectedAt:HH:mm:ss}");
                    report.AppendLine("Statistics:");
                    report.AppendLine(kvp.Value.GetFormattedReport());
                    report.AppendLine(new string('-', 50));
                }
            }
            
            return report.ToString();
        }
        
        /// <summary>
        /// Gets performance data suitable for JSON serialization
        /// </summary>
        /// <returns>Serializable performance data</returns>
        public static Dictionary<string, Dictionary<string, object>> GetSerializablePerformanceData()
        {
            var result = new Dictionary<string, Dictionary<string, object>>();
            
            lock (_lock)
            {
                foreach (var kvp in _performanceStats)
                {
                    var componentData = kvp.Value.GetRawData();
                    componentData["IsActive"] = kvp.Value.IsActive;
                    componentData["CollectedAt"] = kvp.Value.CollectedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    result[kvp.Key] = componentData;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Resets all performance statistics
        /// </summary>
        public static void ResetAllStats()
        {
            lock (_lock)
            {
                foreach (var stats in _performanceStats.Values)
                {
                    stats.Reset();
                }
            }
            
            Debug.Log("UnifiedPerformanceMonitor: All performance statistics reset");
        }
        
        /// <summary>
        /// Gets system-wide performance summary
        /// </summary>
        /// <returns>Performance summary data</returns>
        public static SystemPerformanceSummary GetSystemSummary()
        {
            lock (_lock)
            {
                var summary = new SystemPerformanceSummary
                {
                    TotalComponents = _performanceStats.Count,
                    ActiveComponents = _performanceStats.Values.Count(s => s.IsActive),
                    LastUpdateTime = _performanceStats.Values.Any() ? 
                        _performanceStats.Values.Max(s => s.CollectedAt) : DateTime.MinValue,
                    MemoryUsageKB = GC.GetTotalMemory(false) / 1024
                };
                
                return summary;
            }
        }
        
        /// <summary>
        /// Logs current performance statistics to Unity console
        /// </summary>
        /// <param name="logLevel">Level to log at (Info, Warning, Error)</param>
        public static void LogPerformanceReport(LogLevel logLevel = LogLevel.Info)
        {
            string report = GeneratePerformanceReport();
            
            switch (logLevel)
            {
                case LogLevel.Info:
                    Debug.Log(report);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(report);
                    break;
                case LogLevel.Error:
                    Debug.LogError(report);
                    break;
            }
        }
        
        /// <summary>
        /// Clears all registered performance statistics
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _performanceStats.Clear();
            }
            
            OnPerformanceStatsUpdated = null;
            Debug.Log("UnifiedPerformanceMonitor: Cleared all performance statistics");
        }
    }
    
    /// <summary>
    /// System-wide performance summary
    /// </summary>
    [Serializable]
    public struct SystemPerformanceSummary
    {
        public int TotalComponents;
        public int ActiveComponents;
        public DateTime LastUpdateTime;
        public long MemoryUsageKB;
        
        public override string ToString()
        {
            return $"System Performance - Components: {ActiveComponents}/{TotalComponents}, " +
                   $"Memory: {MemoryUsageKB}KB, Last Update: {LastUpdateTime:HH:mm:ss}";
        }
    }
}