using System;
using System.Text;
using UnityEngine;
using TMPro;

namespace RuntimeLogging
{
    /// <summary>
    /// Optimizes TextMeshPro text updates to maintain smooth frame rates during log bursts
    /// Implements text content truncation strategies and performance optimizations
    /// </summary>
    public class TextMeshProOptimizer
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly StringBuilder _truncationBuilder = new StringBuilder();
        
        // Performance settings
        private int _maxCharacterLimit = 8000;
        private int _maxLineLimit = 100;
        private float _targetFrameTime = 0.016f; // 60 FPS target (16.67ms per frame)
        private int _maxProcessingTimeMs = 5; // Maximum 5ms per frame for text processing
        
        // Truncation strategies
        private TruncationStrategy _truncationStrategy = TruncationStrategy.RemoveOldest;
        private float _truncationRatio = 0.75f; // Keep 75% of content when truncating
        
        // Performance tracking
        private float _lastProcessingTime = 0f;
        private int _truncationCount = 0;
        private int _optimizationCount = 0;
        
        /// <summary>
        /// Optimizes text for TextMeshPro display with performance constraints
        /// </summary>
        /// <param name="text">Input text to optimize</param>
        /// <param name="targetComponent">Target TextMeshPro component for optimization hints</param>
        /// <returns>Optimized text ready for TextMeshPro display</returns>
        public string OptimizeTextForDisplay(string text, TextMeshProUGUI targetComponent = null)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
                
            var startTime = GetThreadSafeTime();
            
            try
            {
                string optimizedText = text;
                
                // Apply character limit optimization
                if (optimizedText.Length > _maxCharacterLimit)
                {
                    optimizedText = TruncateByCharacterLimit(optimizedText);
                    _truncationCount++;
                }
                
                // Apply line limit optimization
                optimizedText = TruncateByLineLimit(optimizedText);
                
                // Apply TextMeshPro-specific optimizations
                if (targetComponent != null)
                {
                    optimizedText = ApplyTextMeshProOptimizations(optimizedText, targetComponent);
                }
                
                _optimizationCount++;
                return optimizedText;
            }
            finally
            {
                _lastProcessingTime = (GetThreadSafeTime() - startTime) * 1000f; // Convert to milliseconds
            }
        }
        
        /// <summary>
        /// Optimizes text incrementally to spread processing across multiple frames
        /// </summary>
        /// <param name="text">Input text to optimize</param>
        /// <param name="maxProcessingTimeMs">Maximum processing time per call in milliseconds</param>
        /// <returns>Optimization result with processed text and completion status</returns>
        public TextOptimizationResult OptimizeTextIncremental(string text, int maxProcessingTimeMs = 5)
        {
            if (string.IsNullOrEmpty(text))
                return new TextOptimizationResult { OptimizedText = string.Empty, IsComplete = true };
                
            var startTime = GetThreadSafeTime();
            var maxTimeSeconds = maxProcessingTimeMs / 1000f;
            
            string optimizedText = text;
            bool isComplete = true;
            
            // Character limit truncation (fast operation)
            if (optimizedText.Length > _maxCharacterLimit)
            {
                optimizedText = TruncateByCharacterLimit(optimizedText);
                _truncationCount++;
                
                // Check if we've exceeded time budget
                if ((Time.realtimeSinceStartup - startTime) > maxTimeSeconds)
                {
                    isComplete = false;
                    return new TextOptimizationResult { OptimizedText = optimizedText, IsComplete = isComplete };
                }
            }
            
            // Line limit truncation (potentially slower operation)
            optimizedText = TruncateByLineLimit(optimizedText);
            
            // Check if we've exceeded time budget
            if ((GetThreadSafeTime() - startTime) > maxTimeSeconds)
            {
                isComplete = false;
            }
            
            _optimizationCount++;
            _lastProcessingTime = (GetThreadSafeTime() - startTime) * 1000f;
            
            return new TextOptimizationResult { OptimizedText = optimizedText, IsComplete = isComplete };
        }
        
        /// <summary>
        /// Sets the maximum character limit for text optimization
        /// </summary>
        public void SetMaxCharacterLimit(int limit)
        {
            _maxCharacterLimit = Mathf.Max(10, limit); // Minimum 10 characters for testing
        }
        
        /// <summary>
        /// Sets the maximum line limit for text optimization
        /// </summary>
        public void SetMaxLineLimit(int limit)
        {
            _maxLineLimit = Mathf.Max(1, limit); // Minimum 1 line for testing
        }
        
        /// <summary>
        /// Sets the truncation strategy for text optimization
        /// </summary>
        public void SetTruncationStrategy(TruncationStrategy strategy)
        {
            _truncationStrategy = strategy;
        }
        
        /// <summary>
        /// Sets the truncation ratio (how much content to keep when truncating)
        /// </summary>
        public void SetTruncationRatio(float ratio)
        {
            _truncationRatio = Mathf.Clamp01(ratio);
        }
        
        /// <summary>
        /// Sets the target frame time for performance optimization
        /// </summary>
        public void SetTargetFrameTime(float frameTimeSeconds)
        {
            _targetFrameTime = Mathf.Max(0.001f, frameTimeSeconds);
            _maxProcessingTimeMs = Mathf.RoundToInt(_targetFrameTime * 1000f * 0.3f); // Use 30% of frame time
        }
        
        /// <summary>
        /// Gets performance statistics for the optimizer
        /// </summary>
        public TextOptimizerStats GetStats()
        {
            return new TextOptimizerStats
            {
                MaxCharacterLimit = _maxCharacterLimit,
                MaxLineLimit = _maxLineLimit,
                TruncationCount = _truncationCount,
                OptimizationCount = _optimizationCount,
                LastProcessingTimeMs = _lastProcessingTime,
                TruncationStrategy = _truncationStrategy,
                TruncationRatio = _truncationRatio,
                TargetFrameTimeMs = _targetFrameTime * 1000f,
                MaxProcessingTimeMs = _maxProcessingTimeMs
            };
        }
        
        /// <summary>
        /// Resets performance statistics
        /// </summary>
        public void ResetStats()
        {
            _truncationCount = 0;
            _optimizationCount = 0;
            _lastProcessingTime = 0f;
        }
        
        /// <summary>
        /// Gets thread-safe time that works in both main thread and background threads
        /// </summary>
        private float GetThreadSafeTime()
        {
            try
            {
                // Try to use Unity's time if we're on the main thread
                return Time.realtimeSinceStartup;
            }
            catch (UnityException)
            {
                // Fall back to DateTime-based timing for background threads
                return (float)(DateTime.UtcNow - DateTime.MinValue).TotalSeconds;
            }
        }
        
        private string TruncateByCharacterLimit(string text)
        {
            if (text.Length <= _maxCharacterLimit)
                return text;
                
            int targetLength = Mathf.RoundToInt(_maxCharacterLimit * _truncationRatio);
            
            switch (_truncationStrategy)
            {
                case TruncationStrategy.RemoveOldest:
                    return TruncateFromBeginning(text, targetLength);
                    
                case TruncationStrategy.RemoveNewest:
                    return TruncateFromEnd(text, targetLength);
                    
                case TruncationStrategy.RemoveMiddle:
                    return TruncateFromMiddle(text, targetLength);
                    
                default:
                    return TruncateFromBeginning(text, targetLength);
            }
        }
        
        private string TruncateByLineLimit(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
                
            // Fast line count check without splitting the string
            int lineCount = CountLines(text);
            if (lineCount <= _maxLineLimit)
                return text;
                
            // Only split when truncation is actually needed
            string[] lines = text.Split('\n');
            int targetLines = Mathf.RoundToInt(_maxLineLimit * _truncationRatio);
            
            switch (_truncationStrategy)
            {
                case TruncationStrategy.RemoveOldest:
                    return JoinLines(lines, lines.Length - targetLines, lines.Length);
                    
                case TruncationStrategy.RemoveNewest:
                    return JoinLines(lines, 0, targetLines);
                    
                case TruncationStrategy.RemoveMiddle:
                    int keepStart = targetLines / 2;
                    int keepEnd = targetLines - keepStart;
                    _stringBuilder.Clear();
                    
                    // Keep first part
                    for (int i = 0; i < keepStart; i++)
                    {
                        if (i > 0) _stringBuilder.AppendLine();
                        _stringBuilder.Append(lines[i]);
                    }
                    
                    // Add truncation indicator
                    if (keepStart > 0 && keepEnd > 0)
                    {
                        _stringBuilder.AppendLine();
                        _stringBuilder.Append("... [truncated] ...");
                    }
                    
                    // Keep last part
                    int startIndex = lines.Length - keepEnd;
                    for (int i = startIndex; i < lines.Length; i++)
                    {
                        _stringBuilder.AppendLine();
                        _stringBuilder.Append(lines[i]);
                    }
                    
                    return _stringBuilder.ToString();
                    
                default:
                    return JoinLines(lines, lines.Length - targetLines, lines.Length);
            }
        }
        
        private string TruncateFromBeginning(string text, int targetLength)
        {
            int startIndex = text.Length - targetLength;
            
            // Try to find a newline to avoid cutting mid-line
            int newlineIndex = text.IndexOf('\n', startIndex);
            if (newlineIndex != -1 && newlineIndex < startIndex + 200)
            {
                startIndex = newlineIndex + 1;
            }
            
            return text.Substring(startIndex);
        }
        
        private string TruncateFromEnd(string text, int targetLength)
        {
            if (targetLength >= text.Length)
                return text;
                
            // Try to find a newline to avoid cutting mid-line
            int endIndex = targetLength;
            int newlineIndex = text.LastIndexOf('\n', endIndex);
            if (newlineIndex != -1 && newlineIndex > endIndex - 200)
            {
                endIndex = newlineIndex;
            }
            
            return text.Substring(0, endIndex);
        }
        
        private string TruncateFromMiddle(string text, int targetLength)
        {
            if (targetLength >= text.Length)
                return text;
                
            int keepStart = targetLength / 2;
            int keepEnd = targetLength - keepStart;
            
            string startPart = text.Substring(0, keepStart);
            string endPart = text.Substring(text.Length - keepEnd);
            
            return startPart + "\n... [truncated] ...\n" + endPart;
        }
        
        private string JoinLines(string[] lines, int startIndex, int endIndex)
        {
            _stringBuilder.Clear();
            
            for (int i = startIndex; i < endIndex && i < lines.Length; i++)
            {
                if (i > startIndex)
                    _stringBuilder.AppendLine();
                _stringBuilder.Append(lines[i]);
            }
            
            return _stringBuilder.ToString();
        }
        
        /// <summary>
        /// Fast line counting without string allocation - counts newline characters
        /// </summary>
        private int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
                
            int count = 1; // Start with 1 line (text without newlines = 1 line)
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                    count++;
            }
            return count;
        }
        
        private string ApplyTextMeshProOptimizations(string text, TextMeshProUGUI component)
        {
            // Apply component-specific optimizations based on TextMeshPro settings
            if (component.textWrappingMode != TextWrappingModes.NoWrap && text.Length > _maxCharacterLimit * 0.8f)
            {
                // Reduce character limit for word-wrapped text to maintain performance
                return TruncateByCharacterLimit(text);
            }
            
            // Additional TextMeshPro-specific optimizations can be added here
            return text;
        }
    }
    
    /// <summary>
    /// Truncation strategies for text optimization
    /// </summary>
    public enum TruncationStrategy
    {
        RemoveOldest,   // Remove oldest content (keep newest)
        RemoveNewest,   // Remove newest content (keep oldest)
        RemoveMiddle    // Remove middle content (keep start and end)
    }
    
    /// <summary>
    /// Result of incremental text optimization
    /// </summary>
    public struct TextOptimizationResult
    {
        public string OptimizedText;
        public bool IsComplete;
    }
    
    /// <summary>
    /// Performance statistics for TextMeshPro optimizer
    /// </summary>
    public struct TextOptimizerStats
    {
        public int MaxCharacterLimit;
        public int MaxLineLimit;
        public int TruncationCount;
        public int OptimizationCount;
        public float LastProcessingTimeMs;
        public TruncationStrategy TruncationStrategy;
        public float TruncationRatio;
        public float TargetFrameTimeMs;
        public int MaxProcessingTimeMs;
        
        public override string ToString()
        {
            return $"Optimizer: {OptimizationCount} optimizations, {TruncationCount} truncations, " +
                   $"Last: {LastProcessingTimeMs:F2}ms, Limits: {MaxCharacterLimit} chars / {MaxLineLimit} lines";
        }
    }
}