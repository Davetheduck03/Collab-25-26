using UnityEngine;
using UnityEngine.EventSystems; // Required for Hover and Click detection
using Phuc.SoundSystem; // Your custom sound system!

public class UISoundTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Menu/Panel Sounds (Optional)")]
    [Tooltip("Plays when this UI element is turned ON (Menu Open)")]
    public SO_SFXEvent openSound;
    [Tooltip("Plays when this UI element is turned OFF (Menu Close)")]
    public SO_SFXEvent closeSound;

    [Header("Button Sounds (Optional)")]
    [Tooltip("Plays when the mouse hovers over this UI element")]
    public SO_SFXEvent hoverSound;
    [Tooltip("Plays when this UI element is clicked")]
    public SO_SFXEvent clickSound;

    // ==========================================
    // --- MENU OPEN / CLOSE ---
    // ==========================================
    private void OnEnable()
    {
        if (openSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(openSound);
        }
    }

    private void OnDisable()
    {
        // Safety Check: We don't want the close sound to play if the game is quitting 
        // or the scene is being destroyed, only when the menu is actually closed!
        if (closeSound != null && SoundManager.Instance != null && gameObject.scene.isLoaded)
        {
            SoundManager.Instance.PlaySfx(closeSound);
        }
    }

    // ==========================================
    // --- MOUSE HOVER / CLICK ---
    // ==========================================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(hoverSound);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(clickSound);
        }
    }
}