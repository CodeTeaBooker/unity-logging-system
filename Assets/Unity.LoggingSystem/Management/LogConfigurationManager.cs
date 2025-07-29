using UnityEngine;
using System;
using System.Collections.Generic;

namespace RuntimeLogging
{
    /// <summary>
    /// Global configuration manager for the logging system
    /// Handles configuration persistence, validation, and runtime changes
    /// </summary>
    public static class LogConfigurationManager
    {
        private static LogConfiguration _globalConfiguration;
        private static readonly List<ILogConfigurationListener> _listeners = new List<ILogConfigurationListener>();
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Event fired when global configuration changes
        /// </summary>
        public static event Action<LogConfiguration> OnGlobalConfigurationChanged;
        
        /// <summary>
        /// Gets or creates the global configuration instance
        /// </summary>
        public static LogConfiguration GlobalConfiguration
        {
            get
            {
                lock (_lock)
                {
                    if (_globalConfiguration == null)
                    {
                        _globalConfiguration = LogConfiguration.CreateDefault();
                        _globalConfiguration.OnConfigurationChanged += OnConfigurationChanged;
                    }
                    return _globalConfiguration;
                }
            }
        }
        
        /// <summary>
        /// Sets the global configuration
        /// </summary>
        /// <param name="configuration">New global configuration</param>
        public static void SetGlobalConfiguration(LogConfiguration configuration)
        {
            lock (_lock)
            {
                if (_globalConfiguration != null)
                {
                    _globalConfiguration.OnConfigurationChanged -= OnConfigurationChanged;
                }
                
                _globalConfiguration = configuration;
                
                if (_globalConfiguration != null)
                {
                    _globalConfiguration.OnConfigurationChanged += OnConfigurationChanged;
                }
            }
            
            NotifyConfigurationChanged();
        }
        
        /// <summary>
        /// Registers a listener for configuration changes
        /// </summary>
        /// <param name="listener">Listener to register</param>
        public static void RegisterListener(ILogConfigurationListener listener)
        {
            if (listener != null)
            {
                lock (_lock)
                {
                    if (!_listeners.Contains(listener))
                    {
                        _listeners.Add(listener);
                    }
                }
            }
        }
        
        /// <summary>
        /// Unregisters a listener for configuration changes
        /// </summary>
        /// <param name="listener">Listener to unregister</param>
        public static void UnregisterListener(ILogConfigurationListener listener)
        {
            if (listener != null)
            {
                lock (_lock)
                {
                    _listeners.Remove(listener);
                }
            }
        }
        
        
        /// <summary>
        /// Applies global configuration changes immediately
        /// </summary>
        public static void ApplyGlobalConfigurationChanges()
        {
            GlobalConfiguration.ApplyConfigurationChanges();
        }
        
        /// <summary>
        /// Updates a log level color globally
        /// </summary>
        /// <param name="level">Log level to update</param>
        /// <param name="hexColor">New hex color code</param>
        public static void UpdateGlobalLogLevelColor(LogLevel level, string hexColor)
        {
            var config = GlobalConfiguration;
            string validatedColor = LogConfiguration.ValidateAndConvertHexColor(hexColor);
            
            switch (level)
            {
                case LogLevel.Info:
                    config.infoColorHex = validatedColor;
                    config.infoColor = LogConfiguration.HexToColor(validatedColor);
                    break;
                case LogLevel.Warning:
                    config.warningColorHex = validatedColor;
                    config.warningColor = LogConfiguration.HexToColor(validatedColor);
                    break;
                case LogLevel.Error:
                    config.errorColorHex = validatedColor;
                    config.errorColor = LogConfiguration.HexToColor(validatedColor);
                    break;
            }
            
            config.ApplyConfigurationChanges();
        }
        
        /// <summary>
        /// Updates the global timestamp format
        /// </summary>
        /// <param name="format">New timestamp format</param>
        public static void UpdateGlobalTimestampFormat(string format)
        {
            var config = GlobalConfiguration;
            config.timestampFormat = string.IsNullOrEmpty(format) ? LogConstants.Formats.DEFAULT_TIMESTAMP : format;
            config.ApplyConfigurationChanges();
        }
        
        /// <summary>
        /// Updates the global max log count
        /// </summary>
        /// <param name="count">New maximum log count</param>
        public static void UpdateGlobalMaxLogCount(int count)
        {
            var config = GlobalConfiguration;
            config.maxLogCount = Mathf.Clamp(count, LogConstants.Limits.MIN_LOG_COUNT, LogConstants.Limits.MAX_LOG_COUNT);
            config.ApplyConfigurationChanges();
        }
        
        /// <summary>
        /// Resets global configuration to defaults
        /// </summary>
        public static void ResetGlobalConfigurationToDefaults()
        {
            var config = GlobalConfiguration;
            config.ResetToDefaults();
            config.ApplyConfigurationChanges();
        }
        
        // Removed redundant wrapper methods - use LogConfiguration static methods directly
        
        /// <summary>
        /// Creates a configuration preset for common scenarios
        /// </summary>
        /// <param name="preset">Preset type to create</param>
        /// <returns>Configuration with preset values</returns>
        public static LogConfiguration CreateConfigurationPreset(LogConfigurationPreset preset)
        {
            var config = LogConfiguration.CreateDefault();
            
            switch (preset)
            {
                case LogConfigurationPreset.Development:
                    config.maxLogCount = 200;
                    config.timestampFormat = "HH:mm:ss.fff";
                    config.infoColorHex = "#CCCCCC";
                    config.warningColorHex = "#FFAA00";
                    config.errorColorHex = "#FF4444";
                    break;
                    
                case LogConfigurationPreset.Production:
                    config.maxLogCount = 50;
                    config.timestampFormat = "HH:mm:ss";
                    config.infoColorHex = "#FFFFFF";
                    config.warningColorHex = "#FFFF00";
                    config.errorColorHex = "#FF0000";
                    break;
                    
                case LogConfigurationPreset.Testing:
                    config.maxLogCount = 500;
                    config.timestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
                    config.infoColorHex = "#00FF00";
                    config.warningColorHex = "#FFA500";
                    config.errorColorHex = "#FF0000";
                    break;
                    
                case LogConfigurationPreset.HighContrast:
                    config.maxLogCount = 100;
                    config.timestampFormat = "HH:mm:ss";
                    config.infoColorHex = "#FFFFFF";
                    config.warningColorHex = "#FFFF00";
                    config.errorColorHex = "#FF0000";
                    config.panelAlpha = 1.0f;
                    break;
            }
            
            config.ValidateSettings();
            return config;
        }
        
        /// <summary>
        /// Event handler for configuration changes
        /// </summary>
        /// <param name="config">Updated configuration</param>
        private static void OnConfigurationChanged(LogConfiguration config)
        {
            NotifyConfigurationChanged();
        }
        
        /// <summary>
        /// Notifies all listeners of configuration changes
        /// </summary>
        private static void NotifyConfigurationChanged()
        {
            LogConfiguration currentConfig;
            List<ILogConfigurationListener> currentListeners;
            
            // Create snapshots under lock to avoid holding lock during event notifications
            lock (_lock)
            {
                currentConfig = _globalConfiguration;
                currentListeners = new List<ILogConfigurationListener>(_listeners);
            }
            
            // Fire events outside of lock to prevent deadlocks
            OnGlobalConfigurationChanged?.Invoke(currentConfig);
            
            // Notify registered listeners with error handling
            var listenersToRemove = new List<ILogConfigurationListener>();
            foreach (var listener in currentListeners)
            {
                if (listener != null)
                {
                    try
                    {
                        listener.OnConfigurationChanged(currentConfig);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"LogConfigurationManager: Error notifying listener {listener.GetType().Name}: {ex.Message}");
                    }
                }
                else
                {
                    listenersToRemove.Add(listener);
                }
            }
            
            // Remove null listeners
            if (listenersToRemove.Count > 0)
            {
                lock (_lock)
                {
                    foreach (var nullListener in listenersToRemove)
                    {
                        _listeners.Remove(nullListener);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Interface for objects that need to be notified of configuration changes
    /// </summary>
    public interface ILogConfigurationListener
    {
        void OnConfigurationChanged(LogConfiguration configuration);
    }
    
    /// <summary>
    /// Predefined configuration presets for common scenarios
    /// </summary>
    public enum LogConfigurationPreset
    {
        Development,
        Production,
        Testing,
        HighContrast
    }
}