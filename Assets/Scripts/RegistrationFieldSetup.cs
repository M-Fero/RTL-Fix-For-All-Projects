using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FieldPreset
{
    public string name;
    public List<FieldMapping> fields;
    
    public FieldPreset(string presetName)
    {
        name = presetName;
        fields = new List<FieldMapping>();
    }
}

/// <summary>
/// Helper component to quickly setup common field configurations for the Registration system
/// </summary>
public class RegistrationFieldSetup : MonoBehaviour
{
    [Header("Registration Reference")]
    [Tooltip("Reference to the Registration component to configure")]
    public Registration registration;
    
    [Header("Field Presets")]
    [Tooltip("Predefined field configurations")]
    public List<FieldPreset> fieldPresets = new List<FieldPreset>();
    
    [Header("Quick Setup")]
    [Tooltip("Apply a preset configuration")]
    public int selectedPresetIndex = 0;
    
    private void Start()
    {
        InitializeDefaultPresets();
    }
    
    private void InitializeDefaultPresets()
    {
        if (fieldPresets.Count == 0)
        {
            CreateDefaultPresets();
        }
    }
    
    private void CreateDefaultPresets()
    {
        // Automotive Service Registration Preset - PERFECT FOR YOUR NEEDS!
        var automotivePreset = new FieldPreset("Automotive Service Registration");
        automotivePreset.fields.Add(new FieldMapping("name", "Customer Name", UIControlType.InputField, true));
        automotivePreset.fields.Add(new FieldMapping("phone_number", "Phone Number", UIControlType.InputField, true));
        automotivePreset.fields.Add(new FieldMapping("car_model", "Car Model", UIControlType.InputField, true));
        automotivePreset.fields.Add(new FieldMapping("car_type", "Car Type", UIControlType.Dropdown, true));
        automotivePreset.fields.Add(new FieldMapping("service_date", "Service Date", UIControlType.InputField, true));
        fieldPresets.Add(automotivePreset);
        
        // Basic Contact Information Preset
        var basicPreset = new FieldPreset("Basic Contact Info");
        basicPreset.fields.Add(new FieldMapping("name", "Full Name", UIControlType.InputField, true));
        basicPreset.fields.Add(new FieldMapping("email", "Email Address", UIControlType.InputField, true));
        basicPreset.fields.Add(new FieldMapping("mobile_number", "Mobile Number", UIControlType.InputField, true));
        basicPreset.fields.Add(new FieldMapping("notes", "Additional Notes", UIControlType.InputField, false));
        fieldPresets.Add(basicPreset);
        
        // Business Registration Preset
        var businessPreset = new FieldPreset("Business Registration");
        businessPreset.fields.Add(new FieldMapping("name", "Contact Name", UIControlType.InputField, true));
        businessPreset.fields.Add(new FieldMapping("email", "Email Address", UIControlType.InputField, true));
        businessPreset.fields.Add(new FieldMapping("mobile_number", "Mobile Number", UIControlType.InputField, true));
        businessPreset.fields.Add(new FieldMapping("company_name", "Company Name", UIControlType.InputField, true));
        businessPreset.fields.Add(new FieldMapping("position", "Position/Title", UIControlType.InputField, false));
        businessPreset.fields.Add(new FieldMapping("company_size", "Company Size", UIControlType.Dropdown, false));
        businessPreset.fields.Add(new FieldMapping("industry", "Industry", UIControlType.Dropdown, false));
        businessPreset.fields.Add(new FieldMapping("newsletter", "Subscribe to Newsletter", UIControlType.Toggle, false));
        fieldPresets.Add(businessPreset);
        
        // Event Registration Preset
        var eventPreset = new FieldPreset("Event Registration");
        eventPreset.fields.Add(new FieldMapping("name", "Full Name", UIControlType.InputField, true));
        eventPreset.fields.Add(new FieldMapping("email", "Email Address", UIControlType.InputField, true));
        eventPreset.fields.Add(new FieldMapping("mobile_number", "Mobile Number", UIControlType.InputField, true));
        eventPreset.fields.Add(new FieldMapping("attendance_type", "Attendance Type", UIControlType.Dropdown, true));
        eventPreset.fields.Add(new FieldMapping("dietary_requirements", "Dietary Requirements", UIControlType.InputField, false));
        eventPreset.fields.Add(new FieldMapping("accessibility_needs", "Accessibility Needs", UIControlType.InputField, false));
        eventPreset.fields.Add(new FieldMapping("marketing_consent", "Marketing Consent", UIControlType.Toggle, false));
        fieldPresets.Add(eventPreset);
        
        // Survey/Feedback Preset
        var surveyPreset = new FieldPreset("Survey/Feedback");
        surveyPreset.fields.Add(new FieldMapping("name", "Name (Optional)", UIControlType.InputField, false));
        surveyPreset.fields.Add(new FieldMapping("email", "Email (Optional)", UIControlType.InputField, false));
        surveyPreset.fields.Add(new FieldMapping("satisfaction_rating", "Satisfaction Rating", UIControlType.Slider, true));
        surveyPreset.fields.Add(new FieldMapping("recommendation_likelihood", "Likelihood to Recommend", UIControlType.Slider, true));
        surveyPreset.fields.Add(new FieldMapping("category", "Feedback Category", UIControlType.Dropdown, true));
        surveyPreset.fields.Add(new FieldMapping("comments", "Additional Comments", UIControlType.InputField, false));
        surveyPreset.fields.Add(new FieldMapping("follow_up_consent", "Allow Follow-up Contact", UIControlType.Toggle, false));
        fieldPresets.Add(surveyPreset);
    }
    
    /// <summary>
    /// Apply the selected preset to the Registration component
    /// </summary>
    [ContextMenu("Apply Selected Preset")]
    public void ApplySelectedPreset()
    {
        if (registration == null)
        {
            Debug.LogError("Registration component reference is not set!");
            return;
        }
        
        if (selectedPresetIndex < 0 || selectedPresetIndex >= fieldPresets.Count)
        {
            Debug.LogError("Selected preset index is out of range!");
            return;
        }
        
        ApplyPreset(fieldPresets[selectedPresetIndex]);
    }
    
    /// <summary>
    /// Apply a specific preset to the Registration component
    /// </summary>
    public void ApplyPreset(FieldPreset preset)
    {
        if (registration == null || preset == null)
        {
            Debug.LogError("Registration component or preset is null!");
            return;
        }
        
        // Clear existing field mappings
        registration.fieldMappings.Clear();
        
        // Add preset fields
        foreach (var field in preset.fields)
        {
            registration.fieldMappings.Add(new FieldMapping
            {
                fieldKey = field.fieldKey,
                displayName = field.displayName,
                controlType = field.controlType,
                isRequired = field.isRequired,
                defaultValue = field.defaultValue
            });
        }
        
        Debug.Log($"Applied preset '{preset.name}' with {preset.fields.Count} fields to Registration component.");
        
        // Trigger reinitialization if in play mode
        if (Application.isPlaying)
        {
            registration.ReSetupTheFile();
        }
    }
    
    /// <summary>
    /// Save the current Registration field mappings as a new preset
    /// </summary>
    [ContextMenu("Save Current Configuration as Preset")]
    public void SaveCurrentAsPreset()
    {
        if (registration == null || registration.fieldMappings.Count == 0)
        {
            Debug.LogError("No field mappings to save!");
            return;
        }
        
        var newPreset = new FieldPreset("Custom Preset");
        
        foreach (var mapping in registration.fieldMappings)
        {
            newPreset.fields.Add(new FieldMapping
            {
                fieldKey = mapping.fieldKey,
                displayName = mapping.displayName,
                controlType = mapping.controlType,
                isRequired = mapping.isRequired,
                defaultValue = mapping.defaultValue
            });
        }
        
        fieldPresets.Add(newPreset);
        Debug.Log($"Saved current configuration as preset with {newPreset.fields.Count} fields.");
    }
    
    /// <summary>
    /// Create a completely empty field mapping setup
    /// </summary>
    [ContextMenu("Clear All Field Mappings")]
    public void ClearAllFieldMappings()
    {
        if (registration != null)
        {
            registration.fieldMappings.Clear();
            Debug.Log("Cleared all field mappings from Registration component.");
        }
    }
    
    /// <summary>
    /// Add a single field to the current Registration setup
    /// </summary>
    public void AddField(string fieldKey, string displayName, UIControlType controlType, bool isRequired = true)
    {
        if (registration == null)
        {
            Debug.LogError("Registration component reference is not set!");
            return;
        }
        
        var newField = new FieldMapping(fieldKey, displayName, controlType, isRequired);
        registration.fieldMappings.Add(newField);
        
        Debug.Log($"Added field '{fieldKey}' to Registration component.");
    }
    
    /// <summary>
    /// Get preset names for dropdown in inspector
    /// </summary>
    public string[] GetPresetNames()
    {
        var names = new string[fieldPresets.Count];
        for (int i = 0; i < fieldPresets.Count; i++)
        {
            names[i] = fieldPresets[i].name;
        }
        return names;
    }
    
    /// <summary>
    /// Runtime method to apply preset by name
    /// </summary>
    public void ApplyPresetByName(string presetName)
    {
        var preset = fieldPresets.Find(p => p.name == presetName);
        if (preset != null)
        {
            ApplyPreset(preset);
        }
        else
        {
            Debug.LogError($"Preset '{presetName}' not found!");
        }
    }
    
    /// <summary>
    /// Apply the automotive service preset - Quick setup for your specific needs!
    /// </summary>
    [ContextMenu("Apply Automotive Service Preset")]
    public void ApplyAutomotivePreset()
    {
        if (registration == null)
        {
            Debug.LogError("Registration component reference is not set!");
            return;
        }
        
        var automotivePreset = fieldPresets.Find(p => p.name == "Automotive Service Registration");
        if (automotivePreset != null)
        {
            ApplyPreset(automotivePreset);
        }
        else
        {
            Debug.LogError("Automotive Service Registration preset not found!");
        }
    }
}