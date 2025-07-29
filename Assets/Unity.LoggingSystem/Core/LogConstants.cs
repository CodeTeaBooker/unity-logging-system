using UnityEngine;

namespace RuntimeLogging
{
    /// <summary>
    /// Centralized constants for the logging system
    /// Eliminates code duplication across multiple files
    /// </summary>
    public static class LogConstants
    {
        /// <summary>
        /// Default color codes for different log levels
        /// </summary>
        public static class Colors
        {
            public const string INFO = "#FFFFFF";
            public const string WARNING = "#FFFF00";
            public const string ERROR = "#FF0000";
            
            // Default fallback color
            public const string DEFAULT = "#FFFFFF";
        }
        
        /// <summary>
        /// Timestamp and formatting constants
        /// </summary>
        public static class Formats
        {
            public const string DEFAULT_TIMESTAMP = "HH:mm:ss";
            public const string MOBILE_TIMESTAMP = "mm:ss";
            public const string LOG_ENTRY_FORMAT = "[{0}][{1}] {2}";
        }
        
        /// <summary>
        /// Default configuration values
        /// </summary>
        public static class Defaults
        {
            public const int MAX_LOG_COUNT = 100;
            public const float PANEL_ALPHA = 0.8f;
            public const bool AUTO_SCROLL = true;
            public const int MEMORY_THRESHOLD_MB = 50;
        }
        
        /// <summary>
        /// Performance and limits
        /// </summary>
        public static class Limits
        {
            public const int MIN_LOG_COUNT = 1;
            public const int MAX_LOG_COUNT = 1000;
            public const int MAX_MESSAGE_LENGTH = 5000;
            public const int POOL_INITIAL_SIZE = 50;
            public const int POOL_MAX_SIZE = 200;
        }
        
        /// <summary>
        /// Platform-specific constants
        /// </summary>
        public static class Platform
        {
            public const int MOBILE_MAX_LOG_COUNT = 50;
            public const int DESKTOP_MAX_LOG_COUNT = 200;
            public const string MOBILE_TIMESTAMP_FORMAT = "HH:mm";
            public const string DESKTOP_TIMESTAMP_FORMAT = "HH:mm:ss";
        }
    }
}