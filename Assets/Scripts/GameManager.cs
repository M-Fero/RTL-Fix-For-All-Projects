using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

    [Header("Cavases Collection")]

    public Canvas startCanvas;
    public Canvas registrationCanvas;
    public Canvas chooseProduct;

    [Header("Brief Canvas Settings")]
    [Tooltip("Reference to the current brief canvas instance")]
    public GameObject briefCanvas; // Reference to the current brief canvas instance

    public List<GameObject> briefProduct1;
    public List<GameObject> briefProduct2;
    public List<GameObject> briefProduct3;

    private GameObject currentBrief;

    private void Start()
    {
        ShowStartCanvas();
    }
    public void ShowStartCanvas()
    {
        startCanvas.enabled = true;
        registrationCanvas.enabled = false;
        chooseProduct.enabled = false;
    }

    public void GoToRegistration()
    {
        startCanvas.enabled = false;
        registrationCanvas.enabled = true;
    }

    public void GoToChooseProduct()
    {
        registrationCanvas.enabled = false;
        chooseProduct.enabled = true;
    }

    public void OnProductSelected(int productIndex)
    {
        chooseProduct.enabled = false;
        ShowRandomBrief(productIndex);
    }

    void ShowRandomBrief(int productIndex)
    {
        // Destroy the previous brief canvas if it exists
        if (briefCanvas != null)
        {
            Destroy(briefCanvas);
            briefCanvas = null;
        }

        List<GameObject> productBriefs = null;
        switch (productIndex)
        {
            case 0:
            productBriefs = briefProduct1;
            break;
            case 1:
            productBriefs = briefProduct2;
            break;
            case 2:
            productBriefs = briefProduct3;
            break;
            default:
            Debug.LogError("Invalid product index: " + productIndex);
            return;
        }

        if (productBriefs == null || productBriefs.Count == 0)
        {
            Debug.LogError("No briefs assigned for product index: " + productIndex);
            return;
        }

        int idx = Random.Range(0, productBriefs.Count);
        // Instantiate the new brief canvas as a root object
        briefCanvas = Instantiate(productBriefs[idx]);
        briefCanvas.SetActive(true);
        Canvas briefCanvasComponent = briefCanvas.GetComponent<Canvas>();
        if (briefCanvasComponent != null && !briefCanvasComponent.enabled)
        {
            briefCanvasComponent.enabled = true;
        }
        briefCanvas.SetActive(true);

    }

    void ShowResult()
    {
        // Show the result canvas based on the game outcome
        SavePlayerResult();
    }

    void SavePlayerResult()
    {
        // Save result to current player data

    }
}
