using System;
using System.Threading;

namespace RuntimeLogging
{
    /// <summary>
    /// Global logger management system providing centralized logger configuration
    /// and thread-safe access to the current logger instance
    /// </summary>
    public static class LogManager
    {
        private static ILogger _currentLogger;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Sets the global logger instance in a thread-safe manner
        /// </summary>
        /// <param name="logger">The logger instance to set as global</param>
        /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
        public static void SetLogger(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
                
            lock (_lock)
            {
                _currentLogger = logger;
            }
        }
        
        /// <summary>
        /// Gets the current global logger instance in a thread-safe manner
        /// </summary>
        /// <returns>The current logger instance, or null if none has been set</returns>
        public static ILogger GetLogger()
        {
            lock (_lock)
            {
                return _currentLogger;
            }
        }
        
        /// <summary>
        /// Convenience method to log an informational message using the current logger
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Log(string message)
        {
            GetLogger()?.Log(message);
        }
        
        /// <summary>
        /// Convenience method to log a warning message using the current logger
        /// </summary>
        /// <param name="message">The warning message to log</param>
        public static void LogWarning(string message)
        {
            GetLogger()?.LogWarning(message);
        }
        
        /// <summary>
        /// Convenience method to log an error message using the current logger
        /// </summary>
        /// <param name="message">The error message to log</param>
        public static void LogError(string message)
        {
            GetLogger()?.LogError(message);
        }
        
        /// <summary>
        /// Clears the current logger instance (useful for testing scenarios)
        /// </summary>
        public static void ClearLogger()
        {
            lock (_lock)
            {
                _currentLogger = null;
            }
        }
        
        /// <summary>
        /// Checks if a logger is currently set
        /// </summary>
        /// <returns>True if a logger is set, false otherwise</returns>
        public static bool HasLogger()
        {
            lock (_lock)
            {
                return _currentLogger != null;
            }
        }
    }
}