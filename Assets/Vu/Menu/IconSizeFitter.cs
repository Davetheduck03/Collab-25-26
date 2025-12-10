using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IconSizeFitter : MonoBehaviour
{
    public float fixedSize = 48f; // Change this number to make icons smaller/bigger

    private void OnEnable()
    {
        Image img = GetComponent<Image>();
        if (img.sprite != null)
        {
            // Force exact size, keep aspect ratio
            RectTransform rt = img.rectTransform;
            rt.sizeDelta = Vector2.one * fixedSize;
        }
    }
}