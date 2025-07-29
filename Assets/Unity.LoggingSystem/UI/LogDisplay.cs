using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace RuntimeLogging
{
    /// <summary>
    /// LogDisplay component for TextMeshPro output
    /// Handles display control and efficient TextMeshPro updates for the logging system
    /// </summary>
    public class LogDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private int maxCharacterLimit = 8000; // Prevent excessive TextMeshPro content
        [SerializeField] private float updateThrottleTime = 0.1f; // Minimum time between updates (100ms)
        [SerializeField] private int maxLinesLimit = 100; // Maximum number of lines to display
        [SerializeField] private bool enableBatchUpdates = true; // Enable batching for performance
        [SerializeField] private float batchUpdateInterval = 0.05f; // Batch update interval (50ms)
        
        // StringBuilder pool for better memory management
        private readonly StringBuilderPool _stringBuilderPool = new StringBuilderPool();
        
        // Keep references to frequently used StringBuilders to avoid pool overhead in hot paths
        private StringBuilder textBuilder = new StringBuilder();
        private StringBuilder batchBuilder = new StringBuilder();
        private float lastUpdateTime;
        private float lastBatchTime;
        private bool pendingUpdate;
        private bool pendingBatchUpdate;
        private string pendingText;
        private Queue<string> batchQueue = new Queue<string>();
        private Coroutine batchUpdateCoroutine;
        
        private void Awake()
        {
            // Try to get TextMeshPro component if not assigned
            if (logText == null)
            {
                logText = GetComponent<TextMeshProUGUI>();
            }
            
            // LogDisplay now only handles text content - UI properties are configured in Unity Editor
        }
        
        private void Update()
        {
            // Handle throttled updates
            if (pendingUpdate && Time.unscaledTime - lastUpdateTime >= updateThrottleTime)
            {
                ApplyTextUpdate(pendingText);
                pendingUpdate = false;
            }
            
            // Handle batch updates for high-frequency scenarios
            if (enableBatchUpdates && pendingBatchUpdate && Time.unscaledTime - lastBatchTime >= batchUpdateInterval)
            {
                ProcessBatchUpdates();
            }
        }
        
        private void OnEnable()
        {
            // Start batch processing when component is enabled
            if (enableBatchUpdates && batchUpdateCoroutine == null)
            {
                batchUpdateCoroutine = StartCoroutine(BatchUpdateCoroutine());
            }
        }
        
        private void OnDisable()
        {
            // Stop batch processing when component is disabled
            if (batchUpdateCoroutine != null)
            {
                StopCoroutine(batchUpdateCoroutine);
                batchUpdateCoroutine = null;
            }
        }
        
        /// <summary>
        /// Show the log display
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Hide the log display
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Update the display with formatted text using efficient throttling
        /// </summary>
        /// <param name="formattedText">The formatted text to display</param>
        public void UpdateDisplay(string formattedText)
        {
            // Ensure TextMeshPro component is available
            if (logText == null)
            {
                logText = GetComponent<TextMeshProUGUI>();
            }
            
            if (logText == null)
            {
                Debug.LogWarning("LogDisplay: TextMeshProUGUI component is null. Cannot update display.", this);
                return;
            }
            
            string processedText = ProcessTextForDisplay(formattedText ?? string.Empty);
            
            // Use batch updates for high-frequency scenarios
            if (enableBatchUpdates && batchQueue.Count > 0)
            {
                QueueBatchUpdate(processedText);
                return;
            }
            
            // Use throttled updates to optimize performance
            if (Time.unscaledTime - lastUpdateTime >= updateThrottleTime)
            {
                ApplyTextUpdate(processedText);
            }
            else
            {
                // Queue update for later
                pendingText = processedText;
                pendingUpdate = true;
            }
        }
        
        /// <summary>
        /// Update display with single log entry using efficient batching
        /// </summary>
        /// <param name="logEntry">Single log entry to add</param>
        /// <param name="configuration">Log configuration for colors</param>
        public void UpdateDisplayWithSingleEntry(LogEntry logEntry, LogConfiguration configuration)
        {
            if (logText == null)
            {
                logText = GetComponent<TextMeshProUGUI>();
                if (logText == null)
                {
                    Debug.LogWarning("LogDisplay: TextMeshProUGUI component is null. Cannot update display.", this);
                    return;
                }
            }
            
            string formattedEntry = FormatLogEntryWithRichText(logEntry, configuration);
            
            if (enableBatchUpdates)
            {
                QueueBatchUpdate(formattedEntry);
            }
            else
            {
                // Append to current text for single entry updates
                string currentText = logText.text ?? string.Empty;
                string newText = string.IsNullOrEmpty(currentText) ? formattedEntry : currentText + "\n" + formattedEntry;
                UpdateDisplay(newText);
            }
        }
        
        /// <summary>
        /// Update display with rich text markup for log levels using efficient processing
        /// </summary>
        /// <param name="logEntries">List of log entries to display</param>
        /// <param name="configuration">Log configuration for colors</param>
        public void UpdateDisplayWithRichText(IEnumerable<LogEntry> logEntries, LogConfiguration configuration)
        {
            if (logEntries == null)
            {
                UpdateDisplay(string.Empty);
                return;
            }
            
            textBuilder.Clear();
            int lineCount = 0;
            
            foreach (var entry in logEntries)
            {
                if (lineCount >= maxLinesLimit)
                    break;
                    
                string formattedEntry = FormatLogEntryWithRichText(entry, configuration);
                
                if (lineCount > 0)
                    textBuilder.AppendLine();
                textBuilder.Append(formattedEntry);
                
                lineCount++;
            }
            
            // Apply text directly without additional line limiting since we already limited during processing
            string finalText = textBuilder.ToString();
            if (finalText.Length > maxCharacterLimit)
            {
                finalText = TruncateTextEfficiently(finalText);
            }
            
            ApplyTextUpdateDirectly(finalText);
        }
        
        /// <summary>
        /// Update display with optimized rich text processing for better performance
        /// </summary>
        /// <param name="logEntries">List of log entries to display</param>
        /// <param name="configuration">Log configuration for colors</param>
        /// <param name="maxEntries">Maximum number of entries to process</param>
        public void UpdateDisplayWithOptimizedRichText(IEnumerable<LogEntry> logEntries, LogConfiguration configuration, int maxEntries = -1)
        {
            if (logEntries == null)
            {
                UpdateDisplay(string.Empty);
                return;
            }
            
            if (maxEntries <= 0)
                maxEntries = maxLinesLimit;
            
            textBuilder.Clear();
            int processedCount = 0;
            
            // Pre-calculate color hex strings for efficiency
            string infoColorHex = configuration?.GetInfoColorHex() ?? "#FFFFFF";
            string warningColorHex = configuration?.GetWarningColorHex() ?? "#FFFF00";
            string errorColorHex = configuration?.GetErrorColorHex() ?? "#FF0000";
            string timestampFormat = configuration?.timestampFormat ?? "HH:mm:ss";
            
            foreach (var entry in logEntries)
            {
                if (processedCount >= maxEntries)
                    break;
                    
                string formattedEntry = FormatLogEntryWithOptimizedRichText(entry, infoColorHex, warningColorHex, errorColorHex, timestampFormat);
                
                if (processedCount > 0)
                    textBuilder.AppendLine();
                textBuilder.Append(formattedEntry);
                
                processedCount++;
            }
            
            // Apply text directly without additional line limiting since we already limited during processing
            string finalText = textBuilder.ToString();
            if (finalText.Length > maxCharacterLimit)
            {
                finalText = TruncateTextEfficiently(finalText);
            }
            
            ApplyTextUpdateDirectly(finalText);
        }
        
        /// <summary>
        /// Clear the display
        /// </summary>
        public void ClearDisplay()
        {
            if (logText == null)
            {
                logText = GetComponent<TextMeshProUGUI>();
            }
            
            if (logText == null)
            {
                Debug.LogWarning("LogDisplay: TextMeshProUGUI component is null. Cannot clear display.", this);
                return;
            }
            
            textBuilder.Clear();
            pendingUpdate = false;
            ApplyTextUpdate(string.Empty);
        }
        
        /// <summary>
        /// Set the TextMeshPro component for runtime assignment
        /// </summary>
        /// <param name="textComponent">The TextMeshProUGUI component to use</param>
        public void SetTextComponent(TextMeshProUGUI textComponent)
        {
            logText = textComponent;
        }
        
        /// <summary>
        /// Get the current TextMeshPro component
        /// </summary>
        public TextMeshProUGUI GetTextComponent()
        {
            return logText;
        }
        
        /// <summary>
        /// Check if the TextMeshPro component is properly assigned
        /// </summary>
        public bool IsTextComponentValid()
        {
            return logText != null;
        }
        
        /// <summary>
        /// Set the maximum character limit for TextMeshPro content
        /// </summary>
        public void SetMaxCharacterLimit(int limit)
        {
            maxCharacterLimit = Mathf.Max(1000, limit); // Minimum 1000 characters
        }
        
        /// <summary>
        /// Set the maximum character limit for testing (allows smaller limits)
        /// </summary>
        public void SetMaxCharacterLimitForTesting(int limit)
        {
            maxCharacterLimit = Mathf.Max(50, limit); // Minimum 50 characters for testing
        }
        
        /// <summary>
        /// Set the maximum lines limit for display optimization
        /// </summary>
        public void SetMaxLinesLimit(int limit)
        {
            maxLinesLimit = Mathf.Max(10, limit); // Minimum 10 lines
        }
        
        /// <summary>
        /// Set the maximum lines limit for testing (allows smaller limits)
        /// </summary>
        public void SetMaxLinesLimitForTesting(int limit)
        {
            maxLinesLimit = Mathf.Max(1, limit); // Minimum 1 line for testing
        }
        
        /// <summary>
        /// Set the update throttle time for performance optimization
        /// </summary>
        public void SetUpdateThrottleTime(float throttleTime)
        {
            updateThrottleTime = Mathf.Max(0.05f, throttleTime); // Minimum 50ms
        }
        
        /// <summary>
        /// Enable or disable batch updates for high-frequency scenarios
        /// </summary>
        public void SetBatchUpdatesEnabled(bool enabled)
        {
            enableBatchUpdates = enabled;
            
            if (enabled && gameObject.activeInHierarchy && batchUpdateCoroutine == null)
            {
                batchUpdateCoroutine = StartCoroutine(BatchUpdateCoroutine());
            }
            else if (!enabled && batchUpdateCoroutine != null)
            {
                StopCoroutine(batchUpdateCoroutine);
                batchUpdateCoroutine = null;
            }
        }
        
        /// <summary>
        /// Set the batch update interval for performance tuning
        /// </summary>
        public void SetBatchUpdateInterval(float interval)
        {
            batchUpdateInterval = Mathf.Max(0.01f, interval); // Minimum 10ms
        }
        
        /// <summary>
        /// Force immediate update without throttling (for testing)
        /// </summary>
        public void ForceImmediateUpdate()
        {
            if (pendingUpdate)
            {
                ApplyTextUpdate(pendingText);
                pendingUpdate = false;
            }
            
            if (pendingBatchUpdate)
            {
                ProcessBatchUpdates();
            }
        }
        
        /// <summary>
        /// Get performance statistics for monitoring
        /// </summary>
        public LogDisplayPerformanceStats GetPerformanceStats()
        {
            return new LogDisplayPerformanceStats
            {
                PendingUpdates = pendingUpdate ? 1 : 0,
                PendingBatchUpdates = batchQueue.Count,
                LastUpdateTime = lastUpdateTime,
                LastBatchTime = lastBatchTime,
                CurrentCharacterCount = logText?.text?.Length ?? 0,
                BatchUpdatesEnabled = enableBatchUpdates
            };
        }
        
        private void ApplyTextUpdate(string text)
        {
            if (logText != null)
            {
                logText.text = text;
                lastUpdateTime = Time.unscaledTime;
            }
            else
            {
                Debug.LogWarning("LogDisplay: TextMeshProUGUI component is null. Cannot update text.", this);
            }
        }
        
        private void ApplyTextUpdateDirectly(string text)
        {
            if (logText != null)
            {
                logText.text = text;
                lastUpdateTime = Time.unscaledTime;
            }
        }
        
        private string ProcessTextForDisplay(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            
            // Manage text length to prevent excessive TextMeshPro content
            if (text.Length > maxCharacterLimit)
            {
                text = TruncateTextEfficiently(text);
            }
            
            // Manage line count to prevent excessive TextMeshPro content
            text = LimitLineCount(text, maxLinesLimit);
            
            return text;
        }
        
        private string TruncateTextEfficiently(string text)
        {
            // Truncate from the beginning, keeping the most recent content
            int startIndex = text.Length - maxCharacterLimit;
            
            // Try to find a newline to avoid cutting mid-line
            int newlineIndex = text.IndexOf('\n', startIndex);
            if (newlineIndex != -1 && newlineIndex < startIndex + 200)
            {
                startIndex = newlineIndex + 1;
            }
            
            return text.Substring(startIndex);
        }
        
        private string LimitLineCount(string text, int maxLines)
        {
            if (string.IsNullOrEmpty(text) || maxLines <= 0)
                return text;
            
            string[] lines = text.Split('\n');
            if (lines.Length <= maxLines)
                return text;
            
            // Keep the most recent lines using pooled StringBuilder
            return _stringBuilderPool.WithStringBuilder(sb =>
            {
                int startIndex = lines.Length - maxLines;
                
                for (int i = startIndex; i < lines.Length; i++)
                {
                    if (i > startIndex)
                        sb.AppendLine();
                    sb.Append(lines[i]);
                }
                
                return sb.ToString();
            });
        }
        
        private string FormatLogEntryWithRichText(LogEntry entry, LogConfiguration configuration)
        {
            if (configuration == null)
            {
                return entry.GetFormattedMessage("HH:mm:ss");
            }
            
            return entry.GetRichTextMessage(configuration);
        }
        
        private string FormatLogEntryWithOptimizedRichText(LogEntry entry, string infoColorHex, string warningColorHex, string errorColorHex, string timestampFormat)
        {
            return entry.GetRichTextMessage(infoColorHex, warningColorHex, errorColorHex, timestampFormat);
        }
        
        private void QueueBatchUpdate(string text)
        {
            batchQueue.Enqueue(text);
            pendingBatchUpdate = true;
        }
        
        private void ProcessBatchUpdates()
        {
            if (batchQueue.Count == 0)
            {
                pendingBatchUpdate = false;
                return;
            }
            
            batchBuilder.Clear();
            string currentText = logText?.text ?? string.Empty;
            
            if (!string.IsNullOrEmpty(currentText))
            {
                batchBuilder.Append(currentText);
            }
            
            // Process all queued updates
            while (batchQueue.Count > 0)
            {
                string queuedText = batchQueue.Dequeue();
                if (!string.IsNullOrEmpty(queuedText))
                {
                    if (batchBuilder.Length > 0)
                        batchBuilder.AppendLine();
                    batchBuilder.Append(queuedText);
                }
            }
            
            string finalText = ProcessTextForDisplay(batchBuilder.ToString());
            ApplyTextUpdate(finalText);
            
            pendingBatchUpdate = false;
            lastBatchTime = Time.unscaledTime;
        }
        
        private IEnumerator BatchUpdateCoroutine()
        {
            while (enableBatchUpdates)
            {
                yield return new WaitForSeconds(batchUpdateInterval);
                
                if (pendingBatchUpdate && batchQueue.Count > 0)
                {
                    ProcessBatchUpdates();
                }
            }
        }
        
        
        
        
    }
    
    /// <summary>
    /// Performance statistics for LogDisplay monitoring
    /// </summary>
    public struct LogDisplayPerformanceStats
    {
        public int PendingUpdates;
        public int PendingBatchUpdates;
        public float LastUpdateTime;
        public float LastBatchTime;
        public int CurrentCharacterCount;
        public bool BatchUpdatesEnabled;
    }
    
    /// <summary>
    /// Simple StringBuilder pool for memory optimization
    /// </summary>
    internal class StringBuilderPool
    {
        private readonly Stack<StringBuilder> _pool = new Stack<StringBuilder>();
        private readonly object _lock = new object();
        private const int MaxPoolSize = 5;
        private const int MaxCapacityBeforeReset = 16384; // 16KB
        
        /// <summary>
        /// Gets a StringBuilder from the pool or creates a new one
        /// </summary>
        public StringBuilder Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    var sb = _pool.Pop();
                    sb.Clear(); // Clear content but keep capacity
                    return sb;
                }
                return new StringBuilder(1024); // Start with reasonable capacity
            }
        }
        
        /// <summary>
        /// Returns a StringBuilder to the pool
        /// </summary>
        public void Return(StringBuilder sb)
        {
            if (sb == null) return;
            
            lock (_lock)
            {
                if (_pool.Count < MaxPoolSize)
                {
                    // Reset capacity if it's grown too large
                    if (sb.Capacity > MaxCapacityBeforeReset)
                    {
                        sb.Clear();
                        sb.Capacity = 1024; // Reset to reasonable size
                    }
                    else
                    {
                        sb.Clear(); // Just clear content
                    }
                    
                    _pool.Push(sb);
                }
                // If pool is full, let GC handle it
            }
        }
        
        /// <summary>
        /// Executes an action with a pooled StringBuilder and automatically returns it
        /// </summary>
        public T WithStringBuilder<T>(System.Func<StringBuilder, T> action)
        {
            var sb = Get();
            try
            {
                return action(sb);
            }
            finally
            {
                Return(sb);
            }
        }
        
        /// <summary>
        /// Executes an action with a pooled StringBuilder and automatically returns it
        /// </summary>
        public void WithStringBuilder(System.Action<StringBuilder> action)
        {
            var sb = Get();
            try
            {
                action(sb);
            }
            finally
            {
                Return(sb);
            }
        }
    }
}