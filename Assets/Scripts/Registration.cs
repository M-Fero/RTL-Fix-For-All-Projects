using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UPersian.Components;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Registration))]
public class RegistrationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Mark the object as dirty when modified
        EditorGUI.BeginChangeCheck();

        serializedObject.Update();

        var modeProp = serializedObject.FindProperty("inputMode");
        EditorGUILayout.PropertyField(modeProp);

        Registration.InputMode selectedMode = (Registration.InputMode)modeProp.enumValueIndex;

        // File Settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("File Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fileName"));

        // UI Controls
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("UI Controls", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveButton"));

        // Conditional Inputs
        if (selectedMode == Registration.InputMode.Legacy)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Legacy Input Fields", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nameLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mobileNumberLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("emailLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("companyNameLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headOfficeLocationLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectNameLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectLocationLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("accountTypeDropdownLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("signatureLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applicationTypeDropdownLegacy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("salesCommentLegacy"));
        }
        else if (selectedMode == Registration.InputMode.TMPRO)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("TMPRO Input Fields", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nameTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mobileNumberTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("emailTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("companyNameTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headOfficeLocationTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectNameTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectLocationTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("accountTypeDropdownTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("signatureTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applicationTypeDropdownTMP"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("salesCommentTMP"));
        }
        else if (selectedMode == Registration.InputMode.UPERSIANRTL)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UPersian RTL Input Fields", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nameUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mobileNumberUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("emailUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("companyNameUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headOfficeLocationUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectNameUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("projectLocationUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("accountTypeDropdownUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("signatureUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applicationTypeDropdownUPersianRTL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("salesCommentUPersianRTL"));
        }

        // Events
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SaveSuccessfullEvent"));

        // Apply modifications
        serializedObject.ApplyModifiedProperties();

        // Mark scene as dirty if changes were made
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif

// Registration data structure with expanded fields
[System.Serializable]
public class RegistrationData
{
    public string name;
    public string mobile_number;
    public string email;
    public string company_name;
    public string head_office_location;
    public string project_name;
    public string project_location;
    public string account_type;
    public string signature;
    public string application_type;
    public string sales_comment;
    public string timestamp;
}

public class Registration : MonoBehaviour
{
    public enum InputMode
    {
        Legacy,
        TMPRO,
        UPERSIANRTL
    }

    public static Registration Instance { get; private set; }

    [Header("File Settings")]
    [Tooltip("Name of the CSV file to save data.")]
    [SerializeField] private string fileName = "Empty.csv";

    // Public property to access fileName
    public string FileName => fileName;

    [Header("Input Mode")]
    [Tooltip("Choose which input system to use: Legacy, TMPRO, or UPersian RTL.")]
    [SerializeField] private InputMode inputMode = InputMode.Legacy;

    [Header("Legacy Input Fields")]
    [Tooltip("Legacy UI InputField for Name.")]
    [SerializeField] private InputField nameLegacy;
    [Tooltip("Legacy UI InputField for Mobile Number.")]
    [SerializeField] private InputField mobileNumberLegacy;
    [Tooltip("Legacy UI InputField for Email.")]
    [SerializeField] private InputField emailLegacy;
    [Tooltip("Legacy UI InputField for Company Name.")]
    [SerializeField] private InputField companyNameLegacy;
    [Tooltip("Legacy UI InputField for Head Office Location.")]
    [SerializeField] private InputField headOfficeLocationLegacy;
    [Tooltip("Legacy UI InputField for Project Name.")]
    [SerializeField] private InputField projectNameLegacy;
    [Tooltip("Legacy UI InputField for Project Location.")]
    [SerializeField] private InputField projectLocationLegacy;
    [Tooltip("Legacy UI Dropdown for Account Type.")]
    [SerializeField] private Dropdown accountTypeDropdownLegacy;
    [Tooltip("Legacy UI InputField for Signature.")]
    [SerializeField] private InputField signatureLegacy;
    [Tooltip("Legacy UI Dropdown for Application Type.")]
    [SerializeField] private Dropdown applicationTypeDropdownLegacy;
    [Tooltip("Legacy UI InputField for Sales Comment.")]
    [SerializeField] private InputField salesCommentLegacy;

    [Header("TMPRO Input Fields")]
    [Tooltip("TMPRO InputField for Name.")]
    [SerializeField] private TMP_InputField nameTMP;
    [Tooltip("TMPRO InputField for Mobile Number.")]
    [SerializeField] private TMP_InputField mobileNumberTMP;
    [Tooltip("TMPRO InputField for Email.")]
    [SerializeField] private TMP_InputField emailTMP;
    [Tooltip("TMPRO InputField for Company Name.")]
    [SerializeField] private TMP_InputField companyNameTMP;
    [Tooltip("TMPRO InputField for Head Office Location.")]
    [SerializeField] private TMP_InputField headOfficeLocationTMP;
    [Tooltip("TMPRO InputField for Project Name.")]
    [SerializeField] private TMP_InputField projectNameTMP;
    [Tooltip("TMPRO InputField for Project Location.")]
    [SerializeField] private TMP_InputField projectLocationTMP;
    [Tooltip("TMPRO Dropdown for Account Type.")]
    [SerializeField] private TMP_Dropdown accountTypeDropdownTMP;
    [Tooltip("TMPRO InputField for Signature.")]
    [SerializeField] private TMP_InputField signatureTMP;
    [Tooltip("TMPRO Dropdown for Application Type.")]
    [SerializeField] private TMP_Dropdown applicationTypeDropdownTMP;
    [Tooltip("TMPRO InputField for Sales Comment.")]
    [SerializeField] private TMP_InputField salesCommentTMP;

    [Header("UPersian RTL Input Fields")]
    [Tooltip("UPersian RTL TMP InputField for Name (Auto RTL/LTR detection).")]
    [SerializeField] private TMP_InputField nameUPersianRTL;
    [Tooltip("UPersian RTL TMP InputField for Mobile Number.")]
    [SerializeField] private TMP_InputField mobileNumberUPersianRTL;
    [Tooltip("UPersian RTL TMP InputField for Email.")]
    [SerializeField] private TMP_InputField emailUPersianRTL;
    [Tooltip("UPersian RTL TMP InputField for Company Name (Auto RTL/LTR detection).")]
    [SerializeField] private TMP_InputField companyNameUPersianRTL;
    [Tooltip("UPersian RTL TMP InputField for Head Office Location (Auto RTL/LTR detection).")]
    [SerializeField] private TMP_InputField headOfficeLocationUPersianRTL;
    [Tooltip("UPersian RTL TMP InputField for Project Name (Auto RTL/LTR detection).")]
    [SerializeField] private TMP_InputField projectNameUPersianRTL;
    [Tooltip("UPersian RTL TMP InputField for Project Location (Auto RTL/LTR detection).")]
    [SerializeField] private TMP_InputField projectLocationUPersianRTL;
    [Tooltip("UPersian RTL TMP Dropdown for Account Type (Auto RTL/LTR detection).")]
    [SerializeField] private TMP_Dropdown accountTypeDropdownUPersianRTL;
    [Tooltip("UPersian RTL TMP InputField for Signature (Auto RTL/LTR detection).")]
    [SerializeField] private TMP_InputField signatureUPersianRTL;
    [Tooltip("UPersian RTL TMP Dropdown for Application Type (Auto RTL/LTR detection).")]
    [SerializeField] private TMP_Dropdown applicationTypeDropdownUPersianRTL;
    [Tooltip("UPersian RTL TMP InputField for Sales Comment (Auto RTL/LTR detection).")]
    [SerializeField] private TMP_InputField salesCommentUPersianRTL;

    [Header("Save Events")]
    [Tooltip("Event triggered when registration is saved successfully.")]
    [SerializeField]
    public UnityEvent SaveSuccessfullEvent;

    [Header("UI Controls")]
    [Tooltip("Button to save registration data. Will be automatically connected to SaveButton method.")]
    [SerializeField] private Button saveButton;

    private string filePath;
    private const string CSV_HEADER = "Name,Mobile_Number,Email,Company_Name,Head_Office_Location,Project_Name,Project_Location,Account_Type,Signature,Application_Type,Sales_Comment";

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
        filePath = Path.Combine(Application.dataPath, fileName);
        InitializeFile();

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
    }

    private void InitializeFile()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.WriteLine(CSV_HEADER);
                }
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
            switch (inputMode)
            {
                case InputMode.TMPRO:
                    if (nameTMP == null || mobileNumberTMP == null || emailTMP == null || 
                        companyNameTMP == null || headOfficeLocationTMP == null || 
                        projectNameTMP == null || projectLocationTMP == null || accountTypeDropdownTMP == null ||
                        signatureTMP == null || applicationTypeDropdownTMP == null || salesCommentTMP == null)
                    {
                        throw new InvalidOperationException("One or more TMPRO Input fields are not assigned.");
                    }

                    return new RegistrationData
                    {
                        name = nameTMP.text?.Trim() ?? "",
                        mobile_number = mobileNumberTMP.text?.Trim() ?? "",
                        email = emailTMP.text?.Trim() ?? "",
                        company_name = companyNameTMP.text?.Trim() ?? "",
                        head_office_location = headOfficeLocationTMP.text?.Trim() ?? "",
                        project_name = projectNameTMP.text?.Trim() ?? "",
                        project_location = projectLocationTMP.text?.Trim() ?? "",
                        account_type = GetSelectedAccountType(accountTypeDropdownTMP),
                        signature = signatureTMP.text?.Trim() ?? "",
                        application_type = GetSelectedApplicationType(applicationTypeDropdownTMP),
                        sales_comment = salesCommentTMP.text?.Trim() ?? "",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                case InputMode.UPERSIANRTL:
                    if (nameUPersianRTL == null || mobileNumberUPersianRTL == null || emailUPersianRTL == null || 
                        companyNameUPersianRTL == null || headOfficeLocationUPersianRTL == null || 
                        projectNameUPersianRTL == null || projectLocationUPersianRTL == null || accountTypeDropdownUPersianRTL == null ||
                        signatureUPersianRTL == null || applicationTypeDropdownUPersianRTL == null || salesCommentUPersianRTL == null)
                    {
                        throw new InvalidOperationException("One or more UPersian RTL Input fields are not assigned.");
                    }

                    return new RegistrationData
                    {
                        name = GetRtlSafeText(nameUPersianRTL),
                        mobile_number = mobileNumberUPersianRTL.text?.Trim() ?? "",
                        email = emailUPersianRTL.text?.Trim() ?? "",
                        company_name = GetRtlSafeText(companyNameUPersianRTL),
                        head_office_location = GetRtlSafeText(headOfficeLocationUPersianRTL),
                        project_name = GetRtlSafeText(projectNameUPersianRTL),
                        project_location = GetRtlSafeText(projectLocationUPersianRTL),
                        account_type = GetSelectedAccountType(accountTypeDropdownUPersianRTL),
                        signature = GetRtlSafeText(signatureUPersianRTL),
                        application_type = GetSelectedApplicationType(applicationTypeDropdownUPersianRTL),
                        sales_comment = GetRtlSafeText(salesCommentUPersianRTL),
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                case InputMode.Legacy:
                    if (nameLegacy == null || mobileNumberLegacy == null || emailLegacy == null || 
                        companyNameLegacy == null || headOfficeLocationLegacy == null || 
                        projectNameLegacy == null || projectLocationLegacy == null || accountTypeDropdownLegacy == null ||
                        signatureLegacy == null || applicationTypeDropdownLegacy == null || salesCommentLegacy == null)
                    {
                        throw new InvalidOperationException("One or more Legacy Input fields are not assigned.");
                    }

                    return new RegistrationData
                    {
                        name = nameLegacy.text?.Trim() ?? "",
                        mobile_number = mobileNumberLegacy.text?.Trim() ?? "",
                        email = emailLegacy.text?.Trim() ?? "",
                        company_name = companyNameLegacy.text?.Trim() ?? "",
                        head_office_location = headOfficeLocationLegacy.text?.Trim() ?? "",
                        project_name = projectNameLegacy.text?.Trim() ?? "",
                        project_location = projectLocationLegacy.text?.Trim() ?? "",
                        account_type = GetSelectedAccountType(accountTypeDropdownLegacy),
                        signature = signatureLegacy.text?.Trim() ?? "",
                        application_type = GetSelectedApplicationType(applicationTypeDropdownLegacy),
                        sales_comment = salesCommentLegacy.text?.Trim() ?? "",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                default:
                    throw new InvalidOperationException("Invalid input mode.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting current inputs: {ex.Message}");
            throw;
        }
    }

    // Helper method to extract RTL-safe text from UPersian InputFields
    private string GetRtlSafeText(TMP_InputField inputField)
    {
        if (inputField == null) return "";
        
        // Get the text component that has RtlText attached
        var textComponent = inputField.transform.GetChild(2)?.GetComponent<RtlText>();
        if (textComponent != null)
        {
            // Use BaseText to get the original text without RTL processing
            return textComponent.BaseText?.Trim() ?? "";
        }
        
        // Fallback to regular text if RtlText component is not found
        return inputField.text?.Trim() ?? "";
    }

    private string GetSelectedAccountType(Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options == null || dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
            return "";
        return dropdown.options[dropdown.value].text;
    }

    private string GetSelectedAccountType(TMP_Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options == null || dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
            return "";
        return dropdown.options[dropdown.value].text;
    }

    private string GetSelectedApplicationType(Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options == null || dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
            return "";
        return dropdown.options[dropdown.value].text;
    }

    private string GetSelectedApplicationType(TMP_Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options == null || dropdown.value < 0 || dropdown.value >= dropdown.options.Count)
            return "";
        return dropdown.options[dropdown.value].text;
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
            
            // Additional validation for required fields
            if (string.IsNullOrEmpty(data.name) || string.IsNullOrEmpty(data.mobile_number))
            {
                Debug.LogWarning("Name and mobile number are required fields and cannot be empty.");
                return;
            }

            // Step 3: Check for duplicate mobile number
            if (IsMobileExists(data.mobile_number))
            {
                Debug.LogWarning($"Mobile number {data.mobile_number} already exists in the database.");
                return;
            }

            // Step 4: Check for duplicate email if provided
            if (!string.IsNullOrEmpty(data.email) && IsEmailExists(data.email))
            {
                Debug.LogWarning($"Email {data.email} already exists in the database.");
                return;
            }

            // Step 5: Save the registration data
            string csvLine = $"{EscapeCsvField(data.name)},{EscapeCsvField(data.mobile_number)},{EscapeCsvField(data.email)},{EscapeCsvField(data.company_name)},{EscapeCsvField(data.head_office_location)},{EscapeCsvField(data.project_name)},{EscapeCsvField(data.project_location)},{EscapeCsvField(data.account_type)},{EscapeCsvField(data.signature)},{EscapeCsvField(data.application_type)},{EscapeCsvField(data.sales_comment)}";
            
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(csvLine);
            }

            Debug.Log($"Registration saved successfully for {data.name} ({data.mobile_number})");

            // Step 6: Clear all input fields after successful save
            ClearAllInputs();
            Debug.Log("Input fields cleared for new registration.");

            // Step 7: Trigger the save success event
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
            
            if (string.IsNullOrEmpty(data.name) || string.IsNullOrEmpty(data.mobile_number))
            {
                Debug.LogWarning("Name and mobile number are required fields and cannot be empty.");
                return;
            }

            string csvLine = $"{EscapeCsvField(data.name)},{EscapeCsvField(data.mobile_number)},{EscapeCsvField(data.email)},{EscapeCsvField(data.company_name)},{EscapeCsvField(data.head_office_location)},{EscapeCsvField(data.project_name)},{EscapeCsvField(data.project_location)},{EscapeCsvField(data.account_type)},{EscapeCsvField(data.signature)},{EscapeCsvField(data.application_type)},{EscapeCsvField(data.sales_comment)}";
            
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
            string csvLine = $"{EscapeCsvField(data.name)},{EscapeCsvField(data.mobile_number)},{EscapeCsvField(data.email)},{EscapeCsvField(data.company_name)},{EscapeCsvField(data.head_office_location)},{EscapeCsvField(data.project_name)},{EscapeCsvField(data.project_location)},{EscapeCsvField(data.account_type)},{EscapeCsvField(data.signature)},{EscapeCsvField(data.application_type)},{EscapeCsvField(data.sales_comment)}";
            
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

    // Legacy method for backward compatibility
    public void SaveNameAndPhone(string name, string phone)
    {
        var data = new RegistrationData
        {
            name = name?.Trim() ?? "",
            mobile_number = phone?.Trim() ?? "",
            email = "",
            company_name = "",
            head_office_location = "",
            project_name = "",
            project_location = "",
            account_type = "",
            signature = "",
            application_type = "",
            sales_comment = "",
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        SaveRegistrationData(data);
    }

    public string ReadAllData()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                InitializeFile();
                return CSV_HEADER;
            }

            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to read file: {ex.Message}");
            return string.Empty;
        }
    }

    public bool IsMobileExists(string mobileToCheck)
    {
        if (string.IsNullOrEmpty(mobileToCheck))
            return false;

        try
        {
            if (!File.Exists(filePath))
                return false;

            using (var reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine) { isFirstLine = false; continue; }
                    
                    var parts = ParseCsvLine(line);
                    if (parts.Count > 1 && parts[1].Trim() == mobileToCheck.Trim())
                        return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to check mobile number: {ex.Message}");
            return false;
        }
    }

    public bool IsEmailExists(string emailToCheck)
    {
        if (string.IsNullOrEmpty(emailToCheck))
            return false;

        try
        {
            if (!File.Exists(filePath))
                return false;

            using (var reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine) { isFirstLine = false; continue; }
                    
                    var parts = ParseCsvLine(line);
                    if (parts.Count > 2 && parts[2].Trim().Equals(emailToCheck.Trim(), StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to check email: {ex.Message}");
            return false;
        }
    }

    // Helper method to escape CSV fields containing commas, quotes, or newlines
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }
        return field;
    }

    // Helper method to parse CSV lines properly handling escaped fields
    private List<string> ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();
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
                    
                    var parts = ParseCsvLine(line);
                    if (parts.Count >= 11) // 11 fields now
                    {
                        registrations.Add(new RegistrationData
                        {
                            name = parts[0].Trim(),
                            mobile_number = parts[1].Trim(),
                            email = parts[2].Trim(),
                            company_name = parts[3].Trim(),
                            head_office_location = parts[4].Trim(),
                            project_name = parts[5].Trim(),
                            project_location = parts[6].Trim(),
                            account_type = parts[7].Trim(),
                            signature = parts[8].Trim(),
                            application_type = parts[9].Trim(),
                            sales_comment = parts[10].Trim(),
                            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        });
                    }
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
        
        // Convert to raw data format for API: all 11 fields separated by commas
        return $"{data.name},{data.mobile_number},{data.email},{data.company_name},{data.head_office_location},{data.project_name},{data.project_location},{data.account_type},{data.signature},{data.application_type},{data.sales_comment}";
    }

    public int GetRegistrationCount()
    {
        return GetAllRegistrationsAsObjects().Count;
    }

    // Clear all input fields
    public void ClearAllInputs()
    {
        try
        {
            switch (inputMode)
            {
                case InputMode.TMPRO:
                    if (nameTMP != null) nameTMP.text = "";
                    if (mobileNumberTMP != null) mobileNumberTMP.text = "";
                    if (emailTMP != null) emailTMP.text = "";
                    if (companyNameTMP != null) companyNameTMP.text = "";
                    if (headOfficeLocationTMP != null) headOfficeLocationTMP.text = "";
                    if (projectNameTMP != null) projectNameTMP.text = "";
                    if (projectLocationTMP != null) projectLocationTMP.text = "";
                    if (accountTypeDropdownTMP != null) accountTypeDropdownTMP.value = 0;
                    if (signatureTMP != null) signatureTMP.text = "";
                    if (applicationTypeDropdownTMP != null) applicationTypeDropdownTMP.value = 0;
                    if (salesCommentTMP != null) salesCommentTMP.text = "";
                    break;

                case InputMode.UPERSIANRTL:
                    if (nameUPersianRTL != null) nameUPersianRTL.text = "";
                    if (mobileNumberUPersianRTL != null) mobileNumberUPersianRTL.text = "";
                    if (emailUPersianRTL != null) emailUPersianRTL.text = "";
                    if (companyNameUPersianRTL != null) companyNameUPersianRTL.text = "";
                    if (headOfficeLocationUPersianRTL != null) headOfficeLocationUPersianRTL.text = "";
                    if (projectNameUPersianRTL != null) projectNameUPersianRTL.text = "";
                    if (projectLocationUPersianRTL != null) projectLocationUPersianRTL.text = "";
                    if (accountTypeDropdownUPersianRTL != null) accountTypeDropdownUPersianRTL.value = 0;
                    if (signatureUPersianRTL != null) signatureUPersianRTL.text = "";
                    if (applicationTypeDropdownUPersianRTL != null) applicationTypeDropdownUPersianRTL.value = 0;
                    if (salesCommentUPersianRTL != null) salesCommentUPersianRTL.text = "";
                    break;

                case InputMode.Legacy:
                    if (nameLegacy != null) nameLegacy.text = "";
                    if (mobileNumberLegacy != null) mobileNumberLegacy.text = "";
                    if (emailLegacy != null) emailLegacy.text = "";
                    if (companyNameLegacy != null) companyNameLegacy.text = "";
                    if (headOfficeLocationLegacy != null) headOfficeLocationLegacy.text = "";
                    if (projectNameLegacy != null) projectNameLegacy.text = "";
                    if (projectLocationLegacy != null) projectLocationLegacy.text = "";
                    if (accountTypeDropdownLegacy != null) accountTypeDropdownLegacy.value = 0;
                    if (signatureLegacy != null) signatureLegacy.text = "";
                    if (applicationTypeDropdownLegacy != null) applicationTypeDropdownLegacy.value = 0;
                    if (salesCommentLegacy != null) salesCommentLegacy.text = "";
                    break;
            }
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
            
            // Check if all required fields are empty (Sales Comment is optional)
            if (string.IsNullOrEmpty(data.name))
            {
                Debug.LogWarning("Name is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.mobile_number))
            {
                Debug.LogWarning("Mobile number is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.email))
            {
                Debug.LogWarning("Email is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.company_name))
            {
                Debug.LogWarning("Company name is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.head_office_location))
            {
                Debug.LogWarning("Head office location is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.project_name))
            {
                Debug.LogWarning("Project name is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.project_location))
            {
                Debug.LogWarning("Project location is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.account_type))
            {
                Debug.LogWarning("Account type is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.signature))
            {
                Debug.LogWarning("Signature is required.");
                return false;
            }
            
            if (string.IsNullOrEmpty(data.application_type))
            {
                Debug.LogWarning("Application type is required.");
                return false;
            }
            
            // Email format validation if provided
            if (!string.IsNullOrEmpty(data.email) && !IsValidEmail(data.email))
            {
                Debug.LogWarning("Invalid email format.");
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Validation error: {ex.Message}");
            return false;
        }
    }

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

    private void OnDestroy()
    {
        // No need to close registrationWriter since we're not using it anymore
    }
}