using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace RuntimeLogging.Tests
{
    [TestFixture]
    public class LogDataManagerTests
    {
        private LogDataManager _logDataManager;
        private LogConfiguration _testConfig;
        
        [SetUp]
        public void SetUp()
        {
            _testConfig = ScriptableObject.CreateInstance<LogConfiguration>();
            _testConfig.maxLogCount = 5; // Small limit for testing
            _testConfig.timestampFormat = "HH:mm:ss";
            
            _logDataManager = new LogDataManager(_testConfig);
        }
        
        [TearDown]
        public void TearDown()
        {
            _logDataManager?.ClearLogs();
            if (_testConfig != null)
            {
                UnityEngine.Object.DestroyImmediate(_testConfig);
            }
        }
        
        [Test]
        public void Constructor_WithConfiguration_SetsMaxLogCount()
        {
            // Arrange & Act
            var manager = new LogDataManager(_testConfig);
            
            // Assert
            Assert.AreEqual(_testConfig.maxLogCount, manager.GetMaxLogCount());
        }
        
        [Test]
        public void Constructor_WithoutConfiguration_UsesDefaultMaxLogCount()
        {
            // Arrange & Act
            var manager = new LogDataManager();
            
            // Assert
            Assert.AreEqual(100, manager.GetMaxLogCount()); // Default value
        }
        
        [Test]
        public void AddLog_WithValidMessage_AddsLogEntry()
        {
            // Arrange
            bool eventFired = false;
            LogEntry receivedEntry = default;
            _logDataManager.OnLogAdded += (entry) => { eventFired = true; receivedEntry = entry; };
            
            // Act
            _logDataManager.AddLog("Test message", LogLevel.Info);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual("Test message", receivedEntry.message);
            Assert.AreEqual(LogLevel.Info, receivedEntry.level);
            Assert.AreEqual(1, _logDataManager.GetLogCount());
        }
        
        [Test]
        public void AddLog_WithNullOrEmptyMessage_DoesNotAddEntry()
        {
            // Arrange
            int eventCount = 0;
            _logDataManager.OnLogAdded += (entry) => eventCount++;
            
            // Act
            _logDataManager.AddLog(null, LogLevel.Info);
            _logDataManager.AddLog("", LogLevel.Warning);
            
            // Assert
            Assert.AreEqual(0, eventCount); // No events should fire for null/empty
            Assert.AreEqual(0, _logDataManager.GetLogCount()); // No entries should be added
        }
        
        [Test]
        public void AddLog_WithWhitespaceMessage_AddsEntry()
        {
            // Arrange
            int eventCount = 0;
            _logDataManager.OnLogAdded += (entry) => eventCount++;
            
            // Act
            _logDataManager.AddLog("   ", LogLevel.Error); // Whitespace should be added
            _logDataManager.AddLog("\t\n", LogLevel.Info); // Other whitespace should be added
            
            // Assert
            Assert.AreEqual(2, eventCount); // Events should fire for whitespace messages
            Assert.AreEqual(2, _logDataManager.GetLogCount()); // Whitespace messages should be added
            
            var logs = _logDataManager.GetLogs();
            Assert.AreEqual("   ", logs[0].message); // Verify first whitespace message
            Assert.AreEqual("\t\n", logs[1].message); // Verify second whitespace message
        }
        
        [Test]
        public void AddLog_ExceedsMaxCount_RemovesOldestEntries()
        {
            // Arrange
            int maxCount = _testConfig.maxLogCount;
            
            // Act - Add more logs than the limit
            for (int i = 0; i < maxCount + 3; i++)
            {
                _logDataManager.AddLog($"Message {i}", LogLevel.Info);
            }
            
            // Assert
            Assert.AreEqual(maxCount, _logDataManager.GetLogCount());
            var logs = _logDataManager.GetLogs();
            Assert.AreEqual($"Message {3}", logs[0].message); // First entry should be message 3 (oldest removed)
            Assert.AreEqual($"Message {maxCount + 2}", logs[logs.Count - 1].message); // Last entry
        }
        
        [Test]
        public void ClearLogs_RemovesAllEntries_FiresEvent()
        {
            // Arrange
            _logDataManager.AddLog("Message 1", LogLevel.Info);
            _logDataManager.AddLog("Message 2", LogLevel.Warning);
            
            bool eventFired = false;
            _logDataManager.OnLogsCleared += () => eventFired = true;
            
            // Act
            _logDataManager.ClearLogs();
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(0, _logDataManager.GetLogCount());
            Assert.AreEqual(0, _logDataManager.GetLogs().Count);
        }
        
        [Test]
        public void GetLogs_ReturnsReadOnlyList_InChronologicalOrder()
        {
            // Arrange
            _logDataManager.AddLog("First", LogLevel.Info);
            Thread.Sleep(1); // Ensure different timestamps
            _logDataManager.AddLog("Second", LogLevel.Warning);
            Thread.Sleep(1);
            _logDataManager.AddLog("Third", LogLevel.Error);
            
            // Act
            var logs = _logDataManager.GetLogs();
            
            // Assert
            Assert.AreEqual(3, logs.Count);
            Assert.AreEqual("First", logs[0].message);
            Assert.AreEqual("Second", logs[1].message);
            Assert.AreEqual("Third", logs[2].message);
            
            // Verify timestamps are in order
            Assert.LessOrEqual(logs[0].timestamp, logs[1].timestamp);
            Assert.LessOrEqual(logs[1].timestamp, logs[2].timestamp);
        }
        
        [Test]
        public void SetMaxLogCount_WithValidValue_UpdatesLimit()
        {
            // Arrange
            _logDataManager.AddLog("Message 1", LogLevel.Info);
            _logDataManager.AddLog("Message 2", LogLevel.Info);
            _logDataManager.AddLog("Message 3", LogLevel.Info);
            
            // Act
            _logDataManager.SetMaxLogCount(2);
            
            // Assert
            Assert.AreEqual(2, _logDataManager.GetMaxLogCount());
            Assert.AreEqual(2, _logDataManager.GetLogCount());
            
            var logs = _logDataManager.GetLogs();
            Assert.AreEqual("Message 2", logs[0].message); // Oldest removed
            Assert.AreEqual("Message 3", logs[1].message);
        }
        
        [Test]
        public void SetMaxLogCount_WithInvalidValues_ClampsToValidRange()
        {
            // Act & Assert
            _logDataManager.SetMaxLogCount(0); // Below minimum
            Assert.AreEqual(1, _logDataManager.GetMaxLogCount()); // Clamped to minimum
            
            _logDataManager.SetMaxLogCount(-5); // Negative value
            Assert.AreEqual(1, _logDataManager.GetMaxLogCount()); // Clamped to minimum
            
            _logDataManager.SetMaxLogCount(1500); // Above maximum
            Assert.AreEqual(1000, _logDataManager.GetMaxLogCount()); // Clamped to maximum
        }
        
        [Test]
        public void GetFormattedLogs_ReturnsFormattedMessages()
        {
            // Arrange
            _logDataManager.AddLog("Test message", LogLevel.Info);
            _logDataManager.AddLog("Warning message", LogLevel.Warning);
            
            // Act
            var formattedLogs = _logDataManager.GetFormattedLogs();
            
            // Assert
            Assert.AreEqual(2, formattedLogs.Length);
            Assert.IsTrue(formattedLogs[0].Contains("[Info] Test message"));
            Assert.IsTrue(formattedLogs[1].Contains("[Warning] Warning message"));
            
            // Verify timestamp format
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(formattedLogs[0], @"\[\d{2}:\d{2}:\d{2}\]"));
        }
        
        [Test]
        public void IsAtCapacity_ReturnsCorrectStatus()
        {
            // Arrange
            int maxCount = _testConfig.maxLogCount;
            
            // Act & Assert - Not at capacity
            Assert.IsFalse(_logDataManager.IsAtCapacity());
            
            // Fill to capacity
            for (int i = 0; i < maxCount; i++)
            {
                _logDataManager.AddLog($"Message {i}", LogLevel.Info);
            }
            
            Assert.IsTrue(_logDataManager.IsAtCapacity());
        }
        
        [Test]
        public void GetEstimatedMemoryUsage_ReturnsReasonableEstimate()
        {
            // Arrange
            _logDataManager.AddLog("Short", LogLevel.Info);
            _logDataManager.AddLog("This is a much longer message for testing memory usage", LogLevel.Warning);
            
            // Act
            long memoryUsage = _logDataManager.GetEstimatedMemoryUsage();
            
            // Assert
            Assert.Greater(memoryUsage, 0);
            Assert.Less(memoryUsage, 10000); // Should be reasonable for test data
        }
        
        [Test]
        public void SetConfiguration_UpdatesMaxLogCount()
        {
            // Arrange
            var newConfig = ScriptableObject.CreateInstance<LogConfiguration>();
            newConfig.maxLogCount = 15;
            
            // Act
            _logDataManager.SetConfiguration(newConfig);
            
            // Assert
            Assert.AreEqual(15, _logDataManager.GetMaxLogCount());
            Assert.AreEqual(newConfig, _logDataManager.GetConfiguration());
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(newConfig);
        }
        
        [Test]
        public void ThreadSafety_ConcurrentAddAndRead_NoDataCorruption()
        {
            // Arrange
            const int threadCount = 5;
            const int messagesPerThread = 20;
            var tasks = new Task[threadCount];
            var exceptions = new List<Exception>();
            
            // Act - Multiple threads adding logs concurrently
            for (int t = 0; t < threadCount; t++)
            {
                int threadId = t;
                tasks[t] = Task.Run(() =>
                {
                    try
                    {
                        for (int i = 0; i < messagesPerThread; i++)
                        {
                            _logDataManager.AddLog($"Thread{threadId}_Message{i}", LogLevel.Info);
                            
                            // Occasionally read logs while writing
                            if (i % 5 == 0)
                            {
                                var logs = _logDataManager.GetLogs();
                                Assert.IsNotNull(logs);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }
            
            Task.WaitAll(tasks);
            
            // Assert
            Assert.AreEqual(0, exceptions.Count, $"Exceptions occurred: {string.Join(", ", exceptions)}");
            
            // Verify final state
            var finalLogs = _logDataManager.GetLogs();
            Assert.IsNotNull(finalLogs);
            Assert.LessOrEqual(finalLogs.Count, _logDataManager.GetMaxLogCount());
        }
        
        [Test]
        public void ThreadSafety_ConcurrentClearAndAdd_NoDeadlock()
        {
            // Arrange
            var clearTask = Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(10);
                    _logDataManager.ClearLogs();
                }
            });
            
            var addTask = Task.Run(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    _logDataManager.AddLog($"Message {i}", LogLevel.Info);
                    Thread.Sleep(5);
                }
            });
            
            // Act & Assert - Should complete without deadlock
            Assert.IsTrue(Task.WaitAll(new[] { clearTask, addTask }, TimeSpan.FromSeconds(10)));
        }
        
        [Test]
        public void MemoryEfficiency_LargeNumberOfLogs_StaysWithinBounds()
        {
            // Arrange
            const int testLogCount = 1000;
            var largeManager = new LogDataManager();
            largeManager.SetMaxLogCount(100); // Limit to 100 entries
            
            // Act - Add many logs
            for (int i = 0; i < testLogCount; i++)
            {
                largeManager.AddLog($"Large test message number {i} with some additional content to increase memory usage", LogLevel.Info);
            }
            
            // Assert
            Assert.AreEqual(100, largeManager.GetLogCount()); // Should not exceed limit
            
            long memoryUsage = largeManager.GetEstimatedMemoryUsage();
            Assert.Less(memoryUsage, 50000); // Should stay within reasonable bounds
        }
        
        [Test]
        public void GetFormattedDisplayText_WithNoLogs_ReturnsEmptyString()
        {
            // Arrange
            var manager = new LogDataManager();
            
            // Act
            var result = manager.GetFormattedDisplayText();
            
            // Assert
            Assert.That(result, Is.EqualTo(string.Empty),
                "GetFormattedDisplayText should return empty string when no logs exist");
        }
        
        [Test]
        public void GetFormattedDisplayText_WithConfiguration_ReturnsRichTextFormatted()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.infoColorHex = "#FFFFFF";
            config.warningColorHex = "#FFFF00";
            config.errorColorHex = "#FF0000";
            config.timestampFormat = "HH:mm:ss";
            
            var manager = new LogDataManager(config);
            manager.AddLog("Info message", LogLevel.Info);
            manager.AddLog("Warning message", LogLevel.Warning);
            manager.AddLog("Error message", LogLevel.Error);
            
            // Act
            var result = manager.GetFormattedDisplayText();
            
            // Assert
            Assert.That(result, Does.Contain("<color=#FFFFFF>"),
                "Should contain info color markup");
            Assert.That(result, Does.Contain("<color=#FFFF00>"),
                "Should contain warning color markup");
            Assert.That(result, Does.Contain("<color=#FF0000>"),
                "Should contain error color markup");
            Assert.That(result, Does.Contain("[Info] Info message"),
                "Should contain formatted info message");
            Assert.That(result, Does.Contain("[Warning] Warning message"),
                "Should contain formatted warning message");
            Assert.That(result, Does.Contain("[Error] Error message"),
                "Should contain formatted error message");
            Assert.That(result, Does.Contain("</color>"),
                "Should contain closing color tags");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(config);
        }
        
        [Test]
        public void GetFormattedDisplayText_WithoutConfiguration_ReturnsPlainFormatted()
        {
            // Arrange
            var manager = new LogDataManager();
            manager.AddLog("Test message", LogLevel.Info);
            manager.AddLog("Another message", LogLevel.Warning);
            
            // Act
            var result = manager.GetFormattedDisplayText();
            
            // Assert
            Assert.That(result, Does.Not.Contain("<color="),
                "Should not contain rich text markup without configuration");
            Assert.That(result, Does.Contain("[Info] Test message"),
                "Should contain formatted info message");
            Assert.That(result, Does.Contain("[Warning] Another message"),
                "Should contain formatted warning message");
            Assert.That(result, Does.Contain("\n"),
                "Should contain newline separators between messages");
        }
        
        [Test]
        public void GetFormattedDisplayText_WithMultipleLogs_JoinsWithNewlines()
        {
            // Arrange
            var manager = new LogDataManager();
            manager.AddLog("First message", LogLevel.Info);
            manager.AddLog("Second message", LogLevel.Warning);
            manager.AddLog("Third message", LogLevel.Error);
            
            // Act
            var result = manager.GetFormattedDisplayText();
            
            // Assert
            var lines = result.Split('\n');
            Assert.That(lines.Length, Is.EqualTo(3),
                "Should have three lines separated by newlines");
            Assert.That(lines[0], Does.Contain("First message"),
                "First line should contain first message");
            Assert.That(lines[1], Does.Contain("Second message"),
                "Second line should contain second message");
            Assert.That(lines[2], Does.Contain("Third message"),
                "Third line should contain third message");
        }
        
        [Test]
        public void GetFormattedDisplayText_WithCustomTimestampFormat_UsesConfiguredFormat()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.timestampFormat = "yyyy-MM-dd HH:mm:ss";
            config.infoColorHex = "#FFFFFF";
            
            var manager = new LogDataManager(config);
            manager.AddLog("Test message", LogLevel.Info);
            
            // Act
            var result = manager.GetFormattedDisplayText();
            
            // Assert
            Assert.That(result, Does.Match(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}"),
                "Should use custom timestamp format");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(config);
        }
        
        [Test]
        public void GetFormattedDisplayText_WithCircularBufferOverflow_ShowsOnlyRecentLogs()
        {
            // Arrange
            var config = ScriptableObject.CreateInstance<LogConfiguration>();
            config.maxLogCount = 2;
            
            var manager = new LogDataManager(config);
            manager.AddLog("Old message 1", LogLevel.Info);
            manager.AddLog("Old message 2", LogLevel.Info);
            manager.AddLog("New message 1", LogLevel.Info); // Should push out first message
            manager.AddLog("New message 2", LogLevel.Info); // Should push out second message
            
            // Act
            var result = manager.GetFormattedDisplayText();
            
            // Assert
            Assert.That(result, Does.Not.Contain("Old message"),
                "Should not contain old messages that were removed from buffer");
            Assert.That(result, Does.Contain("New message 1"),
                "Should contain first new message");
            Assert.That(result, Does.Contain("New message 2"),
                "Should contain second new message");
            
            var lines = result.Split('\n');
            Assert.That(lines.Length, Is.EqualTo(2),
                "Should only show the maximum configured number of log entries");
            
            // Cleanup
            UnityEngine.Object.DestroyImmediate(config);
        }
    }
}