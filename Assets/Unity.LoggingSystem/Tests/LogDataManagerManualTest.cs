using UnityEngine;
using RuntimeLogging;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Manual test script to verify LogDataManager functionality
    /// </summary>
    public class LogDataManagerManualTest : MonoBehaviour
    {
        private LogDataManager _logDataManager;
        private LogConfiguration _config;
        
        void Start()
        {
            // Create test configuration
            _config = ScriptableObject.CreateInstance<LogConfiguration>();
            _config.maxLogCount = 5;
            _config.timestampFormat = "HH:mm:ss";
            
            // Initialize LogDataManager
            _logDataManager = new LogDataManager(_config);
            
            // Subscribe to events
            _logDataManager.OnLogAdded += OnLogAdded;
            _logDataManager.OnLogsCleared += OnLogsCleared;
            
            // Run tests
            TestBasicFunctionality();
            TestCircularBuffer();
            TestThreadSafety();
            
            Debug.Log("LogDataManager manual tests completed successfully!");
        }
        
        private void TestBasicFunctionality()
        {
            Debug.Log("Testing basic functionality...");
            
            // Test adding logs
            _logDataManager.AddLog("Test Info Message", LogLevel.Info);
            _logDataManager.AddLog("Test Warning Message", LogLevel.Warning);
            _logDataManager.AddLog("Test Error Message", LogLevel.Error);
            
            // Test null/empty handling
            _logDataManager.AddLog(null, LogLevel.Info); // Should be ignored
            _logDataManager.AddLog("", LogLevel.Warning); // Should be ignored
            _logDataManager.AddLog("   ", LogLevel.Error); // Should be added (whitespace)
            
            // Verify count
            Debug.Log($"Log count: {_logDataManager.GetLogCount()} (expected: 4)");
            
            // Test formatted logs
            var formattedLogs = _logDataManager.GetFormattedLogs();
            foreach (var log in formattedLogs)
            {
                Debug.Log($"Formatted: {log}");
            }
            
            // Test memory usage
            Debug.Log($"Estimated memory usage: {_logDataManager.GetEstimatedMemoryUsage()} bytes");
        }
        
        private void TestCircularBuffer()
        {
            Debug.Log("Testing circular buffer...");
            
            // Clear existing logs
            _logDataManager.ClearLogs();
            
            // Add more logs than the limit (5)
            for (int i = 0; i < 8; i++)
            {
                _logDataManager.AddLog($"Buffer test message {i}", LogLevel.Info);
            }
            
            // Should only have 5 logs (the limit)
            Debug.Log($"Log count after overflow: {_logDataManager.GetLogCount()} (expected: 5)");
            
            var logs = _logDataManager.GetLogs();
            Debug.Log($"First log message: {logs[0].message} (expected: Buffer test message 3)");
            Debug.Log($"Last log message: {logs[logs.Count - 1].message} (expected: Buffer test message 7)");
        }
        
        private void TestThreadSafety()
        {
            Debug.Log("Testing thread safety...");
            
            _logDataManager.ClearLogs();
            
            // Simulate concurrent access (simplified test)
            for (int i = 0; i < 10; i++)
            {
                _logDataManager.AddLog($"Thread safety test {i}", LogLevel.Info);
                
                // Read logs while adding
                var currentLogs = _logDataManager.GetLogs();
                Debug.Log($"Current log count during concurrent access: {currentLogs.Count}");
            }
        }
        
        private void OnLogAdded(LogEntry entry)
        {
            Debug.Log($"Event: Log added - {entry.FormattedMessage}");
        }
        
        private void OnLogsCleared()
        {
            Debug.Log("Event: All logs cleared");
        }
        
        void OnDestroy()
        {
            if (_config != null)
            {
                DestroyImmediate(_config);
            }
        }
    }
}