using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RegistrationDataHolder : MonoBehaviour
{
    [SerializeField]
    public List<RegistrationData> registrations = new List<RegistrationData>();

    // Runtime methods to add and remove fields dynamically

    // Add a field to a specific registration
    public void AddField(int registrationIndex, string key, string value)
    {
        if (registrationIndex < 0 || registrationIndex >= registrations.Count)
            return;

        registrations[registrationIndex].AddField(key, value);
    }

    // Remove a field by key from a specific registration
    public void RemoveField(int registrationIndex, string key)
    {
        if (registrationIndex < 0 || registrationIndex >= registrations.Count)
            return;

        registrations[registrationIndex].RemoveField(key);
    }

    // Get a field value from a specific registration
    public string GetFieldValue(int registrationIndex, string key)
    {
        if (registrationIndex < 0 || registrationIndex >= registrations.Count)
            return "";

        return registrations[registrationIndex].GetFieldValue(key);
    }

    // Set a field value in a specific registration
    public void SetFieldValue(int registrationIndex, string key, string value)
    {
        if (registrationIndex < 0 || registrationIndex >= registrations.Count)
            return;

        registrations[registrationIndex].SetFieldValue(key, value);
    }

    // Add a new empty registration
    public int AddNewRegistration()
    {
        registrations.Add(new RegistrationData());
        return registrations.Count - 1;
    }

    // Remove a registration
    public void RemoveRegistration(int registrationIndex)
    {
        if (registrationIndex >= 0 && registrationIndex < registrations.Count)
        {
            registrations.RemoveAt(registrationIndex);
        }
    }
}

[System.Serializable]
public class RegistrationField
{
    public string key;
    public string value;

    public RegistrationField() { }
    
    public RegistrationField(string key, string value)
    {
        this.key = key;
        this.value = value;
    }
}

[System.Serializable]
public class RegistrationData
{
    public List<RegistrationField> fields = new List<RegistrationField>();

    // Helper methods to work with fields

    // Add a field (or update if key already exists)
    public void AddField(string key, string value)
    {
        var existingField = fields.FirstOrDefault(f => f.key == key);
        if (existingField != null)
        {
            existingField.value = value;
        }
        else
        {
            fields.Add(new RegistrationField(key, value));
        }
    }

    // Remove a field by key
    public void RemoveField(string key)
    {
        fields.RemoveAll(f => f.key == key);
    }

    // Get field value by key
    public string GetFieldValue(string key)
    {
        var field = fields.FirstOrDefault(f => f.key == key);
        return field?.value ?? "";
    }

    // Set field value by key (adds if doesn't exist)
    public void SetFieldValue(string key, string value)
    {
        AddField(key, value);
    }

    // Check if field exists
    public bool HasField(string key)
    {
        return fields.Any(f => f.key == key);
    }

    // Get all field keys
    public List<string> GetAllKeys()
    {
        return fields.Select(f => f.key).ToList();
    }

    // Convert to dictionary for easier manipulation
    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var field in fields)
        {
            if (!dict.ContainsKey(field.key))
            {
                dict[field.key] = field.value;
            }
        }
        return dict;
    }

    // Create from dictionary
    public void FromDictionary(Dictionary<string, string> dict)
    {
        fields.Clear();
        foreach (var kvp in dict)
        {
            fields.Add(new RegistrationField(kvp.Key, kvp.Value));
        }
    }

    // Convert to CSV line format
    public string ToCsvLine(List<string> orderedKeys)
    {
        var values = new List<string>();
        foreach (string key in orderedKeys)
        {
            string value = GetFieldValue(key);
            // Escape CSV field if it contains commas, quotes, or newlines
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                value = "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            values.Add(value);
        }
        return string.Join(",", values);
    }

    // Create from CSV line
    public void FromCsvLine(string csvLine, List<string> orderedKeys)
    {
        fields.Clear();
        var values = ParseCsvLine(csvLine);
        
        for (int i = 0; i < orderedKeys.Count && i < values.Count; i++)
        {
            fields.Add(new RegistrationField(orderedKeys[i], values[i]));
        }
    }

    // Helper method to parse CSV lines properly handling escaped fields
    private List<string> ParseCsvLine(string line)
    {
        List<string> csvFields = new List<string>();
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
                csvFields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        csvFields.Add(currentField);
        return csvFields;
    }

    // Validation method - check if required fields are present and not empty
    public bool IsValid(List<string> requiredKeys)
    {
        foreach (string requiredKey in requiredKeys)
        {
            if (string.IsNullOrEmpty(GetFieldValue(requiredKey)))
            {
                return false;
            }
        }
        return true;
    }

    // Get validation errors
    public List<string> GetValidationErrors(List<string> requiredKeys)
    {
        var errors = new List<string>();
        foreach (string requiredKey in requiredKeys)
        {
            if (string.IsNullOrEmpty(GetFieldValue(requiredKey)))
            {
                errors.Add($"Field '{requiredKey}' is required");
            }
        }
        return errors;
    }
}