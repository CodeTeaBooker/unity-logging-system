using System;
using UnityEngine;

namespace RuntimeLogging
{
    /// <summary>
    /// Represents a single log entry with timestamp, level, message, and formatting methods
    /// </summary>
    [System.Serializable]
    public class LogEntry
    {
        public string message;
        public LogLevel level;
        public DateTime timestamp;
        public string stackTrace;
        
        /// <summary>
        /// Default constructor for object pooling
        /// </summary>
        public LogEntry()
        {
            this.message = string.Empty;
            this.level = LogLevel.Info;
            this.timestamp = DateTime.MinValue;
            this.stackTrace = string.Empty;
        }
        
        /// <summary>
        /// Constructor for creating a log entry
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The log level</param>
        /// <param name="timestamp">The timestamp when the log was created</param>
        /// <param name="stackTrace">Optional stack trace information</param>
        public LogEntry(string message, LogLevel level, DateTime timestamp, string stackTrace = null)
        {
            this.message = SanitizeLogMessage(message);
            this.level = level;
            this.timestamp = timestamp;
            this.stackTrace = SanitizeLogMessage(stackTrace);
        }
        
        /// <summary>
        /// Initializes or reinitializes this log entry with new values (for pooling)
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The log level</param>
        /// <param name="timestamp">The timestamp when the log was created</param>
        /// <param name="stackTrace">Optional stack trace information</param>
        public void Initialize(string message, LogLevel level, DateTime timestamp, string stackTrace = null)
        {
            this.message = SanitizeLogMessage(message);
            this.level = level;
            this.timestamp = timestamp;
            this.stackTrace = SanitizeLogMessage(stackTrace);
        }
        
        /// <summary>
        /// Gets the formatted message with timestamp and level
        /// Format: [HH:mm:ss][Level] Message
        /// </summary>
        public string FormattedMessage => $"[{timestamp:HH:mm:ss}][{level}] {message}";
        
        /// <summary>
        /// Gets the color associated with the log level
        /// </summary>
        /// <param name="config">Configuration containing color settings</param>
        /// <returns>Color for the log level</returns>
        public Color GetLevelColor(LogConfiguration config)
        {
            if (config == null)
            {
                return GetDefaultLevelColor();
            }
            
            return level switch
            {
                LogLevel.Info => config.infoColor,
                LogLevel.Warning => config.warningColor,
                LogLevel.Error => config.errorColor,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// Gets the default color for the log level when no configuration is available
        /// </summary>
        /// <returns>Default color for the log level</returns>
        public Color GetDefaultLevelColor()
        {
            return level switch
            {
                LogLevel.Info => Color.white,
                LogLevel.Warning => Color.yellow,
                LogLevel.Error => Color.red,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// Gets a formatted message with custom timestamp format
        /// </summary>
        /// <param name="timestampFormat">Custom timestamp format</param>
        /// <returns>Formatted message with custom timestamp</returns>
        public string GetFormattedMessage(string timestampFormat)
        {
            if (string.IsNullOrEmpty(timestampFormat))
                timestampFormat = "HH:mm:ss";
                
            return $"[{timestamp.ToString(timestampFormat)}][{level}] {message}";
        }
        
        /// <summary>
        /// Gets a formatted message with TextMeshPro rich text markup for colors
        /// </summary>
        /// <param name="config">Configuration containing color settings</param>
        /// <returns>Rich text formatted message with color markup</returns>
        public string GetRichTextMessage(LogConfiguration config)
        {
            Color color = GetLevelColor(config);
            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            string timestampFormat = config?.timestampFormat ?? "HH:mm:ss";
            
            return $"<color=#{colorHex}>[{timestamp.ToString(timestampFormat)}][{level}] {message}</color>";
        }
        
        /// <summary>
        /// Gets a formatted message with TextMeshPro rich text markup using hex color codes
        /// </summary>
        /// <param name="infoColorHex">Hex color code for info messages</param>
        /// <param name="warningColorHex">Hex color code for warning messages</param>
        /// <param name="errorColorHex">Hex color code for error messages</param>
        /// <param name="timestampFormat">Timestamp format string (defaults to "HH:mm:ss")</param>
        /// <returns>Rich text formatted message with color markup</returns>
        public string GetRichTextMessage(string infoColorHex, string warningColorHex, string errorColorHex, string timestampFormat = "HH:mm:ss")
        {
            string colorHex = level switch
            {
                LogLevel.Info => infoColorHex ?? LogConstants.Colors.INFO,
                LogLevel.Warning => warningColorHex ?? LogConstants.Colors.WARNING,
                LogLevel.Error => errorColorHex ?? LogConstants.Colors.ERROR,
                _ => LogConstants.Colors.DEFAULT
            };
            
            // Remove # if present to ensure consistent format
            if (colorHex.StartsWith("#"))
                colorHex = colorHex.Substring(1);
                
            if (string.IsNullOrEmpty(timestampFormat))
                timestampFormat = "HH:mm:ss";
                
            return $"<color=#{colorHex}>[{timestamp.ToString(timestampFormat)}][{level}] {message}</color>";
        }
        
        /// <summary>
        /// Sanitizes log message content to prevent potential injection attacks
        /// </summary>
        /// <param name="input">Input string to sanitize</param>
        /// <returns>Sanitized string safe for display</returns>
        private static string SanitizeLogMessage(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // Remove potential script injection characters
            string sanitized = input
                .Replace("<script", "&lt;script")
                .Replace("</script>", "&lt;/script&gt;")
                .Replace("javascript:", "javascript_")
                .Replace("vbscript:", "vbscript_")
                .Replace("data:", "data_")
                .Replace("onclick", "on_click")
                .Replace("onerror", "on_error")
                .Replace("onload", "on_load");
            
            // Limit message length to prevent buffer overflow-like issues
            if (sanitized.Length > 10000)
            {
                sanitized = sanitized.Substring(0, 10000) + "...[truncated]";
            }
            
            return sanitized;
        }
    }
}