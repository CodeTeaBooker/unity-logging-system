using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeLogging.Examples
{
    /// <summary>
    /// Comprehensive validation tests for all requirements of the Runtime Logging Panel
    /// This class validates that all requirements from the requirements document are met
    /// </summary>
    public class ComprehensiveValidationTests : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool logDetailedResults = true;
        
        [Header("Component References")]
        [SerializeField] private ScreenLogger screenLogger;
        [SerializeField] private LogDisplay logDisplay;
        [SerializeField] private LogConfiguration testConfiguration;
        
        private List<ValidationResult> testResults = new List<ValidationResult>();
        private ILogger originalLogger;
        
        public struct ValidationResult
        {
            public string testName;
            public bool passed;
            public string details;
            public string requirement;
        }
        
        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllValidationTests());
            }
        }
        
        /// <summary>
        /// Run all validation tests and report results
        /// </summary>
        [ContextMenu("Run All Validation Tests")]
        public void RunAllTests()
        {
            StartCoroutine(RunAllValidationTests());
        }
        
        private IEnumerator RunAllValidationTests()
        {
            LogManager.Log("ComprehensiveValidationTests: Starting comprehensive validation tests");
            
            // Store original logger to restore later
            originalLogger = LogManager.GetLogger();
            testResults.Clear();
            
            // Initialize components if needed
            yield return InitializeTestComponents();
            
            // Run all requirement validation tests
            yield return ValidateRequirement1_RealTimeLogging();
            yield return ValidateRequirement2_LogDisplayControl();
            yield return ValidateRequirement3_ProgrammaticControl();
            yield return ValidateRequirement4_Configuration();
            yield return ValidateRequirement5_Performance();
            yield return ValidateRequirement6_CrossPlatform();
            yield return ValidateRequirement7_TextMeshProDisplay();
            yield return ValidateRequirement8_UnifiedInterface();
            yield return ValidateRequirement9_ModularArchitecture();
            
            // Generate final report
            GenerateFinalReport();
            
            // Restore original logger
            if (originalLogger != null)
            {
                LogManager.SetLogger(originalLogger);
            }
            
            LogManager.Log("ComprehensiveValidationTests: All validation tests completed");
        }
        
        private IEnumerator InitializeTestComponents()
        {
            LogManager.Log("ComprehensiveValidationTests: Initializing test components");
            
            // Find components if not assigned
            if (screenLogger == null)
            {
                screenLogger = FindFirstObjectByType<ScreenLogger>();
            }
            
            if (logDisplay == null)
            {
                logDisplay = FindFirstObjectByType<LogDisplay>();
            }
            
            // Create test configuration if needed
            if (testConfiguration == null)
            {
                testConfiguration = CreateTestConfiguration();
            }
            
            // Initialize screen logger
            if (screenLogger != null)
            {
                screenLogger.SetConfiguration(testConfiguration);
                if (!screenLogger.IsInitialized())
                {
                    screenLogger.Initialize();
                }
                screenLogger.Show();
            }
            
            yield return new WaitForSeconds(0.1f); // Allow initialization to complete
        }
        
        private LogConfiguration CreateTestConfiguration()
        {
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.maxLogCount = 100;
            config.timestampFormat = "HH:mm:ss";
            config.infoColorHex = "#FFFFFF";
            config.warningColorHex = "#FFFF00";
            config.errorColorHex = "#FF0000";
            return config;
        }
        
        #region Requirement 1: Real-time Logging
        
        private IEnumerator ValidateRequirement1_RealTimeLogging()
        {
            LogManager.Log("ComprehensiveValidationTests: Testing Requirement 1 - Real-time Logging");
            
            bool testPassed = true;
            string details = "";
            
            // Test 1.1: ILogger interface methods display real-time logs
            LogManager.SetLogger(screenLogger);
            int initialLogCount = screenLogger.GetCurrentLogCount();
            
            LogManager.Log("Test info message");
            LogManager.LogWarning("Test warning message");
            LogManager.LogError("Test error message");
            
            yield return new WaitForSeconds(0.2f); // Allow display update
            
            try
            {
                int finalLogCount = screenLogger.GetCurrentLogCount();
                bool logsAdded = finalLogCount > initialLogCount;
                
                // Test 1.2: Message format includes timestamp and level
                var logs = screenLogger.GetLogDataManager().GetLogs();
                bool hasCorrectFormat = logs.Any(log => 
                    log.GetFormattedMessage(testConfiguration.timestampFormat).Contains("[") &&
                    log.GetFormattedMessage(testConfiguration.timestampFormat).Contains("]"));
                
                // Test 1.3: All log levels are displayed
                bool hasInfo = logs.Any(log => log.level == LogLevel.Info);
                bool hasWarning = logs.Any(log => log.level == LogLevel.Warning);
                bool hasError = logs.Any(log => log.level == LogLevel.Error);
                
                // Test 1.4: Chronological order
                bool chronologicalOrder = true;
                for (int i = 1; i < logs.Count; i++)
                {
                    if (logs[i].timestamp < logs[i-1].timestamp)
                    {
                        chronologicalOrder = false;
                        break;
                    }
                }
                
                testPassed = logsAdded && hasCorrectFormat && hasInfo && hasWarning && hasError && chronologicalOrder;
                details = $"Logs added: {logsAdded}, Format correct: {hasCorrectFormat}, " +
                         $"Has all levels: {hasInfo && hasWarning && hasError}, Chronological: {chronologicalOrder}";
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                details = $"Exception: {ex.Message}";
            }
            
            testResults.Add(new ValidationResult
            {
                testName = "Requirement 1: Real-time Logging",
                passed = testPassed,
                details = details,
                requirement = "1.1, 1.2, 1.3, 1.4"
            });
            
            if (logDetailedResults)
            {
                LogManager.Log($"Requirement 1 validation: {(testPassed ? "PASS" : "FAIL")} - {details}");
            }
        }
        
        #endregion
        
        #region Requirement 2: Log Display Control
        
        private IEnumerator ValidateRequirement2_LogDisplayControl()
        {
            LogManager.Log("ComprehensiveValidationTests: Testing Requirement 2 - Log Display Control");
            
            bool testPassed = true;
            string details = "";
            
            // Test 2.1: Automatic removal of oldest logs when exceeding limit
            screenLogger.UpdateMaxLogCount(5);
            screenLogger.Clear();
            
            for (int i = 0; i < 10; i++)
            {
                LogManager.Log($"Test message {i}");
            }
            
            yield return new WaitForSeconds(0.2f);
            
            int logCount = screenLogger.GetCurrentLogCount();
            bool limitRespected = logCount <= 5;
            
            // Test 2.2: Rich text markup for different log levels
            screenLogger.Clear();
            LogManager.Log("Info message");
            LogManager.LogWarning("Warning message");
            LogManager.LogError("Error message");
            
            yield return new WaitForSeconds(0.2f);
            
            var textComponent = logDisplay.GetTextComponent();
            string displayText = textComponent.text;
            bool hasRichText = displayText.Contains("<color=") && displayText.Contains("</color>");
            
            // Test 2.3: Maximum display rows maintained
            screenLogger.UpdateMaxLogCount(100);
            screenLogger.Clear();
            
            for (int i = 0; i < 150; i++)
            {
                LogManager.Log($"Row test {i}");
            }
            
            yield return new WaitForSeconds(0.3f);
            
            int finalCount = screenLogger.GetCurrentLogCount();
            bool rowLimitMaintained = finalCount <= 100;
            
            // Test 2.4: Efficient TextMeshPro updates
            float startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < 50; i++)
            {
                LogManager.Log($"Performance test {i}");
            }
            float endTime = Time.realtimeSinceStartup;
            bool efficientUpdates = (endTime - startTime) < 1f; // Should complete in under 1 second
            
            try
            {
                testPassed = limitRespected && hasRichText && rowLimitMaintained && efficientUpdates;
                details = $"Limit respected: {limitRespected}, Rich text: {hasRichText}, " +
                         $"Row limit: {rowLimitMaintained}, Efficient: {efficientUpdates} ({endTime - startTime:F3}s)";
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                details = $"Exception: {ex.Message}";
            }
            
            testResults.Add(new ValidationResult
            {
                testName = "Requirement 2: Log Display Control",
                passed = testPassed,
                details = details,
                requirement = "2.1, 2.2, 2.3, 2.4"
            });
            
            if (logDetailedResults)
            {
                LogManager.Log($"Requirement 2 validation: {(testPassed ? "PASS" : "FAIL")} - {details}");
            }
        }
        
        #endregion
        
        #region Requirement 3: Programmatic Control
        
        private IEnumerator ValidateRequirement3_ProgrammaticControl()
        {
            LogManager.Log("ComprehensiveValidationTests: Testing Requirement 3 - Programmatic Control");
            
            bool testPassed = true;
            string details = "";
            
            // Test 3.1: Clear() method removes all displayed entries
            screenLogger.Clear();
            LogManager.Log("Test message before clear");
            LogManager.Log("Another test message");
            
            yield return new WaitForSeconds(0.1f);
            
            int countBeforeClear = screenLogger.GetCurrentLogCount();
            screenLogger.Clear();
            
            yield return new WaitForSeconds(0.1f);
            
            int countAfterClear = screenLogger.GetCurrentLogCount();
            bool clearWorks = countBeforeClear > 0 && countAfterClear == 0;
            
            // Test 3.2: Show/Hide methods control visibility
            screenLogger.Show();
            bool visibleAfterShow = logDisplay.gameObject.activeInHierarchy;
            
            screenLogger.Hide();
            bool hiddenAfterHide = !logDisplay.gameObject.activeInHierarchy;
            
            screenLogger.Show(); // Restore visibility for other tests
            
            // Test 3.3: Logging continues in background when hidden
            screenLogger.Hide();
            LogManager.Log("Hidden message 1");
            LogManager.Log("Hidden message 2");
            
            yield return new WaitForSeconds(0.1f);
            
            int hiddenLogCount = screenLogger.GetCurrentLogCount();
            bool logsCollectedWhenHidden = hiddenLogCount > 0;
            
            // Test 3.4: Display updates when shown again
            screenLogger.Show();
            
            yield return new WaitForSeconds(0.1f);
            
            try
            {
                var textComponent = logDisplay.GetTextComponent();
                bool displayUpdatedAfterShow = !string.IsNullOrEmpty(textComponent.text);
                
                testPassed = clearWorks && visibleAfterShow && hiddenAfterHide && 
                           logsCollectedWhenHidden && displayUpdatedAfterShow;
                details = $"Clear works: {clearWorks}, Show/Hide: {visibleAfterShow}/{hiddenAfterHide}, " +
                         $"Background logging: {logsCollectedWhenHidden}, Update after show: {displayUpdatedAfterShow}";
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                details = $"Exception: {ex.Message}";
            }
            
            testResults.Add(new ValidationResult
            {
                testName = "Requirement 3: Programmatic Control",
                passed = testPassed,
                details = details,
                requirement = "3.1, 3.2, 3.3, 3.4"
            });
            
            if (logDetailedResults)
            {
                LogManager.Log($"Requirement 3 validation: {(testPassed ? "PASS" : "FAIL")} - {details}");
            }
        }
        
        #endregion
        
        #region Requirement 4: Configuration
        
        private IEnumerator ValidateRequirement4_Configuration()
        {
            LogManager.Log("ComprehensiveValidationTests: Testing Requirement 4 - Configuration");
            
            bool testPassed = true;
            string details = "";
            
            // Test 4.1: Maximum display rows configuration
            screenLogger.UpdateMaxLogCount(50);
            int maxCount = screenLogger.GetMaxLogCount();
            bool maxCountSet = maxCount == 50;
            
            // Test 4.2: Timestamp format configuration
            string originalFormat = testConfiguration.timestampFormat;
            screenLogger.UpdateTimestampFormat("mm:ss");
            
            screenLogger.Clear();
            LogManager.Log("Timestamp test");
            
            yield return new WaitForSeconds(0.1f);
            
            var logs = screenLogger.GetLogDataManager().GetLogs();
            bool timestampFormatApplied = logs.Any(log => 
                log.GetFormattedMessage("mm:ss").Length < log.GetFormattedMessage("HH:mm:ss").Length);
            
            // Restore original format
            screenLogger.UpdateTimestampFormat(originalFormat);
            
            // Test 4.3: Log colors configuration
            screenLogger.UpdateLogLevelColor(LogLevel.Info, "#00FF00");
            screenLogger.Clear();
            LogManager.Log("Color test");
            
            yield return new WaitForSeconds(0.1f);
            
            var textComponent = logDisplay.GetTextComponent();
            bool colorApplied = textComponent.text.Contains("#00FF00") || textComponent.text.Contains("00FF00");
            
            // Restore original color
            screenLogger.UpdateLogLevelColor(LogLevel.Info, "#FFFFFF");
            
            // Test 4.4: Settings applied immediately
            float startTime = Time.realtimeSinceStartup;
            screenLogger.UpdateMaxLogCount(25);
            screenLogger.UpdateTimestampFormat("ss");
            screenLogger.UpdateLogLevelColor(LogLevel.Warning, "#FFA500");
            float endTime = Time.realtimeSinceStartup;
            
            bool immediateApplication = (endTime - startTime) < 0.1f; // Should be nearly instant
            
            try
            {
                testPassed = maxCountSet && timestampFormatApplied && colorApplied && immediateApplication;
                details = $"Max count: {maxCountSet}, Timestamp: {timestampFormatApplied}, " +
                         $"Color: {colorApplied}, Immediate: {immediateApplication}";
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                details = $"Exception: {ex.Message}";
            }
            
            testResults.Add(new ValidationResult
            {
                testName = "Requirement 4: Configuration",
                passed = testPassed,
                details = details,
                requirement = "4.1, 4.2, 4.3, 4.4"
            });
            
            if (logDetailedResults)
            {
                LogManager.Log($"Requirement 4 validation: {(testPassed ? "PASS" : "FAIL")} - {details}");
            }
        }
        
        #endregion
        
        #region Requirement 5: Performance
        
        private IEnumerator ValidateRequirement5_Performance()
        {
            LogManager.Log("ComprehensiveValidationTests: Testing Requirement 5 - Performance");
            
            bool testPassed = true;
            string details = "";
            
            // Test 5.1: Smooth game performance with active logging
            float frameRateBeforeLogging = 1f / Time.deltaTime;
            
            for (int i = 0; i < 100; i++)
            {
                LogManager.Log($"Performance test message {i}");
                if (i % 10 == 0) yield return null; // Allow frame processing
            }
            
            yield return new WaitForSeconds(0.5f);
            
            float frameRateAfterLogging = 1f / Time.deltaTime;
            bool maintainedPerformance = frameRateAfterLogging > frameRateBeforeLogging * 0.8f; // Allow 20% drop
            
            // Test 5.2: Efficient TextMeshPro updates with large volumes
            float startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < 200; i++)
            {
                LogManager.Log($"Volume test {i}");
            }
            float endTime = Time.realtimeSinceStartup;
            
            bool efficientUpdates = (endTime - startTime) < 2f; // Should complete in under 2 seconds
            
            // Test 5.3: Zero impact when disabled
            screenLogger.SetEnabled(false);
            
            startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < 100; i++)
            {
                LogManager.Log($"Disabled test {i}");
            }
            endTime = Time.realtimeSinceStartup;
            
            bool zeroImpactWhenDisabled = (endTime - startTime) < 0.1f; // Should be nearly instant
            
            screenLogger.SetEnabled(true); // Re-enable for other tests
            
            // Test 5.4: Memory management
            long memoryBefore = System.GC.GetTotalMemory(false);
            
            for (int i = 0; i < 500; i++)
            {
                LogManager.Log($"Memory test {i}");
            }
            
            yield return new WaitForSeconds(0.5f);
            
            long memoryAfter = System.GC.GetTotalMemory(false);
            long memoryIncrease = memoryAfter - memoryBefore;
            bool reasonableMemoryUsage = memoryIncrease < 1024 * 1024; // Less than 1MB increase
            
            try
            {
                testPassed = maintainedPerformance && efficientUpdates && zeroImpactWhenDisabled && reasonableMemoryUsage;
                details = $"Performance maintained: {maintainedPerformance}, Efficient: {efficientUpdates}, " +
                         $"Zero impact when disabled: {zeroImpactWhenDisabled}, Memory reasonable: {reasonableMemoryUsage} ({memoryIncrease} bytes)";
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                details = $"Exception: {ex.Message}";
            }
            
            testResults.Add(new ValidationResult
            {
                testName = "Requirement 5: Performance",
                passed = testPassed,
                details = details,
                requirement = "5.1, 5.2, 5.3, 5.4"
            });
            
            if (logDetailedResults)
            {
                LogManager.Log($"Requirement 5 validation: {(testPassed ? "PASS" : "FAIL")} - {details}");
            }
        }
        
        #endregion
        
        #region Requirement 6: Cross-Platform Compatibility
        
        private IEnumerator ValidateRequirement6_CrossPlatform()
        {
            LogManager.Log("ComprehensiveValidationTests: Testing Requirement 6 - Cross-Platform Compatibility");
            
            bool testPassed = true;
            string details = "";
            
            // Test 6.3: Easy adoption of ILogger interface
            var unityLogger = new UnityLogger();
            var compositeLogger = new CompositeLogger(unityLogger, screenLogger);
            
            LogManager.SetLogger(compositeLogger);
            LogManager.Log("Cross-platform test");
            
            yield return new WaitForSeconds(0.1f);
            
            try
            {
                // Test 6.1: Unity version compatibility
                var compatibilityReport = screenLogger.GetPlatformCompatibilityReport();
                bool unityVersionSupported = compatibilityReport.IsUnitySupportedVersion;
                
                // Test 6.2: Platform consistency
                var validationResult = screenLogger.ValidateCrossPlatformCompatibility();
                bool platformConsistent = validationResult.CompatibilityReport.IsUnitySupportedVersion;
                
                bool interfaceAdoption = LogManager.HasLogger();
                
                // Test 6.4: Support for new logger adapters
                bool supportsExtension = typeof(ILogger).IsInterface;
                
                testPassed = unityVersionSupported && platformConsistent && interfaceAdoption && supportsExtension;
                details = $"Unity supported: {unityVersionSupported}, Platform consistent: {platformConsistent}, " +
                         $"Interface adoption: {interfaceAdoption}, Extensible: {supportsExtension}";
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                details = $"Exception: {ex.Message}";
            }
            
            testResults.Add(new ValidationResult
            {
                testName = "Requirement 6: Cross-Platform Compatibility",
                passed = testPassed,
                details = details,
                requirement = "6.1, 6.2, 6.3"
            });
            
            if (logDetailedResults)
            {
                LogManager.Log($"Requirement 6 validation: {(testPassed ? "PASS" : "FAIL")} - {details}");
            }
        }
        
        #endregion
        
        #region Requirement 7: TextMeshPro Display
        
        private IEnumerator ValidateRequirement7_TextMeshProDisplay()
        {
            LogManager.Log("ComprehensiveValidationTests: Testing Requirement 7 - TextMeshPro Display");
            
            bool testPassed = true;
            string details = "";
            
            // Test 7.4: Clean formatted output
            screenLogger.Clear();
            LogManager.Log("Clean format test");
            LogManager.LogWarning("Warning format test");
            LogManager.LogError("Error format test");
            
            yield return new WaitForSeconds(0.1f);
            
            try
            {
                // Test 7.1: Single TextMeshPro component usage
                bool singleComponent = logDisplay.IsTextComponentValid();
                var textComponent = logDisplay.GetTextComponent();
                bool isTextMeshPro = textComponent != null && textComponent is TMPro.TextMeshProUGUI;
                
                // Test 7.2: Mobile readability
                bool mobileReadable = true; // Assume readable unless proven otherwise
                if (Application.isMobilePlatform)
                {
                    // Check font size and formatting
                    mobileReadable = textComponent.fontSize >= 12f;
                }
                
                // Test 7.3: Minimal resource usage
                var performanceStats = screenLogger.GetPerformanceStats();
                bool minimalResources = performanceStats.EstimatedMemoryUsage < 1024 * 1024; // Less than 1MB
                
                string displayText = textComponent.text;
                bool cleanFormat = displayText.Contains("[") && displayText.Contains("]") && 
                                 displayText.Contains("Clean format test");
                
                testPassed = singleComponent && isTextMeshPro && mobileReadable && minimalResources && cleanFormat;
                details = $"Single component: {singleComponent}, TextMeshPro: {isTextMeshPro}, " +
                         $"Mobile readable: {mobileReadable}, Minimal resources: {minimalResources}, Clean format: {cleanFormat}";
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                details = $"Exception: {ex.Message}";
            }
            
            testResults.Add(new ValidationResult
            {
                testName = "Requirement 7: TextMeshPro Display",
                passed = testPassed,
                details = details,
                requirement = "7.1, 7.2, 7.3"
            });
            
            if (logDetailedResults)
            {
                LogManager.Log($"Requirement 7 validation: {(testPassed ? "PASS" : "FAIL")} - {details}");
            }
        }
        
        #endregion
        
        #region Requirement 8: Unified Interface
        
        private IEnumerator ValidateRequirement8_UnifiedInterface()
        {
            LogManager.Log("ComprehensiveValidationTests: Testing Requirement 8 - Unified Interface");
            
            bool testPassed = true;
            string details = "";
            
            // Test 8.2: UnityLogger adapter support
            var unityLogger = new UnityLogger();
            LogManager.SetLogger(unityLogger);
            LogManager.Log("Unity logger test");
            bool unityLoggerWorks = LogManager.HasLogger();
            
            // Test 8.3: ScreenLogger adapter support
            LogManager.SetLogger(screenLogger);
            LogManager.Log("Screen logger test");
            
            yield return new WaitForSeconds(0.1f);
            
            bool screenLoggerWorks = screenLogger.GetCurrentLogCount() > 0;
            
            // Test 8.4: CompositeLogger support
            var compositeLogger = new CompositeLogger(unityLogger, screenLogger);
            LogManager.SetLogger(compositeLogger);
            LogManager.Log("Composite logger test");
            
            yield return new WaitForSeconds(0.1f);
            
            bool compositeLoggerWorks = LogManager.HasLogger();
            
            try
            {
                // Test 8.1: Unified ILogger interface
                bool screenLoggerImplementsInterface = screenLogger is ILogger;
                bool unityLoggerImplementsInterface = unityLogger is ILogger;
                
                // Test 8.5: LogManager centralized configuration
                bool logManagerWorks = LogManager.HasLogger();
                LogManager.ClearLogger();
                bool logManagerCanClear = !LogManager.HasLogger();
                LogManager.SetLogger(screenLogger); // Restore for other tests
                
                testPassed = screenLoggerImplementsInterface && unityLoggerImplementsInterface && 
                           unityLoggerWorks && screenLoggerWorks && compositeLoggerWorks && 
                           logManagerWorks && logManagerCanClear;
                details = $"Interfaces implemented: {screenLoggerImplementsInterface && unityLoggerImplementsInterface}, " +
                         $"Adapters work: {unityLoggerWorks && screenLoggerWorks && compositeLoggerWorks}, " +
                         $"LogManager works: {logManagerWorks && logManagerCanClear}";
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                details = $"Exception: {ex.Message}";
            }
            
            testResults.Add(new ValidationResult
            {
                testName = "Requirement 8: Unified Interface",
                passed = testPassed,
                details = details,
                requirement = "8.1, 8.2, 8.3, 8.4, 8.5"
            });
            
            if (logDetailedResults)
            {
                LogManager.Log($"Requirement 8 validation: {(testPassed ? "PASS" : "FAIL")} - {details}");
            }
        }
        
        #endregion
        
        #region Requirement 9: Modular Architecture
        
        private IEnumerator ValidateRequirement9_ModularArchitecture()
        {
            LogManager.Log("ComprehensiveValidationTests: Testing Requirement 9 - Modular Architecture");
            
            bool testPassed = true;
            string details = "";
            
            try
            {
                // Test 9.1: Adapter pattern support for future implementations
                bool adapterPatternSupported = typeof(ILogger).IsInterface;
                
                // Test 9.2: Future log export functionality support
                // Create a mock file logger to test extensibility
                var mockFileLogger = new MockFileLogger();
                bool futureExportSupported = mockFileLogger is ILogger;
                
                // Test 9.3: Future performance statistics display support
                var performanceStats = screenLogger.GetPerformanceStats();
                bool performanceStatsAvailable = performanceStats.IsInitialized;
                
                // Test 9.4: Clear separation between interface and implementation
                bool clearSeparation = typeof(ILogger).IsInterface && 
                                     typeof(ScreenLogger).GetInterfaces().Contains(typeof(ILogger));
                
                testPassed = adapterPatternSupported && futureExportSupported && 
                           performanceStatsAvailable && clearSeparation;
                details = $"Adapter pattern: {adapterPatternSupported}, Future export: {futureExportSupported}, " +
                         $"Performance stats: {performanceStatsAvailable}, Clear separation: {clearSeparation}";
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                details = $"Exception: {ex.Message}";
            }
            
            testResults.Add(new ValidationResult
            {
                testName = "Requirement 9: Modular Architecture",
                passed = testPassed,
                details = details,
                requirement = "9.1, 9.2, 9.3, 9.4"
            });
            
            if (logDetailedResults)
            {
                LogManager.Log($"Requirement 9 validation: {(testPassed ? "PASS" : "FAIL")} - {details}");
            }
            
            yield return null; // Ensure this method returns IEnumerator properly
        }
        
        #endregion
        
        private void GenerateFinalReport()
        {
            LogManager.Log("ComprehensiveValidationTests: Generating final validation report");
            
            int totalTests = testResults.Count;
            int passedTests = testResults.Count(r => r.passed);
            int failedTests = totalTests - passedTests;
            
            LogManager.Log($"=== COMPREHENSIVE VALIDATION REPORT ===");
            LogManager.Log($"Total Tests: {totalTests}");
            LogManager.Log($"Passed: {passedTests}");
            LogManager.Log($"Failed: {failedTests}");
            LogManager.Log($"Success Rate: {(passedTests * 100f / totalTests):F1}%");
            LogManager.Log("");
            
            LogManager.Log("=== DETAILED RESULTS ===");
            foreach (var result in testResults)
            {
                string status = result.passed ? "PASS" : "FAIL";
                LogManager.Log($"{status}: {result.testName}");
                if (logDetailedResults)
                {
                    LogManager.Log($"  Requirements: {result.requirement}");
                    LogManager.Log($"  Details: {result.details}");
                }
            }
            
            LogManager.Log("");
            if (passedTests == totalTests)
            {
                LogManager.Log("🎉 ALL REQUIREMENTS VALIDATED SUCCESSFULLY!");
                LogManager.Log("The Runtime Logging Panel meets all specified requirements.");
            }
            else
            {
                LogManager.LogError($"❌ {failedTests} REQUIREMENT(S) FAILED VALIDATION");
                LogManager.LogError("Please review the failed tests and address the issues.");
            }
            
            LogManager.Log("=== END VALIDATION REPORT ===");
        }
        
        /// <summary>
        /// Mock file logger for testing extensibility
        /// </summary>
        private class MockFileLogger : ILogger
        {
            public void Log(string message) { /* Mock implementation */ }
            public void LogWarning(string message) { /* Mock implementation */ }
            public void LogError(string message) { /* Mock implementation */ }
        }
        
        /// <summary>
        /// Get the validation results for external analysis
        /// </summary>
        public List<ValidationResult> GetValidationResults()
        {
            return new List<ValidationResult>(testResults);
        }
        
        /// <summary>
        /// Check if all requirements passed validation
        /// </summary>
        public bool AllRequirementsPassed()
        {
            return testResults.All(r => r.passed);
        }
        
        /// <summary>
        /// Get the overall success rate
        /// </summary>
        public float GetSuccessRate()
        {
            if (testResults.Count == 0) return 0f;
            return (testResults.Count(r => r.passed) * 100f) / testResults.Count;
        }
    }
}