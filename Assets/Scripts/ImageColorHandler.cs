using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ImageColorHandler : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }
    public void SetAlphaToZero()
    {
        if (targetImage == null)
        {
            Debug.Log("No Image");
            return;
        }

        if (targetImage != null)
        {
            targetImage.DOColor(Color.clear, 0f);
            //Debug.Log("Image alpha set to zero" + targetImage.name);
        }
    }
}
