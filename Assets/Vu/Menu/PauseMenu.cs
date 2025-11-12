using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem; 

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform pausePanel; // Your main pause menu panel
    public GameObject gameUI;        // Optional: gameplay UI to hide while paused

    [Header("Animation Settings")]
    public float scaleIn = 1.05f;    // Slight overshoot when opening
    public float duration = 0.5f;    // Animation time
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.localScale = Vector3.zero;
        pausePanel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            pausePanel.gameObject.SetActive(true);
            if (gameUI != null) gameUI.SetActive(false);

            //  Open animation
            Sequence openSeq = DOTween.Sequence();
            openSeq.Append(pausePanel.DOScale(scaleIn, duration * 0.7f).SetEase(openEase));
            openSeq.Append(pausePanel.DOScale(1f, 0.2f).SetEase(Ease.OutQuad));

        }
        else
        {
            //  Close animation
            Sequence closeSeq = DOTween.Sequence();
            closeSeq.Append(pausePanel.DOScale(0f, duration * 0.6f).SetEase(closeEase));
            closeSeq.OnComplete(() =>
            {
                pausePanel.gameObject.SetActive(false);
                if (gameUI != null) gameUI.SetActive(true);
            });
        }
    }
}
