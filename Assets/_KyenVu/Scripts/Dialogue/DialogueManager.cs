using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Required for Buttons

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private Image portraitImage;

    [Header("Choices UI")]
    [SerializeField] private GameObject choicesContainer; // Parent object holding the buttons
    // NEW: We replaced the arrays with a single prefab
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.02f;

    private DialogueNode[] currentNodes;
    private int currentNodeIndex;
    private bool isTyping = false;
    private string currentFormattedSentence;

    private UnityEngine.Events.UnityEvent currentDialogueEndEvent;

    public bool isDialogueActive { get; private set; }
    public bool isWaitingForChoice { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);
    }

    public void StartDialogue(string npcName, Sprite portrait, DialogueNode[] nodes, UnityEngine.Events.UnityEvent onEndEvent = null)
    {
        isDialogueActive = true;
        isWaitingForChoice = false;
        dialoguePanel.SetActive(true);
        nameText.text = npcName;

        currentDialogueEndEvent = onEndEvent;
        if (portraitImage != null)
        {
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                // Hide the portrait UI if the NPC has no image
                portraitImage.gameObject.SetActive(false);
            }
        }
        currentNodes = nodes;
        currentNodeIndex = 0; // Always start at Node 0

        DisplayNode(currentNodeIndex);
    }

    private void DisplayNode(int nodeIndex)
    {
        currentNodeIndex = nodeIndex;
        DialogueNode node = currentNodes[currentNodeIndex];

        choicesContainer.SetActive(false);

        currentFormattedSentence = node.sentence;

        // CHANGED: Now we make sure 'isMission' is true before generating mission text!
        if (node.isMission && node.missionDetails != null)
        {
            currentFormattedSentence = GenerateMissionText(node.missionDetails);
        }

        StartCoroutine(TypeSentence(currentFormattedSentence));
    }

    public void DisplayNextSentence()
    {
        if (isWaitingForChoice) return;

        DialogueNode node = currentNodes[currentNodeIndex];

        if (isTyping)
        {
            StopAllCoroutines();

            dialogueText.text = currentFormattedSentence;

            // NEW: Instantly reveal all characters when the player skips!
            dialogueText.maxVisibleCharacters = 99999;

            isTyping = false;

            if (node.choices.Length > 0) ShowChoices();
            return;
        }

        if (node.choices.Length == 0)
        {
            if (node.nextNodeIndex == -1) EndDialogue();
            else DisplayNode(node.nextNodeIndex);
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;

        // 1. Give TMP the full text immediately so it parses the <color> and <b> tags!
        dialogueText.text = sentence;

        // 2. Hide all the text initially
        dialogueText.maxVisibleCharacters = 0;

        // Force TMP to update its mesh so we can count the actual visible characters (ignoring tags)
        dialogueText.ForceMeshUpdate();
        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        int currentVisibleCharacters = 0;

        // 3. Slowly reveal the text
        while (currentVisibleCharacters < totalVisibleCharacters)
        {
            currentVisibleCharacters++;
            dialogueText.maxVisibleCharacters = currentVisibleCharacters;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Automatically show choices when typing finishes naturally
        if (currentNodes[currentNodeIndex].choices.Length > 0)
        {
            ShowChoices();
        }
    }

    private void ShowChoices()
    {
        isWaitingForChoice = true;
        choicesContainer.SetActive(true);

        // NEW: Destroy old buttons left over from previous choices
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }

        DialogueNode node = currentNodes[currentNodeIndex];

        // NEW: Loop through the exact number of choices and spawn a button for each
        for (int i = 0; i < node.choices.Length; i++)
        {
            // Spawn the button prefab as a child of the choices container
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);

            // Get the Button and Text components from the spawned object
            Button buttonComp = buttonObj.GetComponent<Button>();
            TextMeshProUGUI textComp = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            // Set the text
            if (textComp != null)
            {
                textComp.text = node.choices[i].choiceText;
            }

            int targetIndex = node.choices[i].nextNodeIndex;
            UnityEngine.Events.UnityEvent onSelectEvent = node.choices[i].onChoiceSelected;

            // Add the listener to the newly spawned button
            buttonComp.onClick.AddListener(() =>
            {
                // Fire the custom event if one was assigned!
                onSelectEvent?.Invoke();

                // Move to the next node (or end conversation)
                MakeChoice(targetIndex);
            });
        }
    }
    private string GenerateMissionText(MissionSO mission)
    {
        // 1. Mission Name & Description
        string text = $"<b><color=#FFD700>Mission: {mission.missionName}</color></b>\n";
        text += $"{mission.description}\n";

        // 2. Objectives List
        text += "<b><color=#FF8C00>Objectives:</color></b>\n";
        foreach (var obj in mission.objectives)
        {
            text += $"- {obj.missionType.ToString()} {obj.GetTargetID()} x{obj.targetAmount}\n";
        }

        // 3. Rewards List
        text += "<b><color=#00FF7F>Rewards:</color></b>\n";
        if (mission.goldReward > 0) text += $"- {mission.goldReward} Gold\n";

        // Uncomment below if you added the ItemReward fields to MissionSO!
        // if (mission.itemReward != null) text += $"- {mission.itemReward.name} x{mission.itemRewardAmount}\n";

        return text;
    }
    public void MakeChoice(int targetNodeIndex)
    {
        isWaitingForChoice = false;

        if (targetNodeIndex == -1)
        {
            EndDialogue();
        }
        else
        {
            DisplayNode(targetNodeIndex);
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        isWaitingForChoice = false;
        dialoguePanel.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);

        Debug.Log("End of conversation.");

        // NEW: Fire the end event before clearing it!
        currentDialogueEndEvent?.Invoke();
        currentDialogueEndEvent = null;

        PlayerStateManager player = Object.FindFirstObjectByType<PlayerStateManager>();
        if (player != null)
        {
            player.EndInteraction();
        }
    }
}