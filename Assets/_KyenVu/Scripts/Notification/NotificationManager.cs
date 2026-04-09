using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    public GameObject notificationPanel;
    public TMP_Text notificationText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        HideNotification();
    }
    public void ShowNotification(string message)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);

        // Optional: Auto-hide the notification after 3 seconds
        CancelInvoke(nameof(HideNotification));
        Invoke(nameof(HideNotification), 3f);
    }

    public void HideNotification()
    {
        notificationPanel.SetActive(false);
        PlayerStateManager player = Object.FindFirstObjectByType<PlayerStateManager>();
        if (player != null && player.currentState == player.InteractState)
        {
            player.EndInteraction();
        }
    }
}