using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class PageBase : MonoBehaviour
{
    [Header("Shared Animation Settings")]
    public Image animationImage;        // Same for all pages
    public Sprite[] frames;             // Shared animation
    public float frameRate = 0.05f;

    // Called by BookManager when opening this page
    public virtual IEnumerator PlayShowAnimation()
    {
        gameObject.SetActive(true);

        if (animationImage != null && frames != null && frames.Length > 0)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                animationImage.sprite = frames[i];
                yield return new WaitForSeconds(frameRate);
            }
        }
    }

    // Called by BookManager when closing this page
    public virtual IEnumerator PlayHideAnimation()
    {
        if (animationImage != null && frames != null && frames.Length > 0)
        {
            for (int i = frames.Length - 1; i >= 0; i--)
            {
                animationImage.sprite = frames[i];
                yield return new WaitForSeconds(frameRate);
            }
        }

        gameObject.SetActive(false);
    }
}
