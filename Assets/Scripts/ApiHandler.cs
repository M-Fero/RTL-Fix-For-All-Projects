using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

[System.Serializable]
public class ContractorData
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
}

[System.Serializable]
public class ApiContractorData
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
    public string app_type;
    public string sales_comment;
}

public class ApiHandler : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text statusText;
    public Button syncButton;
    public Image syncProgressImage;

    [Header("API Settings")]
    [Tooltip("The API URL endpoint for sending registration data")]
    public string apiUrl = "https://tempweb90.com/knauf/api.php";

    [Header("Batch Processing")]
    [Tooltip("Number of records to send in each batch array")]
    [SerializeField] private int batchSize = 50; // Changed back
                                                 // to 50 for array batching
    [Tooltip("Base delay between batches in seconds")]
    [SerializeField] private float baseBatchDelay = 10f;
    [Tooltip("Delay between individual requests in a batch (seconds)")]
    [SerializeField] private float requestDelay = 0.5f;
    [Tooltip("Maximum delay between batches based on response time")]
    [SerializeField] private float maxResponseBasedDelay = 60f;

    [Header("File Management")]
    [Tooltip("Automatically rename processed CSV files")]
    [SerializeField] private bool autoRenameAfterSync = true;
    [Tooltip("Prefix for renamed files (e.g., 'Uploaded_' results in 'Uploaded_1.csv')")]
    [SerializeField] private string renamedFilePrefix = "Uploaded_";

    [Header("CSV Configuration")]
    [Tooltip("CSV file name in the Assets folder")]
    public string csvFileName = "data.csv";
    [Tooltip("Whether the CSV file has headers")]
    public bool hasHeader = true;
    [Tooltip("CSV delimiter character")]
    public char delimiter = ',';

    [Header("Status Messages")]
    public string messageIdle = "Ready to sync";
    public string messageChecking = "Checking connection...";
    public string messageSyncing = "Syncing data...";
    public string messageSuccess = "Sync completed";
    public string messageAlreadyExists = "Already registered";
    public string messageError = "Sync error";
    public string messageNoInternet = "No internet connection";
    public string messageNoData = "No data to sync";

    [Header("API Response Data (Inspector)")]
    public bool lastSyncSuccess;
    public string lastApiMessage;
    public int pendingSyncCount = 0;

    [Header("Registration Integration")]
    [Tooltip("Reference to the Registration component for file reset after processing")]
    [SerializeField] private Registration registrationComponent;

    // Events for external scripts to subscribe to
    public System.Action<ContractorData[]> OnDataLoaded;
    public System.Action<string> OnError;

    private bool isSyncing = false;
    private Coroutine progressAnimationCoroutine;
    private string csvFilePath;
    private ContractorData[] csvData;

    public static ApiHandler Instance { get; private set; }

    [System.Serializable]
    private class ApiResponse
    {
        public bool success;
        public string message;
        public string count;
    }

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool debugParsingDetails = true;
    [SerializeField] private bool debugCsvLineByLine = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeUI();
        SetupCsvPath();
        LoadCsvData();
        UpdateStatusDisplay(messageIdle);
        UpdateButtonStates();
    }

    private void SetupCsvPath()
    {
        // Use dataPath for file location
        csvFilePath = Application.dataPath + "/" + csvFileName;
        Debug.Log($"[ApiHandler] CSV file path: {csvFilePath}");
    }

    private void LoadCsvData()
    {
        try
        {
            if (enableDebugLogging)
                Debug.Log($"[ApiHandler DEBUG] Starting CSV data loading from: {csvFilePath}");

            if (!File.Exists(csvFilePath))
            {
                if (enableDebugLogging)
                    Debug.LogWarning($"[ApiHandler DEBUG] CSV file not found at: {csvFilePath}");
                csvData = new ContractorData[0];
                return;
            }

            string content = File.ReadAllText(csvFilePath, Encoding.UTF8);
            if (enableDebugLogging)
                Debug.Log($"[ApiHandler DEBUG] File content length: {content.Length} characters");

            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (enableDebugLogging)
                Debug.Log($"[ApiHandler DEBUG] Split into {lines.Length} lines");

            if (lines.Length == 0)
            {
                if (enableDebugLogging)
                    Debug.LogWarning("[ApiHandler DEBUG] CSV file is empty after splitting");
                csvData = new ContractorData[0];
                return;
            }

            if (debugCsvLineByLine)
            {
                Debug.Log("[ApiHandler DEBUG] Raw CSV lines:");
                for (int i = 0; i < lines.Length; i++)
                {
                    Debug.Log($"[ApiHandler DEBUG] Line {i}: '{lines[i]}'");
                }
            }

            csvData = ParseCsvToContractorData(lines);
            pendingSyncCount = csvData.Length;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[ApiHandler DEBUG] Successfully loaded {csvData.Length} records from CSV");
                Debug.Log($"[ApiHandler DEBUG] Pending sync count set to: {pendingSyncCount}");
            }
            
            // Trigger OnDataLoaded event
            OnDataLoaded?.Invoke(csvData);
            
            if (enableDebugLogging)
                Debug.Log("[ApiHandler DEBUG] OnDataLoaded event triggered");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ApiHandler DEBUG] Error loading CSV: {ex.Message}");
            Debug.LogError($"[ApiHandler DEBUG] Stack trace: {ex.StackTrace}");
            csvData = new ContractorData[0];
            OnError?.Invoke($"Error loading CSV: {ex.Message}");
        }
    }

    private ContractorData[] ParseCsvToContractorData(string[] lines)
    {
        if (enableDebugLogging)
            Debug.Log($"[ApiHandler DEBUG] Starting CSV parsing with {lines.Length} lines");

        List<ContractorData> contractors = new List<ContractorData>();
        string[] headers = null;
        int startIndex = 0;

        // Parse headers if they exist
        if (hasHeader && lines.Length > 0)
        {
            headers = ParseCsvLine(lines[0]);
            startIndex = 1;
            
            if (debugParsingDetails)
            {
                Debug.Log($"[ApiHandler DEBUG] Headers found: [{string.Join(", ", headers)}]");
                Debug.Log($"[ApiHandler DEBUG] Header count: {headers.Length}");
            }
        }
        else if (enableDebugLogging)
        {
            Debug.Log("[ApiHandler DEBUG] No headers mode - using positional mapping");
        }

        // Parse data rows
        int successfulRows = 0;
        int failedRows = 0;

        for (int i = startIndex; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
            {
                if (debugParsingDetails)
                    Debug.Log($"[ApiHandler DEBUG] Skipping empty line at index {i}");
                continue;
            }

            if (debugParsingDetails)
                Debug.Log($"[ApiHandler DEBUG] Processing line {i}: '{line}'");

            string[] values = ParseCsvLine(line);
            if (debugParsingDetails)
                Debug.Log($"[ApiHandler DEBUG] Parsed {values.Length} values: [{string.Join(", ", values)}]");

            ContractorData contractor = CreateContractorFromCsvRow(values, headers);
            
            if (contractor != null)
            {
                contractors.Add(contractor);
                successfulRows++;
                
                if (debugParsingDetails)
                {
                    Debug.Log($"[ApiHandler DEBUG] Successfully created contractor:");
                    Debug.Log($"[ApiHandler DEBUG]   Name: '{contractor.name}'");
                    Debug.Log($"[ApiHandler DEBUG]   Mobile: '{contractor.mobile_number}'");
                    Debug.Log($"[ApiHandler DEBUG]   Email: '{contractor.email}'");
                    Debug.Log($"[ApiHandler DEBUG]   Company: '{contractor.company_name}'");
                    Debug.Log($"[ApiHandler DEBUG]   Head Office: '{contractor.head_office_location}'");
                    Debug.Log($"[ApiHandler DEBUG]   Project: '{contractor.project_name}'");
                    Debug.Log($"[ApiHandler DEBUG]   Project Location: '{contractor.project_location}'");
                    Debug.Log($"[ApiHandler DEBUG]   Account Type: '{contractor.account_type}'");
                    Debug.Log($"[ApiHandler DEBUG]   Signature: '{contractor.signature}'");
                    Debug.Log($"[ApiHandler DEBUG]   Application Type: '{contractor.application_type}'");
                    Debug.Log($"[ApiHandler DEBUG]   Sales Comment: '{contractor.sales_comment}'");
                }
            }
            else
            {
                failedRows++;
                if (enableDebugLogging)
                    Debug.LogWarning($"[ApiHandler DEBUG] Failed to create contractor from line {i}");
            }
        }

        if (enableDebugLogging)
        {
            Debug.Log($"[ApiHandler DEBUG] Parsing completed:");
            Debug.Log($"[ApiHandler DEBUG]   Successful rows: {successfulRows}");
            Debug.Log($"[ApiHandler DEBUG]   Failed rows: {failedRows}");
            Debug.Log($"[ApiHandler DEBUG]   Total contractors: {contractors.Count}");
        }

        return contractors.ToArray();
    }

    private string[] ParseCsvLine(string line)
    {
        if (debugParsingDetails)
            Debug.Log($"[ApiHandler DEBUG] Parsing CSV line: '{line}'");

        List<string> values = new List<string>();
        bool inQuotes = false;
        bool inDoubleQuotes = false;
        string currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            char nextChar = i + 1 < line.Length ? line[i + 1] : '\0';

            if (debugParsingDetails && i < 50) // Limit debug output for long lines
            {
                Debug.Log($"[ApiHandler DEBUG]   Char {i}: '{c}' (inQuotes: {inQuotes}, inDoubleQuotes: {inDoubleQuotes})");
            }

            // Handle different quote types
            if (c == '"' && !inQuotes)
            {
                inDoubleQuotes = !inDoubleQuotes;
                if (debugParsingDetails)
                    Debug.Log($"[ApiHandler DEBUG]   Double quote toggle: {inDoubleQuotes}");
            }
            else if (c == '\'' && !inDoubleQuotes)
            {
                inQuotes = !inQuotes;
                if (debugParsingDetails)
                    Debug.Log($"[ApiHandler DEBUG]   Single quote toggle: {inQuotes}");
            }
            // Handle escaped quotes
            else if (c == '"' && inDoubleQuotes && nextChar == '"')
            {
                currentValue += c;
                i++; // Skip the next quote
                if (debugParsingDetails)
                    Debug.Log($"[ApiHandler DEBUG]   Escaped double quote added");
            }
            else if (c == '\'' && inQuotes && nextChar == '\'')
            {
                currentValue += c;
                i++; // Skip the next quote
                if (debugParsingDetails)
                    Debug.Log($"[ApiHandler DEBUG]   Escaped single quote added");
            }
            // Handle delimiter when not in quotes
            else if (c == delimiter && !inQuotes && !inDoubleQuotes)
            {
                string cleanedValue = CleanValue(currentValue);
                values.Add(cleanedValue);
                if (debugParsingDetails)
                    Debug.Log($"[ApiHandler DEBUG]   Value added: '{cleanedValue}' (raw: '{currentValue}')");
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }

        // Add the last value
        string finalValue = CleanValue(currentValue);
        values.Add(finalValue);
        if (debugParsingDetails)
            Debug.Log($"[ApiHandler DEBUG]   Final value added: '{finalValue}' (raw: '{currentValue}')");

        if (debugParsingDetails)
            Debug.Log($"[ApiHandler DEBUG] Line parsing complete. Total values: {values.Count}");

        return values.ToArray();
    }

    private string CleanValue(string value)
    {
        if (debugParsingDetails)
            Debug.Log($"[ApiHandler DEBUG] Cleaning value: '{value}'");

        if (string.IsNullOrEmpty(value))
        {
            if (debugParsingDetails)
                Debug.Log("[ApiHandler DEBUG] Value is null or empty, returning as-is");
            return value;
        }

        string originalValue = value;
        value = value.Trim();
        
        if (debugParsingDetails && originalValue != value)
            Debug.Log($"[ApiHandler DEBUG] After trim: '{value}'");
        
        // Remove surrounding quotes
        if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
            (value.StartsWith("'") && value.EndsWith("'")))
        {
            string beforeQuoteRemoval = value;
            value = value.Substring(1, value.Length - 2);
            if (debugParsingDetails)
                Debug.Log($"[ApiHandler DEBUG] Removed surrounding quotes: '{beforeQuoteRemoval}' -> '{value}'");
        }

        // Handle escaped quotes
        string beforeEscapeHandling = value;
        value = value.Replace("\"\"", "\"").Replace("''", "'");
        if (debugParsingDetails && beforeEscapeHandling != value)
            Debug.Log($"[ApiHandler DEBUG] After escape handling: '{beforeEscapeHandling}' -> '{value}'");

        if (debugParsingDetails)
            Debug.Log($"[ApiHandler DEBUG] Final cleaned value: '{value}'");

        return value;
    }

    private ContractorData CreateContractorFromCsvRow(string[] values, string[] headers)
    {
        if (debugParsingDetails)
            Debug.Log($"[ApiHandler DEBUG] Creating contractor from {values.Length} values");

        try
        {
            ContractorData contractor = new ContractorData();

            // Map values based on headers or default positions
            if (hasHeader && headers != null)
            {
                if (debugParsingDetails)
                    Debug.Log($"[ApiHandler DEBUG] Using header-based mapping with {headers.Length} headers");

                for (int i = 0; i < values.Length && i < headers.Length; i++)
                {
                    string header = headers[i].ToLower().Trim();
                    string value = i < values.Length ? values[i] : "";

                    if (debugParsingDetails)
                        Debug.Log($"[ApiHandler DEBUG] Mapping header '{header}' -> value '{value}'");

                    switch (header)
                    {
                        case "name":
                            contractor.name = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set name: '{value}'");
                            break;
                        case "mobile_number":
                        case "mobile number":
                        case "phone":
                            contractor.mobile_number = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set mobile_number: '{value}'");
                            break;
                        case "email":
                            contractor.email = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set email: '{value}'");
                            break;
                        case "company_name":
                        case "company name":
                        case "company":
                            contractor.company_name = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set company_name: '{value}'");
                            break;
                        case "head_office_location":
                        case "head office location":
                        case "office location":
                            contractor.head_office_location = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set head_office_location: '{value}'");
                            break;
                        case "project_name":
                        case "project name":
                        case "project":
                            contractor.project_name = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set project_name: '{value}'");
                            break;
                        case "project_location":
                        case "project location":
                            contractor.project_location = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set project_location: '{value}'");
                            break;
                        case "account_type":
                        case "account type":
                        case "type":
                            contractor.account_type = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set account_type: '{value}'");
                            break;
                        case "signature":
                            contractor.signature = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set signature: '{value}'");
                            break;
                        case "application_type":
                        case "application type":
                            contractor.application_type = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set application_type: '{value}'");
                            break;
                        case "sales_comment":
                        case "sales comment":
                        case "comment":
                            contractor.sales_comment = value;
                            if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Set sales_comment: '{value}'");
                            break;
                        default:
                            if (debugParsingDetails) Debug.LogWarning($"[ApiHandler DEBUG] Unrecognized header: '{header}'");
                            break;
                    }
                }
            }
            else
            {
                if (debugParsingDetails)
                    Debug.Log($"[ApiHandler DEBUG] Using positional mapping for {values.Length} values");

                // Default position mapping (without headers)
                if (values.Length > 0) { contractor.name = values[0]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 0 -> name: '{values[0]}'"); }
                if (values.Length > 1) { contractor.mobile_number = values[1]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 1 -> mobile_number: '{values[1]}'"); }
                if (values.Length > 2) { contractor.email = values[2]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 2 -> email: '{values[2]}'"); }
                if (values.Length > 3) { contractor.company_name = values[3]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 3 -> company_name: '{values[3]}'"); }
                if (values.Length > 4) { contractor.head_office_location = values[4]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 4 -> head_office_location: '{values[4]}'"); }
                if (values.Length > 5) { contractor.project_name = values[5]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 5 -> project_name: '{values[5]}'"); }
                if (values.Length > 6) { contractor.project_location = values[6]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 6 -> project_location: '{values[6]}'"); }
                if (values.Length > 7) { contractor.account_type = values[7]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 7 -> account_type: '{values[7]}'"); }
                if (values.Length > 8) { contractor.signature = values[8]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 8 -> signature: '{values[8]}'"); }
                if (values.Length > 9) { contractor.application_type = values[9]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 9 -> application_type: '{values[9]}'"); }
                if (values.Length > 10) { contractor.sales_comment = values[10]; if (debugParsingDetails) Debug.Log($"[ApiHandler DEBUG] Position 10 -> sales_comment: '{values[10]}'"); }
            }

            if (debugParsingDetails)
                Debug.Log($"[ApiHandler DEBUG] Contractor creation successful");

            return contractor;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ApiHandler DEBUG] Error creating contractor from CSV row: {ex.Message}");
            Debug.LogError($"[ApiHandler DEBUG] Stack trace: {ex.StackTrace}");
            if (debugParsingDetails)
            {
                Debug.LogError($"[ApiHandler DEBUG] Values that caused error: [{string.Join(", ", values)}]");
                if (headers != null)
                    Debug.LogError($"[ApiHandler DEBUG] Headers: [{string.Join(", ", headers)}]");
            }
            return null;
        }
    }

    private void InitializeUI()
    {
        if (syncButton != null)
        {
            syncButton.onClick.AddListener(ManualSyncAllData);
        }

        if (syncProgressImage != null)
        {
            syncProgressImage.fillAmount = 0f;
            syncProgressImage.gameObject.SetActive(false);
        }
    }

    private void UpdateButtonStates()
    {
        if (syncButton != null)
        {
            // Only disable button when actually syncing, not when no data is loaded
            bool shouldBeEnabled = !isSyncing;
            syncButton.interactable = shouldBeEnabled;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[ApiHandler DEBUG] UpdateButtonStates:");
                Debug.Log($"[ApiHandler DEBUG]   Should be enabled: {shouldBeEnabled}");
                Debug.Log($"[ApiHandler DEBUG]   isSyncing: {isSyncing}");
                Debug.Log($"[ApiHandler DEBUG]   csvData != null: {csvData != null}");
                Debug.Log($"[ApiHandler DEBUG]   csvData.Length: {csvData?.Length ?? 0}");
                Debug.Log($"[ApiHandler DEBUG]   Button.interactable set to: {shouldBeEnabled}");
            }
        }
        else if (enableDebugLogging)
        {
            Debug.LogWarning("[ApiHandler DEBUG] syncButton is null in UpdateButtonStates!");
        }
    }

    public void ManualSyncAllData()
    {
        // Check if already syncing
        if (isSyncing)
        {
            Debug.LogWarning("[ApiHandler] Sync already in progress!");
            UpdateStatusDisplay("Sync already in progress...");
            return;
        }

        // Check if we have data to sync
        if (csvData == null || csvData.Length == 0)
        {
            Debug.LogWarning("[ApiHandler] No data to sync! Loading CSV data...");
            UpdateStatusDisplay("No data found - loading CSV...");
            
            // Try to reload CSV data
            LoadCsvData();
            
            // Check again after loading
            if (csvData == null || csvData.Length == 0)
            {
                Debug.LogError("[ApiHandler] Still no data after reload!");
                UpdateStatusDisplay(messageNoData);
                return;
            }
        }

        // Start the sync process
        Debug.Log($"[ApiHandler] Starting sync with {csvData.Length} records");
        StartCoroutine(SyncAllCsvData());
    }

    public void ReloadCsvData()
    {
        if (enableDebugLogging)
            Debug.Log("[ApiHandler DEBUG] Manual CSV data reload triggered");

        LoadCsvData();
        UpdateButtonStates();
        
        if (enableDebugLogging)
            Debug.Log($"[ApiHandler DEBUG] CSV data reloaded. Found {csvData?.Length ?? 0} records");
    }

    // Debug method to manually trigger parsing test
    [ContextMenu("Debug Parse CSV")]
    public void DebugParseCsv()
    {
        Debug.Log("[ApiHandler DEBUG] Manual debug parse triggered");
        bool originalDebugState = enableDebugLogging;
        bool originalParsingState = debugParsingDetails;
        
        // Temporarily enable all debug options
        enableDebugLogging = true;
        debugParsingDetails = true;
        
        LoadCsvData();
        
        // Restore original debug states
        enableDebugLogging = originalDebugState;
        debugParsingDetails = originalParsingState;
        
        Debug.Log("[ApiHandler DEBUG] Manual debug parse completed");
    }

    // Debug method to show current CSV data
    [ContextMenu("Debug Show Current Data")]
    public void DebugShowCurrentData()
    {
        if (csvData == null || csvData.Length == 0)
        {
            Debug.Log("[ApiHandler DEBUG] No CSV data currently loaded");
            return;
        }

        Debug.Log($"[ApiHandler DEBUG] Current CSV data ({csvData.Length} records):");
        for (int i = 0; i < csvData.Length; i++)
        {
            var contractor = csvData[i];
            Debug.Log($"[ApiHandler DEBUG] Record {i + 1}:");
            Debug.Log($"[ApiHandler DEBUG]   Name: '{contractor.name ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Mobile: '{contractor.mobile_number ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Email: '{contractor.email ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Company: '{contractor.company_name ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Head Office: '{contractor.head_office_location ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Project: '{contractor.project_name ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Project Location: '{contractor.project_location ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Account Type: '{contractor.account_type ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Signature: '{contractor.signature ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Application Type: '{contractor.application_type ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   Sales Comment: '{contractor.sales_comment ?? "NULL"}'");
            Debug.Log($"[ApiHandler DEBUG]   ---");
        }
    }

    public IEnumerator SyncAllCsvData()
    {
        if (isSyncing || csvData == null || csvData.Length == 0)
            yield break;

        isSyncing = true;
        UpdateButtonStates();
        UpdateStatusDisplay(messageChecking);
        StartProgressAnimation(true);

        // Check internet connectivity first
        yield return StartCoroutine(CheckInternetConnection());

        if (!lastSyncSuccess)
        {
            UpdateStatusDisplay(messageNoInternet);
            StopProgressAnimation();
            isSyncing = false;
            UpdateButtonStates();
            yield break;
        }

        pendingSyncCount = csvData.Length;
        int totalBatches = Mathf.CeilToInt((float)csvData.Length / batchSize);
        
        UpdateStatusDisplay($"{messageSyncing} ({pendingSyncCount} records in {totalBatches} batch arrays)");
        Debug.Log($"[ApiHandler] Starting batch array sync: {csvData.Length} records in {totalBatches} batches of {batchSize} records each");

        int overallSuccessCount = 0;
        int overallErrorCount = 0;

        // Process data in batches with persistent retry
        for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
        {
            int startIndex = batchIndex * batchSize;
            int endIndex = Mathf.Min(startIndex + batchSize, csvData.Length);
            int batchRecordCount = endIndex - startIndex;

            Debug.Log($"[ApiHandler] Processing batch {batchIndex + 1}/{totalBatches} (records {startIndex + 1}-{endIndex})");
            UpdateStatusDisplay($"Batch {batchIndex + 1}/{totalBatches} - Sending {batchRecordCount} records as array");

            // Create array for this batch
            ContractorData[] batchArray = new ContractorData[batchRecordCount];
            Array.Copy(csvData, startIndex, batchArray, 0, batchRecordCount);

            // Keep retrying this batch until it's processed successfully
            bool batchCompleted = false;
            int batchAttempt = 0;
            int maxBatchAttempts = 10;

            while (!batchCompleted && batchAttempt < maxBatchAttempts)
            {
                batchAttempt++;
                if (batchAttempt > 1)
                {
                    Debug.Log($"[ApiHandler] Retrying batch {batchIndex + 1}, attempt {batchAttempt}/{maxBatchAttempts}");
                    UpdateStatusDisplay($"Retrying batch {batchIndex + 1}, attempt {batchAttempt}/{maxBatchAttempts}");
                }

                // Track batch timing
                float batchStartTime = Time.time;

                // Send the entire batch as an array
                yield return StartCoroutine(SendBatchArray(batchArray));

                float batchTotalTime = Time.time - batchStartTime;

                if (lastSyncSuccess)
                {
                    batchCompleted = true;
                    overallSuccessCount += batchRecordCount; // All records in batch succeeded
                    Debug.Log($"[ApiHandler] Batch {batchIndex + 1} completed successfully in {batchTotalTime:F1}s!");
                    Debug.Log($"[ApiHandler]   {batchRecordCount} records processed as array");
                }
                else
                {
                    overallErrorCount += batchRecordCount; // All records in batch failed
                    
                    // Calculate delay before retrying this batch
                    float retryDelay = Mathf.Min(baseBatchDelay * batchAttempt, maxResponseBasedDelay);
                    
                    // Extra delay for rate limiting
                    if (lastApiMessage != null && (
                        lastApiMessage.ToLower().Contains("rate limit") ||
                        lastApiMessage.ToLower().Contains("too many requests") ||
                        lastApiMessage.ToLower().Contains("limit exceeded") ||
                        lastApiMessage.ToLower().Contains("quota exceeded")))
                    {
                        retryDelay = Mathf.Max(retryDelay * 2f, 15f);
                        Debug.Log($"[ApiHandler] Rate limiting detected, applying extended retry delay of {retryDelay:F1}s");
                    }

                    if (batchAttempt < maxBatchAttempts)
                    {
                        Debug.Log($"[ApiHandler] Batch {batchIndex + 1} failed in {batchTotalTime:F1}s. Retrying in {retryDelay:F1}s...");
                        UpdateStatusDisplay($"Batch {batchIndex + 1} failed - retrying in {retryDelay:F1}s...");

                        // Show countdown for retry delay
                        for (int countdown = Mathf.RoundToInt(retryDelay); countdown > 0; countdown--)
                        {
                            UpdateStatusDisplay($"Retrying batch {batchIndex + 1} in {countdown}s... (Attempt {batchAttempt + 1}/{maxBatchAttempts})");
                            yield return new WaitForSeconds(1f);
                        }

                        // Reset error count for retry
                        overallErrorCount -= batchRecordCount;
                    }
                    else
                    {
                        Debug.LogError($"[ApiHandler] Batch {batchIndex + 1} failed after {maxBatchAttempts} attempts. Moving to next batch.");
                        batchCompleted = true; // Force completion to move to next batch
                    }
                }

                // Update overall progress
                float overallProgress = (float)(overallSuccessCount + overallErrorCount) / csvData.Length;
                if (syncProgressImage != null)
                {
                    syncProgressImage.fillAmount = overallProgress;
                }
            }

            // Delay before next batch (only if not the last batch and batch was successful)
            if (batchIndex < totalBatches - 1 && batchAttempt <= maxBatchAttempts)
            {
                float interBatchDelay = baseBatchDelay;
                Debug.Log($"[ApiHandler] Waiting {interBatchDelay:F1}s before next batch array...");
                UpdateStatusDisplay($"Waiting {interBatchDelay:F1}s before batch {batchIndex + 2}...");

                // Show countdown for inter-batch delay
                for (int countdown = Mathf.RoundToInt(interBatchDelay); countdown > 0; countdown--)
                {
                    UpdateStatusDisplay($"Next batch in {countdown}s... (Batch {batchIndex + 2}/{totalBatches})");
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        // Final status update
        Debug.Log($"[ApiHandler] Batch array sync completed - Total: {csvData.Length}, Success: {overallSuccessCount}, Errors: {overallErrorCount}, Batches: {totalBatches}");
        
        if (overallErrorCount == 0)
        {
            UpdateStatusDisplay($"{messageSuccess} ({overallSuccessCount} records in {totalBatches} batch arrays)");
            
            // Automatically rename the file if sync was completely successful
            if (autoRenameAfterSync)
            {
                yield return new WaitForSeconds(2f); // Brief delay to show success message
                //TODO: Changing the name of The File
                //RenameProcessedCsvFile();
            }
        }
        else
        {
            UpdateStatusDisplay($"Partial sync: {overallSuccessCount} success, {overallErrorCount} failed ({totalBatches} batch arrays)");
        }

        pendingSyncCount = overallErrorCount;
        StopProgressAnimation();
        isSyncing = false;
        UpdateButtonStates();
    }

    private IEnumerator CheckInternetConnection()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://www.google.com"))
        {
            www.timeout = 5;
            yield return www.SendWebRequest();

            lastSyncSuccess = www.result == UnityWebRequest.Result.Success;
        }
    }

    private IEnumerator SendSingleRecord(ContractorData record)
    {
        // Convert ContractorData to JSON format for API
        string jsonData = ConvertContractorToJson(record);

        if (enableDebugLogging)
        {
            Debug.Log($"[ApiHandler] Sending contractor data:");
            Debug.Log($"[ApiHandler] Name: {record.name}");
            Debug.Log($"[ApiHandler] Mobile: {record.mobile_number}");
            Debug.Log($"[ApiHandler] JSON Data: {jsonData}");
        }

        int maxRetries = 5; // Increased from 3 to 5
        float retryDelay = 2f; // Increased base delay

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 20; // Increased timeout

                yield return request.SendWebRequest();

                if (enableDebugLogging)
                {
                    Debug.Log($"[ApiHandler] Response Code: {request.responseCode}");
                    Debug.Log($"[ApiHandler] Response: {request.downloadHandler.text}");
                }

                // Handle 429 Too Many Requests specifically
                if (request.responseCode == 429)
                {
                    lastSyncSuccess = false;
                    lastApiMessage = "Rate limited - too many requests";
                    
                    if (attempt < maxRetries - 1)
                    {
                        // Progressive delay: 2s, 4s, 8s, 16s, 32s
                        float waitTime = retryDelay * Mathf.Pow(2, attempt);
                        waitTime = Mathf.Min(waitTime, 60f); // Cap at 60 seconds
                        
                        Debug.LogWarning($"[ApiHandler] 429 Rate Limited - Retrying in {waitTime}s (attempt {attempt + 1}/{maxRetries}) for: {record.name}");
                        yield return new WaitForSeconds(waitTime);
                        continue;
                    }
                    else
                    {
                        Debug.LogError($"[ApiHandler] 429 Rate Limited - Max retries exceeded for: {record.name}");
                        yield break;
                    }
                }

                // Handle other HTTP errors with retry logic
                if (request.result != UnityWebRequest.Result.Success)
                {
                    lastSyncSuccess = false;
                    lastApiMessage = $"Network error: {request.error} (Code: {request.responseCode})";
                    
                    // Retry on certain network errors
                    if (attempt < maxRetries - 1 && 
                        (request.result == UnityWebRequest.Result.ConnectionError ||
                         request.result == UnityWebRequest.Result.DataProcessingError ||
                         request.responseCode >= 500)) // Server errors
                    {
                        float waitTime = retryDelay * (attempt + 1); // Linear increase for network errors
                        Debug.LogWarning($"[ApiHandler] Network error - Retrying in {waitTime}s (attempt {attempt + 1}/{maxRetries}) for: {record.name}");
                        yield return new WaitForSeconds(waitTime);
                        continue;
                    }
                    else
                    {
                        Debug.LogError($"[ApiHandler] Failed to send record: {request.error} (HTTP {request.responseCode})");
                        yield break;
                    }
                }

                // Success - process the response
                yield return StartCoroutine(ProcessApiResponse(request.downloadHandler.text, record));
                break; // Exit retry loop on success
            }
        }
    }

    private string ConvertContractorToJson(ContractorData contractor)
    {
        // Create API-compatible object with correct field names
        ApiContractorData apiData = new ApiContractorData
        {
            name = contractor.name ?? "",
            mobile_number = contractor.mobile_number ?? "",
            email = contractor.email ?? "",
            company_name = contractor.company_name ?? "",
            head_office_location = contractor.head_office_location ?? "",
            project_name = contractor.project_name ?? "",
            project_location = contractor.project_location ?? "",
            account_type = contractor.account_type ?? "",
            signature = contractor.signature ?? "",
            app_type = contractor.application_type ?? "",  // Note: mapped to 'app_type' for API
            sales_comment = contractor.sales_comment ?? ""
        };

        // Convert to JSON string using Unity's JsonUtility
        string jsonString = JsonUtility.ToJson(apiData);
        
        if (enableDebugLogging)
            Debug.Log($"[ApiHandler DEBUG] Generated JSON: {jsonString}");
        
        return jsonString;
    }

    private IEnumerator ProcessApiResponse(string responseText, ContractorData record)
    {
        try
        {
            if (string.IsNullOrEmpty(responseText))
            {
                lastSyncSuccess = false;
                lastApiMessage = "Empty response from server";
                yield break;
            }

            // Check for rate limiting indicators in response text before JSON parsing
            string lowerResponse = responseText.ToLower();
            if (lowerResponse.Contains("rate limit") || 
                lowerResponse.Contains("too many requests") || 
                lowerResponse.Contains("limit exceeded") ||
                lowerResponse.Contains("quota exceeded"))
            {
                lastSyncSuccess = false;
                lastApiMessage = "Rate limited by API";
                Debug.LogWarning($"[ApiHandler] Rate limiting detected in response: {responseText}");
                yield break;
            }

            // Handle response that might have error message before JSON
            string jsonPart = responseText;
            int jsonStart = responseText.IndexOf('{');
            if (jsonStart >= 0)
            {
                jsonPart = responseText.Substring(jsonStart);
            }

            ApiResponse apiResponse = null;

            try
            {
                apiResponse = JsonUtility.FromJson<ApiResponse>(jsonPart);
            }
            catch
            {
                // If JSON parsing fails, analyze raw response
                ProcessRawResponse(responseText, record);
                yield break;
            }

            if (apiResponse != null)
            {
                lastApiMessage = apiResponse.message ?? "";

                // Check for rate limiting in API response message
                if (!string.IsNullOrEmpty(lastApiMessage))
                {
                    string lowerMessage = lastApiMessage.ToLower();
                    if (lowerMessage.Contains("rate limit") || 
                        lowerMessage.Contains("too many requests") || 
                        lowerMessage.Contains("limit exceeded") ||
                        lowerMessage.Contains("quota exceeded"))
                    {
                        lastSyncSuccess = false;
                        lastApiMessage = "Rate limited by API";
                        Debug.LogWarning($"[ApiHandler] Rate limiting detected in API message: {lastApiMessage}");
                        yield break;
                    }
                    else if (lowerMessage.Contains("already") || lowerMessage.Contains("exist"))
                    {
                        lastSyncSuccess = true; // Consider as success since record exists
                        Debug.Log($"[ApiHandler] Record already exists: {record.name} ({record.mobile_number})");
                    }
                    else if (lowerMessage.Contains("success"))
                    {
                        lastSyncSuccess = true;
                        Debug.Log($"[ApiHandler] Successfully synced: {record.name} ({record.mobile_number})");
                    }
                    else
                    {
                        lastSyncSuccess = false;
                        Debug.LogWarning($"[ApiHandler] Unknown response: {lastApiMessage}");
                    }
                }
                else
                {
                    lastSyncSuccess = false;
                    Debug.LogWarning("[ApiHandler] Empty message in API response");
                }
            }
            else
            {
                lastSyncSuccess = false;
                lastApiMessage = "Failed to parse API response";
            }
        }
        catch (Exception ex)
        {
            lastSyncSuccess = false;
            lastApiMessage = $"Error processing response: {ex.Message}";
            Debug.LogError($"[ApiHandler] Response processing error: {ex.Message}");
        }

        yield return null;
    }

    private void ProcessRawResponse(string responseText, ContractorData record)
    {
        string lowerResponse = responseText.ToLower();

        // Check for rate limiting first
        if (lowerResponse.Contains("rate limit") || 
            lowerResponse.Contains("too many requests") || 
            lowerResponse.Contains("limit exceeded") ||
            lowerResponse.Contains("quota exceeded"))
        {
            lastSyncSuccess = false;
            lastApiMessage = "Rate limited by API";
            Debug.LogWarning($"[ApiHandler] Rate limiting detected in raw response: {responseText}");
        }
        else if (lowerResponse.Contains("already") || lowerResponse.Contains("exist"))
        {
            lastSyncSuccess = true;
            lastApiMessage = "Already registered";
            Debug.Log($"[ApiHandler] Raw response - Already exists: {record.name} ({record.mobile_number})");
        }
        else if (lowerResponse.Contains("success"))
        {
            lastSyncSuccess = true;
            lastApiMessage = "Successfully registered";
            Debug.Log($"[ApiHandler] Raw response - Success: {record.name} ({record.mobile_number})");
        }
        else
        {
            lastSyncSuccess = false;
            lastApiMessage = "Unknown response format";
            Debug.LogWarning($"[ApiHandler] Unknown raw response: {responseText}");
        }
    }

    private void UpdateStatusDisplay(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
        Debug.Log($"[ApiHandler] Status: {status}");
    }

    private void StartProgressAnimation(bool loop)
    {
        if (syncProgressImage != null)
        {
            syncProgressImage.gameObject.SetActive(true);
            if (progressAnimationCoroutine != null)
            {
                StopCoroutine(progressAnimationCoroutine);
            }
            progressAnimationCoroutine = StartCoroutine(ProgressAnimationCoroutine(loop));
        }
    }

    private void StopProgressAnimation()
    {
        if (progressAnimationCoroutine != null)
        {
            StopCoroutine(progressAnimationCoroutine);
            progressAnimationCoroutine = null;
        }

        if (syncProgressImage != null)
        {
            syncProgressImage.fillAmount = 0f;
            syncProgressImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator ProgressAnimationCoroutine(bool loop)
    {
        if (syncProgressImage == null) yield break;

        float fillSpeed = 1.5f;
        syncProgressImage.fillAmount = 0f;

        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * fillSpeed;
                syncProgressImage.fillAmount = Mathf.Clamp01(t);
                yield return null;
            }

            if (loop)
            {
                syncProgressImage.fillAmount = 0f;
            }
            else
            {
                break;
            }
        }
    }

    // Public methods for external access
    public bool IsSyncing()
    {
        return isSyncing;
    }

    public int GetPendingSyncCount()
    {
        return pendingSyncCount;
    }

    public int GetTotalRecordCount()
    {
        return csvData?.Length ?? 0;
    }

    // Send single contractor record (for immediate sync after adding to CSV)
    public void SyncSingleRecord(ContractorData record)
    {
        if (!isSyncing && record != null)
        {
            StartCoroutine(SendSingleRecord(record));
        }
    }

    // Get sync statistics
    public string GetSyncStatistics()
    {
        int totalRecords = csvData?.Length ?? 0;
        return $"Total Records: {totalRecords}, Pending Sync: {pendingSyncCount}, Last Status: {lastApiMessage}";
    }

    // Get current data from CSV
    public ContractorData[] GetAllCsvData()
    {
        return csvData ?? new ContractorData[0];
    }

    // Get contractor data (same as GetAllCsvData for compatibility)
    public ContractorData[] GetContractorData()
    {
        return GetAllCsvData();
    }

    // Filter contractor data by account type
    public ContractorData[] FilterByAccountType(string accountType)
    {
        if (csvData == null) return new ContractorData[0];
        
        return csvData.Where(contractor => 
            contractor.account_type.Equals(accountType, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    // Filter contractor data by location
    public ContractorData[] FilterByLocation(String location)
    {
        if (csvData == null) return new ContractorData[0];
        
        return csvData.Where(contractor => 
            contractor.head_office_location.Contains(location) || 
            contractor.project_location.Contains(location)).ToArray();
    }

    // Get a specific column as an array of strings
    public string[] GetColumn(string columnName)
    {
        if (csvData == null) return new string[0];

        switch (columnName.ToLower())
        {
            case "name":
                return csvData.Select(c => c.name ?? "").ToArray();
            case "mobile_number":
            case "mobile number":
                return csvData.Select(c => c.mobile_number ?? "").ToArray();
            case "email":
                return csvData.Select(c => c.email ?? "").ToArray();
            case "company_name":
            case "company name":
                return csvData.Select(c => c.company_name ?? "").ToArray();
            case "head_office_location":
            case "head office location":
                return csvData.Select(c => c.head_office_location ?? "").ToArray();
            case "project_name":
            case "project name":
                return csvData.Select(c => c.project_name ?? "").ToArray();
            case "project_location":
            case "project location":
                return csvData.Select(c => c.project_location ?? "").ToArray();
            case "account_type":
            case "account type":
                return csvData.Select(c => c.account_type ?? "").ToArray();
            default:
                return new string[0];
        }
    }

    // Filter data based on a predicate
    public ContractorData[] FilterData(System.Func<ContractorData, bool> predicate)
    {
        if (csvData == null) return new ContractorData[0];
        
        return csvData.Where(predicate).ToArray();
    }

    // Refresh data count from CSV
    public void RefreshDataCount()
    {
        LoadCsvData();
        int count = csvData?.Length ?? 0;
        pendingSyncCount = count;
        UpdateButtonStates();
        Debug.Log($"[ApiHandler] Current CSV data count: {count}");
    }

    private void OnDestroy()
    {
        if (progressAnimationCoroutine != null)
        {
            StopCoroutine(progressAnimationCoroutine);
        }
    }

    // Public method to rename the current CSV file
    public void RenameProcessedCsvFile()
    {
        if (string.IsNullOrEmpty(csvFilePath) || !File.Exists(csvFilePath))
        {
            Debug.LogWarning("[ApiHandler] Cannot rename CSV file - file not found or path is empty");
            return;
        }

        try
        {
            string directory = Path.GetDirectoryName(csvFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(csvFilePath);
            string extension = Path.GetExtension(csvFilePath);

            // Find the next available number
            int counter = 1;
            string newFileName;
            string newFilePath;

            do
            {
                newFileName = $"{renamedFilePrefix}{counter}{extension}";
                newFilePath = Path.Combine(directory, newFileName);
                counter++;
            }
            while (File.Exists(newFilePath));

            // Rename the file
            File.Move(csvFilePath, newFilePath);
            
            Debug.Log($"[ApiHandler] CSV file renamed from '{Path.GetFileName(csvFilePath)}' to '{newFileName}'");
            Debug.Log($"[ApiHandler] New file path: {newFilePath}");

            // Update the UI status
            UpdateStatusDisplay($"File renamed to {newFileName}");

            // Clear the current data since the file is processed
            csvData = new ContractorData[0];
            pendingSyncCount = 0;
            UpdateButtonStates();

            // Reset the Registration file system after successful rename
            if (registrationComponent != null)
            {
                registrationComponent.ReSetupTheFile();
                Debug.Log("[ApiHandler] Registration file system reset after CSV rename");
            }
            else
            {
                Debug.LogWarning("[ApiHandler] Registration component not assigned - cannot reset file system");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ApiHandler] Error renaming CSV file: {ex.Message}");
            OnError?.Invoke($"Error renaming file: {ex.Message}");
        }
    }

    // Public method to get the next rename filename without actually renaming
    public string GetNextRenameFileName()
    {
        if (string.IsNullOrEmpty(csvFilePath))
            return "";

        string directory = Path.GetDirectoryName(csvFilePath);
        string extension = Path.GetExtension(csvFilePath);

        // Find the next available number
        int counter = 1;
        string newFileName;
        string newFilePath;

        do
        {
            newFileName = $"{renamedFilePrefix}{counter}{extension}";
            newFilePath = Path.Combine(directory, newFileName);
            counter++;
        }
        while (File.Exists(newFilePath));

        return newFileName;
    }

    // Public method to check if original CSV file exists
    public bool OriginalCsvFileExists()
    {
        return !string.IsNullOrEmpty(csvFilePath) && File.Exists(csvFilePath);
    }

    // Debug method to manually rename CSV file
    [ContextMenu("Rename CSV File")]
    public void DebugRenameCsvFile()
    {
        Debug.Log("[ApiHandler DEBUG] Manual file rename triggered");
        RenameProcessedCsvFile();
    }

    // Debug method to check button state
    [ContextMenu("Debug Button State")]
    public void DebugButtonState()
    {
        Debug.Log($"[ApiHandler DEBUG] Button State Analysis:");
        Debug.Log($"[ApiHandler DEBUG]   syncButton null: {syncButton == null}");
        Debug.Log($"[ApiHandler DEBUG]   isSyncing: {isSyncing}");
        Debug.Log($"[ApiHandler DEBUG]   csvData null: {csvData == null}");
        Debug.Log($"[ApiHandler DEBUG]   csvData length: {csvData?.Length ?? 0}");
        Debug.Log($"[ApiHandler DEBUG]   csvFilePath: '{csvFilePath}'");
        Debug.Log($"[ApiHandler DEBUG]   File exists: {File.Exists(csvFilePath)}");
        
        if (syncButton != null)
        {
            Debug.Log($"[ApiHandler DEBUG]   Button interactable: {syncButton.interactable}");
            Debug.Log($"[ApiHandler DEBUG]   Button active: {syncButton.gameObject.activeInHierarchy}");
            Debug.Log($"[ApiHandler DEBUG]   Button should be enabled: {!isSyncing}");
        }
        
        Debug.Log("[ApiHandler DEBUG] NEW LOGIC: Button is always enabled unless syncing");
        Debug.Log("[ApiHandler DEBUG] Button will handle missing data by reloading CSV when clicked");
    }

    private IEnumerator SendBatchArray(ContractorData[] batchRecords)
    {
        // Convert array of ContractorData to JSON array format for API
        ApiContractorData[] apiBatchData = new ApiContractorData[batchRecords.Length];
        
        for (int i = 0; i < batchRecords.Length; i++)
        {
            apiBatchData[i] = new ApiContractorData
            {
                name = batchRecords[i].name ?? "",
                mobile_number = batchRecords[i].mobile_number ?? "",
                email = batchRecords[i].email ?? "",
                company_name = batchRecords[i].company_name ?? "",
                head_office_location = batchRecords[i].head_office_location ?? "",
                project_name = batchRecords[i].project_name ?? "",
                project_location = batchRecords[i].project_location ?? "",
                account_type = batchRecords[i].account_type ?? "",
                signature = batchRecords[i].signature ?? "",
                app_type = batchRecords[i].application_type ?? "",
                sales_comment = batchRecords[i].sales_comment ?? ""
            };
        }

        // Create wrapper object for the array
        string jsonData = ConvertBatchToJson(apiBatchData);

        if (enableDebugLogging)
        {
            Debug.Log($"[ApiHandler] Sending batch array with {batchRecords.Length} records");
            Debug.Log($"[ApiHandler] First record: {batchRecords[0].name}");
            Debug.Log($"[ApiHandler] Last record: {batchRecords[batchRecords.Length - 1].name}");
            Debug.Log($"[ApiHandler] JSON Data length: {jsonData.Length} characters");
        }

        int maxRetries = 5;
        float retryDelay = 2f;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 30; // Increased timeout for batch requests

                yield return request.SendWebRequest();

                if (enableDebugLogging)
                {
                    Debug.Log($"[ApiHandler] Batch Response Code: {request.responseCode}");
                    Debug.Log($"[ApiHandler] Batch Response: {request.downloadHandler.text}");
                }

                // Handle 429 Too Many Requests specifically
                if (request.responseCode == 429)
                {
                    lastSyncSuccess = false;
                    lastApiMessage = "Rate limited - too many requests";
                    
                    if (attempt < maxRetries - 1)
                    {
                        float waitTime = retryDelay * Mathf.Pow(2, attempt);
                        waitTime = Mathf.Min(waitTime, 60f);
                        
                        Debug.LogWarning($"[ApiHandler] 429 Rate Limited - Retrying batch in {waitTime}s (attempt {attempt + 1}/{maxRetries})");
                        yield return new WaitForSeconds(waitTime);
                        continue;
                    }
                    else
                    {
                        Debug.LogError($"[ApiHandler] 429 Rate Limited - Max retries exceeded for batch of {batchRecords.Length} records");
                        yield break;
                    }
                }

                // Handle other HTTP errors with retry logic
                if (request.result != UnityWebRequest.Result.Success)
                {
                    lastSyncSuccess = false;
                    lastApiMessage = $"Network error: {request.error} (Code: {request.responseCode})";
                    
                    if (attempt < maxRetries - 1 && 
                        (request.result == UnityWebRequest.Result.ConnectionError ||
                         request.result == UnityWebRequest.Result.DataProcessingError ||
                         request.responseCode >= 500))
                    {
                        float waitTime = retryDelay * (attempt + 1);
                        Debug.LogWarning($"[ApiHandler] Network error - Retrying batch in {waitTime}s (attempt {attempt + 1}/{maxRetries})");
                        yield return new WaitForSeconds(waitTime);
                        continue;
                    }
                    else
                    {
                        Debug.LogError($"[ApiHandler] Failed to send batch: {request.error} (HTTP {request.responseCode})");
                        yield break;
                    }
                }

                // Success - process the response
                yield return StartCoroutine(ProcessBatchApiResponse(request.downloadHandler.text, batchRecords));
                break; // Exit retry loop on success
            }
        }
    }

    private string ConvertBatchToJson(ApiContractorData[] batchData)
    {
        // Convert array directly to JSON string without wrapper
        // Unity's JsonUtility doesn't handle arrays directly, so we need to manually create the JSON
        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("[");
        
        for (int i = 0; i < batchData.Length; i++)
        {
            // Convert each object to JSON
            string objectJson = JsonUtility.ToJson(batchData[i]);
            jsonBuilder.Append(objectJson);
            
            // Add comma if not the last item
            if (i < batchData.Length - 1)
            {
                jsonBuilder.Append(",");
            }
        }
        
        jsonBuilder.Append("]");
        string jsonString = jsonBuilder.ToString();
        
        if (enableDebugLogging)
        {
            Debug.Log($"[ApiHandler DEBUG] Generated batch JSON array with {batchData.Length} records");
            Debug.Log($"[ApiHandler DEBUG] JSON Preview: {jsonString.Substring(0, Mathf.Min(500, jsonString.Length))}...");
        }
        
        return jsonString;
    }

    private IEnumerator ProcessBatchApiResponse(string responseText, ContractorData[] batchRecords)
    {
        try
        {
            if (string.IsNullOrEmpty(responseText))
            {
                lastSyncSuccess = false;
                lastApiMessage = "Empty response from server";
                yield break;
            }

            // Check for rate limiting indicators in response text
            string lowerResponse = responseText.ToLower();
            if (lowerResponse.Contains("rate limit") || 
                lowerResponse.Contains("too many requests") || 
                lowerResponse.Contains("limit exceeded") ||
                lowerResponse.Contains("quota exceeded"))
            {
                lastSyncSuccess = false;
                lastApiMessage = "Rate limited by API response";
                Debug.LogWarning($"[ApiHandler] Rate limiting detected in batch response: {responseText}");
                yield break;
            }

            // Try to parse as JSON first
            string jsonPart = responseText;
            int jsonStart = responseText.IndexOf('{');
            if (jsonStart >= 0)
            {
                jsonPart = responseText.Substring(jsonStart);
            }

            ApiResponse apiResponse = null;

            try
            {
                apiResponse = JsonUtility.FromJson<ApiResponse>(jsonPart);
            }
            catch
            {
                // If JSON parsing fails, analyze raw response
                ProcessBatchRawResponse(responseText, batchRecords);
                yield break;
            }

            if (apiResponse != null)
            {
                lastApiMessage = apiResponse.message ?? "";

                // Check for rate limiting in API response message
                if (!string.IsNullOrEmpty(lastApiMessage))
                {
                    string lowerMessage = lastApiMessage.ToLower();
                    if (lowerMessage.Contains("rate limit") || 
                        lowerMessage.Contains("too many requests") || 
                        lowerMessage.Contains("limit exceeded") ||
                        lowerMessage.Contains("quota exceeded"))
                    {
                        lastSyncSuccess = false;
                        lastApiMessage = "Rate limited by API";
                        Debug.LogWarning($"[ApiHandler] Rate limiting detected in batch API message: {lastApiMessage}");
                        yield break;
                    }
                    else if (lowerMessage.Contains("success") || lowerMessage.Contains("processed"))
                    {
                        lastSyncSuccess = true;
                        Debug.Log($"[ApiHandler] Successfully synced batch of {batchRecords.Length} records");
                    }
                    else
                    {
                        lastSyncSuccess = false;
                        Debug.LogWarning($"[ApiHandler] Unknown batch response: {lastApiMessage}");
                    }
                }
                else
                {
                    lastSyncSuccess = false;
                    Debug.LogWarning("[ApiHandler] Empty message in batch API response");
                }
            }
            else
            {
                lastSyncSuccess = false;
                lastApiMessage = "Failed to parse batch API response";
            }
        }
        catch (Exception ex)
        {
            lastSyncSuccess = false;
            lastApiMessage = $"Error processing batch response: {ex.Message}";
            Debug.LogError($"[ApiHandler] Batch response processing error: {ex.Message}");
        }

        yield return null;
    }

    private void ProcessBatchRawResponse(string responseText, ContractorData[] batchRecords)
    {
        string lowerResponse = responseText.ToLower();

        // Check for rate limiting first
        if (lowerResponse.Contains("rate limit") || 
            lowerResponse.Contains("too many requests") || 
            lowerResponse.Contains("limit exceeded") ||
            lowerResponse.Contains("quota exceeded"))
        {
            lastSyncSuccess = false;
            lastApiMessage = "Rate limited by API";
            Debug.LogWarning($"[ApiHandler] Rate limiting detected in batch raw response: {responseText}");
        }
        else if (lowerResponse.Contains("success") || lowerResponse.Contains("processed"))
        {
            lastSyncSuccess = true;
            lastApiMessage = "Batch successfully registered";
            Debug.Log($"[ApiHandler] Batch raw response - Success: {batchRecords.Length} records");
        }
        else
        {
            lastSyncSuccess = false;
            lastApiMessage = "Unknown batch response format";
            Debug.LogWarning($"[ApiHandler] Unknown batch raw response: {responseText}");
        }
    }

    // Public method to force enable the sync button (for debugging)
    public void ForceEnableSyncButton()
    {
        if (syncButton != null)
        {
            syncButton.interactable = true;
            Debug.Log("[ApiHandler] Sync button force enabled");
        }
        else
        {
            Debug.LogWarning("[ApiHandler] Cannot force enable - syncButton is null");
        }
    }

    // Public method to manually trigger button state update
    public void ManualUpdateButtonStates()
    {
        Debug.Log("[ApiHandler] Manual button state update triggered");
        UpdateButtonStates();
    }
}
