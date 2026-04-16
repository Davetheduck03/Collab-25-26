using UnityEngine;
using System.Collections;

public class DailyMonologueManager : MonoBehaviour
{
    [Header("Maelle Settings")]
    public string characterName = "Maelle";
    public Sprite maellePortrait;

    [Header("Settings")]
    public float startDelay = 1.5f; // How long to wait before she speaks

    private void Awake()
    {
        // Subscribe to the TimeManager's Day Changed event.
        // This fires when the player sleeps AND when the scene first loads!
        TimeManager.OnDayChanged += HandleDayChanged;
    }

    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        TimeManager.OnDayChanged -= HandleDayChanged;
    }

    private void HandleDayChanged(int newDay)
    {
        // IMPORTANT: If this is the very first time they are playing, 
        // the GameIntroManager is handling the Gustaff story! 
        // We only want to play this daily reminder if the intro is finished.
        if (PlayerPrefs.GetInt("HasPlayedIntro", 0) == 0) return;

        // Wait a little bit for the scene to fade in before talking
        StartCoroutine(PlayMorningMonologue());
    }

    private IEnumerator PlayMorningMonologue()
    {
        yield return new WaitForSeconds(startDelay);

        // Get the current day and the current quota target safely
        int currentDay = TimeManager.Instance != null ? TimeManager.Instance.currentDay : 1;
        int currentQuota = QuotaManager.Instance != null ? QuotaManager.Instance.GoldTarget : 0;

        // Create a dialogue node dynamically through code!
        DialogueNode dailyNode = new DialogueNode();
        dailyNode.sentence = $"This is Day <color=#FF8C00>{currentDay}</color>... I need to earn <color=#FFD700>{currentQuota} gold</color> today to pay Mr. Gustaff.";

        // Prevent null errors in the DialogueManager
        dailyNode.choices = new DialogueChoice[0];
        dailyNode.nextNodeIndex = -1; // End dialogue after this

        DialogueNode[] nodesToPlay = new DialogueNode[] { dailyNode };

        // Start the dialogue!
        DialogueManager.Instance.StartDialogue(characterName, maellePortrait, nodesToPlay, null);
    }

    // =======================================================
    // --- NEW: TEST BUTTON ---
    // =======================================================
    [ContextMenu("▶ Play Morning Monologue (Test)")]
    public void TestMonologue()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ You must be in Play Mode to test the dialogue!");
            return;
        }

        // Stop the coroutine if it's already running to prevent overlap
        StopAllCoroutines();

        // Start the dialogue sequence
        StartCoroutine(PlayMorningMonologue());
    }
}