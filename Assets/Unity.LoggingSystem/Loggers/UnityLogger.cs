using UnityEngine;

namespace RuntimeLogging
{
    /// <summary>
    /// Unity Console adapter implementation of ILogger that outputs to Unity's Debug console
    /// Maps ILogger methods to Unity Debug.Log, Debug.LogWarning, and Debug.LogError
    /// </summary>
    public class UnityLogger : ILogger
    {
        /// <summary>
        /// Logs an informational message to Unity Console
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Log(string message)
        {
            if (message == null)
            {
                Debug.Log("(null)");
                return;
            }
            
            Debug.Log(message);
        }
        
        /// <summary>
        /// Logs a warning message to Unity Console
        /// </summary>
        /// <param name="message">The warning message to log</param>
        public void LogWarning(string message)
        {
            if (message == null)
            {
                Debug.LogWarning("(null)");
                return;
            }
            
            Debug.LogWarning(message);
        }
        
        /// <summary>
        /// Logs an error message to Unity Console
        /// </summary>
        /// <param name="message">The error message to log</param>
        public void LogError(string message)
        {
            if (message == null)
            {
                Debug.LogError("(null)");
                return;
            }
            
            Debug.LogError(message);
        }
    }
}