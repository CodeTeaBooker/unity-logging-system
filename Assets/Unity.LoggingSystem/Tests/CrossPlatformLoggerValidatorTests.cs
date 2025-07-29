using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using System;
using System.Linq;
using RuntimeLogging.Tests.TestUtilities;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Tests for CrossPlatformLoggerValidator class
    /// Validates cross-platform logger validation functionality and TextMeshPro consistency checks
    /// </summary>
    [TestFixture]
    public class CrossPlatformLoggerValidatorTests
    {
        private GameObject testGameObject;
        private ScreenLogger testScreenLogger;
        private LogDisplay testLogDisplay;
        private TextMeshProUGUI testTextComponent;
        
        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with required components
            testGameObject = new GameObject("TestScreenLogger");
            testScreenLogger = testGameObject.AddComponent<ScreenLogger>();
            testLogDisplay = testGameObject.AddComponent<LogDisplay>();
            testTextComponent = testGameObject.AddComponent<TextMeshProUGUI>();
            
            // Configure components
            testLogDisplay.SetTextComponent(testTextComponent);
            testScreenLogger.SetLogDisplay(testLogDisplay);
            testScreenLogger.Initialize();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testGameObject);
            }
        }
        
        [Test]
        public void ValidateLoggerInterface_WithNullLogger_ReturnsInvalidResult()
        {
            // Arrange
            ILogger nullLogger = null;
            
            // Act
            var result = CrossPlatformLoggerValidator.ValidateLoggerInterface(nullLogger);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null, "Validation result should not be null");
                Assert.That(result.IsValid, Is.False, "Validation should fail for null logger");
                Assert.That(result.ValidationError, Is.Not.Null.And.Not.Empty, "ValidationError should contain error message");
            });
        }
        
        [Test]
        public void ValidateLoggerInterface_WithValidLogger_ReturnsValidResult()
        {
            // Arrange
            var logger = new UnityLogger();
            
            // Expect Unity console log messages from validation tests
            LogAssert.ignoreFailingMessages = true;
            
            // Act
            var result = CrossPlatformLoggerValidator.ValidateLoggerInterface(logger);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null, "Validation result should not be null");
                Assert.That(result.Logger, Is.EqualTo(logger), "Result should reference the validated logger");
                Assert.That(result.Platform, Is.EqualTo(Application.platform), "Platform should match current platform");
                Assert.That(result.UnityVersion, Is.Not.Null, "UnityVersion should not be null");
                Assert.That(result.TestResults, Is.Not.Null, "TestResults should not be null");
                Assert.That(result.ValidationTime, Is.LessThanOrEqualTo(DateTime.Now), "ValidationTime should be recent");
            });
            
            // Reset log assertion behavior
            LogAssert.ignoreFailingMessages = false;
        }
        
        [Test]
        public void ValidateLoggerInterface_WithScreenLogger_RunsAllTests()
        {
            // Arrange & Act
            var result = CrossPlatformLoggerValidator.ValidateLoggerInterface(testScreenLogger);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.TestResults, Is.Not.Empty, "Should run multiple test cases");
                Assert.That(result.TestResults.Count, Is.GreaterThanOrEqualTo(5), "Should run at least 5 test cases");
                
                // Verify specific tests are included
                var testNames = result.TestResults.Select(t => t.TestName).ToList();
                Assert.That(testNames, Does.Contain("Basic Logging"), "Should include Basic Logging test");
                Assert.That(testNames, Does.Contain("Null Safety"), "Should include Null Safety test");
                Assert.That(testNames, Does.Contain("Message Formatting"), "Should include Message Formatting test");
                Assert.That(testNames, Does.Contain("Performance Characteristics"), "Should include Performance test");
                Assert.That(testNames, Does.Contain("Platform Specific Behavior"), "Should include Platform Specific test");
            });
        }
        
        [Test]
        public void ValidateLoggerInterface_WithValidLogger_PassesBasicTests()
        {
            // Arrange & Act
            var result = CrossPlatformLoggerValidator.ValidateLoggerInterface(testScreenLogger);
            
            // Assert
            var basicLoggingTest = result.TestResults.FirstOrDefault(t => t.TestName == "Basic Logging");
            Assert.That(basicLoggingTest, Is.Not.Null, "Basic Logging test should be present");
            Assert.That(basicLoggingTest.Passed, Is.True, $"Basic Logging test should pass: {basicLoggingTest.Message}");
        }
        
        [Test]
        public void ValidateTextMeshProConsistency_WithNullLogDisplay_ReturnsInvalidResult()
        {
            // Arrange
            LogDisplay nullLogDisplay = null;
            
            // Act
            var result = CrossPlatformLoggerValidator.ValidateTextMeshProConsistency(nullLogDisplay);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null, "Validation result should not be null");
                Assert.That(result.IsValid, Is.False, "Validation should fail for null LogDisplay");
                Assert.That(result.ValidationError, Does.Contain("null"), "ValidationError should mention null LogDisplay");
            });
        }
        
        [Test]
        public void ValidateTextMeshProConsistency_WithValidLogDisplay_ReturnsValidResult()
        {
            // Arrange & Act
            var result = CrossPlatformLoggerValidator.ValidateTextMeshProConsistency(testLogDisplay);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null, "Validation result should not be null");
                Assert.That(result.Platform, Is.EqualTo(Application.platform), "Platform should match current platform");
                Assert.That(result.UnityVersion, Is.Not.Null, "UnityVersion should not be null");
                Assert.That(result.TestResults, Is.Not.Null, "TestResults should not be null");
                Assert.That(result.ValidationTime, Is.LessThanOrEqualTo(DateTime.Now), "ValidationTime should be recent");
            });
        }
        
        [Test]
        public void ValidateTextMeshProConsistency_WithValidLogDisplay_RunsAllTests()
        {
            // Arrange & Act
            var result = CrossPlatformLoggerValidator.ValidateTextMeshProConsistency(testLogDisplay);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.TestResults, Is.Not.Empty, "Should run multiple test cases");
                Assert.That(result.TestResults.Count, Is.GreaterThanOrEqualTo(5), "Should run at least 5 test cases");
                
                // Verify specific tests are included
                var testNames = result.TestResults.Select(t => t.TestName).ToList();
                Assert.That(testNames, Does.Contain("TextMeshPro Component"), "Should include TextMeshPro Component test");
                Assert.That(testNames, Does.Contain("Rich Text Rendering"), "Should include Rich Text Rendering test");
                Assert.That(testNames, Does.Contain("Text Update Performance"), "Should include Text Update Performance test");
                Assert.That(testNames, Does.Contain("Platform Specific Rendering"), "Should include Platform Specific Rendering test");
                Assert.That(testNames, Does.Contain("Memory Usage"), "Should include Memory Usage test");
            });
        }
        
        [Test]
        public void ValidateCompleteSystem_WithNullScreenLogger_ReturnsInvalidResult()
        {
            // Arrange
            ScreenLogger nullScreenLogger = null;
            
            // Act
            var result = CrossPlatformLoggerValidator.ValidateCompleteSystem(nullScreenLogger);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null, "Validation result should not be null");
                Assert.That(result.IsValid, Is.False, "Validation should fail for null ScreenLogger");
                Assert.That(result.ValidationError, Does.Contain("null"), "ValidationError should mention null ScreenLogger");
            });
        }
        
        [Test]
        public void ValidateCompleteSystem_WithValidScreenLogger_ReturnsValidResult()
        {
            // Arrange & Act
            var result = CrossPlatformLoggerValidator.ValidateCompleteSystem(testScreenLogger);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null, "Validation result should not be null");
                Assert.That(result.Platform, Is.EqualTo(Application.platform), "Platform should match current platform");
                Assert.That(result.UnityVersion, Is.Not.Null, "UnityVersion should not be null");
                Assert.That(result.CompatibilityReport.Platform, Is.EqualTo(Application.platform), "CompatibilityReport should have correct platform");
                Assert.That(result.ValidationTime, Is.LessThanOrEqualTo(DateTime.Now), "ValidationTime should be recent");
            });
        }
        
        [Test]
        public void ValidateCompleteSystem_WithValidScreenLogger_ValidatesAllComponents()
        {
            // Arrange & Act
            var result = CrossPlatformLoggerValidator.ValidateCompleteSystem(testScreenLogger);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.LoggerValidation, Is.Not.Null, "LoggerValidation should not be null");
                Assert.That(result.TextMeshProValidation, Is.Not.Null, "TextMeshProValidation should not be null");
                Assert.That(result.IntegrationTests, Is.Not.Null, "IntegrationTests should not be null");
                Assert.That(result.IntegrationTests, Is.Not.Empty, "IntegrationTests should contain test results");
            });
        }
        
        [Test]
        public void ValidateCompleteSystem_WithValidScreenLogger_RunsIntegrationTests()
        {
            // Arrange & Act
            var result = CrossPlatformLoggerValidator.ValidateCompleteSystem(testScreenLogger);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result.IntegrationTests.Count, Is.GreaterThanOrEqualTo(4), "Should run at least 4 integration tests");
                
                // Verify specific integration tests are included
                var testNames = result.IntegrationTests.Select(t => t.TestName).ToList();
                Assert.That(testNames, Does.Contain("Logger Initialization"), "Should include Logger Initialization test");
                Assert.That(testNames, Does.Contain("Configuration Integration"), "Should include Configuration Integration test");
                Assert.That(testNames, Does.Contain("Display Integration"), "Should include Display Integration test");
                Assert.That(testNames, Does.Contain("Performance Integration"), "Should include Performance Integration test");
            });
        }
        
        [Test]
        public void LoggerTestResult_Structure_HasValidProperties()
        {
            // Arrange & Act
            var testResult = new LoggerTestResult
            {
                TestName = "Test Name",
                Passed = true,
                Message = "Test message"
            };
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(testResult.TestName, Is.EqualTo("Test Name"), "TestName should be set correctly");
                Assert.That(testResult.Passed, Is.True, "Passed should be set correctly");
                Assert.That(testResult.Message, Is.EqualTo("Test message"), "Message should be set correctly");
            });
        }
        
        [Test]
        public void TextMeshProTestResult_Structure_HasValidProperties()
        {
            // Arrange & Act
            var testResult = new TextMeshProTestResult
            {
                TestName = "TextMeshPro Test",
                Passed = false,
                Message = "TextMeshPro test message"
            };
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(testResult.TestName, Is.EqualTo("TextMeshPro Test"), "TestName should be set correctly");
                Assert.That(testResult.Passed, Is.False, "Passed should be set correctly");
                Assert.That(testResult.Message, Is.EqualTo("TextMeshPro test message"), "Message should be set correctly");
            });
        }
        
        [Test]
        public void IntegrationTestResult_Structure_HasValidProperties()
        {
            // Arrange & Act
            var testResult = new IntegrationTestResult
            {
                TestName = "Integration Test",
                Passed = true,
                Message = "Integration test message"
            };
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(testResult.TestName, Is.EqualTo("Integration Test"), "TestName should be set correctly");
                Assert.That(testResult.Passed, Is.True, "Passed should be set correctly");
                Assert.That(testResult.Message, Is.EqualTo("Integration test message"), "Message should be set correctly");
            });
        }
        
        [Test]
        public void LoggerValidationResult_Structure_HasValidProperties()
        {
            // Arrange
            var logger = new UnityLogger();
            var testResults = new System.Collections.Generic.List<LoggerTestResult>
            {
                new LoggerTestResult { TestName = "Test1", Passed = true, Message = "Success" }
            };
            
            // Act
            var validationResult = new LoggerValidationResult
            {
                Logger = logger,
                Platform = RuntimePlatform.WindowsEditor,
                UnityVersion = new Version(2021, 3, 0),
                IsValid = true,
                TestResults = testResults,
                ValidationError = null,
                ValidationTime = DateTime.Now
            };
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(validationResult.Logger, Is.EqualTo(logger), "Logger should be set correctly");
                Assert.That(validationResult.Platform, Is.EqualTo(RuntimePlatform.WindowsEditor), "Platform should be set correctly");
                Assert.That(validationResult.UnityVersion, Is.EqualTo(new Version(2021, 3, 0)), "UnityVersion should be set correctly");
                Assert.That(validationResult.IsValid, Is.True, "IsValid should be set correctly");
                Assert.That(validationResult.TestResults, Is.EqualTo(testResults), "TestResults should be set correctly");
                Assert.That(validationResult.ValidationError, Is.Null, "ValidationError should be null");
                Assert.That(validationResult.ValidationTime, Is.LessThanOrEqualTo(DateTime.Now), "ValidationTime should be recent");
            });
        }
        
        [Test]
        public void SystemValidationResult_Structure_HasValidProperties()
        {
            // Arrange
            var loggerValidation = new LoggerValidationResult { IsValid = true };
            var textMeshProValidation = new TextMeshProValidationResult { IsValid = true };
            var integrationTests = new System.Collections.Generic.List<IntegrationTestResult>();
            var compatibilityReport = PlatformCompatibility.GenerateCompatibilityReport();
            
            // Act
            var systemResult = new SystemValidationResult
            {
                Platform = RuntimePlatform.WindowsEditor,
                UnityVersion = new Version(2021, 3, 0),
                IsValid = true,
                LoggerValidation = loggerValidation,
                TextMeshProValidation = textMeshProValidation,
                IntegrationTests = integrationTests,
                CompatibilityReport = compatibilityReport,
                ValidationError = null,
                ValidationTime = DateTime.Now
            };
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(systemResult.Platform, Is.EqualTo(RuntimePlatform.WindowsEditor), "Platform should be set correctly");
                Assert.That(systemResult.UnityVersion, Is.EqualTo(new Version(2021, 3, 0)), "UnityVersion should be set correctly");
                Assert.That(systemResult.IsValid, Is.True, "IsValid should be set correctly");
                Assert.That(systemResult.LoggerValidation, Is.EqualTo(loggerValidation), "LoggerValidation should be set correctly");
                Assert.That(systemResult.TextMeshProValidation, Is.EqualTo(textMeshProValidation), "TextMeshProValidation should be set correctly");
                Assert.That(systemResult.IntegrationTests, Is.EqualTo(integrationTests), "IntegrationTests should be set correctly");
                Assert.That(systemResult.CompatibilityReport.Platform, Is.EqualTo(compatibilityReport.Platform), "CompatibilityReport should be set correctly");
                Assert.That(systemResult.ValidationError, Is.Null, "ValidationError should be null");
                Assert.That(systemResult.ValidationTime, Is.LessThanOrEqualTo(DateTime.Now), "ValidationTime should be recent");
            });
        }
        
        [Test]
        public void ValidateLoggerInterface_WithCompositeLogger_ValidatesCorrectly()
        {
            // Arrange
            var unityLogger = new UnityLogger();
            var compositeLogger = new CompositeLogger(unityLogger, testScreenLogger);
            
            // Expect Unity console log messages from validation tests
            LogAssert.ignoreFailingMessages = true;
            
            // Act
            var result = CrossPlatformLoggerValidator.ValidateLoggerInterface(compositeLogger);
            
            // Assert
            MultipleAssert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null, "Validation result should not be null");
                Assert.That(result.Logger, Is.EqualTo(compositeLogger), "Result should reference the composite logger");
                Assert.That(result.TestResults, Is.Not.Empty, "Should run test cases for composite logger");
            });
            
            // Reset log assertion behavior
            LogAssert.ignoreFailingMessages = false;
        }
        
        [Test]
        public void ValidateTextMeshProConsistency_WithLogDisplayWithoutTextComponent_HandlesGracefully()
        {
            // Arrange
            var gameObject = new GameObject("TestLogDisplayNoText");
            var logDisplayWithoutText = gameObject.AddComponent<LogDisplay>();
            // Don't set TextMeshPro component
            
            try
            {
                // Act
                var result = CrossPlatformLoggerValidator.ValidateTextMeshProConsistency(logDisplayWithoutText);
                
                // Assert
                MultipleAssert.Multiple(() =>
                {
                    Assert.That(result, Is.Not.Null, "Validation result should not be null");
                    Assert.That(result.IsValid, Is.False, "Validation should fail without TextMeshPro component");
                    Assert.That(result.ValidationError, Does.Contain("null"), "ValidationError should mention null component");
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}