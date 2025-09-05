using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

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

public class ApiHandlers : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text statusText;
    public Button syncButton;
    public Button closeButton;
    public Image syncProgressImage;

    [Header("API Settings")]
    public string apiUrl = "https://tempweb90.com/knauf/api.php";

    [Header("Batch Processing")]
    [SerializeField] private int batchSize = 50;
    [SerializeField] private float baseBatchDelay = 10f;
    [SerializeField] private float maxResponseBasedDelay = 60f;

    [Header("File Management")]
    public string csvFileName = "data.csv";

    [Header("Status Messages")]
    public string messageIdle = "Ready to sync";
    public string messageChecking = "Checking connection...";
    public string messageSyncing = "Syncing data...";
    public string messageSuccess = "Sync completed";
    public string messageNoInternet = "No internet connection";
    public string messageNoData = "No data to sync";

    private bool isSyncing = false;
    private string csvFilePath;
    private ContractorData[] csvData;

    public static ApiHandlers Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        csvFilePath = Application.persistentDataPath + "/" + csvFileName;
        LoadCsvData();
        InitializeUI();
        UpdateStatusDisplay(messageIdle);
        UpdateButtonStates();
    }

    private void InitializeUI()
    {
        if (syncButton != null) syncButton.onClick.AddListener(ManualSyncAllData);
        if (syncProgressImage != null)
        {
            syncProgressImage.fillAmount = 0f;
            syncProgressImage.gameObject.SetActive(false);
        }
    }

    private void UpdateButtonStates()
    {
        if (syncButton != null) syncButton.interactable = !isSyncing;
        if (closeButton != null) closeButton.interactable = !isSyncing;
    }

    private void LoadCsvData()
    {
        if (!File.Exists(csvFilePath))
        {
            Debug.LogWarning($"CSV not found: {csvFilePath}");
            csvData = new ContractorData[0];
            return;
        }

        string[] lines = File.ReadAllLines(csvFilePath, Encoding.UTF8);
        if (lines.Length <= 1) { csvData = new ContractorData[0]; return; }

        csvData = lines.Skip(1).Select(line =>
        {
            string[] v = line.Split(',');
            if (v.Length < 11) return null;
            return new ContractorData
            {
                name = v[0],
                mobile_number = v[1],
                email = v[2],
                company_name = v[3],
                head_office_location = v[4],
                project_name = v[5],
                project_location = v[6],
                account_type = v[7],
                signature = v[8],
                application_type = v[9],
                sales_comment = v[10]
            };
        }).Where(x => x != null).ToArray();

        Debug.Log($"Loaded {csvData.Length} records from CSV.");
    }

    public void ManualSyncAllData()
    {
        if (isSyncing) { UpdateStatusDisplay("Sync already running..."); return; }
        if (csvData == null || csvData.Length == 0) { UpdateStatusDisplay(messageNoData); return; }
        StartCoroutine(SyncAllCsvData());
    }

    public IEnumerator SyncAllCsvData()
    {
        isSyncing = true;
        UpdateButtonStates();
        UpdateStatusDisplay(messageChecking);
        if (syncProgressImage != null) { syncProgressImage.fillAmount = 0f; syncProgressImage.gameObject.SetActive(true); }

        yield return StartCoroutine(CheckInternetConnection());
        if (!lastSyncSuccess)
        {
            UpdateStatusDisplay(messageNoInternet);
            isSyncing = false;
            UpdateButtonStates();
            yield break;
        }

        int totalBatches = Mathf.CeilToInt((float)csvData.Length / batchSize);
        int successCount = 0, failCount = 0;

        for (int b = 0; b < totalBatches; b++)
        {
            int start = b * batchSize;
            int end = Mathf.Min(start + batchSize, csvData.Length);
            ContractorData[] batchArray = new ContractorData[end - start];
            Array.Copy(csvData, start, batchArray, 0, batchArray.Length);

            bool batchSuccess = false;
            int attempts = 0;
            int maxAttempts = 5;

            while (!batchSuccess && attempts < maxAttempts)
            {
                attempts++;
                yield return StartCoroutine(SendBatchArray(batchArray, r => batchSuccess = r));

                if (!batchSuccess && attempts < maxAttempts)
                {
                    float wait = Mathf.Min(baseBatchDelay * attempts, maxResponseBasedDelay);
                    Debug.LogWarning($"Batch {b + 1}/{totalBatches} failed. Retrying in {wait:F1}s...");
                    UpdateStatusDisplay($"Retrying batch {b + 1} in {wait:F1}s...");
                    yield return new WaitForSeconds(wait);
                }
            }

            if (batchSuccess) successCount += batchArray.Length;
            else failCount += batchArray.Length;

            if (syncProgressImage != null)
                syncProgressImage.fillAmount = (float)(successCount + failCount) / csvData.Length;

            if (b < totalBatches - 1) yield return new WaitForSeconds(baseBatchDelay);
        }

        if (failCount == 0)
            UpdateStatusDisplay($"{messageSuccess} ({successCount} records)");
        else
            UpdateStatusDisplay($"Partial: {successCount} success, {failCount} failed");

        isSyncing = false;
        UpdateButtonStates();
        if (syncProgressImage != null) syncProgressImage.gameObject.SetActive(false);
    }

    private IEnumerator SendBatchArray(ContractorData[] batchRecords, Action<bool> callback)
    {
        bool success = false;

        ApiContractorData[] apiBatch = batchRecords.Select(r => new ApiContractorData
        {
            name = r.name ?? "",
            mobile_number = r.mobile_number ?? "",
            email = r.email ?? "",
            company_name = r.company_name ?? "",
            head_office_location = r.head_office_location ?? "",
            project_name = r.project_name ?? "",
            project_location = r.project_location ?? "",
            account_type = r.account_type ?? "",
            signature = r.signature ?? "",
            app_type = r.application_type ?? "",
            sales_comment = r.sales_comment ?? ""
        }).ToArray();

        StringBuilder sb = new StringBuilder("[");
        for (int i = 0; i < apiBatch.Length; i++)
        {
            if (i > 0) sb.Append(",");
            sb.Append(JsonUtility.ToJson(apiBatch[i]));
        }
        sb.Append("]");
        string jsonData = sb.ToString();

        using (UnityWebRequest req = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 30;

            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            bool ok = req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 300;
#else
            bool ok = !req.isNetworkError && !req.isHttpError && req.responseCode >= 200 && req.responseCode < 300;
#endif
            if (ok)
            {
                success = true;
                lastSyncSuccess = true;
                lastApiMessage = req.downloadHandler.text;
                Debug.Log($"Batch sent. Response: {lastApiMessage}");
            }
            else
            {
                success = false;
                lastSyncSuccess = false;
                lastApiMessage = req.error;
                Debug.LogError($"Batch failed. HTTP {req.responseCode} | {req.error}");
            }
        }

        callback?.Invoke(success);
    }

    public void SyncSingleRecord(ContractorData record)
    {
        if (!isSyncing && record != null)
            StartCoroutine(SendSingleRecord(record));
    }

    private IEnumerator SendSingleRecord(ContractorData record)
    {
        string json = JsonUtility.ToJson(new ApiContractorData
        {
            name = record.name ?? "",
            mobile_number = record.mobile_number ?? "",
            email = record.email ?? "",
            company_name = record.company_name ?? "",
            head_office_location = record.head_office_location ?? "",
            project_name = record.project_name ?? "",
            project_location = record.project_location ?? "",
            account_type = record.account_type ?? "",
            signature = record.signature ?? "",
            app_type = record.application_type ?? "",
            sales_comment = record.sales_comment ?? ""
        });

        bool success = false;
        int attempts = 0;
        int maxAttempts = 3;

        while (!success && attempts < maxAttempts)
        {
            attempts++;
            using (UnityWebRequest req = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 20;

                yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                success = req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 300;
#else
                success = !req.isNetworkError && !req.isHttpError && req.responseCode >= 200 && req.responseCode < 300;
#endif

                if (success)
                {
                    Debug.Log($"Single record sent: {req.downloadHandler.text}");
                }
                else if (attempts < maxAttempts)
                {
                    Debug.LogWarning($"Retry {attempts}/{maxAttempts} failed. Waiting 2s...");
                    yield return new WaitForSeconds(2f);
                }
                else
                {
                    Debug.LogError($"Record failed after {maxAttempts} attempts. Error: {req.error}");
                }
            }
        }
    }

    private IEnumerator CheckInternetConnection()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://www.google.com"))
        {
            www.timeout = 5;
            yield return www.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
            lastSyncSuccess = www.result == UnityWebRequest.Result.Success;
#else
            lastSyncSuccess = !www.isNetworkError && !www.isHttpError;
#endif
        }
    }

    private bool lastSyncSuccess;
    private string lastApiMessage;

    private void UpdateStatusDisplay(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"[ApiHandler] {msg}");
    }
}
