using UnityEngine;
using System;

namespace RuntimeLogging
{
    /// <summary>
    /// ScreenLogger MonoBehaviour class implementing ILogger interface
    /// Integrates with LogDataManager for log storage and LogDisplay for TextMeshPro output
    /// Provides public API methods for external control and performance optimization
    /// </summary>
    public class ScreenLogger : MonoBehaviour, ILogger
    {
        [Header("Components")]
        [SerializeField] private LogDisplay logDisplay;
        [SerializeField] private LogConfiguration configuration;
        
        [Header("Performance Settings")]
        [SerializeField] private bool enableWhenDisabled = false;
        [SerializeField] private bool autoCreateLogDisplay = true;
        
        private LogDataManager logDataManager;
        private bool isInitialized = false;
        private bool isEnabled = true;
        
        /// <summary>
        /// Event fired when the logger is enabled or disabled
        /// </summary>
        public event Action<bool> OnLoggerStateChanged;
        
        /// <summary>
        /// Event fired when logs are cleared
        /// </summary>
        public event Action OnLogsCleared;
        
        private void Awake()
        {
            Debug.Log($"ScreenLogger: Awake called on Platform: {Application.platform}");
            InitializeComponents();
        }
        
        private void OnEnable()
        {
            if (isInitialized)
            {
                isEnabled = true;
                OnLoggerStateChanged?.Invoke(true);
            }
        }
        
        private void OnDisable()
        {
            if (isInitialized)
            {
                isEnabled = false;
                OnLoggerStateChanged?.Invoke(false);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (logDataManager != null)
            {
                logDataManager.OnLogAdded -= OnLogAdded;
                logDataManager.OnLogsCleared -= OnLogDataManagerLogsCleared;
            }
            
            if (configuration != null)
            {
                configuration.OnConfigurationChanged -= OnConfigurationChanged;
            }
        }
        
        /// <summary>
        /// Initialize components and set up event connections
        /// </summary>
        private void InitializeComponents()
        {
            Debug.Log($"ScreenLogger: InitializeComponents called. AutoCreate: {autoCreateLogDisplay}");
            
            // Auto-create LogDisplay if needed and enabled
            if (logDisplay == null && autoCreateLogDisplay)
            {
                logDisplay = GetComponent<LogDisplay>();
                Debug.Log($"ScreenLogger: Found existing LogDisplay: {logDisplay != null}");
                
                if (logDisplay == null)
                {
                    logDisplay = gameObject.AddComponent<LogDisplay>();
                    Debug.Log($"ScreenLogger: Created new LogDisplay component: {logDisplay != null}");
                }
            }
            
            // Create default configuration if none provided
            if (configuration == null)
            {
                configuration = CreateDefaultConfiguration();
            }
            
            // Initialize LogDataManager
            logDataManager = new LogDataManager(configuration);
            
            // Connect LogDataManager events to LogDisplay for automatic TextMeshPro updates
            logDataManager.OnLogAdded += OnLogAdded;
            logDataManager.OnLogsCleared += OnLogDataManagerLogsCleared;
            
            isInitialized = true;
        }
        
        /// <summary>
        /// Creates a default configuration if none is provided
        /// </summary>
        /// <returns>Default LogConfiguration instance</returns>
        private LogConfiguration CreateDefaultConfiguration()
        {
            // Use platform-optimized configuration
            var config = PlatformCompatibility.GetPlatformOptimizedConfiguration();
            
            // Subscribe to configuration changes for runtime updates
            config.OnConfigurationChanged += OnConfigurationChanged;
            
            return config;
        }
        
        /// <summary>
        /// Log an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Log(string message)
        {
            // Performance optimization: zero impact when disabled
            if (!isEnabled && !enableWhenDisabled)
                return;
                
            if (!isInitialized)
                InitializeComponents();
                
            logDataManager?.AddLog(message, LogLevel.Info);
        }
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">The warning message to log</param>
        public void LogWarning(string message)
        {
            // Performance optimization: zero impact when disabled
            if (!isEnabled && !enableWhenDisabled)
                return;
                
            if (!isInitialized)
                InitializeComponents();
                
            logDataManager?.AddLog(message, LogLevel.Warning);
        }
        
        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="message">The error message to log</param>
        public void LogError(string message)
        {
            // Performance optimization: zero impact when disabled
            if (!isEnabled && !enableWhenDisabled)
                return;
                
            if (!isInitialized)
                InitializeComponents();
                
            logDataManager?.AddLog(message, LogLevel.Error);
        }
        
        /// <summary>
        /// Show the log display
        /// </summary>
        public void Show()
        {
            if (!isInitialized)
                InitializeComponents();
                
            logDisplay?.Show();
            
            // Update display with current logs when showing
            UpdateDisplayWithCurrentLogs();
        }
        
        /// <summary>
        /// Hide the log display
        /// </summary>
        public void Hide()
        {
            logDisplay?.Hide();
        }
        
        /// <summary>
        /// Clear all accumulated logs
        /// </summary>
        public void Clear()
        {
            if (!isInitialized)
                InitializeComponents();
                
            logDataManager?.ClearLogs();
        }
        
        /// <summary>
        /// Set the maximum number of log entries to store
        /// </summary>
        /// <param name="count">Maximum log count</param>
        public void SetMaxLogCount(int count)
        {
            if (!isInitialized)
                InitializeComponents();
                
            logDataManager?.SetMaxLogCount(count);
            
            // Update configuration if available
            if (configuration != null)
            {
                configuration.maxLogCount = count;
                configuration.ValidateSettings();
            }
        }
        
        /// <summary>
        /// Get the current maximum log count
        /// </summary>
        /// <returns>Maximum log count</returns>
        public int GetMaxLogCount()
        {
            if (!isInitialized)
                InitializeComponents();
                
            return logDataManager?.GetMaxLogCount() ?? 100;
        }
        
        /// <summary>
        /// Get the current number of stored log entries
        /// </summary>
        /// <returns>Current log count</returns>
        public int GetCurrentLogCount()
        {
            if (!isInitialized)
                InitializeComponents();
                
            return logDataManager?.GetLogCount() ?? 0;
        }
        
        /// <summary>
        /// Set the LogDisplay component
        /// </summary>
        /// <param name="display">LogDisplay component to use</param>
        public void SetLogDisplay(LogDisplay display)
        {
            logDisplay = display;
        }
        
        /// <summary>
        /// Get the current LogDisplay component
        /// </summary>
        /// <returns>Current LogDisplay component</returns>
        public LogDisplay GetLogDisplay()
        {
            return logDisplay;
        }
        
        /// <summary>
        /// Set the LogDataManager instance for testing and external configuration
        /// </summary>
        /// <param name="dataManager">LogDataManager instance to use</param>
        public void SetLogDataManager(LogDataManager dataManager)
        {
            if (logDataManager != null)
            {
                // Unsubscribe from old data manager events
                logDataManager.OnLogAdded -= OnLogAdded;
                logDataManager.OnLogsCleared -= OnLogDataManagerLogsCleared;
            }
            
            logDataManager = dataManager;
            
            if (logDataManager != null)
            {
                // Subscribe to new data manager events
                logDataManager.OnLogAdded += OnLogAdded;
                logDataManager.OnLogsCleared += OnLogDataManagerLogsCleared;
            }
        }
        
        /// <summary>
        /// Get the current LogDataManager instance
        /// </summary>
        /// <returns>Current LogDataManager instance</returns>
        public LogDataManager GetLogDataManager()
        {
            return logDataManager;
        }
        
        /// <summary>
        /// Set the LogConfiguration
        /// </summary>
        /// <param name="config">LogConfiguration to use</param>
        public void SetConfiguration(LogConfiguration config)
        {
            // Unsubscribe from old configuration
            if (configuration != null)
            {
                configuration.OnConfigurationChanged -= OnConfigurationChanged;
            }
            
            configuration = config;
            
            // Subscribe to new configuration changes
            if (configuration != null)
            {
                configuration.OnConfigurationChanged += OnConfigurationChanged;
            }
            
            if (isInitialized && logDataManager != null)
            {
                logDataManager.SetConfiguration(config);
                UpdateDisplayWithCurrentLogs();
            }
        }
        
        /// <summary>
        /// Get the current LogConfiguration
        /// </summary>
        /// <returns>Current LogConfiguration</returns>
        public LogConfiguration GetConfiguration()
        {
            return configuration;
        }
        
        /// <summary>
        /// Check if the logger is currently enabled
        /// </summary>
        /// <returns>True if enabled, false otherwise</returns>
        public bool IsEnabled()
        {
            return isEnabled;
        }
        
        /// <summary>
        /// Enable or disable the logger
        /// </summary>
        /// <param name="enabled">True to enable, false to disable</param>
        public void SetEnabled(bool enabled)
        {
            bool wasEnabled = isEnabled;
            isEnabled = enabled;
            
            if (wasEnabled != enabled)
            {
                OnLoggerStateChanged?.Invoke(enabled);
            }
        }
        
        /// <summary>
        /// Get performance statistics for monitoring
        /// </summary>
        /// <returns>Performance statistics</returns>
        public ScreenLoggerPerformanceStats GetPerformanceStats()
        {
            return new ScreenLoggerPerformanceStats
            {
                IsInitialized = isInitialized,
                IsEnabled = isEnabled,
                CurrentLogCount = GetCurrentLogCount(),
                MaxLogCount = GetMaxLogCount(),
                EstimatedMemoryUsage = logDataManager?.GetEstimatedMemoryUsage() ?? 0,
                DisplayStats = logDisplay?.GetPerformanceStats() ?? default
            };
        }
        
        /// <summary>
        /// Force immediate update of the display (for testing)
        /// </summary>
        public void ForceDisplayUpdate()
        {
            UpdateDisplayWithCurrentLogs();
            logDisplay?.ForceImmediateUpdate();
        }
        
        /// <summary>
        /// Check if the logger is properly initialized
        /// </summary>
        /// <returns>True if initialized, false otherwise</returns>
        public bool IsInitialized()
        {
            return isInitialized;
        }
        
        /// <summary>
        /// Manually initialize the logger (useful for testing)
        /// </summary>
        public void Initialize()
        {
            if (!isInitialized)
            {
                InitializeComponents();
            }
        }
        
        /// <summary>
        /// Event handler for when a log is added to LogDataManager
        /// </summary>
        /// <param name="logEntry">The log entry that was added</param>
        private void OnLogAdded(LogEntry logEntry)
        {
            // Update display only if logger is enabled or if enableWhenDisabled is true
            if (isEnabled || enableWhenDisabled)
            {
                // Use optimized single-entry update for real-time performance
                if (logDisplay != null && configuration != null)
                {
                    logDisplay.UpdateDisplayWithSingleEntry(logEntry, configuration);
                }
                else
                {
                    // Fallback to full update if components not ready
                    UpdateDisplayWithCurrentLogs();
                }
            }
        }
        
        /// <summary>
        /// Event handler for when logs are cleared from LogDataManager
        /// </summary>
        private void OnLogDataManagerLogsCleared()
        {
            logDisplay?.ClearDisplay();
            OnLogsCleared?.Invoke();
        }
        
        /// <summary>
        /// Update the display with current logs from LogDataManager
        /// </summary>
        private void UpdateDisplayWithCurrentLogs()
        {
            if (logDisplay == null || logDataManager == null)
                return;
                
            var logs = logDataManager.GetLogs();
            logDisplay.UpdateDisplayWithOptimizedRichText(logs, configuration);
        }
        
        /// <summary>
        /// Event handler for configuration changes - applies changes immediately
        /// </summary>
        /// <param name="config">Updated configuration</param>
        private void OnConfigurationChanged(LogConfiguration config)
        {
            if (!isInitialized)
                return;
                
            // Update LogDataManager with new configuration
            if (logDataManager != null)
            {
                logDataManager.SetConfiguration(config);
            }
            
            // Immediately update display with new settings
            UpdateDisplayWithCurrentLogs();
        }
        
        
        /// <summary>
        /// Apply configuration changes immediately
        /// </summary>
        public void ApplyConfigurationChanges()
        {
            configuration?.ApplyConfigurationChanges();
        }
        
        /// <summary>
        /// Update a specific color setting and apply immediately
        /// </summary>
        /// <param name="level">Log level to update color for</param>
        /// <param name="hexColor">New hex color code</param>
        public void UpdateLogLevelColor(LogLevel level, string hexColor)
        {
            if (configuration == null)
                return;
                
            string validatedColor = LogConfiguration.ValidateAndConvertHexColor(hexColor);
            
            switch (level)
            {
                case LogLevel.Info:
                    configuration.infoColorHex = validatedColor;
                    configuration.infoColor = LogConfiguration.HexToColor(validatedColor);
                    break;
                case LogLevel.Warning:
                    configuration.warningColorHex = validatedColor;
                    configuration.warningColor = LogConfiguration.HexToColor(validatedColor);
                    break;
                case LogLevel.Error:
                    configuration.errorColorHex = validatedColor;
                    configuration.errorColor = LogConfiguration.HexToColor(validatedColor);
                    break;
            }
            
            configuration.ApplyConfigurationChanges();
        }
        
        /// <summary>
        /// Update timestamp format and apply immediately
        /// </summary>
        /// <param name="format">New timestamp format</param>
        public void UpdateTimestampFormat(string format)
        {
            if (configuration == null)
                return;
                
            configuration.timestampFormat = string.IsNullOrEmpty(format) ? "HH:mm:ss" : format;
            configuration.ApplyConfigurationChanges();
        }
        
        /// <summary>
        /// Update max log count and apply immediately
        /// </summary>
        /// <param name="count">New maximum log count</param>
        public void UpdateMaxLogCount(int count)
        {
            if (configuration == null)
                return;
                
            configuration.maxLogCount = Mathf.Clamp(count, 1, 1000);
            configuration.ApplyConfigurationChanges();
        }
        
        /// <summary>
        /// Reset configuration to defaults and apply immediately
        /// </summary>
        public void ResetConfigurationToDefaults()
        {
            if (configuration == null)
                return;
                
            configuration.ResetToDefaults();
            configuration.ApplyConfigurationChanges();
        }
        
        /// <summary>
        /// Validate cross-platform compatibility
        /// </summary>
        public SystemValidationResult ValidateCrossPlatformCompatibility()
        {
            return CrossPlatformLoggerValidator.ValidateCompleteSystem(this);
        }
        
        /// <summary>
        /// Get platform compatibility report
        /// </summary>
        public PlatformCompatibilityReport GetPlatformCompatibilityReport()
        {
            return PlatformCompatibility.GenerateCompatibilityReport();
        }
        
        /// <summary>
        /// Apply platform-specific performance optimizations
        /// UI properties are now configured in Unity Editor
        /// </summary>
        public void ApplyPlatformOptimizations()
        {
            // Get platform-optimized configuration (performance settings only)
            var platformConfig = PlatformCompatibility.GetPlatformOptimizedConfiguration();
            
            // Set configuration for this ScreenLogger (ensures data manager gets updated)
            SetConfiguration(platformConfig);
        }
    }
    
    /// <summary>
    /// Performance statistics for ScreenLogger monitoring
    /// </summary>
    public struct ScreenLoggerPerformanceStats
    {
        public bool IsInitialized;
        public bool IsEnabled;
        public int CurrentLogCount;
        public int MaxLogCount;
        public long EstimatedMemoryUsage;
        public LogDisplayPerformanceStats DisplayStats;
    }
}