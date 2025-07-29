using UnityEngine;
using UnityEditor;

namespace RuntimeLogging.Editor
{
    /// <summary>
    /// Custom inspector for LogConfiguration with enhanced UI and validation
    /// </summary>
    [CustomEditor(typeof(LogConfiguration))]
    public class LogConfigurationEditor : UnityEditor.Editor
    {
        private LogConfiguration config;
        private bool showAdvancedSettings = false;
        private bool showColorPreview = true;
        private bool showValidationInfo = false;
        
        private void OnEnable()
        {
            config = (LogConfiguration)target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Log Panel Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Display Settings Section
            DrawDisplaySettings();
            EditorGUILayout.Space();
            
            // Color Settings Section
            DrawColorSettings();
            EditorGUILayout.Space();
            
            // Font Settings Section
            DrawFontSettings();
            EditorGUILayout.Space();
            
            // Advanced Settings Section
            DrawAdvancedSettings();
            EditorGUILayout.Space();
            
            // Validation Section
            DrawValidationSection();
            EditorGUILayout.Space();
            
            // Action Buttons
            DrawActionButtons();
            
            if (serializedObject.ApplyModifiedProperties())
            {
                // Apply changes immediately in play mode
                if (Application.isPlaying)
                {
                    config.ApplyConfigurationChanges();
                }
            }
        }
        
        private void DrawDisplaySettings()
        {
            EditorGUILayout.LabelField("Display Settings", EditorStyles.boldLabel);
            
            // Max Log Count with validation
            EditorGUILayout.BeginHorizontal();
            int newMaxLogCount = EditorGUILayout.IntField("Max Log Count", config.maxLogCount);
            if (newMaxLogCount != config.maxLogCount)
            {
                config.maxLogCount = Mathf.Clamp(newMaxLogCount, 1, 1000);
                EditorUtility.SetDirty(config);
            }
            
            if (config.maxLogCount < 10 || config.maxLogCount > 500)
            {
                EditorGUILayout.LabelField("⚠", GUILayout.Width(20));
            }
            EditorGUILayout.EndHorizontal();
            
            if (config.maxLogCount < 10)
            {
                EditorGUILayout.HelpBox("Very low log count may cause frequent log removal.", MessageType.Warning);
            }
            else if (config.maxLogCount > 500)
            {
                EditorGUILayout.HelpBox("High log count may impact performance.", MessageType.Warning);
            }
            
            // Auto Scroll
            config.autoScroll = EditorGUILayout.Toggle("Auto Scroll", config.autoScroll);
            
            // Timestamp Format with validation
            EditorGUILayout.BeginHorizontal();
            string newTimestampFormat = EditorGUILayout.TextField("Timestamp Format", config.timestampFormat);
            if (newTimestampFormat != config.timestampFormat)
            {
                config.timestampFormat = string.IsNullOrEmpty(newTimestampFormat) ? "HH:mm:ss" : newTimestampFormat;
                EditorUtility.SetDirty(config);
            }
            
            // Show timestamp preview
            try
            {
                string preview = System.DateTime.Now.ToString(config.timestampFormat);
                EditorGUILayout.LabelField($"({preview})", GUILayout.Width(100));
            }
            catch
            {
                EditorGUILayout.LabelField("(Invalid)", GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();
            
            // Panel Alpha
            config.panelAlpha = EditorGUILayout.Slider("Panel Alpha", config.panelAlpha, 0f, 1f);
        }
        
        private void DrawColorSettings()
        {
            EditorGUILayout.LabelField("Color Settings", EditorStyles.boldLabel);
            
            showColorPreview = EditorGUILayout.Toggle("Show Color Preview", showColorPreview);
            
            // Info Color
            DrawColorField("Info Color", ref config.infoColor, ref config.infoColorHex);
            
            // Warning Color
            DrawColorField("Warning Color", ref config.warningColor, ref config.warningColorHex);
            
            // Error Color
            DrawColorField("Error Color", ref config.errorColor, ref config.errorColorHex);
            
            if (showColorPreview)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Color Preview:", EditorStyles.boldLabel);
                
                var originalColor = GUI.color;
                
                GUI.color = config.infoColor;
                EditorGUILayout.LabelField($"[{System.DateTime.Now:HH:mm:ss}][Info] Sample info message");
                
                GUI.color = config.warningColor;
                EditorGUILayout.LabelField($"[{System.DateTime.Now:HH:mm:ss}][Warning] Sample warning message");
                
                GUI.color = config.errorColor;
                EditorGUILayout.LabelField($"[{System.DateTime.Now:HH:mm:ss}][Error] Sample error message");
                
                GUI.color = originalColor;
            }
        }
        
        private void DrawColorField(string label, ref Color colorField, ref string hexField)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Color picker
            Color newColor = EditorGUILayout.ColorField(label, colorField);
            if (newColor != colorField)
            {
                colorField = newColor;
                hexField = LogConfiguration.ColorToHex(newColor);
                EditorUtility.SetDirty(config);
            }
            
            // Hex field
            string newHex = EditorGUILayout.TextField(hexField, GUILayout.Width(80));
            if (newHex != hexField)
            {
                string validatedHex = LogConfiguration.ValidateAndConvertHexColor(newHex, hexField);
                if (validatedHex != hexField)
                {
                    hexField = validatedHex;
                    colorField = LogConfiguration.HexToColor(validatedHex, colorField);
                    EditorUtility.SetDirty(config);
                }
            }
            
            // Validation indicator
            if (!IsValidHexColor(hexField))
            {
                EditorGUILayout.LabelField("⚠", GUILayout.Width(20));
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawFontSettings()
        {
            EditorGUILayout.LabelField("Font Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Font settings are now configured directly on TextMeshPro components in Unity Editor.\nThis ensures 'what you see is what you get' across all platforms.", MessageType.Info);
        }
        
        private void DrawAdvancedSettings()
        {
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings");
            
            if (showAdvancedSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("No advanced settings available.", EditorStyles.helpBox);
                
                EditorGUI.indentLevel--;
            }
        }
        
        
        private void DrawValidationSection()
        {
            showValidationInfo = EditorGUILayout.Foldout(showValidationInfo, "Validation Info");
            
            if (showValidationInfo)
            {
                EditorGUI.indentLevel++;
                
                // Validate settings and show results
                bool isValid = true;
                
                if (config.maxLogCount < 1 || config.maxLogCount > 1000)
                {
                    EditorGUILayout.HelpBox("Max log count should be between 1 and 1000.", MessageType.Error);
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(config.timestampFormat))
                {
                    EditorGUILayout.HelpBox("Timestamp format cannot be empty.", MessageType.Error);
                    isValid = false;
                }
                
                if (!IsValidHexColor(config.infoColorHex) || !IsValidHexColor(config.warningColorHex) || !IsValidHexColor(config.errorColorHex))
                {
                    EditorGUILayout.HelpBox("One or more hex color codes are invalid.", MessageType.Error);
                    isValid = false;
                }
                
                
                if (isValid)
                {
                    EditorGUILayout.HelpBox("All settings are valid.", MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset to Defaults"))
            {
                if (EditorUtility.DisplayDialog("Reset Configuration", "Are you sure you want to reset all settings to their default values?", "Yes", "No"))
                {
                    config.ResetToDefaults();
                    EditorUtility.SetDirty(config);
                }
            }
            
            if (GUILayout.Button("Validate Settings"))
            {
                config.ValidateSettings();
                EditorUtility.SetDirty(config);
                EditorUtility.DisplayDialog("Validation Complete", "Settings have been validated and corrected if necessary.", "OK");
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Apply Changes"))
                {
                    config.ApplyConfigurationChanges();
                    EditorUtility.DisplayDialog("Changes Applied", "Configuration changes have been applied to all active loggers.", "OK");
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private bool IsValidHexColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return false;
                
            return ColorUtility.TryParseHtmlString(hexColor, out Color _);
        }
    }
}