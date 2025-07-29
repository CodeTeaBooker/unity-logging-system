using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuntimeLogging
{
    /// <summary>
    /// CompositeLogger class that combines multiple ILogger implementations
    /// Implements error isolation so one adapter failure doesn't affect others
    /// Supports adding/removing loggers at runtime for flexible configuration
    /// </summary>
    public class CompositeLogger : ILogger, IDisposable
    {
        private readonly List<ILogger> loggers;
        private readonly object lockObject = new object();
        private bool _disposed = false;
        
        /// <summary>
        /// Event fired when a logger fails during operation
        /// </summary>
        public event Action<ILogger, Exception> OnLoggerFailed;
        
        /// <summary>
        /// Event fired when a logger is added to the composite
        /// </summary>
        public event Action<ILogger> OnLoggerAdded;
        
        /// <summary>
        /// Event fired when a logger is removed from the composite
        /// </summary>
        public event Action<ILogger> OnLoggerRemoved;
        
        /// <summary>
        /// Constructor that accepts variable number of logger adapters
        /// </summary>
        /// <param name="loggers">Variable number of ILogger implementations</param>
        public CompositeLogger(params ILogger[] loggers)
        {
            this.loggers = new List<ILogger>();
            
            if (loggers != null)
            {
                foreach (var logger in loggers)
                {
                    if (logger != null)
                    {
                        this.loggers.Add(logger);
                    }
                }
            }
        }
        
        /// <summary>
        /// Constructor that accepts a collection of logger adapters
        /// </summary>
        /// <param name="loggers">Collection of ILogger implementations</param>
        public CompositeLogger(IEnumerable<ILogger> loggers)
        {
            this.loggers = new List<ILogger>();
            
            if (loggers != null)
            {
                foreach (var logger in loggers)
                {
                    if (logger != null)
                    {
                        this.loggers.Add(logger);
                    }
                }
            }
        }
        
        /// <summary>
        /// Log an informational message to all registered loggers
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Log(string message)
        {
            ExecuteOnAllLoggers(logger => logger.Log(message));
        }
        
        /// <summary>
        /// Log a warning message to all registered loggers
        /// </summary>
        /// <param name="message">The warning message to log</param>
        public void LogWarning(string message)
        {
            ExecuteOnAllLoggers(logger => logger.LogWarning(message));
        }
        
        /// <summary>
        /// Log an error message to all registered loggers
        /// </summary>
        /// <param name="message">The error message to log</param>
        public void LogError(string message)
        {
            ExecuteOnAllLoggers(logger => logger.LogError(message));
        }
        
        /// <summary>
        /// Add a logger to the composite at runtime
        /// </summary>
        /// <param name="logger">The logger to add</param>
        /// <returns>True if added successfully, false if null or already exists</returns>
        public bool AddLogger(ILogger logger)
        {
            if (logger == null)
                return false;
                
            lock (lockObject)
            {
                if (loggers.Contains(logger))
                    return false;
                    
                loggers.Add(logger);
                OnLoggerAdded?.Invoke(logger);
                return true;
            }
        }
        
        /// <summary>
        /// Remove a logger from the composite at runtime
        /// </summary>
        /// <param name="logger">The logger to remove</param>
        /// <returns>True if removed successfully, false if not found</returns>
        public bool RemoveLogger(ILogger logger)
        {
            if (logger == null)
                return false;
                
            lock (lockObject)
            {
                bool removed = loggers.Remove(logger);
                if (removed)
                {
                    OnLoggerRemoved?.Invoke(logger);
                }
                return removed;
            }
        }
        
        /// <summary>
        /// Remove all loggers of a specific type
        /// </summary>
        /// <typeparam name="T">The type of logger to remove</typeparam>
        /// <returns>Number of loggers removed</returns>
        public int RemoveLoggersOfType<T>() where T : ILogger
        {
            lock (lockObject)
            {
                var loggersToRemove = loggers.OfType<T>().Cast<ILogger>().ToList();
                int removedCount = 0;
                
                foreach (var logger in loggersToRemove)
                {
                    if (loggers.Remove(logger))
                    {
                        removedCount++;
                        OnLoggerRemoved?.Invoke(logger);
                    }
                }
                
                return removedCount;
            }
        }
        
        /// <summary>
        /// Clear all loggers from the composite
        /// </summary>
        public void ClearLoggers()
        {
            lock (lockObject)
            {
                var loggersToRemove = loggers.ToList();
                loggers.Clear();
                
                foreach (var logger in loggersToRemove)
                {
                    OnLoggerRemoved?.Invoke(logger);
                }
            }
        }
        
        /// <summary>
        /// Get the current number of registered loggers
        /// </summary>
        /// <returns>Number of registered loggers</returns>
        public int GetLoggerCount()
        {
            lock (lockObject)
            {
                return loggers.Count;
            }
        }
        
        /// <summary>
        /// Get a read-only list of all registered loggers
        /// </summary>
        /// <returns>Read-only list of loggers</returns>
        public IReadOnlyList<ILogger> GetLoggers()
        {
            lock (lockObject)
            {
                return loggers.ToList().AsReadOnly();
            }
        }
        
        /// <summary>
        /// Check if a specific logger is registered
        /// </summary>
        /// <param name="logger">The logger to check for</param>
        /// <returns>True if the logger is registered, false otherwise</returns>
        public bool ContainsLogger(ILogger logger)
        {
            if (logger == null)
                return false;
                
            lock (lockObject)
            {
                return loggers.Contains(logger);
            }
        }
        
        /// <summary>
        /// Check if any logger of a specific type is registered
        /// </summary>
        /// <typeparam name="T">The type of logger to check for</typeparam>
        /// <returns>True if any logger of the specified type is registered</returns>
        public bool ContainsLoggerOfType<T>() where T : ILogger
        {
            lock (lockObject)
            {
                return loggers.OfType<T>().Any();
            }
        }
        
        /// <summary>
        /// Get all loggers of a specific type
        /// </summary>
        /// <typeparam name="T">The type of logger to retrieve</typeparam>
        /// <returns>List of loggers of the specified type</returns>
        public List<T> GetLoggersOfType<T>() where T : ILogger
        {
            lock (lockObject)
            {
                return loggers.OfType<T>().ToList();
            }
        }
        
        /// <summary>
        /// Execute an action on all loggers with error isolation
        /// </summary>
        /// <param name="action">The action to execute on each logger</param>
        private void ExecuteOnAllLoggers(Action<ILogger> action)
        {
            if (action == null)
                return;
                
            List<ILogger> currentLoggers;
            
            // Create a snapshot of loggers to avoid holding the lock during execution
            lock (lockObject)
            {
                if (loggers.Count == 0)
                    return;
                    
                currentLoggers = new List<ILogger>(loggers);
            }
            
            // Execute action on each logger with error isolation
            foreach (var logger in currentLoggers)
            {
                try
                {
                    action(logger);
                }
                catch (Exception ex)
                {
                    // Error isolation: one adapter failure doesn't affect others
                    HandleLoggerError(logger, ex);
                }
            }
        }
        
        /// <summary>
        /// Handle errors from individual loggers
        /// </summary>
        /// <param name="logger">The logger that failed</param>
        /// <param name="exception">The exception that occurred</param>
        private void HandleLoggerError(ILogger logger, Exception exception)
        {
            try
            {
                // Notify listeners about the failure
                OnLoggerFailed?.Invoke(logger, exception);
            }
            catch (Exception eventException)
            {
                // If event handling fails, log to Unity console as fallback
                Debug.LogError($"CompositeLogger: Failed to handle logger error event. " +
                              $"Original error: {exception?.Message}, Event error: {eventException?.Message}");
            }
            
            // Log the error to Unity console as fallback
            Debug.LogError($"CompositeLogger: Logger {logger?.GetType()?.Name} failed with error: {exception?.Message}");
        }
        
        /// <summary>
        /// Get performance statistics for the composite logger
        /// </summary>
        /// <returns>Performance statistics</returns>
        public CompositeLoggerPerformanceStats GetPerformanceStats()
        {
            lock (lockObject)
            {
                var stats = new CompositeLoggerPerformanceStats
                {
                    TotalLoggerCount = loggers.Count,
                    LoggerTypes = new Dictionary<string, int>()
                };
                
                // Count loggers by type
                foreach (var logger in loggers)
                {
                    var typeName = logger.GetType().Name;
                    if (stats.LoggerTypes.ContainsKey(typeName))
                    {
                        stats.LoggerTypes[typeName]++;
                    }
                    else
                    {
                        stats.LoggerTypes[typeName] = 1;
                    }
                }
                
                return stats;
            }
        }
        
        /// <summary>
        /// Finalizer to ensure resources are disposed even if Dispose is not called explicitly
        /// </summary>
        ~CompositeLogger()
        {
            Dispose(false);
        }
        
        /// <summary>
        /// Disposes of resources and clears event subscriptions to prevent memory leaks
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Protected dispose pattern implementation
        /// </summary>
        /// <param name="disposing">Whether disposing managed resources</param>
        protected void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                lock (lockObject)
                {
                    // Dispose individual loggers if they implement IDisposable
                    foreach (var logger in loggers)
                    {
                        if (logger is IDisposable disposableLogger)
                        {
                            try
                            {
                                disposableLogger.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"Error disposing logger {logger.GetType().Name}: {ex.Message}");
                            }
                        }
                    }
                    
                    loggers.Clear();
                }
                
                // Clear all event subscriptions
                OnLoggerFailed = null;
                OnLoggerAdded = null;
                OnLoggerRemoved = null;
                
                _disposed = true;
            }
        }
        
    }
    
    /// <summary>
    /// Performance statistics for CompositeLogger monitoring
    /// </summary>
    public struct CompositeLoggerPerformanceStats
    {
        public int TotalLoggerCount;
        public Dictionary<string, int> LoggerTypes;
    }
}