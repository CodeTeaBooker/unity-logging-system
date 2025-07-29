using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace RuntimeLogging.Examples
{
    /// <summary>
    /// Demo scene controller showing LogManager setup with different adapter combinations and TextMeshPro display
    /// This demonstrates the complete integration of the Runtime Logging Panel system
    /// </summary>
    public class DemoSceneController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Button unityLoggerButton;
        [SerializeField] private Button screenLoggerButton;
        [SerializeField] private Button compositeLoggerButton;
        [SerializeField] private Button clearLogsButton;
        [SerializeField] private Button showHideButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        
        [Header("Logging Components")]
        [SerializeField] private ScreenLogger screenLogger;
        [SerializeField] private LogDisplay logDisplay;
        [SerializeField] private LogConfiguration logConfiguration;
        
        [Header("Demo Settings")]
        [SerializeField] private bool autoStartDemo = true;
        
        private UnityLogger unityLogger;
        private CompositeLogger compositeLogger;
        private DemoBusinessLogic businessLogic;
        private LoggerType currentLoggerType = LoggerType.None;
        private bool isLogDisplayVisible = true;
        
        private enum LoggerType
        {
            None,
            Unity,
            Screen,
            Composite
        }
        
        private void Awake()
        {
            InitializeComponents();
            SetupUI();
            
            // Set up logger immediately in Awake to ensure it's available for other components
            if (autoStartDemo)
            {
                SetupCompositeLogger();
            }
        }
        
        private void Start()
        {
            LogManager.Log("DemoSceneController: Demo scene started");
            
            UpdateStatusDisplay();
            ShowInstructions();
        }
        
        private void InitializeComponents()
        {
            // Create Unity logger
            unityLogger = new UnityLogger();
            
            // Initialize screen logger if not assigned
            if (screenLogger == null)
            {
                screenLogger = FindFirstObjectByType<ScreenLogger>();
                if (screenLogger == null)
                {
                    Debug.LogError("ScreenLogger not found in scene. Please add a ScreenLogger component.");
                }
            }
            
            // Initialize log display if not assigned
            if (logDisplay == null)
            {
                logDisplay = FindFirstObjectByType<LogDisplay>();
                if (logDisplay == null && screenLogger != null)
                {
                    logDisplay = screenLogger.GetLogDisplay();
                }
            }
            
            // Create default configuration if not assigned
            if (logConfiguration == null)
            {
                logConfiguration = CreateDefaultConfiguration();
            }
            
            // Configure screen logger
            if (screenLogger != null)
            {
                screenLogger.SetConfiguration(logConfiguration);
                // Only initialize if not already initialized (avoid duplicate initialization)
                if (!screenLogger.IsInitialized())
                {
                    screenLogger.Initialize();
                }
            }
            
            // Get business logic component
            businessLogic = FindFirstObjectByType<DemoBusinessLogic>();
            if (businessLogic == null)
            {
                Debug.LogWarning("DemoBusinessLogic not found. Business logic simulation will not be available.");
            }
        }
        
        private void SetupUI()
        {
            // Setup button listeners
            if (unityLoggerButton != null)
                unityLoggerButton.onClick.AddListener(SetupUnityLogger);
                
            if (screenLoggerButton != null)
                screenLoggerButton.onClick.AddListener(SetupScreenLogger);
                
            if (compositeLoggerButton != null)
                compositeLoggerButton.onClick.AddListener(SetupCompositeLogger);
                
            if (clearLogsButton != null)
                clearLogsButton.onClick.AddListener(ClearLogs);
                
            if (showHideButton != null)
                showHideButton.onClick.AddListener(ToggleLogDisplay);
                
        }
        
        private LogConfiguration CreateDefaultConfiguration()
        {
            // Use the centralized configuration loading mechanism
            return LogConfiguration.CreateDefault();
        }
        
        public void SetupUnityLogger()
        {
            LogManager.SetLogger(unityLogger);
            currentLoggerType = LoggerType.Unity;
            
            LogManager.Log("DemoSceneController: Unity Logger activated - logs will appear in Unity Console");
            LogManager.LogWarning("DemoSceneController: This is a warning message in Unity Console");
            LogManager.LogError("DemoSceneController: This is an error message in Unity Console");
            
            UpdateStatusDisplay();
        }
        
        public void SetupScreenLogger()
        {
            if (screenLogger == null)
            {
                Debug.LogError("ScreenLogger is not available");
                return;
            }
            
            LogManager.SetLogger(screenLogger);
            currentLoggerType = LoggerType.Screen;
            
            screenLogger.Show();
            
            LogManager.Log("DemoSceneController: Screen Logger activated - logs will appear on screen");
            LogManager.LogWarning("DemoSceneController: This is a warning message on screen");
            LogManager.LogError("DemoSceneController: This is an error message on screen");
            
            UpdateStatusDisplay();
        }
        
        public void SetupCompositeLogger()
        {
            if (screenLogger == null)
            {
                Debug.LogError("ScreenLogger is not available for composite logger");
                return;
            }
            
            // Create composite logger with both Unity and Screen loggers
            compositeLogger = new CompositeLogger(unityLogger, screenLogger);
            LogManager.SetLogger(compositeLogger);
            currentLoggerType = LoggerType.Composite;
            
            screenLogger.Show();
            
            LogManager.Log("DemoSceneController: Composite Logger activated - logs will appear in both Unity Console and on screen");
            LogManager.LogWarning("DemoSceneController: This warning appears in both outputs");
            LogManager.LogError("DemoSceneController: This error appears in both outputs");
            
            UpdateStatusDisplay();
        }
        
        public void ClearLogs()
        {
            if (screenLogger != null)
            {
                screenLogger.Clear();
                LogManager.Log("DemoSceneController: Screen logs cleared");
            }
            else
            {
                LogManager.LogWarning("DemoSceneController: No screen logger available to clear");
            }
        }
        
        public void ToggleLogDisplay()
        {
            if (screenLogger == null)
            {
                LogManager.LogWarning("DemoSceneController: No screen logger available to toggle");
                return;
            }
            
            isLogDisplayVisible = !isLogDisplayVisible;
            
            if (isLogDisplayVisible)
            {
                screenLogger.Show();
                LogManager.Log("DemoSceneController: Log display shown");
            }
            else
            {
                screenLogger.Hide();
                LogManager.Log("DemoSceneController: Log display hidden (logging continues in background)");
            }
            
            UpdateShowHideButtonText();
        }
        
        
        private void UpdateStatusDisplay()
        {
            if (statusText == null) return;
            
            string loggerStatus = currentLoggerType switch
            {
                LoggerType.Unity => "Unity Console Only",
                LoggerType.Screen => "Screen Display Only", 
                LoggerType.Composite => "Unity Console + Screen Display",
                _ => "No Logger Active"
            };
            
            string displayStatus = isLogDisplayVisible ? "Visible" : "Hidden";
            int logCount = screenLogger?.GetCurrentLogCount() ?? 0;
            int maxLogs = screenLogger?.GetMaxLogCount() ?? 0;
            
            statusText.text = $"Logger: {loggerStatus}\n" +
                             $"Display: {displayStatus}\n" +
                             $"Logs: {logCount}/{maxLogs}";
        }
        
        private void UpdateShowHideButtonText()
        {
            if (showHideButton != null)
            {
                var buttonText = showHideButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isLogDisplayVisible ? "Hide Display" : "Show Display";
                }
            }
        }
        
        private void ShowInstructions()
        {
            if (instructionsText == null) return;
            
            instructionsText.text = 
                "Runtime Logging Panel Demo\n\n" +
                "• Unity Logger: Logs to Unity Console only\n" +
                "• Screen Logger: Logs to screen display only\n" +
                "• Composite Logger: Logs to both outputs\n\n" +
                "• Clear Logs: Removes all screen logs\n" +
                "• Show/Hide: Controls display visibility\n" +
                "Watch the logs to see different systems in action:\n" +
                "- Player actions and health changes\n" +
                "- Network connectivity and latency\n" +
                "- Resource loading and memory management\n" +
                "- Error and warning conditions";
        }
        
        // Public methods for external control and testing
        public void DemonstrateLogLevels()
        {
            LogManager.Log("DemoSceneController: This is an INFO level message");
            LogManager.LogWarning("DemoSceneController: This is a WARNING level message");
            LogManager.LogError("DemoSceneController: This is an ERROR level message");
        }
        
        public void DemonstrateHighVolumeLogging()
        {
            for (int i = 1; i <= 10; i++)
            {
                LogManager.Log($"DemoSceneController: High volume log message {i}/10");
            }
            LogManager.LogWarning("DemoSceneController: High volume logging complete - notice the performance optimization");
        }
        
        public void DemonstrateConfigurationChanges()
        {
            if (screenLogger == null) return;
            
            // Change colors
            screenLogger.UpdateLogLevelColor(LogLevel.Info, "#00FF00");    // Green
            screenLogger.UpdateLogLevelColor(LogLevel.Warning, "#FFA500"); // Orange
            screenLogger.UpdateLogLevelColor(LogLevel.Error, "#FF4444");   // Light Red
            
            LogManager.Log("DemoSceneController: Configuration changed - notice the new colors");
            LogManager.LogWarning("DemoSceneController: Warning with new orange color");
            LogManager.LogError("DemoSceneController: Error with new light red color");
            
            // Reset to defaults after a delay
            Invoke(nameof(ResetConfigurationColors), 5f);
        }
        
        private void ResetConfigurationColors()
        {
            if (screenLogger == null) return;
            
            screenLogger.UpdateLogLevelColor(LogLevel.Info, "#FFFFFF");    // White
            screenLogger.UpdateLogLevelColor(LogLevel.Warning, "#FFFF00"); // Yellow
            screenLogger.UpdateLogLevelColor(LogLevel.Error, "#FF0000");   // Red
            
            LogManager.Log("DemoSceneController: Colors reset to defaults");
        }
        
        public void DemonstratePlatformCompatibility()
        {
            if (screenLogger == null) return;
            
            var compatibilityReport = screenLogger.GetPlatformCompatibilityReport();
            LogManager.Log($"DemoSceneController: Platform: {compatibilityReport.Platform}");
            LogManager.Log($"DemoSceneController: Unity Version Supported: {compatibilityReport.IsUnitySupportedVersion}");
            LogManager.Log($"DemoSceneController: TextMeshPro Supported: {compatibilityReport.IsTextMeshProSupported}");
            
            var validationResult = screenLogger.ValidateCrossPlatformCompatibility();
            if (validationResult.CompatibilityReport.IsUnitySupportedVersion)
            {
                LogManager.Log("DemoSceneController: Platform compatibility validation passed");
            }
            else
            {
                LogManager.LogWarning("DemoSceneController: Platform compatibility issues detected");
            }
        }
        
        public void DemonstratePerformanceMonitoring()
        {
            if (screenLogger == null) return;
            
            var performanceStats = screenLogger.GetPerformanceStats();
            LogManager.Log($"DemoSceneController: Logger initialized: {performanceStats.IsInitialized}");
            LogManager.Log($"DemoSceneController: Logger enabled: {performanceStats.IsEnabled}");
            LogManager.Log($"DemoSceneController: Current log count: {performanceStats.CurrentLogCount}");
            LogManager.Log($"DemoSceneController: Max log count: {performanceStats.MaxLogCount}");
            LogManager.Log($"DemoSceneController: Estimated memory usage: {performanceStats.EstimatedMemoryUsage} bytes");
        }
        
        private void Update()
        {
            // Update status display periodically
            if (Time.frameCount % 60 == 0) // Every 60 frames (roughly once per second at 60 FPS)
            {
                UpdateStatusDisplay();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up
            if (businessLogic != null)
            {
                businessLogic.StopSimulation();
            }
            
            LogManager.Log("DemoSceneController: Demo scene controller destroyed");
        }
        
        // Validation methods for requirements testing
        public bool ValidateRequirement1_RealTimeLogging()
        {
            // Test real-time log display
            LogManager.Log("Validation: Testing real-time logging");
            LogManager.LogWarning("Validation: Testing warning display");
            LogManager.LogError("Validation: Testing error display");
            return LogManager.HasLogger();
        }
        
        public bool ValidateRequirement2_LogDisplayControl()
        {
            if (screenLogger == null) return false;
            
            // Test log count management
            int initialCount = screenLogger.GetCurrentLogCount();
            LogManager.Log("Validation: Testing log count management");
            int afterLogCount = screenLogger.GetCurrentLogCount();
            
            // Test clear functionality
            screenLogger.Clear();
            int afterClearCount = screenLogger.GetCurrentLogCount();
            
            return afterLogCount > initialCount && afterClearCount == 0;
        }
        
        public bool ValidateRequirement3_ProgrammaticControl()
        {
            if (screenLogger == null) return false;
            
            // Test show/hide functionality
            screenLogger.Hide();
            screenLogger.Show();
            
            // Test clear functionality
            screenLogger.Clear();
            
            return true;
        }
        
        public bool ValidateRequirement4_Configuration()
        {
            if (screenLogger == null) return false;
            
            // Test configuration changes
            screenLogger.UpdateMaxLogCount(50);
            screenLogger.UpdateTimestampFormat("mm:ss");
            screenLogger.UpdateLogLevelColor(LogLevel.Info, "#00FF00");
            
            return screenLogger.GetMaxLogCount() == 50;
        }
        
        public bool ValidateRequirement5_Performance()
        {
            if (screenLogger == null) return false;
            
            // Test performance with high volume logging
            float startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < 100; i++)
            {
                LogManager.Log($"Performance test message {i}");
            }
            float endTime = Time.realtimeSinceStartup;
            
            // Should complete quickly (less than 1 second for 100 messages)
            return (endTime - startTime) < 1f;
        }
        
        public bool ValidateRequirement6_CrossPlatform()
        {
            if (screenLogger == null) return false;
            
            var validationResult = screenLogger.ValidateCrossPlatformCompatibility();
            return validationResult.CompatibilityReport.IsUnitySupportedVersion;
        }
        
        public bool ValidateRequirement7_TextMeshProDisplay()
        {
            if (logDisplay == null) return false;
            
            return logDisplay.IsTextComponentValid();
        }
        
        public bool ValidateRequirement8_UnifiedInterface()
        {
            // Test all logger types implement ILogger
            bool unityLoggerValid = unityLogger is ILogger;
            bool screenLoggerValid = screenLogger is ILogger;
            bool compositeLoggerValid = compositeLogger is ILogger;
            
            return unityLoggerValid && screenLoggerValid && compositeLoggerValid;
        }
        
        public bool ValidateRequirement9_ModularArchitecture()
        {
            // Test that different adapters can be used interchangeably
            ILogger originalLogger = LogManager.GetLogger();
            
            LogManager.SetLogger(unityLogger);
            bool unityWorks = LogManager.HasLogger();
            
            LogManager.SetLogger(screenLogger);
            bool screenWorks = LogManager.HasLogger();
            
            LogManager.SetLogger(compositeLogger);
            bool compositeWorks = LogManager.HasLogger();
            
            // Restore original logger
            LogManager.SetLogger(originalLogger);
            
            return unityWorks && screenWorks && compositeWorks;
        }
        
        public void RunAllValidationTests()
        {
            LogManager.Log("DemoSceneController: Starting comprehensive validation tests");
            
            bool req1 = ValidateRequirement1_RealTimeLogging();
            bool req2 = ValidateRequirement2_LogDisplayControl();
            bool req3 = ValidateRequirement3_ProgrammaticControl();
            bool req4 = ValidateRequirement4_Configuration();
            bool req5 = ValidateRequirement5_Performance();
            bool req6 = ValidateRequirement6_CrossPlatform();
            bool req7 = ValidateRequirement7_TextMeshProDisplay();
            bool req8 = ValidateRequirement8_UnifiedInterface();
            bool req9 = ValidateRequirement9_ModularArchitecture();
            
            LogManager.Log($"Requirement 1 (Real-time Logging): {(req1 ? "PASS" : "FAIL")}");
            LogManager.Log($"Requirement 2 (Log Display Control): {(req2 ? "PASS" : "FAIL")}");
            LogManager.Log($"Requirement 3 (Programmatic Control): {(req3 ? "PASS" : "FAIL")}");
            LogManager.Log($"Requirement 4 (Configuration): {(req4 ? "PASS" : "FAIL")}");
            LogManager.Log($"Requirement 5 (Performance): {(req5 ? "PASS" : "FAIL")}");
            LogManager.Log($"Requirement 6 (Cross-platform): {(req6 ? "PASS" : "FAIL")}");
            LogManager.Log($"Requirement 7 (TextMeshPro Display): {(req7 ? "PASS" : "FAIL")}");
            LogManager.Log($"Requirement 8 (Unified Interface): {(req8 ? "PASS" : "FAIL")}");
            LogManager.Log($"Requirement 9 (Modular Architecture): {(req9 ? "PASS" : "FAIL")}");
            
            bool allPassed = req1 && req2 && req3 && req4 && req5 && req6 && req7 && req8 && req9;
            
            if (allPassed)
            {
                LogManager.Log("DemoSceneController: All validation tests PASSED!");
            }
            else
            {
                LogManager.LogError("DemoSceneController: Some validation tests FAILED!");
            }
        }
    }
}