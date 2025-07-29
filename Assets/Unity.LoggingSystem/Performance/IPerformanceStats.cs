using System;

namespace RuntimeLogging
{
    /// <summary>
    /// Common interface for all performance statistics in the logging system
    /// Provides a unified way to collect and report performance data
    /// </summary>
    public interface IPerformanceStats
    {
        /// <summary>
        /// Name of the component being monitored
        /// </summary>
        string ComponentName { get; }
        
        /// <summary>
        /// Timestamp when the statistics were collected
        /// </summary>
        DateTime CollectedAt { get; }
        
        /// <summary>
        /// Whether the component is currently active
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Gets a formatted string representation of the statistics
        /// </summary>
        /// <returns>Human-readable performance statistics</returns>
        string GetFormattedReport();
        
        /// <summary>
        /// Gets raw performance data as key-value pairs
        /// </summary>
        /// <returns>Dictionary of performance metrics</returns>
        System.Collections.Generic.Dictionary<string, object> GetRawData();
        
        /// <summary>
        /// Resets performance counters to initial state
        /// </summary>
        void Reset();
    }
    
    /// <summary>
    /// Base implementation of performance statistics
    /// </summary>
    [Serializable]
    public abstract class BasePerformanceStats : IPerformanceStats
    {
        public string ComponentName { get; protected set; }
        public DateTime CollectedAt { get; protected set; }
        public bool IsActive { get; protected set; }
        
        protected BasePerformanceStats(string componentName)
        {
            ComponentName = componentName;
            CollectedAt = DateTime.Now;
            IsActive = true;
        }
        
        public abstract string GetFormattedReport();
        public abstract System.Collections.Generic.Dictionary<string, object> GetRawData();
        public abstract void Reset();
        
        /// <summary>
        /// Updates the collection timestamp
        /// </summary>
        protected void UpdateTimestamp()
        {
            CollectedAt = DateTime.Now;
        }
    }
}