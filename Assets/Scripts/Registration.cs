using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UPersian.Components;
using System.Linq;

// Enum for UI control types
[System.Serializable]
public enum UIControlType
{
    InputField,
    Dropdown,
    Toggle,
    Slider
}

// Serializable class to define field mappings
[System.Serializable]
public class FieldMapping
{
    [Tooltip("The key name for this field (e.g., 'name', 'email', etc.)")]
    public string fieldKey;
    
    [Tooltip("Display name for this field (used in validation messages)")]
    public string displayName;
    
    [Tooltip("Type of UI control for this field")]
    public UIControlType controlType;
    
    [Tooltip("Is this field required for validation?")]
    public bool isRequired = true;
    
    [Tooltip("TMP InputField component (use when controlType is InputField)")]
    public TMP_InputField inputField;
    
    [Tooltip("TMP Dropdown component (use when controlType is Dropdown)")]
    public TMP_Dropdown dropdown;
    
    [Tooltip("Toggle component (use when controlType is Toggle)")]
    public Toggle toggle;
    
    [Tooltip("Slider component (use when controlType is Slider)")]
    public Slider slider;
    
    [Tooltip("Required field warning label for this field")]
    public TextMeshProUGUI requiredLabel;
    
    [Tooltip("Default value for this field if empty")]
    public string defaultValue = "";
    
    public FieldMapping()
    {
        fieldKey = "";
        displayName = "";
        controlType = UIControlType.InputField;
        isRequired = true;
        defaultValue = "";
    }
    
    public FieldMapping(string key, string display, UIControlType type, bool required = true)
    {
        fieldKey = key;
        displayName = display;
        controlType = type;
        isRequired = required;
        defaultValue = "";
    }
}

public class Registration : MonoBehaviour
{
    public static Registration Instance { get; private set; }

    [Header("File Settings")]
    [Tooltip("Name of the CSV file to save data.")]
    [SerializeField] private string fileName = "Empty.csv";

    // Public property to access fileName
    public string FileName => fileName;
    
    // Public property to access full file path
    public string FilePath => filePath;

    [Header("Dynamic Field Configuration")]
    [Tooltip("Configure the dynamic fields and their UI mappings")]
    [SerializeField] public List<FieldMapping> fieldMappings = new List<FieldMapping>();
    
    [Header("CSV Export Settings")]
    [Tooltip("Custom order for CSV export (leave empty to use field mapping order)")]
    [SerializeField] public List<string> csvFieldOrder = new List<string>();

    [Header("Save Events")]
    [Tooltip("Event triggered when registration is saved successfully.")]
    [SerializeField]
    public UnityEvent SaveSuccessfullEvent;

    [Header("UI Controls")]
    [Tooltip("Button to save registration data. Will be automatically connected to SaveButton method.")]
    [SerializeField] private Button saveButton;

    [Header("Debug Info")]
    [SerializeField] private TextMeshProUGUI pathlocation;
    
    private string filePath;
    private List<string> csvHeader;

    // Helper method to get current timestamp in consistent format
    private string GetTimestamp()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Use persistentDataPath for cross-platform compatibility (especially Android)
        filePath = Application.persistentDataPath + "/" + fileName;
        
        // Initialize the CSV header based on field mappings
        InitializeCsvHeader();
        
        InitializeFile();

        // Validate dynamic field setup
        ValidateFieldMappings();

        // Connect the save button to the SaveButton method
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveButton);
            Debug.Log("Save button connected to SaveButton method.");
        }
        else
        {
            Debug.LogWarning("Save button is not assigned in the inspector.");
        }

        // Update debug display
        if (pathlocation != null)
        {
            pathlocation.text = filePath;
        }
    }
    
    private void InitializeCsvHeader()
    {
        if (csvFieldOrder.Count > 0)
        {
            csvHeader = new List<string>(csvFieldOrder);
        }
        else
        {
            csvHeader = fieldMappings.Select(fm => fm.fieldKey).ToList();
        }
        
        // Always add timestamp if not present
        if (!csvHeader.Contains("timestamp"))
        {
            csvHeader.Add("timestamp");
        }
        
        Debug.Log($"CSV Header initialized: {string.Join(", ", csvHeader)}");
    }
    
    public void ReSetupTheFile()
    {
        // Use dataPath for file location
        filePath = Application.persistentDataPath + "/" + fileName;
        
        InitializeCsvHeader();
        InitializeFile();

        // Validate field mappings
        ValidateFieldMappings();

        // Connect the save button to the SaveButton method
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(SaveButton); // Remove existing
            saveButton.onClick.AddListener(SaveButton);    // Add new
            Debug.Log("Save button connected to SaveButton method.");
        }
        else
        {
            Debug.LogWarning("Save button is not assigned in the inspector.");
        }
        
        if (filePath != null) 
        {
            pathlocation.text = filePath;
        }
    }

    // Validate that field mappings are properly set up
    private void ValidateFieldMappings()
    {
        var issues = new List<string>();
        
        for (int i = 0; i < fieldMappings.Count; i++)
        {
            var mapping = fieldMappings[i];
            
            // Check for empty field key
            if (string.IsNullOrEmpty(mapping.fieldKey))
            {
                issues.Add($"Field mapping {i}: Field key is empty");
                continue;
            }
            
            // Check for duplicate field keys
            var duplicates = fieldMappings.Count(fm => fm.fieldKey == mapping.fieldKey);
            if (duplicates > 1)
            {
                issues.Add($"Field mapping {i}: Duplicate field key '{mapping.fieldKey}'");
            }
            
            // Check if UI control is assigned based on control type
            switch (mapping.controlType)
            {
                case UIControlType.InputField:
                    if (mapping.inputField == null)
                        issues.Add($"Field '{mapping.fieldKey}': InputField is not assigned");
                    break;
                case UIControlType.Dropdown:
                    if (mapping.dropdown == null)
                        issues.Add($"Field '{mapping.fieldKey}': Dropdown is not assigned");
                    break;
                case UIControlType.Toggle:
                    if (mapping.toggle == null)
                        issues.Add($"Field '{mapping.fieldKey}': Toggle is not assigned");
                    break;
                case UIControlType.Slider:
                    if (mapping.slider == null)
                        issues.Add($"Field '{mapping.fieldKey}': Slider is not assigned");
                    break;
            }
        }
        
        if (issues.Count > 0)
        {
            Debug.LogWarning($"Field mapping validation issues:\n{string.Join("\n", issues)}");
        }
        else
        {
            Debug.Log("All field mappings are properly configured.");
        }

        // Initialize required field labels
        InitializeRequiredFieldLabels();
    }

    // Initialize required field warning labels
    private void InitializeRequiredFieldLabels()
    {
        const string requiredText = "This field is required";
        Color warningColor = Color.red;

        foreach (var mapping in fieldMappings)
        {
            if (mapping.requiredLabel != null)
            {
                mapping.requiredLabel.text = requiredText;
                mapping.requiredLabel.color = warningColor;
                mapping.requiredLabel.gameObject.SetActive(false);
            }
        }
    }

    private void InitializeFile()
    {
        try
        {
            // Ensure the directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log($"Created directory: {directory}");
            }

            if (!File.Exists(filePath))
            {
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    string headerLine = string.Join(",", csvHeader.Select(h => h.Replace("_", " ").ToUpper()));
                    writer.WriteLine(headerLine);
                }
                Debug.Log($"Created new CSV file: {filePath}");
            }
            else
            {
                Debug.Log($"Using existing CSV file: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize file: {ex.Message}");
        }
    }

    private RegistrationData GetCurrentInputs()
    {
        try
        {
            var data = new RegistrationData();
            
            // Process each field mapping
            foreach (var mapping in fieldMappings)
            {
                string value = GetFieldValueFromUI(mapping);
                data.AddField(mapping.fieldKey, value);
            }
            
            // Always add timestamp
            data.AddField("timestamp", GetTimestamp());

            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting current inputs: {ex.Message}");
            throw;
        }
    }

    // Get field value from UI based on control type
    private string GetFieldValueFromUI(FieldMapping mapping)
    {
        try
        {
            switch (mapping.controlType)
            {
                case UIControlType.InputField:
                    if (mapping.inputField != null)
                        return GetRtlSafeText(mapping.inputField);
                    break;
                    
                case UIControlType.Dropdown:
                    if (mapping.dropdown != null)
                        return GetSelectedDropdownValue(mapping.dropdown);
                    break;
                    
                case UIControlType.Toggle:
                    if (mapping.toggle != null)
                        return mapping.toggle.isOn.ToString();
                    break;
                    
                case UIControlType.Slider:
                    if (mapping.slider != null)
                        return mapping.slider.value.ToString();
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to get value for field '{mapping.fieldKey}': {ex.Message}");
        }
        
        return mapping.defaultValue;
    }

    // Set field value to UI based on control type
    private void SetFieldValueToUI(FieldMapping mapping, string value)
    {
        try
        {
            switch (mapping.controlType)
            {
                case UIControlType.InputField:
                    if (mapping.inputField != null)
                        mapping.inputField.text = value;
                    break;
                    
                case UIControlType.Dropdown:
                    if (mapping.dropdown != null)
                        SetDropdownValue(mapping.dropdown, value);
                    break;
                    
                case UIControlType.Toggle:
                    if (mapping.toggle != null && bool.TryParse(value, out bool toggleValue))
                        mapping.toggle.isOn = toggleValue;
                    break;
                    
                case UIControlType.Slider:
                    if (mapping.slider != null && float.TryParse(value, out float sliderValue))
                        mapping.slider.value = sliderValue;
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to set value for field '{mapping.fieldKey}': {ex.Message}");
        }
    }

    // Helper method to extract RTL-safe text from UPersian InputFields
    private string GetRtlSafeText(TMP_InputField inputField)
    {
        if (inputField == null) return "";
        
        try
        {
            // First try to get the RtlText component from the text component itself
            var rtlTextComponent = inputField.textComponent?.GetComponent<RtlText>();
            if (rtlTextComponent != null)
            {
                return rtlTextComponent.BaseText?.Trim() ?? "";
            }
            
            // Alternative: Try to find RtlText in child objects (common UPersian setup)
            var textComponent = inputField.transform.GetChild(2)?.GetComponent<RtlText>();
            if (textComponent != null)
            {
                return textComponent.BaseText?.Trim() ?? "";
            }
            
            // Final fallback: use regular text if RtlText component is not found
            return inputField.text?.Trim() ?? "";
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to extract RTL text from {inputField.name}: {ex.Message}. Using fallback.");
            return inputField.text?.Trim() ?? "";
        }
    }

    private string GetSelectedDropdownValue(TMP_Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options == null || dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
            return "";
        return dropdown.options[dropdown.value].text;
    }
    
    private void SetDropdownValue(TMP_Dropdown dropdown, string value)
    {
        if (dropdown == null || dropdown.options == null) return;
        
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (dropdown.options[i].text == value)
            {
                dropdown.value = i;
                return;
            }
        }
    }

    // Comprehensive save button method that handles the complete save workflow
    public void SaveButton()
    {
        try
        {
            // Step 1: Validate current inputs
            if (!ValidateCurrentInputs())
            {
                Debug.LogWarning("Validation failed. Please check your input data.");
                return;
            }

            // Step 2: Get and validate registration data
            RegistrationData data = GetCurrentInputs();

            // Step 3: Save the registration data
            string csvLine = data.ToCsvLine(csvHeader);
            
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(csvLine);
            }

            Debug.Log($"Registration saved successfully");

            // Step 4: Clear all input fields after successful save
            ClearAllInputs();
            Debug.Log("Input fields cleared for new registration.");

            // Step 5: Trigger the save success event
            SaveSuccessfullEvent?.Invoke();
            Debug.Log("Save success event triggered.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save registration: {ex.Message}");
        }
    }

    public void SaveRegistration()
    {
        try
        {
            RegistrationData data = GetCurrentInputs();
            
            // Basic validation for at least one required field
            var requiredFields = fieldMappings.Where(fm => fm.isRequired).ToList();
            if (requiredFields.Count > 0)
            {
                bool hasRequiredData = requiredFields.Any(rf => !string.IsNullOrEmpty(data.GetFieldValue(rf.fieldKey)));
                if (!hasRequiredData)
                {
                    Debug.LogWarning("At least one required field must be filled.");
                    return;
                }
            }

            string csvLine = data.ToCsvLine(csvHeader);
            
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(csvLine);
            }
            
            SaveSuccessfullEvent?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save registration: {ex.Message}");
        }
    }

    public void SaveRegistrationData(RegistrationData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Registration data cannot be null.");
            return;
        }

        try
        {
            string csvLine = data.ToCsvLine(csvHeader);
            
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(csvLine);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save registration data: {ex.Message}");
        }
    }

    // Load registration data into UI
    public void LoadRegistrationToUI(RegistrationData data)
    {
        if (data == null) return;
        
        foreach (var mapping in fieldMappings)
        {
            string value = data.GetFieldValue(mapping.fieldKey);
            SetFieldValueToUI(mapping, value);
        }
    }

    public string ReadAllData()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                InitializeFile();
                return string.Join(",", csvHeader);
            }

            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to read file: {ex.Message}");
            return string.Empty;
        }
    }

    // Dynamic field existence check
    public bool IsFieldValueExists(string fieldKey, string valueToCheck)
    {
        if (string.IsNullOrEmpty(valueToCheck) || string.IsNullOrEmpty(fieldKey))
            return false;

        try
        {
            if (!File.Exists(filePath))
                return false;

            int fieldIndex = csvHeader.IndexOf(fieldKey);
            if (fieldIndex == -1) return false;

            using (var reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine) { isFirstLine = false; continue; }
                    
                    var parts = ParseCsvLine(line);
                    if (parts.Count > fieldIndex && parts[fieldIndex].Trim().Equals(valueToCheck.Trim(), StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to check field value: {ex.Message}");
            return false;
        }
    }

    // Legacy method for backward compatibility
    public bool IsMobileExists(string mobileToCheck)
    {
        return IsFieldValueExists("mobile_number", mobileToCheck);
    }

    public bool IsEmailExists(string emailToCheck)
    {
        return IsFieldValueExists("email", emailToCheck);
    }

    // Helper method to parse CSV lines properly handling escaped fields
    private List<string> ParseCsvLine(string line)
    {
        List<String> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField += '"';
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        fields.Add(currentField);
        return fields;
    }

    // API integration methods
    public List<RegistrationData> GetAllRegistrationsAsObjects()
    {
        List<RegistrationData> registrations = new List<RegistrationData>();
        
        try
        {
            if (!File.Exists(filePath))
                return registrations;

            using (var reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine) { isFirstLine = false; continue; }
                    
                    var data = new RegistrationData();
                    data.FromCsvLine(line, csvHeader);
                    
                    // Set timestamp for internal use if not present
                    if (string.IsNullOrEmpty(data.GetFieldValue("timestamp")))
                    {
                        data.SetFieldValue("timestamp", GetTimestamp());
                    }
                    
                    registrations.Add(data);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to read registrations: {ex.Message}");
        }
        
        return registrations;
    }

    public string ConvertToJSON()
    {
        try
        {
            List<RegistrationData> registrations = GetAllRegistrationsAsObjects();
            var wrapper = new { registrations = registrations };
            return JsonUtility.ToJson(wrapper, true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to convert to JSON: {ex.Message}");
            return string.Empty;
        }
    }

    public string ConvertSingleRegistrationToRawData(RegistrationData data)
    {
        if (data == null) return "";
        
        // Convert to raw data format for API: exclude timestamp
        var fieldsWithoutTimestamp = csvHeader.Where(key => key != "timestamp").ToList();
        return data.ToCsvLine(fieldsWithoutTimestamp);
    }

    public int GetRegistrationCount()
    {
        return GetAllRegistrationsAsObjects().Count;
    }

    // Get summary of current data for debugging
    public string GetDataSummary()
    {
        try
        {
            var registrations = GetAllRegistrationsAsObjects();
            return $"Total registrations: {registrations.Count}\nFile path: {filePath}\nFile exists: {File.Exists(filePath)}\nFields: {string.Join(", ", csvHeader)}";
        }
        catch (Exception ex)
        {
            return $"Error getting summary: {ex.Message}";
        }
    }

    // Clear all input fields
    public void ClearAllInputs()
    {
        try
        {
            foreach (var mapping in fieldMappings)
            {
                SetFieldValueToUI(mapping, mapping.defaultValue);
            }

            // Hide all required field labels when clearing inputs
            HideAllRequiredLabels();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to clear inputs: {ex.Message}");
        }
    }

    // Validate current inputs
    public bool ValidateCurrentInputs()
    {
        try
        {
            RegistrationData data = GetCurrentInputs();
            bool isValid = true;

            // Hide all warning labels first
            HideAllRequiredLabels();
            
            // Check required fields
            foreach (var mapping in fieldMappings)
            {
                if (mapping.isRequired)
                {
                    string fieldValue = data.GetFieldValue(mapping.fieldKey);
                    if (string.IsNullOrEmpty(fieldValue))
                    {
                        string displayName = string.IsNullOrEmpty(mapping.displayName) ? mapping.fieldKey : mapping.displayName;
                        Debug.LogWarning($"{displayName} is required.");
                        ShowRequiredLabel(mapping.requiredLabel, $"{displayName} is required");
                        isValid = false;
                    }
                    else
                    {
                        // Additional validation based on field type
                        if (mapping.fieldKey == "email" && !IsValidEmail(fieldValue))
                        {
                            Debug.LogWarning("Invalid email format.");
                            ShowRequiredLabel(mapping.requiredLabel, "Invalid email format");
                            isValid = false;
                        }
                        else if (mapping.fieldKey.Contains("mobile") && !IsValidMobileNumber(fieldValue))
                        {
                            Debug.LogWarning("Invalid mobile number format.");
                            ShowRequiredLabel(mapping.requiredLabel, "Invalid mobile number format");
                            isValid = false;
                        }
                    }
                }
            }
            
            return isValid;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Validation error: {ex.Message}");
            return false;
        }
    }

    // Helper method to show a required field label
    private void ShowRequiredLabel(TextMeshProUGUI label, string customMessage = null)
    {
        if (label != null)
        {
            if (!string.IsNullOrEmpty(customMessage))
            {
                label.text = customMessage;
            }
            else
            {
                label.text = "This field is required";
            }
            label.gameObject.SetActive(true);
        }
    }

    // Helper method to hide all required field labels
    private void HideAllRequiredLabels()
    {
        foreach (var mapping in fieldMappings)
        {
            if (mapping.requiredLabel != null)
                mapping.requiredLabel.gameObject.SetActive(false);
        }
    }

    // Email validation method
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // Mobile number validation method
    private bool IsValidMobileNumber(string mobile)
    {
        if (string.IsNullOrEmpty(mobile))
            return false;
            
        // Allow digits, spaces, hyphens, parentheses, and plus sign
        return System.Text.RegularExpressions.Regex.IsMatch(mobile, @"^[\d\s\-\(\)\+]+$") && mobile.Length >= 7;
    }

    // Helper methods for field management
    public void AddFieldMapping(string fieldKey, string displayName, UIControlType controlType, bool isRequired = true)
    {
        var newMapping = new FieldMapping(fieldKey, displayName, controlType, isRequired);
        fieldMappings.Add(newMapping);
        
        // Refresh CSV header
        InitializeCsvHeader();
        
        Debug.Log($"Added field mapping: {fieldKey}");
    }

    public void RemoveFieldMapping(string fieldKey)
    {
        fieldMappings.RemoveAll(fm => fm.fieldKey == fieldKey);
        
        // Refresh CSV header
        InitializeCsvHeader();
        
        Debug.Log($"Removed field mapping: {fieldKey}");
    }

    public FieldMapping GetFieldMapping(string fieldKey)
    {
        return fieldMappings.FirstOrDefault(fm => fm.fieldKey == fieldKey);
    }

    private void OnDestroy()
    {
        // Cleanup: remove listener from button to prevent memory leaks
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(SaveButton);
        }
    }
}