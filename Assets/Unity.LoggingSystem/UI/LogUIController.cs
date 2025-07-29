using UnityEngine;
using System;

namespace RuntimeLogging
{
    /// <summary>
    /// Dedicated UI controller for managing log display interface
    /// Separates UI concerns from logging logic in ScreenLogger
    /// </summary>
    public class LogUIController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private LogDisplay logDisplay;
        
        [Header("UI Settings")]
        [SerializeField] private bool autoCreateLogDisplay = true;
        [SerializeField] private bool hideOnAwake = false;
        
        private LogConfiguration uiConfiguration;
        private bool isUIInitialized = false;
        
        /// <summary>
        /// Event fired when UI state changes
        /// </summary>
        public event Action<bool> OnUIStateChanged;
        
        /// <summary>
        /// Gets the log display component
        /// </summary>
        public LogDisplay LogDisplay => logDisplay;
        
        /// <summary>
        /// Gets whether the UI is initialized
        /// </summary>
        public bool IsUIInitialized => isUIInitialized;
        
        private void Awake()
        {
            InitializeUI();
            
            if (hideOnAwake)
            {
                SetUIVisible(false);
            }
        }
        
        /// <summary>
        /// Initializes the UI components
        /// </summary>
        private void InitializeUI()
        {
            // Auto-create LogDisplay if needed and enabled
            if (logDisplay == null && autoCreateLogDisplay)
            {
                logDisplay = GetComponent<LogDisplay>();
                
                if (logDisplay == null)
                {
                    logDisplay = gameObject.AddComponent<LogDisplay>();
                    Debug.Log("LogUIController: Created new LogDisplay component");
                }
            }
            
            if (logDisplay != null)
            {
                isUIInitialized = true;
                Debug.Log("LogUIController: UI initialized successfully");
            }
            else
            {
                Debug.LogError("LogUIController: Failed to initialize UI - no LogDisplay component");
            }
        }
        
        /// <summary>
        /// Sets the log display component
        /// </summary>
        /// <param name="display">LogDisplay component to use</param>
        public void SetLogDisplay(LogDisplay display)
        {
            logDisplay = display;
            isUIInitialized = display != null;
            
            if (isUIInitialized)
            {
                Debug.Log("LogUIController: LogDisplay set externally");
            }
        }
        
        /// <summary>
        /// Updates the UI with new log content
        /// </summary>
        /// <param name="logContent">Formatted log content to display</param>
        public void UpdateLogContent(string logContent)
        {
            if (logDisplay != null && isUIInitialized)
            {
                logDisplay.UpdateDisplay(logContent);
            }
        }
        
        /// <summary>
        /// Sets the UI visibility
        /// </summary>
        /// <param name="visible">Whether the UI should be visible</param>
        public void SetUIVisible(bool visible)
        {
            if (logDisplay != null)
            {
                logDisplay.gameObject.SetActive(visible);
                OnUIStateChanged?.Invoke(visible);
            }
        }
        
        /// <summary>
        /// Gets the current UI visibility state
        /// </summary>
        /// <returns>True if UI is visible</returns>
        public bool IsUIVisible()
        {
            return logDisplay != null && logDisplay.gameObject.activeSelf;
        }
        
        /// <summary>
        /// Applies UI configuration settings
        /// Note: In the new architecture, most UI properties are configured directly in Unity Editor
        /// This method only applies settings that don't conflict with the WYSIWYG principle
        /// </summary>
        /// <param name="config">Configuration to apply</param>
        public void ApplyUIConfiguration(LogConfiguration config)
        {
            uiConfiguration = config;
            
            if (logDisplay != null && config != null)
            {
                // Only apply panel alpha as it's a runtime setting, not a visual design property
                var canvasGroup = logDisplay.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = config.panelAlpha;
                }
                
                Debug.Log($"LogUIController: Applied UI configuration (Panel Alpha: {config.panelAlpha})");
            }
        }
        
        /// <summary>
        /// Forces a UI display update
        /// </summary>
        public void ForceUIUpdate()
        {
            if (logDisplay != null)
            {
                logDisplay.ForceImmediateUpdate();
            }
        }
        
        /// <summary>
        /// Validates the UI component setup
        /// </summary>
        /// <returns>True if UI is properly configured</returns>
        public bool ValidateUISetup()
        {
            if (logDisplay == null)
            {
                Debug.LogError("LogUIController: LogDisplay component is missing");
                return false;
            }
            
            if (!logDisplay.IsTextComponentValid())
            {
                Debug.LogError("LogUIController: LogDisplay has invalid TextMeshPro component");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Gets UI performance statistics
        /// </summary>
        /// <returns>UI performance data</returns>
        public UIPerformanceStats GetUIPerformanceStats()
        {
            var stats = new UIPerformanceStats
            {
                IsInitialized = isUIInitialized,
                IsVisible = IsUIVisible(),
                HasValidTextComponent = logDisplay?.IsTextComponentValid() ?? false,
                ComponentCount = GetComponentsInChildren<Component>().Length
            };
            
            return stats;
        }
        
        private void OnDestroy()
        {
            // Clean up any UI-specific resources
            OnUIStateChanged = null;
        }
    }
    
    /// <summary>
    /// Performance statistics for UI components
    /// </summary>
    [System.Serializable]
    public struct UIPerformanceStats
    {
        public bool IsInitialized;
        public bool IsVisible;
        public bool HasValidTextComponent;
        public int ComponentCount;
        
        public override string ToString()
        {
            return $"UI Stats - Initialized: {IsInitialized}, Visible: {IsVisible}, " +
                   $"Valid Text: {HasValidTextComponent}, Components: {ComponentCount}";
        }
    }
}