using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextAnimation : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private TextMeshProUGUI precentageImageText;
    [SerializeField] private Image fillImage;

    private void Start()
    {
        UpdatePercentage();
    }

    private void UpdatePercentage()
    {
        int percentage = Random.Range(70, 101); // 70 to 100 inclusive
        if (percentageText != null)
            percentageText.text = $"Oh.. You've got {percentage}% germs detected!";
            precentageImageText.text = $"{percentage}%";
        if (fillImage != null)
            fillImage.fillAmount = percentage / 100f;
    }
}