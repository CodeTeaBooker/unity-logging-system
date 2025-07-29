using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using TMPro;

namespace RuntimeLogging
{
    /// <summary>
    /// Utility for running performance benchmarks and generating reports
    /// Provides tools for validating TextMeshPro integration performance in production
    /// </summary>
    public class PerformanceBenchmarkUtility : MonoBehaviour
    {
        [Header("Benchmark Configuration")]
        [SerializeField] private int logEntryCount = 10000;
        [SerializeField] private int textMeshProUpdateCount = 1000;
        [SerializeField] private int concurrentThreadCount = 4;
        [SerializeField] private bool saveReportsToFile = true;
        
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI testTextComponent;
        [SerializeField] private LogConfiguration testConfiguration;
        
        private LoggingPerformanceProfiler _profiler;
        private LogDataManager _logDataManager;
        private MemoryMonitor _memoryMonitor;
        private TextMeshProOptimizer _textOptimizer;
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            // Create test configuration if not provided
            if (testConfiguration == null)
            {
                testConfiguration = ScriptableObject.CreateInstance<LogConfiguration>();
                testConfiguration.maxLogCount = 1000;
                testConfiguration.timestampFormat = "HH:mm:ss";
                testConfiguration.infoColorHex = "#FFFFFF";
                testConfiguration.warningColorHex = "#FFFF00";
                testConfiguration.errorColorHex = "#FF0000";
            }
            
            // Initialize performance components
            _profiler = new LoggingPerformanceProfiler();
            _logDataManager = new LogDataManager(testConfiguration);
            _memoryMonitor = new MemoryMonitor();
            _textOptimizer = new TextMeshProOptimizer();
            
            // Configure for benchmarking
            _profiler.SetEnabled(true);
            _memoryMonitor.StartMonitoring();
            _memoryMonitor.SetMemoryThreshold(50 * 1024 * 1024); // 50MB
            _memoryMonitor.SetCriticalMemoryThreshold(100 * 1024 * 1024); // 100MB
        }
        
        /// <summary>
        /// Runs a comprehensive performance benchmark
        /// </summary>
        [ContextMenu("Run Full Benchmark")]
        public void RunFullBenchmark()
        {
            if (!Application.isPlaying)
            {
                UnityEngine.Debug.LogWarning("Benchmark can only be run in Play mode");
                return;
            }
            
            UnityEngine.Debug.Log("Starting comprehensive performance benchmark...");
            
            var benchmarkResults = new BenchmarkReport
            {
                StartTime = DateTime.Now,
                Configuration = new BenchmarkConfiguration
                {
                    LogEntryCount = logEntryCount,
                    TextMeshProUpdateCount = textMeshProUpdateCount,
                    ConcurrentThreadCount = concurrentThreadCount,
                    UnityVersion = Application.unityVersion,
                    Platform = Application.platform.ToString(),
                    DeviceModel = SystemInfo.deviceModel,
                    ProcessorType = SystemInfo.processorType,
                    SystemMemorySize = SystemInfo.systemMemorySize
                }
            };
            
            try
            {
                // Run individual benchmark tests
                benchmarkResults.LogCreationResults = BenchmarkLogCreation();
                benchmarkResults.TextFormattingResults = BenchmarkTextFormatting();
                benchmarkResults.TextMeshProResults = BenchmarkTextMeshProPerformance();
                benchmarkResults.MemoryManagementResults = BenchmarkMemoryManagement();
                benchmarkResults.ConcurrencyResults = BenchmarkConcurrentLogging();
                benchmarkResults.OptimizationResults = BenchmarkTextOptimization();
                
                benchmarkResults.EndTime = DateTime.Now;
                benchmarkResults.TotalDuration = benchmarkResults.EndTime - benchmarkResults.StartTime;
                benchmarkResults.Success = true;
                
                // Generate and display report
                string report = GenerateBenchmarkReport(benchmarkResults);
                UnityEngine.Debug.Log(report);
                
                if (saveReportsToFile)
                {
                    SaveReportToFile(report, benchmarkResults);
                }
            }
            catch (Exception ex)
            {
                benchmarkResults.Success = false;
                benchmarkResults.ErrorMessage = ex.Message;
                UnityEngine.Debug.LogError($"Benchmark failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Benchmarks log entry creation performance
        /// </summary>
        private BenchmarkTestResult BenchmarkLogCreation()
        {
            var result = new BenchmarkTestResult { TestName = "Log Creation" };
            var stopwatch = Stopwatch.StartNew();
            long initialMemory = GC.GetTotalMemory(true);
            
            _profiler.StartProfiling("LogCreation");
            
            for (int i = 0; i < logEntryCount; i++)
            {
                LogLevel level = (LogLevel)(i % 3);
                string message = $"Benchmark log message {i} with content";
                _logDataManager.AddLog(message, level);
            }
            
            _profiler.StopProfiling("LogCreation");
            
            stopwatch.Stop();
            long finalMemory = GC.GetTotalMemory(false);
            
            var stats = _profiler.GetStats("LogCreation");
            result.ExecutionTimeMs = (float)stopwatch.ElapsedMilliseconds;
            result.MemoryUsedBytes = finalMemory - initialMemory;
            result.AverageOperationTimeMs = stats?.AverageTimeMs ?? 0f;
            result.OperationsPerSecond = logEntryCount / (result.ExecutionTimeMs / 1000f);
            result.Success = result.ExecutionTimeMs < 5000; // Should complete within 5 seconds
            
            return result;
        }
        
        /// <summary>
        /// Benchmarks text formatting performance
        /// </summary>
        private BenchmarkTestResult BenchmarkTextFormatting()
        {
            var result = new BenchmarkTestResult { TestName = "Text Formatting" };
            var stopwatch = Stopwatch.StartNew();
            long initialMemory = GC.GetTotalMemory(true);
            
            // Generate test logs
            var testLogs = new List<LogEntry>();
            for (int i = 0; i < logEntryCount; i++)
            {
                testLogs.Add(new LogEntry($"Format test {i}", (LogLevel)(i % 3), DateTime.Now));
            }
            
            _profiler.StartProfiling("TextFormatting");
            
            // Test different formatting methods
            foreach (var log in testLogs)
            {
                string formatted = log.GetFormattedMessage("HH:mm:ss");
                string richText = log.GetRichTextMessage(testConfiguration);
            }
            
            _profiler.StopProfiling("TextFormatting");
            
            stopwatch.Stop();
            long finalMemory = GC.GetTotalMemory(false);
            
            var stats = _profiler.GetStats("TextFormatting");
            result.ExecutionTimeMs = (float)stopwatch.ElapsedMilliseconds;
            result.MemoryUsedBytes = finalMemory - initialMemory;
            result.AverageOperationTimeMs = stats?.AverageTimeMs ?? 0f;
            result.OperationsPerSecond = (logEntryCount * 2) / (result.ExecutionTimeMs / 1000f); // 2 operations per log
            result.Success = result.ExecutionTimeMs < 3000; // Should complete within 3 seconds
            
            return result;
        }
        
        /// <summary>
        /// Benchmarks TextMeshPro update performance
        /// </summary>
        private BenchmarkTestResult BenchmarkTextMeshProPerformance()
        {
            var result = new BenchmarkTestResult { TestName = "TextMeshPro Updates" };
            
            if (testTextComponent == null)
            {
                result.Success = false;
                result.ErrorMessage = "No TextMeshPro component assigned";
                return result;
            }
            
            var stopwatch = Stopwatch.StartNew();
            long initialMemory = GC.GetTotalMemory(true);
            
            // Generate initial logs
            for (int i = 0; i < 100; i++)
            {
                _logDataManager.AddLog($"TextMeshPro test {i}", LogLevel.Info);
            }
            
            _profiler.StartProfiling("TextMeshProUpdates");
            
            for (int i = 0; i < textMeshProUpdateCount; i++)
            {
                // Add new log
                _logDataManager.AddLog($"Update {i}", (LogLevel)(i % 3));
                
                // Update TextMeshPro
                string displayText = _logDataManager.GetFormattedDisplayText();
                testTextComponent.text = displayText;
                
                // Force text generation to measure actual performance
                testTextComponent.ForceMeshUpdate();
            }
            
            _profiler.StopProfiling("TextMeshProUpdates");
            
            stopwatch.Stop();
            long finalMemory = GC.GetTotalMemory(false);
            
            var stats = _profiler.GetStats("TextMeshProUpdates");
            result.ExecutionTimeMs = (float)stopwatch.ElapsedMilliseconds;
            result.MemoryUsedBytes = finalMemory - initialMemory;
            result.AverageOperationTimeMs = stats?.AverageTimeMs ?? 0f;
            result.OperationsPerSecond = textMeshProUpdateCount / (result.ExecutionTimeMs / 1000f);
            result.Success = result.AverageOperationTimeMs < 5f; // Each update should be under 5ms
            
            return result;
        }
        
        /// <summary>
        /// Benchmarks memory management and cleanup
        /// </summary>
        private BenchmarkTestResult BenchmarkMemoryManagement()
        {
            var result = new BenchmarkTestResult { TestName = "Memory Management" };
            var stopwatch = Stopwatch.StartNew();
            
            long initialMemory = GC.GetTotalMemory(true);
            bool memoryThresholdReached = false;
            
            _memoryMonitor.OnMemoryThresholdReached += () => memoryThresholdReached = true;
            
            _profiler.StartProfiling("MemoryManagement");
            
            // Generate memory pressure
            for (int i = 0; i < logEntryCount * 2; i++)
            {
                string largeMessage = $"Large message {i} " + new string('X', 500);
                _logDataManager.AddLog(largeMessage, LogLevel.Info);
                
                if (i % 100 == 0)
                {
                    _memoryMonitor.Update();
                }
            }
            
            // Test cleanup
            long beforeCleanup = GC.GetTotalMemory(false);
            long memoryFreed = _logDataManager.ForceCleanup();
            long afterCleanup = GC.GetTotalMemory(false);
            
            _profiler.StopProfiling("MemoryManagement");
            
            stopwatch.Stop();
            
            var memoryStats = _memoryMonitor.GetMemoryStats();
            var performanceStats = _logDataManager.GetPerformanceStats();
            
            result.ExecutionTimeMs = (float)stopwatch.ElapsedMilliseconds;
            result.MemoryUsedBytes = beforeCleanup - initialMemory;
            result.AdditionalMetrics = new Dictionary<string, object>
            {
                ["MemoryFreed"] = memoryFreed,
                ["CleanupTriggered"] = memoryStats.CleanupTriggeredCount > 0,
                ["PoolReuseRatio"] = performanceStats.PoolStats.ReuseRatio,
                ["FinalLogCount"] = _logDataManager.GetLogCount(),
                ["MemoryThresholdReached"] = memoryThresholdReached
            };
            result.Success = _logDataManager.GetLogCount() <= testConfiguration.maxLogCount;
            
            return result;
        }
        
        /// <summary>
        /// Benchmarks concurrent logging performance
        /// </summary>
        private BenchmarkTestResult BenchmarkConcurrentLogging()
        {
            var result = new BenchmarkTestResult { TestName = "Concurrent Logging" };
            var stopwatch = Stopwatch.StartNew();
            long initialMemory = GC.GetTotalMemory(true);
            
            _profiler.StartProfiling("ConcurrentLogging");
            
            var tasks = new System.Threading.Tasks.Task[concurrentThreadCount];
            int logsPerThread = logEntryCount / concurrentThreadCount;
            
            for (int threadId = 0; threadId < concurrentThreadCount; threadId++)
            {
                int capturedThreadId = threadId;
                tasks[threadId] = System.Threading.Tasks.Task.Run(() =>
                {
                    for (int i = 0; i < logsPerThread; i++)
                    {
                        _logDataManager.AddLog($"Thread {capturedThreadId} message {i}", (LogLevel)(i % 3));
                    }
                });
            }
            
            System.Threading.Tasks.Task.WaitAll(tasks);
            
            _profiler.StopProfiling("ConcurrentLogging");
            
            stopwatch.Stop();
            long finalMemory = GC.GetTotalMemory(false);
            
            var stats = _profiler.GetStats("ConcurrentLogging");
            result.ExecutionTimeMs = (float)stopwatch.ElapsedMilliseconds;
            result.MemoryUsedBytes = finalMemory - initialMemory;
            result.AverageOperationTimeMs = stats?.AverageTimeMs ?? 0f;
            result.OperationsPerSecond = logEntryCount / (result.ExecutionTimeMs / 1000f);
            result.Success = result.ExecutionTimeMs < 10000 && _logDataManager.GetLogCount() <= testConfiguration.maxLogCount;
            
            return result;
        }
        
        /// <summary>
        /// Benchmarks text optimization performance
        /// </summary>
        private BenchmarkTestResult BenchmarkTextOptimization()
        {
            var result = new BenchmarkTestResult { TestName = "Text Optimization" };
            var stopwatch = Stopwatch.StartNew();
            long initialMemory = GC.GetTotalMemory(true);
            
            // Generate test text of various sizes
            var testTexts = new[]
            {
                GenerateTestText(1000, 20),
                GenerateTestText(5000, 100),
                GenerateTestText(20000, 400),
                GenerateTestText(50000, 1000)
            };
            
            _profiler.StartProfiling("TextOptimization");
            
            foreach (var testText in testTexts)
            {
                for (int i = 0; i < 100; i++)
                {
                    string optimized = _textOptimizer.OptimizeTextForDisplay(testText, testTextComponent);
                    var incrementalResult = _textOptimizer.OptimizeTextIncremental(testText, 5);
                }
            }
            
            _profiler.StopProfiling("TextOptimization");
            
            stopwatch.Stop();
            long finalMemory = GC.GetTotalMemory(false);
            
            var stats = _profiler.GetStats("TextOptimization");
            var optimizerStats = _textOptimizer.GetStats();
            
            result.ExecutionTimeMs = (float)stopwatch.ElapsedMilliseconds;
            result.MemoryUsedBytes = finalMemory - initialMemory;
            result.AverageOperationTimeMs = stats?.AverageTimeMs ?? 0f;
            result.AdditionalMetrics = new Dictionary<string, object>
            {
                ["TruncationCount"] = optimizerStats.TruncationCount,
                ["OptimizationCount"] = optimizerStats.OptimizationCount,
                ["LastProcessingTime"] = optimizerStats.LastProcessingTimeMs
            };
            result.Success = result.AverageOperationTimeMs < 10f;
            
            return result;
        }
        
        private string GenerateTestText(int characterCount, int lineCount)
        {
            var lines = new List<string>();
            int charactersPerLine = characterCount / lineCount;
            
            for (int i = 0; i < lineCount; i++)
            {
                string line = $"Line {i}: " + new string((char)('A' + (i % 26)), Math.Max(1, charactersPerLine - 10));
                lines.Add(line);
            }
            
            return string.Join("\n", lines);
        }
        
        private string GenerateBenchmarkReport(BenchmarkReport report)
        {
            var reportBuilder = new System.Text.StringBuilder();
            
            reportBuilder.AppendLine("=== LOGGING SYSTEM PERFORMANCE BENCHMARK REPORT ===");
            reportBuilder.AppendLine($"Generated: {report.EndTime}");
            reportBuilder.AppendLine($"Duration: {report.TotalDuration.TotalSeconds:F2} seconds");
            reportBuilder.AppendLine($"Success: {report.Success}");
            
            if (!string.IsNullOrEmpty(report.ErrorMessage))
            {
                reportBuilder.AppendLine($"Error: {report.ErrorMessage}");
            }
            
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("=== SYSTEM INFORMATION ===");
            reportBuilder.AppendLine($"Unity Version: {report.Configuration.UnityVersion}");
            reportBuilder.AppendLine($"Platform: {report.Configuration.Platform}");
            reportBuilder.AppendLine($"Device: {report.Configuration.DeviceModel}");
            reportBuilder.AppendLine($"Processor: {report.Configuration.ProcessorType}");
            reportBuilder.AppendLine($"System Memory: {report.Configuration.SystemMemorySize}MB");
            
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("=== BENCHMARK CONFIGURATION ===");
            reportBuilder.AppendLine($"Log Entry Count: {report.Configuration.LogEntryCount:N0}");
            reportBuilder.AppendLine($"TextMeshPro Update Count: {report.Configuration.TextMeshProUpdateCount:N0}");
            reportBuilder.AppendLine($"Concurrent Thread Count: {report.Configuration.ConcurrentThreadCount}");
            
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("=== BENCHMARK RESULTS ===");
            
            var allResults = new[]
            {
                report.LogCreationResults,
                report.TextFormattingResults,
                report.TextMeshProResults,
                report.MemoryManagementResults,
                report.ConcurrencyResults,
                report.OptimizationResults
            };
            
            foreach (var result in allResults)
            {
                if (result != null)
                {
                    reportBuilder.AppendLine($"\n--- {result.TestName} ---");
                    reportBuilder.AppendLine($"Success: {result.Success}");
                    reportBuilder.AppendLine($"Execution Time: {result.ExecutionTimeMs:F2}ms");
                    reportBuilder.AppendLine($"Memory Used: {result.MemoryUsedBytes / (1024 * 1024):F2}MB");
                    reportBuilder.AppendLine($"Average Operation Time: {result.AverageOperationTimeMs:F3}ms");
                    reportBuilder.AppendLine($"Operations/Second: {result.OperationsPerSecond:F0}");
                    
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        reportBuilder.AppendLine($"Error: {result.ErrorMessage}");
                    }
                    
                    if (result.AdditionalMetrics != null)
                    {
                        foreach (var metric in result.AdditionalMetrics)
                        {
                            reportBuilder.AppendLine($"{metric.Key}: {metric.Value}");
                        }
                    }
                }
            }
            
            return reportBuilder.ToString();
        }
        
        private void SaveReportToFile(string report, BenchmarkReport benchmarkReport)
        {
            try
            {
                // Use UTC time to avoid timezone manipulation and ensure consistent format
                string fileName = $"LoggingBenchmark_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
                
                // Validate filename doesn't contain invalid characters
                char[] invalidChars = Path.GetInvalidFileNameChars();
                foreach (char invalidChar in invalidChars)
                {
                    fileName = fileName.Replace(invalidChar, '_');
                }
                
                string filePath = Path.Combine(Application.persistentDataPath, fileName);
                
                // Validate the final path is within the expected directory
                string normalizedPath = Path.GetFullPath(filePath);
                string expectedBasePath = Path.GetFullPath(Application.persistentDataPath);
                
                if (!normalizedPath.StartsWith(expectedBasePath))
                {
                    UnityEngine.Debug.LogError("Invalid file path detected, using safe fallback");
                    fileName = $"LoggingBenchmark_Safe_{DateTime.UtcNow.Ticks}.txt";
                    filePath = Path.Combine(Application.persistentDataPath, fileName);
                }
                
                File.WriteAllText(filePath, report);
                UnityEngine.Debug.Log($"Benchmark report saved to: {filePath}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to save benchmark report: {ex.Message}");
            }
        }
        
        private void OnDestroy()
        {
            _memoryMonitor?.StopMonitoring();
            _profiler?.SetEnabled(false);
        }
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class BenchmarkConfiguration
    {
        public int LogEntryCount;
        public int TextMeshProUpdateCount;
        public int ConcurrentThreadCount;
        public string UnityVersion;
        public string Platform;
        public string DeviceModel;
        public string ProcessorType;
        public int SystemMemorySize;
    }
    
    [System.Serializable]
    public class BenchmarkReport
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public TimeSpan TotalDuration;
        public bool Success;
        public string ErrorMessage;
        public BenchmarkConfiguration Configuration;
        public BenchmarkTestResult LogCreationResults;
        public BenchmarkTestResult TextFormattingResults;
        public BenchmarkTestResult TextMeshProResults;
        public BenchmarkTestResult MemoryManagementResults;
        public BenchmarkTestResult ConcurrencyResults;
        public BenchmarkTestResult OptimizationResults;
    }
    
    [System.Serializable]
    public class BenchmarkTestResult
    {
        public string TestName;
        public bool Success;
        public string ErrorMessage;
        public float ExecutionTimeMs;
        public long MemoryUsedBytes;
        public float AverageOperationTimeMs;
        public float OperationsPerSecond;
        public Dictionary<string, object> AdditionalMetrics;
    }
    
    #endregion
}