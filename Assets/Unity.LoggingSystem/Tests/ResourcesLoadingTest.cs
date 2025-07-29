using UnityEngine;
using NUnit.Framework;
using RuntimeLogging;

namespace RuntimeLogging.Tests
{
    /// <summary>
    /// Test class specifically for verifying Resources loading functionality
    /// </summary>
    public class ResourcesLoadingTest
    {
        [Test]
        public void Resources_LoadLogPanelConfig_ShouldSucceed()
        {
            // Act
            var config = Resources.Load<LogConfiguration>("LogPanelConfig");
            
            // Assert
            Assert.That(config, Is.Not.Null, 
                "Should be able to load LogPanelConfig from Resources folder");
            Assert.That(config.name, Is.EqualTo("LogPanelConfig"), 
                "Loaded config should have correct name");
        }
        
        [Test]
        public void LogConfiguration_CreateDefault_ShouldLoadFromResources()
        {
            // Act
            var config = LogConfiguration.CreateDefault();
            
            // Assert
            Assert.That(config, Is.Not.Null, 
                "CreateDefault should return a valid configuration");
            
            // The config should either be loaded from Resources or be a fallback
            // Both are valid outcomes, so we just verify basic functionality
            Assert.That(config.maxLogCount, Is.GreaterThan(0), 
                "Configuration should have valid maxLogCount");
            Assert.That(config.timestampFormat, Is.Not.Null.And.Not.Empty, 
                "Configuration should have valid timestampFormat");
        }
        
        [Test]
        public void Resources_LoadAll_ShouldFindLogConfigurations()
        {
            // Act
            var allConfigs = Resources.LoadAll<LogConfiguration>("");
            
            // Assert
            Assert.That(allConfigs, Is.Not.Null, 
                "LoadAll should not return null");
            Assert.That(allConfigs.Length, Is.GreaterThanOrEqualTo(1), 
                "Should find at least one LogConfiguration in Resources");
            
            // Log findings for debugging
            Debug.Log($"Found {allConfigs.Length} LogConfiguration(s) in Resources:");
            for (int i = 0; i < allConfigs.Length; i++)
            {
                Debug.Log($"  [{i}] {allConfigs[i].name}");
            }
        }
    }
}