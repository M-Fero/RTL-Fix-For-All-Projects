using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UPersian.Components;
using System.Linq;

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

    // Define the standard field keys used in registration
    private static readonly List<string> STANDARD_FIELD_KEYS = new List<string>
    {
        "name", "mobile_number", "email", "company_name", "head_office_location",
        "project_name", "project_location", "account_type", "signature",
        "application_type", "sales_comment"
    };

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

    [Header("Required Field Warning Labels")]
    [Tooltip("TextMeshPro component to show 'This field is required' for Name field.")]
    [SerializeField] private TextMeshProUGUI nameRequiredLabel;
    [Tooltip("TextMeshPro component to show 'This field is required' for Mobile Number field.")]
    [SerializeField] private TextMeshProUGUI mobileNumberRequiredLabel;
    [Tooltip("TextMeshPro component to show 'This field is required' for Email field.")]
    [SerializeField] private TextMeshProUGUI emailRequiredLabel;
    [Tooltip("TextMeshPro component to show 'This field is required' for Company Name field.")]
    [SerializeField] private TextMeshProUGUI companyNameRequiredLabel;
    [Tooltip("TextMeshPro component to show 'This field is required' for Head Office Location field.")]
    [SerializeField] private TextMeshProUGUI headOfficeLocationRequiredLabel;
    [Tooltip("TextMeshPro component to show 'This field is required' for Project Name field.")]
    [SerializeField] private TextMeshProUGUI projectNameRequiredLabel;
    [Tooltip("TextMeshPro component to show 'This field is required' for Project Location field.")]
    [SerializeField] private TextMeshProUGUI projectLocationRequiredLabel;
    [Tooltip("TextMeshPro component to show 'This field is required' for Account Type field.")]
    [SerializeField] private TextMeshProUGUI accountTypeRequiredLabel;
    [Tooltip("TextMeshPro component to show 'This field is required' for Signature field.")]
    [SerializeField] private TextMeshProUGUI signatureRequiredLabel;
    [Tooltip("TextMeshPro component to show 'This field is required' for Application Type field.")]
    [SerializeField] private TextMeshProUGUI applicationTypeRequiredLabel;

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
    private const string CSV_HEADER = "Name,Mobile_Number,Email,Company_Name,Head_Office_Location,Project_Name,Project_Location,Account_Type,Signature,Application_Type,Sales_Comment";

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
        InitializeFile();

        // Validate RTL setup
        ValidateRTLSetup();

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
    
    public void ReSetupTheFile()
    {
        // Use dataPath for file location
        filePath = Application.persistentDataPath + "/" + fileName;
        
        InitializeFile();

        // Validate RTL setup
        ValidateRTLSetup();

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

    // Validate that RTL components are properly set up
    private void ValidateRTLSetup()
    {
        var missingFields = new List<string>();
        
        if (nameUPersianRTL == null) missingFields.Add("Name");
        if (mobileNumberUPersianRTL == null) missingFields.Add("Mobile Number");
        if (emailUPersianRTL == null) missingFields.Add("Email");
        if (companyNameUPersianRTL == null) missingFields.Add("Company Name");
        if (headOfficeLocationUPersianRTL == null) missingFields.Add("Head Office Location");
        if (projectNameUPersianRTL == null) missingFields.Add("Project Name");
        if (projectLocationUPersianRTL == null) missingFields.Add("Project Location");
        if (accountTypeDropdownUPersianRTL == null) missingFields.Add("Account Type Dropdown");
        if (signatureUPersianRTL == null) missingFields.Add("Signature");
        if (applicationTypeDropdownUPersianRTL == null) missingFields.Add("Application Type Dropdown");
        if (salesCommentUPersianRTL == null) missingFields.Add("Sales Comment");
        
        if (missingFields.Count > 0)
        {
            Debug.LogWarning($"Missing RTL field assignments: {string.Join(", ", missingFields)}");
        }
        else
        {
            Debug.Log("All RTL input fields are properly assigned.");
        }

        // Initialize required field labels
        InitializeRequiredFieldLabels();
    }

    // Initialize required field warning labels
    private void InitializeRequiredFieldLabels()
    {
        const string requiredText = "This field is required";
        Color warningColor = Color.red;

        // Initialize each label if it exists
        if (nameRequiredLabel != null)
        {
            nameRequiredLabel.text = requiredText;
            nameRequiredLabel.color = warningColor;
            nameRequiredLabel.gameObject.SetActive(false);
        }

        if (mobileNumberRequiredLabel != null)
        {
            mobileNumberRequiredLabel.text = requiredText;
            mobileNumberRequiredLabel.color = warningColor;
            mobileNumberRequiredLabel.gameObject.SetActive(false);
        }

        if (emailRequiredLabel != null)
        {
            emailRequiredLabel.text = requiredText;
            emailRequiredLabel.color = warningColor;
            emailRequiredLabel.gameObject.SetActive(false);
        }

        if (companyNameRequiredLabel != null)
        {
            companyNameRequiredLabel.text = requiredText;
            companyNameRequiredLabel.color = warningColor;
            companyNameRequiredLabel.gameObject.SetActive(false);
        }

        if (headOfficeLocationRequiredLabel != null)
        {
            headOfficeLocationRequiredLabel.text = requiredText;
            headOfficeLocationRequiredLabel.color = warningColor;
            headOfficeLocationRequiredLabel.gameObject.SetActive(false);
        }

        if (projectNameRequiredLabel != null)
        {
            projectNameRequiredLabel.text = requiredText;
            projectNameRequiredLabel.color = warningColor;
            projectNameRequiredLabel.gameObject.SetActive(false);
        }

        if (projectLocationRequiredLabel != null)
        {
            projectLocationRequiredLabel.text = requiredText;
            projectLocationRequiredLabel.color = warningColor;
            projectLocationRequiredLabel.gameObject.SetActive(false);
        }

        if (accountTypeRequiredLabel != null)
        {
            accountTypeRequiredLabel.text = requiredText;
            accountTypeRequiredLabel.color = warningColor;
            accountTypeRequiredLabel.gameObject.SetActive(false);
        }

        if (signatureRequiredLabel != null)
        {
            signatureRequiredLabel.text = requiredText;
            signatureRequiredLabel.color = warningColor;
            signatureRequiredLabel.gameObject.SetActive(false);
        }

        if (applicationTypeRequiredLabel != null)
        {
            applicationTypeRequiredLabel.text = requiredText;
            applicationTypeRequiredLabel.color = warningColor;
            applicationTypeRequiredLabel.gameObject.SetActive(false);
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
                    writer.WriteLine(CSV_HEADER);
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
            if (nameUPersianRTL == null || mobileNumberUPersianRTL == null || emailUPersianRTL == null || 
                companyNameUPersianRTL == null || headOfficeLocationUPersianRTL == null || 
                projectNameUPersianRTL == null || projectLocationUPersianRTL == null || accountTypeDropdownUPersianRTL == null ||
                signatureUPersianRTL == null || applicationTypeDropdownUPersianRTL == null || salesCommentUPersianRTL == null)
            {
                throw new InvalidOperationException("One or more UPersian RTL Input fields are not assigned.");
            }

            var data = new RegistrationData();
            
            // Add all the standard fields using the dynamic field system
            data.AddField("name", GetRtlSafeText(nameUPersianRTL));
            data.AddField("mobile_number", GetRtlSafeText(mobileNumberUPersianRTL));
            data.AddField("email", GetRtlSafeText(emailUPersianRTL));
            data.AddField("company_name", GetRtlSafeText(companyNameUPersianRTL));
            data.AddField("head_office_location", GetRtlSafeText(headOfficeLocationUPersianRTL));
            data.AddField("project_name", GetRtlSafeText(projectNameUPersianRTL));
            data.AddField("project_location", GetRtlSafeText(projectLocationUPersianRTL));
            data.AddField("account_type", GetSelectedAccountType(accountTypeDropdownUPersianRTL));
            data.AddField("signature", GetRtlSafeText(signatureUPersianRTL));
            data.AddField("application_type", GetSelectedApplicationType(applicationTypeDropdownUPersianRTL));
            data.AddField("sales_comment", GetRtlSafeText(salesCommentUPersianRTL));
            data.AddField("timestamp", GetTimestamp());

            return data;
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

    private string GetSelectedAccountType(TMP_Dropdown dropdown)
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
            if (string.IsNullOrEmpty(data.GetFieldValue("name")) || string.IsNullOrEmpty(data.GetFieldValue("mobile_number")) ||
                string.IsNullOrEmpty(data.GetFieldValue("email")) || string.IsNullOrEmpty(data.GetFieldValue("company_name")) ||
                string.IsNullOrEmpty(data.GetFieldValue("head_office_location")) || string.IsNullOrEmpty(data.GetFieldValue("project_name")) ||
                string.IsNullOrEmpty(data.GetFieldValue("project_location")) || string.IsNullOrEmpty(data.GetFieldValue("account_type")) ||
                string.IsNullOrEmpty(data.GetFieldValue("signature")) || string.IsNullOrEmpty(data.GetFieldValue("application_type")))
            {
                Debug.LogWarning("All required fields must be filled. Sales comment is optional.");
                return;
            }

            // Step 5: Save the registration data
            string csvLine = data.ToCsvLine(STANDARD_FIELD_KEYS);
            
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(csvLine);
            }

            Debug.Log($"Registration saved successfully for {data.GetFieldValue("name")} ({data.GetFieldValue("mobile_number")})");

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
            
            if (string.IsNullOrEmpty(data.GetFieldValue("name")) || string.IsNullOrEmpty(data.GetFieldValue("mobile_number")))
            {
                Debug.LogWarning("Name and mobile number are required fields and cannot be empty.");
                return;
            }

            string csvLine = data.ToCsvLine(STANDARD_FIELD_KEYS);
            
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
            string csvLine = data.ToCsvLine(STANDARD_FIELD_KEYS);
            
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
        var data = new RegistrationData();
        data.AddField("name", name?.Trim() ?? "");
        data.AddField("mobile_number", phone?.Trim() ?? "");
        data.AddField("email", "");
        data.AddField("company_name", "");
        data.AddField("head_office_location", "");
        data.AddField("project_name", "");
        data.AddField("project_location", "");
        data.AddField("account_type", "");
        data.AddField("signature", "");
        data.AddField("application_type", "");
        data.AddField("sales_comment", "");
        data.AddField("timestamp", GetTimestamp());
        
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

    // Helper method to handle sales comment field - returns "Null" if empty
    private string ProcessSalesComment(string salesComment)
    {
        return string.IsNullOrEmpty(salesComment?.Trim()) ? "Null" : salesComment.Trim();
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
                    data.FromCsvLine(line, STANDARD_FIELD_KEYS);
                    
                    // Set timestamp for internal use
                    data.SetFieldValue("timestamp", GetTimestamp());
                    
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
        
        // Convert to raw data format for API: all 11 fields separated by commas (no timestamp)
        var fieldsWithoutTimestamp = STANDARD_FIELD_KEYS.Where(key => key != "timestamp").ToList();
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
            return $"Total registrations: {registrations.Count}\nFile path: {filePath}\nFile exists: {File.Exists(filePath)}";
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
            
            // Check required fields (Sales Comment is optional)
            if (string.IsNullOrEmpty(data.GetFieldValue("name")))
            {
                Debug.LogWarning("Name is required.");
                ShowRequiredLabel(nameRequiredLabel);
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(data.GetFieldValue("mobile_number")))
            {
                Debug.LogWarning("Mobile number is required.");
                ShowRequiredLabel(mobileNumberRequiredLabel);
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(data.GetFieldValue("email")))
            {
                Debug.LogWarning("Email is required.");
                ShowRequiredLabel(emailRequiredLabel);
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(data.GetFieldValue("company_name")))
            {
                Debug.LogWarning("Company name is required.");
                ShowRequiredLabel(companyNameRequiredLabel);
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(data.GetFieldValue("head_office_location")))
            {
                Debug.LogWarning("Head office location is required.");
                ShowRequiredLabel(headOfficeLocationRequiredLabel);
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(data.GetFieldValue("project_name")))
            {
                Debug.LogWarning("Project name is required.");
                ShowRequiredLabel(projectNameRequiredLabel);
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(data.GetFieldValue("project_location")))
            {
                Debug.LogWarning("Project location is required.");
                ShowRequiredLabel(projectLocationRequiredLabel);
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(data.GetFieldValue("account_type")))
            {
                Debug.LogWarning("Account type must be selected.");
                ShowRequiredLabel(accountTypeRequiredLabel);
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(data.GetFieldValue("signature")))
            {
                Debug.LogWarning("Signature is required.");
                ShowRequiredLabel(signatureRequiredLabel);
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(data.GetFieldValue("application_type")))
            {
                Debug.LogWarning("Application type must be selected.");
                ShowRequiredLabel(applicationTypeRequiredLabel);
                isValid = false;
            }
            
            // Email format validation
            string email = data.GetFieldValue("email");
            if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
            {
                Debug.LogWarning("Invalid email format.");
                ShowRequiredLabel(emailRequiredLabel, "Invalid email format");
                isValid = false;
            }
            
            // Mobile number basic validation (should contain only digits and common characters)
            string mobileNumber = data.GetFieldValue("mobile_number");
            if (!string.IsNullOrEmpty(mobileNumber) && !IsValidMobileNumber(mobileNumber))
            {
                Debug.LogWarning("Invalid mobile number format.");
                ShowRequiredLabel(mobileNumberRequiredLabel, "Invalid mobile number format");
                isValid = false;
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
        if (nameRequiredLabel != null) nameRequiredLabel.gameObject.SetActive(false);
        if (mobileNumberRequiredLabel != null) mobileNumberRequiredLabel.gameObject.SetActive(false);
        if (emailRequiredLabel != null) emailRequiredLabel.gameObject.SetActive(false);
        if (companyNameRequiredLabel != null) companyNameRequiredLabel.gameObject.SetActive(false);
        if (headOfficeLocationRequiredLabel != null) headOfficeLocationRequiredLabel.gameObject.SetActive(false);
        if (projectNameRequiredLabel != null) projectNameRequiredLabel.gameObject.SetActive(false);
        if (projectLocationRequiredLabel != null) projectLocationRequiredLabel.gameObject.SetActive(false);
        if (accountTypeRequiredLabel != null) accountTypeRequiredLabel.gameObject.SetActive(false);
        if (signatureRequiredLabel != null) signatureRequiredLabel.gameObject.SetActive(false);
        if (applicationTypeRequiredLabel != null) applicationTypeRequiredLabel.gameObject.SetActive(false);
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

    private void OnDestroy()
    {
        // Cleanup: remove listener from button to prevent memory leaks
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(SaveButton);
        }
    }
}