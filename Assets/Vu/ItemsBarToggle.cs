using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public class ItemsBarToggle : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform bookPanel;  // Your UI bar (the book page)

    [Header("Animation Settings")]
    public float slideDistance = 200f;   // How far to move when hidden
    public float duration = 0.6f;        // Animation duration
    public float scalePunch = 0.05f;     // Little scale bounce
    public Ease openEase = Ease.OutBack; // Opening smoothness
    public Ease closeEase = Ease.InBack; // Closing smoothness

    private bool isOpen = false;
    private Vector2 originalPos;
    private Color originalColor;

    void Start()
    {
        originalPos = bookPanel.anchoredPosition;
        

        // Start hidden
        bookPanel.anchoredPosition = originalPos - new Vector2(0, slideDistance);
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

            // Open animation
            Sequence openSeq = DOTween.Sequence();
            openSeq.Append(bookPanel.DOAnchorPos(originalPos, duration).SetEase(openEase));
            openSeq.Join(bookPanel.DOPunchScale(Vector3.one * scalePunch, 0.4f, 8, 1));

          
        }
        else
        {
            // Close animation
            Sequence closeSeq = DOTween.Sequence();
            closeSeq.Append(bookPanel.DOAnchorPos(originalPos - new Vector2(0, slideDistance), duration).SetEase(closeEase));

         

            closeSeq.OnComplete(() => bookPanel.gameObject.SetActive(false));
        }
    }
}
