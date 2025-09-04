using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI; // For legacy InputField


public class ValidationButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField1;
    [SerializeField] private TMP_InputField inputField2;
    [SerializeField] private Text requiredMessageText1; // Assign a UI Text in the Inspector for field 1
    [SerializeField] private Text requiredMessageText2; // Assign a UI Text in the Inspector for field 2

    [SerializeField] private TextMeshProUGUI requiredMessageTextPro1; // Assign a UI Text in the Inspector for field 1
    [SerializeField] private TextMeshProUGUI requiredMessageTextPro2; // Assign a UI Text in the Inspector for field 2

    [Header("Validation Message")]
    [SerializeField] private string requiredMessage1 = "* This field is required.";
    [SerializeField] private string requiredMessage2 = "* This field is required.";
    [SerializeField] private string requiredMessage3 = "* This field is required.";
    [SerializeField] private string requiredMessage4 = "* This field is required.";
    //[SerializeField] private GameObject alreadyRegisteredMobile;

    public UnityEvent onValidationSuccess; // Optional event to call on successful validation
    private string originalPlaceholder1;
    private string originalPlaceholder2;

    private void Start()
    {
        if (inputField1 != null && inputField1.placeholder is Text placeholderText1)
        {
            originalPlaceholder1 = placeholderText1.text;
        }
        if (inputField2 != null && inputField2.placeholder is Text placeholderText2)
        {
            originalPlaceholder2 = placeholderText2.text;
        }
        if (requiredMessageText1 != null)
        {
            requiredMessageText1.text = ""; // Hide message at start
        }
        if (requiredMessageText2 != null)
        {
            requiredMessageText2.text = ""; // Hide message at start
        }
    }

    // Call this method to validate both input fields
    public void ValidateInputs()
    {
        bool isField1Valid = !ValidateSingleInput(inputField1, originalPlaceholder1, requiredMessageText1, requiredMessage1);
        bool isField2Valid = !ValidateSingleInput(inputField2, originalPlaceholder2, requiredMessageText2, requiredMessage2);
        // Only check for duplicate if both fields are valid
        if (isField1Valid && isField2Valid)
        {
            // Check if mobile number is exactly 11 digits
            if (inputField2.text.Length != 11)
            {
                if (requiredMessageText2 != null)
                    requiredMessageText2.text = requiredMessage4;
                return;
            }
            // Check if mobile exists
            if (ScanningFileHandler.Instance != null &&
                ScanningFileHandler.Instance.IsMobileExists(inputField2.text))
            {
                if (requiredMessageText2 != null)
                    requiredMessageText2.text = requiredMessage3;
                return;
            }
            if (isField1Valid && isField2Valid && onValidationSuccess != null)
            {
                onValidationSuccess.Invoke();
            }
        }
    }

    // Returns true if the field is empty, otherwise false
    private bool ValidateSingleInput(TMP_InputField inputField, string originalPlaceholder, Text messageText, string message)
    {
        if (inputField == null) return false;

        if (string.IsNullOrEmpty(inputField.text))
        {
            if (inputField.placeholder is Text placeholderText)
            {
                placeholderText.text = "*";
            }
            if (messageText != null)
            {
                messageText.text = message;
            }
            return true;
        }
        else
        {
            if (inputField.placeholder is Text placeholderText)
            {
                placeholderText.text = originalPlaceholder;
            }
            if (messageText != null)
            {
                messageText.text = "";
            }
            return false;
        }
    }
}