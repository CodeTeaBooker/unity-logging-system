namespace RuntimeLogging
{
    /// <summary>
    /// Unified logging interface for all logging operations across different adapters
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        void Log(string message);
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">The warning message to log</param>
        void LogWarning(string message);
        
        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="message">The error message to log</param>
        void LogError(string message);
    }
}