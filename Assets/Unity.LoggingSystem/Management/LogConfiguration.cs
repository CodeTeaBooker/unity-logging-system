using UnityEngine;
using System;

namespace RuntimeLogging
{
    /// <summary>
    /// ScriptableObject configuration for the logging panel with default settings and persistence
    /// </summary>
    [CreateAssetMenu(fileName = "LogPanelConfig", menuName = "Logging/Panel Configuration")]
    public class LogConfiguration : ScriptableObject
    {
        [Header("Display Settings")]
        [Tooltip("Maximum number of log entries to display (1-1000, default: 100)")]
        public int maxLogCount = LogConstants.Defaults.MAX_LOG_COUNT;
        
        [Tooltip("Automatically scroll to show newest log entries (default: true)")]
        public bool autoScroll = true;
        
        [Tooltip("Timestamp format for log entries (default: HH:mm:ss)")]
        public string timestampFormat = LogConstants.Formats.DEFAULT_TIMESTAMP;
        
        [Header("Color Settings")]
        [Tooltip("Color for Info level messages")]
        public Color infoColor = Color.white;
        
        [Tooltip("Color for Warning level messages")]
        public Color warningColor = Color.yellow;
        
        [Tooltip("Color for Error level messages")]
        public Color errorColor = Color.red;
        
        [Header("Hex Color Codes")]
        [Tooltip("Hex color code for Info level messages (e.g., #FFFFFF)")]
        public string infoColorHex = LogConstants.Colors.INFO;
        
        [Tooltip("Hex color code for Warning level messages (e.g., #FFFF00)")]
        public string warningColorHex = LogConstants.Colors.WARNING;
        
        [Tooltip("Hex color code for Error level messages (e.g., #FF0000)")]
        public string errorColorHex = LogConstants.Colors.ERROR;
        
        [Header("UI Settings")]
        [Tooltip("Panel background transparency (0-1)")]
        [Range(0f, 1f)]
        public float panelAlpha = LogConstants.Defaults.PANEL_ALPHA;
        
        
        
        /// <summary>
        /// Event fired when configuration changes are applied
        /// </summary>
        public event Action<LogConfiguration> OnConfigurationChanged;
        
        /// <summary>
        /// Validates configuration values and applies sensible bounds
        /// </summary>
        public void ValidateSettings()
        {
            // Ensure max log count is within reasonable bounds
            maxLogCount = Mathf.Clamp(maxLogCount, LogConstants.Limits.MIN_LOG_COUNT, LogConstants.Limits.MAX_LOG_COUNT);
            
            // Validate and fix timestamp format
            ValidateTimestampFormat();
            
            // Ensure panel alpha is within valid range
            panelAlpha = Mathf.Clamp01(panelAlpha);
            
            
            // Validate hex color codes
            ValidateHexColor(ref infoColorHex, LogConstants.Colors.INFO);
            ValidateHexColor(ref warningColorHex, LogConstants.Colors.WARNING);
            ValidateHexColor(ref errorColorHex, LogConstants.Colors.ERROR);
            
            // Sync Color fields with validated hex codes
            infoColor = HexToColor(infoColorHex, Color.white);
            warningColor = HexToColor(warningColorHex, Color.yellow);
            errorColor = HexToColor(errorColorHex, Color.red);
        }
        
        /// <summary>
        /// Validates timestamp format and fixes common issues
        /// </summary>
        private void ValidateTimestampFormat()
        {
            if (string.IsNullOrEmpty(timestampFormat))
            {
                timestampFormat = LogConstants.Formats.DEFAULT_TIMESTAMP;
                return;
            }
            
            // Try to format current time with the format string
            try
            {
                string testFormat = DateTime.Now.ToString(timestampFormat);
                
                // Check for reasonable length (avoid extremely long formats)
                if (testFormat.Length > 50)
                {
                    Debug.LogWarning($"LogConfiguration: Timestamp format produces very long output ({testFormat.Length} chars). Consider using shorter format.");
                }
            }
            catch (System.FormatException)
            {
                Debug.LogWarning($"LogConfiguration: Invalid timestamp format '{timestampFormat}', reverting to default '{LogConstants.Formats.DEFAULT_TIMESTAMP}'");
                timestampFormat = LogConstants.Formats.DEFAULT_TIMESTAMP;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"LogConfiguration: Unexpected error validating timestamp format '{timestampFormat}': {ex.Message}");
                timestampFormat = LogConstants.Formats.DEFAULT_TIMESTAMP;
            }
        }
        
        
        /// <summary>
        /// Validates and corrects a hex color code
        /// </summary>
        /// <param name="hexColor">Reference to the hex color string to validate</param>
        /// <param name="defaultColor">Default color to use if validation fails</param>
        private void ValidateHexColor(ref string hexColor, string defaultColor)
        {
            hexColor = ValidateAndConvertHexColor(hexColor, defaultColor);
        }
        
        /// <summary>
        /// Resets all settings to their default values
        /// </summary>
        public void ResetToDefaults()
        {
            maxLogCount = LogConstants.Defaults.MAX_LOG_COUNT;
            autoScroll = LogConstants.Defaults.AUTO_SCROLL;
            timestampFormat = LogConstants.Formats.DEFAULT_TIMESTAMP;
            infoColor = Color.white;
            warningColor = Color.yellow;
            errorColor = Color.red;
            infoColorHex = LogConstants.Colors.INFO;
            warningColorHex = LogConstants.Colors.WARNING;
            errorColorHex = LogConstants.Colors.ERROR;
            panelAlpha = LogConstants.Defaults.PANEL_ALPHA;
        }
        
        /// <summary>
        /// Gets the hex color code for info level messages
        /// </summary>
        /// <returns>Hex color code string</returns>
        public string GetInfoColorHex() => infoColorHex;
        
        /// <summary>
        /// Gets the hex color code for warning level messages
        /// </summary>
        /// <returns>Hex color code string</returns>
        public string GetWarningColorHex() => warningColorHex;
        
        /// <summary>
        /// Gets the hex color code for error level messages
        /// </summary>
        /// <returns>Hex color code string</returns>
        public string GetErrorColorHex() => errorColorHex;
        
        /// <summary>
        /// Applies configuration changes immediately and notifies listeners
        /// </summary>
        public void ApplyConfigurationChanges()
        {
            ValidateSettings();
            OnConfigurationChanged?.Invoke(this);
        }
        
        
        
        
        /// <summary>
        /// Updates Color fields from hex color codes
        /// </summary>
        private void UpdateColorsFromHex()
        {
            if (ColorUtility.TryParseHtmlString(infoColorHex, out Color infoCol))
                infoColor = infoCol;
                
            if (ColorUtility.TryParseHtmlString(warningColorHex, out Color warningCol))
                warningColor = warningCol;
                
            if (ColorUtility.TryParseHtmlString(errorColorHex, out Color errorCol))
                errorColor = errorCol;
        }
        
        /// <summary>
        /// Creates a default configuration instance by loading from Resources or creating default values
        /// </summary>
        /// <returns>LogConfiguration instance loaded from Resources or with default values</returns>
        public static LogConfiguration CreateDefault()
        {
            Debug.Log($"LogConfiguration: CreateDefault called on Platform: {Application.platform}");
            
            try
            {
                // Try to load configuration from Resources folder
                Debug.Log("LogConfiguration: Attempting Resources.Load<LogConfiguration>(\"LogPanelConfig\")...");
                var resourceConfig = Resources.Load<LogConfiguration>("LogPanelConfig");
                
                if (resourceConfig != null)
                {
                    Debug.Log($"LogConfiguration: Successfully loaded configuration from Resources/LogPanelConfig.asset (Platform: {Application.platform})");
                    
                    // Create a copy to avoid modifying the original asset
                    var configCopy = Instantiate(resourceConfig);
                    
                    // Validate the copied configuration with error handling
                    try
                    {
                        configCopy.ValidateSettings();
                        return configCopy;
                    }
                    catch (System.Exception validationEx)
                    {
                        Debug.LogError($"LogConfiguration: Validation failed for loaded configuration: {validationEx.Message}");
                        // Clean up the failed copy and continue to fallback creation
                        DestroyImmediate(configCopy);
                    }
                }
                else
                {
                    // Diagnostic information when resource loading fails
                    Debug.LogError($"LogConfiguration: Resources.Load<LogConfiguration>(\"LogPanelConfig\") returned NULL (Platform: {Application.platform})");
                    
                    try
                    {
                        LogResourcesLoadingDiagnostics();
                    }
                    catch (System.Exception diagnosticEx)
                    {
                        Debug.LogError($"LogConfiguration: Error during diagnostic logging: {diagnosticEx.Message}");
                    }
                }
            }
            catch (UnityEngine.UnityException unityEx)
            {
                Debug.LogError($"LogConfiguration: Unity-specific exception during Resources.Load: {unityEx.Message} (Platform: {Application.platform})");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"LogConfiguration: Unexpected exception during Resources.Load: {ex.GetType().Name}: {ex.Message} (Platform: {Application.platform})");
            }
            
            // Fallback: Create instance with default values
            Debug.LogWarning("LogConfiguration: Using fallback configuration with default values");
            try
            {
                var config = CreateInstance<LogConfiguration>();
                config.ResetToDefaults();
                config.ValidateSettings(); // Ensure defaults are valid
                return config;
            }
            catch (System.Exception fallbackEx)
            {
                Debug.LogError($"LogConfiguration: Critical error creating fallback configuration: {fallbackEx.Message}");
                // Last resort: return a basic configuration without ScriptableObject
                return CreateBasicFallbackConfiguration();
            }
        }
        
        /// <summary>
        /// Creates a basic fallback configuration when all else fails
        /// </summary>
        /// <returns>Basic LogConfiguration with hardcoded defaults</returns>
        private static LogConfiguration CreateBasicFallbackConfiguration()
        {
            var config = new LogConfiguration();
            config.maxLogCount = LogConstants.Defaults.MAX_LOG_COUNT;
            config.autoScroll = LogConstants.Defaults.AUTO_SCROLL;
            config.timestampFormat = LogConstants.Formats.DEFAULT_TIMESTAMP;
            config.infoColor = Color.white;
            config.warningColor = Color.yellow;
            config.errorColor = Color.red;
            config.infoColorHex = LogConstants.Colors.INFO;
            config.warningColorHex = LogConstants.Colors.WARNING;
            config.errorColorHex = LogConstants.Colors.ERROR;
            config.panelAlpha = LogConstants.Defaults.PANEL_ALPHA;
            
            Debug.LogWarning("LogConfiguration: Using basic fallback configuration (not a ScriptableObject)");
            return config;
        }
        
        /// <summary>
        /// Logs diagnostic information about Resources loading
        /// </summary>
        private static void LogResourcesLoadingDiagnostics()
        {
            // Check if Resources folder exists in build
            var allLogConfigs = Resources.LoadAll<LogConfiguration>("");
            Debug.Log($"LogConfiguration: Found {allLogConfigs.Length} LogConfiguration assets in Resources");
            
            // List all found configurations
            for (int i = 0; i < allLogConfigs.Length; i++)
            {
                Debug.Log($"LogConfiguration: Found asset [{i}]: {allLogConfigs[i].name}");
            }
            
            // Additional platform-specific diagnostics
            Debug.Log($"LogConfiguration: Application.platform = {Application.platform}");
            Debug.Log($"LogConfiguration: Application.isEditor = {Application.isEditor}");
            Debug.Log($"LogConfiguration: Resources loading diagnostics complete");
        }
        
        
        /// <summary>
        /// Validates and converts a hex color code with enhanced validation
        /// </summary>
        /// <param name="hexColor">Hex color code to validate</param>
        /// <param name="defaultColor">Default color to use if validation fails</param>
        /// <returns>Valid hex color code</returns>
        public static string ValidateAndConvertHexColor(string hexColor, string defaultColor = LogConstants.Colors.DEFAULT)
        {
            if (string.IsNullOrEmpty(hexColor))
                return defaultColor;
            
            // Ensure hex color starts with #
            if (!hexColor.StartsWith("#"))
                hexColor = "#" + hexColor;
            
            // Validate hex color format and convert using Unity's ColorUtility
            if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
            {
                // Convert back to ensure consistent format
                string validHex = "#" + ColorUtility.ToHtmlStringRGB(color);
                return validHex;
            }
            
            return defaultColor;
        }
        
        /// <summary>
        /// Converts a Unity Color to hex color code
        /// </summary>
        /// <param name="color">Unity Color to convert</param>
        /// <returns>Hex color code string</returns>
        public static string ColorToHex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }
        
        /// <summary>
        /// Converts a hex color code to Unity Color
        /// </summary>
        /// <param name="hexColor">Hex color code to convert</param>
        /// <param name="defaultColor">Default color if conversion fails</param>
        /// <returns>Unity Color</returns>
        public static Color HexToColor(string hexColor, Color defaultColor = default)
        {
            if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
                return color;
            
            return defaultColor == default ? Color.white : defaultColor;
        }
        
        /// <summary>
        /// Called when the ScriptableObject is loaded or values change in the inspector
        /// </summary>
        private void OnValidate()
        {
            ValidateSettings();
            
            // Sync hex codes with Color fields only if colors actually changed
            string newInfoHex = "#" + ColorUtility.ToHtmlStringRGB(infoColor);
            string newWarningHex = "#" + ColorUtility.ToHtmlStringRGB(warningColor);
            string newErrorHex = "#" + ColorUtility.ToHtmlStringRGB(errorColor);
            
            bool colorsChanged = false;
            if (infoColorHex != newInfoHex)
            {
                infoColorHex = newInfoHex;
                colorsChanged = true;
            }
            if (warningColorHex != newWarningHex)
            {
                warningColorHex = newWarningHex;
                colorsChanged = true;
            }
            if (errorColorHex != newErrorHex)
            {
                errorColorHex = newErrorHex;
                colorsChanged = true;
            }
            
            // Apply changes immediately if in play mode and colors actually changed
            if (Application.isPlaying && colorsChanged)
            {
                ApplyConfigurationChanges();
            }
        }
        
    }
    
}