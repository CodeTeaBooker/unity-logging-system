using UnityEngine;
using System;
using System.Collections.Generic;

namespace RuntimeLogging
{
    /// <summary>
    /// Platform compatibility utilities for cross-platform logging system
    /// Provides Unity version checks, platform detection, and TextMeshPro compatibility validation
    /// </summary>
    public static class PlatformCompatibility
    {
        /// <summary>
        /// Minimum supported Unity version (2019.4.0)
        /// </summary>
        public static readonly Version MinimumUnityVersion = new Version(2019, 4, 0);
        
        /// <summary>
        /// Current Unity version
        /// </summary>
        public static Version CurrentUnityVersion => GetUnityVersion();
        
        /// <summary>
        /// Check if current Unity version is supported
        /// </summary>
        public static bool IsUnitySupportedVersion => CurrentUnityVersion >= MinimumUnityVersion;
        
        /// <summary>
        /// Check if TextMeshPro is available in the current Unity version
        /// </summary>
        public static bool IsTextMeshProSupported => IsUnitySupportedVersion && HasTextMeshProPackage();
        
        /// <summary>
        /// Current runtime platform
        /// </summary>
        public static RuntimePlatform CurrentPlatform => Application.platform;
        
        /// <summary>
        /// Check if running on a supported platform
        /// </summary>
        public static bool IsSupportedPlatform => GetSupportedPlatforms().Contains(CurrentPlatform);
        
        /// <summary>
        /// Check if running on a mobile platform
        /// </summary>
        public static bool IsMobilePlatform => CurrentPlatform == RuntimePlatform.Android || 
                                               CurrentPlatform == RuntimePlatform.IPhonePlayer;
        
        /// <summary>
        /// Check if running on a desktop platform
        /// </summary>
        public static bool IsDesktopPlatform => CurrentPlatform == RuntimePlatform.WindowsPlayer ||
                                                CurrentPlatform == RuntimePlatform.WindowsEditor ||
                                                CurrentPlatform == RuntimePlatform.OSXPlayer ||
                                                CurrentPlatform == RuntimePlatform.OSXEditor ||
                                                CurrentPlatform == RuntimePlatform.LinuxPlayer ||
                                                CurrentPlatform == RuntimePlatform.LinuxEditor ||
                                                // Add potential Windows standalone variants
                                                CurrentPlatform.ToString().Contains("Windows");
        
        /// <summary>
        /// Check if running in Unity Editor
        /// </summary>
        public static bool IsEditor => Application.isEditor;
        
        /// <summary>
        /// Get platform-specific performance recommendations
        /// </summary>
        public static PlatformPerformanceSettings GetPlatformPerformanceSettings()
        {
            if (IsMobilePlatform)
            {
                return new PlatformPerformanceSettings
                {
                    MaxLogCount = 50,
                    UpdateThrottleTime = 0.2f,
                    MaxCharacterLimit = 4000,
                    BatchUpdatesEnabled = true,
                    BatchUpdateInterval = 0.1f,
                    MaxLinesLimit = 50
                };
            }
            else if (IsDesktopPlatform)
            {
                return new PlatformPerformanceSettings
                {
                    MaxLogCount = 100,
                    UpdateThrottleTime = 0.1f,
                    MaxCharacterLimit = 8000,
                    BatchUpdatesEnabled = true,
                    BatchUpdateInterval = 0.05f,
                    MaxLinesLimit = 100
                };
            }
            else
            {
                // Default/fallback settings
                return new PlatformPerformanceSettings
                {
                    MaxLogCount = 75,
                    UpdateThrottleTime = 0.15f,
                    MaxCharacterLimit = 6000,
                    BatchUpdatesEnabled = true,
                    BatchUpdateInterval = 0.075f,
                    MaxLinesLimit = 75
                };
            }
        }
        
        
        
        
        /// <summary>
        /// Get Unity version from Application.unityVersion
        /// </summary>
        private static Version GetUnityVersion()
        {
            try
            {
                string versionString = Application.unityVersion;
                
                if (string.IsNullOrEmpty(versionString))
                {
                    Debug.LogWarning($"PlatformCompatibility: Unity version string is null or empty, using minimum version {MinimumUnityVersion}");
                    return MinimumUnityVersion;
                }
                
                // Parse version string (e.g., "2021.3.15f1" -> "2021.3.15")
                var parts = versionString.Split('.');
                if (parts.Length >= 2) // At least major.minor is required
                {
                    int major = 0, minor = 0, patch = 0;
                    
                    // Parse major version
                    if (!int.TryParse(parts[0], out major) || major < 0)
                    {
                        Debug.LogWarning($"PlatformCompatibility: Invalid major version in '{versionString}', using minimum version");
                        return MinimumUnityVersion;
                    }
                    
                    // Parse minor version
                    if (!int.TryParse(parts[1], out minor) || minor < 0)
                    {
                        Debug.LogWarning($"PlatformCompatibility: Invalid minor version in '{versionString}', using minimum version");
                        return MinimumUnityVersion;
                    }
                    
                    // Parse patch version (optional, defaults to 0)
                    if (parts.Length >= 3)
                    {
                        string patchVersion = parts[2];
                        // Remove any suffix from the patch version (e.g., "15f1" -> "15")
                        int suffixIndex = -1;
                        for (int i = 0; i < patchVersion.Length; i++)
                        {
                            if (!char.IsDigit(patchVersion[i]))
                            {
                                suffixIndex = i;
                                break;
                            }
                        }
                        
                        if (suffixIndex > 0)
                        {
                            patchVersion = patchVersion.Substring(0, suffixIndex);
                        }
                        
                        if (!int.TryParse(patchVersion, out patch) || patch < 0)
                        {
                            Debug.LogWarning($"PlatformCompatibility: Invalid patch version in '{versionString}', using patch = 0");
                            patch = 0;
                        }
                    }
                    
                    return new Version(major, minor, patch);
                }
                else
                {
                    Debug.LogWarning($"PlatformCompatibility: Unexpected version format '{versionString}', using minimum version");
                    return MinimumUnityVersion;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"PlatformCompatibility: Exception parsing Unity version '{Application.unityVersion}': {ex.Message}, using minimum version");
                return MinimumUnityVersion;
            }
        }
        
        /// <summary>
        /// Check if TextMeshPro package is available
        /// </summary>
        private static bool HasTextMeshProPackage()
        {
            try
            {
                // Try to access TextMeshPro types to verify package availability
                var tmpType = typeof(TMPro.TextMeshProUGUI);
                return tmpType != null;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Get list of supported platforms
        /// </summary>
        private static HashSet<RuntimePlatform> GetSupportedPlatforms()
        {
            return new HashSet<RuntimePlatform>
            {
                RuntimePlatform.WindowsEditor,
                RuntimePlatform.WindowsPlayer,
                RuntimePlatform.OSXEditor,
                RuntimePlatform.OSXPlayer,
                RuntimePlatform.LinuxEditor,
                RuntimePlatform.LinuxPlayer,
                RuntimePlatform.Android,
                RuntimePlatform.IPhonePlayer
            };
        }
        
        /// <summary>
        /// Get platform-specific logger configuration
        /// Only applies performance optimizations, UI settings are handled by Unity Editor
        /// </summary>
        public static LogConfiguration GetPlatformOptimizedConfiguration()
        {
            // Load user configuration from Resources
            var config = LogConfiguration.CreateDefault();
            var perfSettings = GetPlatformPerformanceSettings();
            
            // Apply performance optimizations only
            config.maxLogCount = perfSettings.MaxLogCount;
            
            // Platform-specific timestamp formats (only if not already customized)
            if (config.timestampFormat == LogConstants.Formats.DEFAULT_TIMESTAMP) // Only override if using default
            {
                if (IsMobilePlatform)
                {
                    config.timestampFormat = LogConstants.Platform.MOBILE_TIMESTAMP_FORMAT; // Shorter format for mobile
                }
                // Desktop keeps the default timestamp format
            }
            
            return config;
        }
        
        /// <summary>
        /// Create platform-specific performance report
        /// </summary>
        public static PlatformCompatibilityReport GenerateCompatibilityReport()
        {
            return new PlatformCompatibilityReport
            {
                UnityVersion = CurrentUnityVersion,
                IsUnitySupportedVersion = IsUnitySupportedVersion,
                Platform = CurrentPlatform,
                IsSupportedPlatform = IsSupportedPlatform,
                IsTextMeshProSupported = IsTextMeshProSupported,
                IsMobilePlatform = IsMobilePlatform,
                IsDesktopPlatform = IsDesktopPlatform,
                IsEditor = IsEditor,
                PerformanceSettings = GetPlatformPerformanceSettings(),
                GeneratedAt = DateTime.Now
            };
        }
    }
    
    /// <summary>
    /// Platform-specific performance settings
    /// </summary>
    [System.Serializable]
    public struct PlatformPerformanceSettings
    {
        public int MaxLogCount;
        public float UpdateThrottleTime;
        public int MaxCharacterLimit;
        public bool BatchUpdatesEnabled;
        public float BatchUpdateInterval;
        public int MaxLinesLimit;
    }
    
    
    /// <summary>
    /// Platform compatibility report
    /// </summary>
    [System.Serializable]
    public struct PlatformCompatibilityReport
    {
        public Version UnityVersion;
        public bool IsUnitySupportedVersion;
        public RuntimePlatform Platform;
        public bool IsSupportedPlatform;
        public bool IsTextMeshProSupported;
        public bool IsMobilePlatform;
        public bool IsDesktopPlatform;
        public bool IsEditor;
        public PlatformPerformanceSettings PerformanceSettings;
        public DateTime GeneratedAt;
    }
}