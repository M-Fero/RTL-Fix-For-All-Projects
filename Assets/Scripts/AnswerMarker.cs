using UnityEngine;
using UnityEngine.UI;

public class AnswerMarker : MonoBehaviour
{
    [Header("Assign these in Inspector (children of this button)")]
    public GameObject rightImage;
    public GameObject wrongImage;

    public void ShowRight()
    {
        if (rightImage != null) rightImage.SetActive(true);
        if (wrongImage != null) wrongImage.SetActive(false);
    }

    public void ShowWrong()
    {
        if (rightImage != null) rightImage.SetActive(false);
        if (wrongImage != null) wrongImage.SetActive(true);
    }

    public void HideAll()
    {
        if (rightImage != null) rightImage.SetActive(false);
        if (wrongImage != null) wrongImage.SetActive(false);
    }
}
