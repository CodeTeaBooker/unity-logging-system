using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuntimeLogging
{
    /// <summary>
    /// Cross-platform validator for ILogger interface behavior and TextMeshPro consistency
    /// Provides comprehensive validation across Unity Editor, Windows, Android, and iOS platforms
    /// </summary>
    public class CrossPlatformLoggerValidator
    {
        /// <summary>
        /// Validate ILogger interface behavior across platforms
        /// </summary>
        public static LoggerValidationResult ValidateLoggerInterface(ILogger logger)
        {
            var result = new LoggerValidationResult
            {
                Logger = logger,
                Platform = Application.platform,
                UnityVersion = PlatformCompatibility.CurrentUnityVersion,
                TestResults = new List<LoggerTestResult>()
            };
            
            // Check for null logger first
            if (logger == null)
            {
                result.IsValid = false;
                result.ValidationError = "Logger is null";
                result.ValidationTime = DateTime.Now;
                return result;
            }
            
            try
            {
                // Test basic logging functionality
                result.TestResults.Add(TestBasicLogging(logger));
                
                // Test null safety
                result.TestResults.Add(TestNullSafety(logger));
                
                // Test message formatting
                result.TestResults.Add(TestMessageFormatting(logger));
                
                // Test performance characteristics
                result.TestResults.Add(TestPerformanceCharacteristics(logger));
                
                // Test platform-specific behavior
                result.TestResults.Add(TestPlatformSpecificBehavior(logger));
                
                result.IsValid = result.TestResults.All(t => t.Passed);
                result.ValidationTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ValidationError = ex.Message;
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate TextMeshPro consistency across platforms
        /// </summary>
        public static TextMeshProValidationResult ValidateTextMeshProConsistency(LogDisplay logDisplay)
        {
            var result = new TextMeshProValidationResult
            {
                Platform = Application.platform,
                UnityVersion = PlatformCompatibility.CurrentUnityVersion,
                TestResults = new List<TextMeshProTestResult>()
            };
            
            if (logDisplay == null)
            {
                result.IsValid = false;
                result.ValidationError = "LogDisplay is null";
                return result;
            }
            
            var textComponent = logDisplay.GetTextComponent();
            if (textComponent == null)
            {
                result.IsValid = false;
                result.ValidationError = "TextMeshProUGUI component is null";
                return result;
            }
            
            try
            {
                // Test TextMeshPro component validation
                result.TestResults.Add(TestTextMeshProComponent(textComponent));
                
                // Test rich text rendering
                result.TestResults.Add(TestRichTextRendering(logDisplay, textComponent));
                
                // Test text update performance
                result.TestResults.Add(TestTextUpdatePerformance(logDisplay));
                
                // Test platform-specific rendering
                result.TestResults.Add(TestPlatformSpecificRendering(textComponent));
                
                // Test memory usage
                result.TestResults.Add(TestMemoryUsage(logDisplay));
                
                result.IsValid = result.TestResults.All(t => t.Passed);
                result.ValidationTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ValidationError = ex.Message;
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate complete logging system across platforms
        /// </summary>
        public static SystemValidationResult ValidateCompleteSystem(ScreenLogger screenLogger)
        {
            var result = new SystemValidationResult
            {
                Platform = Application.platform,
                UnityVersion = PlatformCompatibility.CurrentUnityVersion,
                CompatibilityReport = PlatformCompatibility.GenerateCompatibilityReport()
            };
            
            if (screenLogger == null)
            {
                result.IsValid = false;
                result.ValidationError = "ScreenLogger is null";
                return result;
            }
            
            try
            {
                // Validate logger interface
                result.LoggerValidation = ValidateLoggerInterface(screenLogger);
                
                // Validate TextMeshPro consistency
                var logDisplay = screenLogger.GetLogDisplay();
                if (logDisplay != null)
                {
                    result.TextMeshProValidation = ValidateTextMeshProConsistency(logDisplay);
                }
                
                // Test integration behavior
                result.IntegrationTests = TestSystemIntegration(screenLogger);
                
                result.IsValid = result.LoggerValidation.IsValid && 
                                (result.TextMeshProValidation?.IsValid ?? true) &&
                                result.IntegrationTests.All(t => t.Passed);
                                
                result.ValidationTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ValidationError = ex.Message;
            }
            
            return result;
        }
        
        private static LoggerTestResult TestBasicLogging(ILogger logger)
        {
            var result = new LoggerTestResult { TestName = "Basic Logging" };
            
            try
            {
                // Test that methods don't throw exceptions
                logger.Log("Test info message");
                logger.LogWarning("Test warning message");
                logger.LogError("Test error message");
                
                result.Passed = true;
                result.Message = "Basic logging methods executed without exceptions";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Basic logging failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static LoggerTestResult TestNullSafety(ILogger logger)
        {
            var result = new LoggerTestResult { TestName = "Null Safety" };
            
            try
            {
                // Test null message handling
                logger.Log(null);
                logger.LogWarning(null);
                logger.LogError(null);
                
                // Test empty message handling
                logger.Log("");
                logger.LogWarning("");
                logger.LogError("");
                
                result.Passed = true;
                result.Message = "Null and empty message handling works correctly";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Null safety test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static LoggerTestResult TestMessageFormatting(ILogger logger)
        {
            var result = new LoggerTestResult { TestName = "Message Formatting" };
            
            try
            {
                // Test various message formats
                logger.Log("Simple message");
                logger.Log("Message with special characters: !@#$%^&*()");
                logger.Log("Message with unicode: 🚀 ✅ ❌");
                logger.Log("Very long message: " + new string('A', 1000));
                
                result.Passed = true;
                result.Message = "Message formatting handled correctly";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Message formatting test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static LoggerTestResult TestPerformanceCharacteristics(ILogger logger)
        {
            var result = new LoggerTestResult { TestName = "Performance Characteristics" };
            
            try
            {
                var startTime = DateTime.Now;
                
                // Test high-volume logging
                for (int i = 0; i < 100; i++)
                {
                    logger.Log($"Performance test message {i}");
                }
                
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                
                // Performance should be reasonable (less than 1 second for 100 messages)
                if (duration.TotalSeconds < 1.0)
                {
                    result.Passed = true;
                    result.Message = $"Performance test passed: {duration.TotalMilliseconds:F2}ms for 100 messages";
                }
                else
                {
                    result.Passed = false;
                    result.Message = $"Performance test failed: {duration.TotalMilliseconds:F2}ms for 100 messages (too slow)";
                }
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Performance test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static LoggerTestResult TestPlatformSpecificBehavior(ILogger logger)
        {
            var result = new LoggerTestResult { TestName = "Platform Specific Behavior" };
            
            try
            {
                // Test platform-specific message handling
                string platformMessage = $"Platform test on {Application.platform}";
                logger.Log(platformMessage);
                
                // Test platform-specific character encoding
                if (PlatformCompatibility.IsMobilePlatform)
                {
                    logger.Log("Mobile platform test: 📱");
                }
                else if (PlatformCompatibility.IsDesktopPlatform)
                {
                    logger.Log("Desktop platform test: 🖥️");
                }
                
                result.Passed = true;
                result.Message = $"Platform-specific behavior validated for {Application.platform}";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Platform-specific test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static TextMeshProTestResult TestTextMeshProComponent(TMPro.TextMeshProUGUI textComponent)
        {
            var result = new TextMeshProTestResult { TestName = "TextMeshPro Component" };
            
            try
            {
                // Basic validation - UI properties are now configured in Unity Editor
                bool isValid = textComponent != null && PlatformCompatibility.IsTextMeshProSupported;
                
                result.Passed = isValid;
                if (isValid)
                {
                    result.Message = "TextMeshPro component validation passed - UI properties configured in Unity Editor";
                }
                else
                {
                    result.Message = textComponent == null ? 
                        "TextMeshPro component is null" : 
                        "TextMeshPro not supported on this platform";
                }
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"TextMeshPro component test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static TextMeshProTestResult TestRichTextRendering(LogDisplay logDisplay, TMPro.TextMeshProUGUI textComponent)
        {
            var result = new TextMeshProTestResult { TestName = "Rich Text Rendering" };
            
            try
            {
                // Test rich text markup
                string richTextTest = "<color=#FF0000>Red text</color> <color=#00FF00>Green text</color> <color=#0000FF>Blue text</color>";
                textComponent.text = richTextTest;
                
                // Verify rich text is enabled
                if (!textComponent.richText)
                {
                    result.Passed = false;
                    result.Message = "Rich text is not enabled on TextMeshPro component";
                    return result;
                }
                
                result.Passed = true;
                result.Message = "Rich text rendering test passed";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Rich text rendering test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static TextMeshProTestResult TestTextUpdatePerformance(LogDisplay logDisplay)
        {
            var result = new TextMeshProTestResult { TestName = "Text Update Performance" };
            
            try
            {
                var startTime = DateTime.Now;
                
                // Test multiple text updates
                for (int i = 0; i < 50; i++)
                {
                    logDisplay.UpdateDisplay($"Performance test update {i}");
                }
                
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                
                // Performance should be reasonable (less than 500ms for 50 updates)
                if (duration.TotalMilliseconds < 500)
                {
                    result.Passed = true;
                    result.Message = $"Text update performance test passed: {duration.TotalMilliseconds:F2}ms for 50 updates";
                }
                else
                {
                    result.Passed = false;
                    result.Message = $"Text update performance test failed: {duration.TotalMilliseconds:F2}ms for 50 updates (too slow)";
                }
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Text update performance test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static TextMeshProTestResult TestPlatformSpecificRendering(TMPro.TextMeshProUGUI textComponent)
        {
            var result = new TextMeshProTestResult { TestName = "Platform Specific Rendering" };
            
            try
            {
                // UI properties are now configured in Unity Editor - no platform modifications needed
                
                // Test platform-specific text
                string platformText = $"Platform rendering test on {Application.platform}";
                textComponent.text = platformText;
                
                result.Passed = true;
                result.Message = $"Platform-specific rendering validated for {Application.platform}";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Platform-specific rendering test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static TextMeshProTestResult TestMemoryUsage(LogDisplay logDisplay)
        {
            var result = new TextMeshProTestResult { TestName = "Memory Usage" };
            
            try
            {
                // Get initial memory usage
                long initialMemory = GC.GetTotalMemory(false);
                
                // Perform memory-intensive operations
                for (int i = 0; i < 100; i++)
                {
                    logDisplay.UpdateDisplay($"Memory test {i}: " + new string('X', 100));
                }
                
                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                long finalMemory = GC.GetTotalMemory(false);
                long memoryIncrease = finalMemory - initialMemory;
                
                // Memory increase should be reasonable (less than 1MB)
                if (memoryIncrease < 1024 * 1024)
                {
                    result.Passed = true;
                    result.Message = $"Memory usage test passed: {memoryIncrease / 1024}KB increase";
                }
                else
                {
                    result.Passed = false;
                    result.Message = $"Memory usage test failed: {memoryIncrease / 1024}KB increase (too high)";
                }
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Memory usage test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static List<IntegrationTestResult> TestSystemIntegration(ScreenLogger screenLogger)
        {
            var results = new List<IntegrationTestResult>();
            
            // Test logger initialization
            results.Add(TestLoggerInitialization(screenLogger));
            
            // Test configuration integration
            results.Add(TestConfigurationIntegration(screenLogger));
            
            // Test display integration
            results.Add(TestDisplayIntegration(screenLogger));
            
            // Test performance integration
            results.Add(TestPerformanceIntegration(screenLogger));
            
            return results;
        }
        
        private static IntegrationTestResult TestLoggerInitialization(ScreenLogger screenLogger)
        {
            var result = new IntegrationTestResult { TestName = "Logger Initialization" };
            
            try
            {
                bool wasInitialized = screenLogger.IsInitialized();
                
                if (!wasInitialized)
                {
                    screenLogger.Initialize();
                }
                
                result.Passed = screenLogger.IsInitialized();
                result.Message = result.Passed ? 
                    "Logger initialization successful" : 
                    "Logger initialization failed";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Logger initialization test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static IntegrationTestResult TestConfigurationIntegration(ScreenLogger screenLogger)
        {
            var result = new IntegrationTestResult { TestName = "Configuration Integration" };
            
            try
            {
                var config = screenLogger.GetConfiguration();
                if (config == null)
                {
                    result.Passed = false;
                    result.Message = "Configuration is null";
                    return result;
                }
                
                // Test configuration changes
                int originalMaxCount = config.maxLogCount;
                screenLogger.SetMaxLogCount(50);
                
                if (screenLogger.GetMaxLogCount() == 50)
                {
                    result.Passed = true;
                    result.Message = "Configuration integration successful";
                }
                else
                {
                    result.Passed = false;
                    result.Message = "Configuration changes not applied correctly";
                }
                
                // Restore original value
                screenLogger.SetMaxLogCount(originalMaxCount);
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Configuration integration test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static IntegrationTestResult TestDisplayIntegration(ScreenLogger screenLogger)
        {
            var result = new IntegrationTestResult { TestName = "Display Integration" };
            
            try
            {
                var logDisplay = screenLogger.GetLogDisplay();
                if (logDisplay == null)
                {
                    result.Passed = false;
                    result.Message = "LogDisplay is null";
                    return result;
                }
                
                // Test display operations
                screenLogger.Clear();
                screenLogger.Log("Integration test message");
                screenLogger.Show();
                screenLogger.Hide();
                
                result.Passed = true;
                result.Message = "Display integration successful";
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Display integration test failed: {ex.Message}";
            }
            
            return result;
        }
        
        private static IntegrationTestResult TestPerformanceIntegration(ScreenLogger screenLogger)
        {
            var result = new IntegrationTestResult { TestName = "Performance Integration" };
            
            try
            {
                var startTime = DateTime.Now;
                
                // Test integrated performance
                for (int i = 0; i < 50; i++)
                {
                    screenLogger.Log($"Performance integration test {i}");
                }
                
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                
                // Performance should be reasonable
                if (duration.TotalMilliseconds < 1000)
                {
                    result.Passed = true;
                    result.Message = $"Performance integration test passed: {duration.TotalMilliseconds:F2}ms";
                }
                else
                {
                    result.Passed = false;
                    result.Message = $"Performance integration test failed: {duration.TotalMilliseconds:F2}ms (too slow)";
                }
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.Message = $"Performance integration test failed: {ex.Message}";
            }
            
            return result;
        }
    }
    
    // Result classes
    public class LoggerValidationResult
    {
        public ILogger Logger { get; set; }
        public RuntimePlatform Platform { get; set; }
        public Version UnityVersion { get; set; }
        public bool IsValid { get; set; }
        public List<LoggerTestResult> TestResults { get; set; }
        public string ValidationError { get; set; }
        public DateTime ValidationTime { get; set; }
    }
    
    public class TextMeshProValidationResult
    {
        public RuntimePlatform Platform { get; set; }
        public Version UnityVersion { get; set; }
        public bool IsValid { get; set; }
        public List<TextMeshProTestResult> TestResults { get; set; }
        public string ValidationError { get; set; }
        public DateTime ValidationTime { get; set; }
    }
    
    public class SystemValidationResult
    {
        public RuntimePlatform Platform { get; set; }
        public Version UnityVersion { get; set; }
        public bool IsValid { get; set; }
        public LoggerValidationResult LoggerValidation { get; set; }
        public TextMeshProValidationResult TextMeshProValidation { get; set; }
        public List<IntegrationTestResult> IntegrationTests { get; set; }
        public PlatformCompatibilityReport CompatibilityReport { get; set; }
        public string ValidationError { get; set; }
        public DateTime ValidationTime { get; set; }
    }
    
    public class LoggerTestResult
    {
        public string TestName { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; }
    }
    
    public class TextMeshProTestResult
    {
        public string TestName { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; }
    }
    
    public class IntegrationTestResult
    {
        public string TestName { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; }
    }
}