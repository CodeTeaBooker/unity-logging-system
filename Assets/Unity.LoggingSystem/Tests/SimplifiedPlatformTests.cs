using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using RuntimeLogging;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Simplified platform tests after UI refactoring
    /// UI properties are now configured in Unity Editor
    /// </summary>
    public class SimplifiedPlatformTests
    {
        [Test]
        public void PlatformCompatibility_CheckBasicSupport()
        {
            // Test basic platform support
            Assert.That(PlatformCompatibility.IsTextMeshProSupported, Is.True, 
                "TextMeshPro should be supported");
            Assert.That(PlatformCompatibility.IsSupportedPlatform, Is.True,
                "Current platform should be supported");
        }
        
        [Test] 
        public void PlatformCompatibility_GenerateReport()
        {
            // Test compatibility report generation
            var report = PlatformCompatibility.GenerateCompatibilityReport();
            
            Assert.That(report.Platform, Is.EqualTo(Application.platform));
            Assert.That(report.IsTextMeshProSupported, Is.True);
            Assert.That(report.PerformanceSettings.MaxLogCount, Is.GreaterThan(0));
        }
        
        [Test]
        public void LogConfiguration_BasicFunctionality()
        {
            // Test configuration loading without UI properties
            var config = LogConfiguration.CreateDefault();
            
            Assert.That(config, Is.Not.Null);
            Assert.That(config.maxLogCount, Is.GreaterThan(0));
            Assert.That(config.timestampFormat, Is.Not.Null.And.Not.Empty);
        }
    }
}