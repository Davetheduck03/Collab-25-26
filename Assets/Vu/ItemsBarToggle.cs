using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem; 

public class UIBookBarToggle : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform bookPanel;
    public Image backgroundImage; // optional background for fade

    [Header("Animation Settings")]
    public float slideDistance = 200f;
    public float duration = 0.6f;
    public float scalePunch = 0.05f;
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack;

    private bool isOpen = false;
    private Vector2 originalPos;
    private Color originalColor;

    void Start()
    {
        originalPos = bookPanel.anchoredPosition;
        if (backgroundImage != null)
            originalColor = backgroundImage.color;

        // start hidden
        bookPanel.anchoredPosition = originalPos - new Vector2(0, slideDistance);
        if (backgroundImage != null)
            backgroundImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        bookPanel.gameObject.SetActive(false);
    }

    void Update()
    {
        
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleBook();
        }
    }

    void ToggleBook()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            bookPanel.gameObject.SetActive(true);

            Sequence openSeq = DOTween.Sequence();
            openSeq.Append(bookPanel.DOAnchorPos(originalPos, duration).SetEase(openEase));
            openSeq.Join(bookPanel.DOPunchScale(Vector3.one * scalePunch, 0.4f, 8, 1));

            if (backgroundImage != null)
                openSeq.Join(backgroundImage.DOColor(originalColor, duration * 0.8f)
                    .From(new Color(originalColor.r, originalColor.g, originalColor.b, 0)));
        }
        else
        {
            Sequence closeSeq = DOTween.Sequence();
            closeSeq.Append(bookPanel.DOAnchorPos(originalPos - new Vector2(0, slideDistance), duration).SetEase(closeEase));

            if (backgroundImage != null)
                closeSeq.Join(backgroundImage.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0), duration * 0.6f));

            closeSeq.OnComplete(() => bookPanel.gameObject.SetActive(false));
        }
    }
}
