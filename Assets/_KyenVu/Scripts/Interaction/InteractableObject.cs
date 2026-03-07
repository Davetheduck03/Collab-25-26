using UnityEngine;
using UnityEngine.Events; // Required for UnityEvents

public class InteractableObject : MonoBehaviour, I_Interactable
{
    public enum InteractType { Dialogue, Item, Door, Shop }

    [Header("Interaction Settings")]
    public InteractType interactType;

    [Tooltip("Drag the child GameObject containing your 'E' sprite here")]
    [SerializeField] private GameObject ePromptIcon;

    [Header("What happens when interacted?")]
    public UnityEvent onInteract;

    void Start()
    {
        // Ensure the prompt is hidden when the game starts
        if (ePromptIcon != null) ePromptIcon.SetActive(false);
    }

    public void Interaction()
    {
        Debug.Log($"Player interacted with an object of type: {interactType}");

        // This triggers whatever you set up in the Unity Inspector!
        onInteract?.Invoke();
    }

    public void ShowPrompt()
    {
        if (ePromptIcon != null) ePromptIcon.SetActive(true);
    }

    public void HidePrompt()
    {
        if (ePromptIcon != null) ePromptIcon.SetActive(false);
    }
}