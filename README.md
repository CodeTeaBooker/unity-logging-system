# Unity Logging System

A comprehensive, high-performance logging system for Unity applications with runtime display capabilities, cross-platform support, and advanced performance monitoring.

[![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Cross--Platform-lightgrey.svg)]()

## 🚀 Features

### Core Logging Capabilities
- **Unified Logging Interface**: Consistent `ILogger` interface across all logging implementations
- **Multiple Logger Types**: Unity Console, On-Screen Display, and Composite loggers
- **Real-time Log Display**: In-game log panel with TextMeshPro integration
- **Thread-Safe Operations**: Safe concurrent access from multiple threads
- **Performance Optimized**: Built-in object pooling and memory management

### Advanced Features
- **Cross-Platform Compatibility**: Validated support across all Unity platforms
- **Performance Monitoring**: Real-time performance statistics and memory usage tracking
- **Configurable Display**: Customizable colors, timestamps, and log count limits
- **Modular Architecture**: Easy integration and extensibility
- **Comprehensive Testing**: Extensive test coverage with validation framework

### UI Integration
- **TextMeshPro Support**: High-quality text rendering for log displays
- **Runtime Controls**: Show/hide, clear logs, and real-time configuration
- **Visual Feedback**: Color-coded log levels (Info, Warning, Error)
- **Performance Dashboard**: Built-in performance monitoring display

## 📦 Installation

### Method 1: Unity Package Manager (Recommended)
1. Open Unity Package Manager
2. Click "Add package from git URL"
3. Enter: `https://github.com/yourusername/unity-logging-system.git`

### Method 2: Download .unitypackage
1. Download the latest `.unitypackage` from [Releases](https://github.com/CodeTeaBooker/unity-logging-system/releases)
2. Import into your Unity project via `Assets > Import Package > Custom Package`

### Method 3: Manual Installation
1. Clone or download this repository
2. Copy the `Assets/Unity.LoggingSystem` folder to your Unity project's Assets folder

## 🔧 Quick Start

### Basic Setup

```csharp
using RuntimeLogging;

// Set up a basic Unity console logger
var unityLogger = new UnityLogger();
LogManager.SetLogger(unityLogger);

// Start logging
LogManager.Log("Hello, Unity Logging System!");
LogManager.LogWarning("This is a warning");
LogManager.LogError("This is an error");
```

### On-Screen Display Setup

```csharp
// Add ScreenLogger component to a GameObject
var screenLogger = gameObject.AddComponent<ScreenLogger>();

// Set as the global logger
LogManager.SetLogger(screenLogger);

// Configure display settings
screenLogger.UpdateMaxLogCount(100);
screenLogger.UpdateLogLevelColor(LogLevel.Info, "#00FF00");
```

### Composite Logger (Multiple Outputs)

```csharp
// Create multiple loggers
var unityLogger = new UnityLogger();
var screenLogger = GetComponent<ScreenLogger>();

// Combine them
var compositeLogger = new CompositeLogger(unityLogger, screenLogger);
LogManager.SetLogger(compositeLogger);

// Now logs appear in both Unity Console and on-screen
LogManager.Log("This appears in both outputs!");
```

## 📚 Core Components

### ILogger Interface
The foundation of the logging system providing consistent logging methods:
- `Log(string message)` - Information level logging
- `LogWarning(string message)` - Warning level logging  
- `LogError(string message)` - Error level logging

### LogManager
Central logger management with thread-safe operations:
- `SetLogger(ILogger logger)` - Set the global logger
- `GetLogger()` - Get the current logger
- `Log/LogWarning/LogError(string message)` - Convenience methods

### Logger Implementations

#### UnityLogger
Logs directly to Unity's console system.

#### ScreenLogger  
MonoBehaviour-based logger with on-screen display:
- TextMeshPro integration for high-quality text rendering
- Configurable colors and display settings
- Performance monitoring and optimization
- Cross-platform compatibility validation

#### CompositeLogger
Combines multiple loggers for simultaneous output to different targets.

## ⚙️ Configuration

### LogConfiguration
Centralized configuration management:

```csharp
var config = LogConfiguration.CreateDefault();
config.maxLogCount = 200;
config.timestampFormat = "HH:mm:ss";
config.infoColor = "#FFFFFF";
config.warningColor = "#FFFF00"; 
config.errorColor = "#FF0000";

screenLogger.SetConfiguration(config);
```

### Runtime Configuration

```csharp
// Update settings during runtime
screenLogger.UpdateMaxLogCount(150);
screenLogger.UpdateTimestampFormat("mm:ss");
screenLogger.UpdateLogLevelColor(LogLevel.Warning, "#FFA500");
```

## 📊 Performance Monitoring

### Built-in Performance Stats

```csharp
// Get performance statistics
var stats = screenLogger.GetPerformanceStats();
Debug.Log($"Current logs: {stats.CurrentLogCount}");
Debug.Log($"Memory usage: {stats.EstimatedMemoryUsage} bytes");
Debug.Log($"Initialized: {stats.IsInitialized}");
```

### Unified Performance Monitor

```csharp
// Register for performance monitoring
UnifiedPerformanceMonitor.RegisterStats(performanceStats);

// Listen for updates
UnifiedPerformanceMonitor.OnPerformanceStatsUpdated += (name, stats) => {
    Debug.Log($"Component {name} updated: {stats.EstimatedMemoryUsage} bytes");
};
```

## 🧪 Testing & Validation

The system includes comprehensive validation tests:

```csharp
// Run all validation tests
demoController.RunAllValidationTests();

// Individual requirement validation
bool realTimeLogging = demoController.ValidateRequirement1_RealTimeLogging();
bool displayControl = demoController.ValidateRequirement2_LogDisplayControl();
bool performance = demoController.ValidateRequirement5_Performance();
```

## 🎮 Demo Scene

The included demo scene demonstrates all features:

1. **Logger Type Selection**: Switch between Unity, Screen, and Composite loggers
2. **Real-time Controls**: Show/hide display, clear logs, configuration changes
3. **Performance Testing**: High-volume logging and performance validation
4. **Cross-platform Testing**: Platform compatibility verification
5. **Business Logic Integration**: Example of logging in game systems

### Running the Demo

1. Open the `DemoScene` in the Examples folder
2. Play the scene
3. Use the UI buttons to test different logger configurations
4. Monitor the on-screen display and Unity console

## 🔧 Advanced Usage

### Custom Logger Implementation

```csharp
public class FileLogger : ILogger
{
    private string logFilePath;
    
    public FileLogger(string filePath)
    {
        logFilePath = filePath;
    }
    
    public void Log(string message)
    {
        File.AppendAllText(logFilePath, $"[INFO] {DateTime.Now}: {message}\n");
    }
    
    public void LogWarning(string message)
    {
        File.AppendAllText(logFilePath, $"[WARNING] {DateTime.Now}: {message}\n");
    }
    
    public void LogError(string message)
    {
        File.AppendAllText(logFilePath, $"[ERROR] {DateTime.Now}: {message}\n");
    }
}

// Use custom logger
var fileLogger = new FileLogger("game.log");
var compositeLogger = new CompositeLogger(
    new UnityLogger(),
    GetComponent<ScreenLogger>(),
    fileLogger
);
LogManager.SetLogger(compositeLogger);
```

### Memory Optimization

```csharp
// Configure memory settings
screenLogger.UpdateMaxLogCount(50); // Limit log history
logConfiguration.enableLogEntryPooling = true; // Enable object pooling

// Monitor memory usage
var memoryMonitor = new MemoryMonitor();
memoryMonitor.OnMemoryThresholdExceeded += (usage) => {
    LogManager.LogWarning($"Memory usage high: {usage} MB");
    screenLogger.Clear(); // Clear logs to free memory
};
```

## 🌐 Platform Compatibility

Tested and validated on:
- ✅ Windows (Standalone)
- ✅ macOS (Standalone) 
- ✅ Linux (Standalone)
- ✅ iOS
- ✅ Android

### Cross-Platform Validation

```csharp
// Validate platform compatibility
var validator = new CrossPlatformLoggerValidator();
var result = screenLogger.ValidateCrossPlatformCompatibility();

if (result.CompatibilityReport.IsUnitySupportedVersion)
{
    LogManager.Log("Platform compatibility validated");
}
else
{
    LogManager.LogWarning("Platform compatibility issues detected");
}
```

## 📁 Project Structure

```
Assets/Unity.LoggingSystem/
├── Core/                    # Core interfaces and data structures
│   ├── ILogger.cs
│   ├── LogEntry.cs
│   ├── LogLevel.cs
│   └── LogConstants.cs
├── Loggers/                 # Logger implementations
│   ├── UnityLogger.cs
│   ├── ScreenLogger.cs
│   └── CompositeLogger.cs
├── Management/              # Configuration and management
│   ├── LogManager.cs
│   ├── LogConfiguration.cs
│   └── LogConfigurationManager.cs
├── Data/                    # Data management and pooling
│   ├── LogDataManager.cs
│   └── LogEntryPool.cs
├── UI/                      # User interface components
│   ├── LogDisplay.cs
│   ├── LogUIController.cs
│   └── TextMeshProOptimizer.cs
├── Performance/             # Performance monitoring
│   ├── UnifiedPerformanceMonitor.cs
│   ├── MemoryMonitor.cs
│   └── LoggingPerformanceProfiler.cs
├── Platform/                # Cross-platform compatibility
│   ├── PlatformCompatibility.cs
│   └── CrossPlatformLoggerValidator.cs
├── Examples/                # Demo scene and example code
│   ├── DemoSceneController.cs
│   ├── DemoBusinessLogic.cs
│   └── ComprehensiveValidationTests.cs
├── Tests/                   # Unit and integration tests
└── Editor/                  # Unity Editor extensions
```

## Contributing

Issues and Pull Requests are welcome.


## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ by CodeTeaBooker**
