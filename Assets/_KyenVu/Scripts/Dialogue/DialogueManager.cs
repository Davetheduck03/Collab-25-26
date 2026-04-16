using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Choices UI")]
    [SerializeField] private GameObject choicesContainer;
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

    private bool suppressEndEvent = false;

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
        suppressEndEvent = false;

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
                portraitImage.gameObject.SetActive(false);
            }
        }
        currentNodes = nodes;
        currentNodeIndex = 0;

        DisplayNode(currentNodeIndex);
    }

    private void DisplayNode(int nodeIndex)
    {
        currentNodeIndex = nodeIndex;
        DialogueNode node = currentNodes[currentNodeIndex];

        choicesContainer.SetActive(false);

        currentFormattedSentence = node.sentence;

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
        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();
        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        int currentVisibleCharacters = 0;

        while (currentVisibleCharacters < totalVisibleCharacters)
        {
            currentVisibleCharacters++;
            dialogueText.maxVisibleCharacters = currentVisibleCharacters;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        if (currentNodes[currentNodeIndex].choices.Length > 0)
        {
            ShowChoices();
        }
    }

    private void ShowChoices()
    {
        isWaitingForChoice = true;
        choicesContainer.SetActive(true);

        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }

        DialogueNode node = currentNodes[currentNodeIndex];

        for (int i = 0; i < node.choices.Length; i++)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            Button buttonComp = buttonObj.GetComponent<Button>();
            TextMeshProUGUI textComp = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (textComp != null)
            {
                textComp.text = node.choices[i].choiceText;
            }

            int targetIndex = node.choices[i].nextNodeIndex;
            UnityEngine.Events.UnityEvent onSelectEvent = node.choices[i].onChoiceSelected;

            bool shouldSuppress = node.choices[i].suppressEndEvent;

            // --- NEW: Read the money reward settings from the inspector ---
            bool givesMoney = node.choices[i].givesMoney;
            int moneyToGive = node.choices[i].moneyToGive;

            buttonComp.onClick.AddListener(() =>
            {
                if (shouldSuppress)
                {
                    suppressEndEvent = true;
                }

                if (givesMoney && CurrencyManager.Instance != null)
                {
                    // CHANGE THIS LINE:
                    CurrencyManager.Instance.AddFreeCurrency(moneyToGive);

                    // Optional: Pop up a notification so the player knows they got it!
                    if (NotificationManager.Instance != null)
                    {
                        NotificationManager.Instance.ShowNotification($"Received {moneyToGive} Gold!");
                    }
                }

                onSelectEvent?.Invoke();
                MakeChoice(targetIndex);
            });
        }
    }

    private string GenerateMissionText(MissionSO mission)
    {
        string text = $"<b><color=#FFD700>Mission: {mission.missionName}</color></b>\n";
        text += $"{mission.description}\n";
        text += "<b><color=#FF8C00>Objectives:</color></b>\n";
        foreach (var obj in mission.objectives)
        {
            text += $"- {obj.missionType.ToString()} {obj.GetTargetID()} x{obj.targetAmount}\n";
        }
        text += "<b><color=#00FF7F>Rewards:</color></b>\n";
        if (mission.goldReward > 0) text += $"- {mission.goldReward} Gold\n";
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

        // Check the flag before firing the event! 
        if (!suppressEndEvent)
        {
            currentDialogueEndEvent?.Invoke();
            Debug.Log("End of conversation. Goodbye event fired.");
        }
        else
        {
            Debug.Log("End event suppressed (Transitioning to menu).");
        }

        currentDialogueEndEvent = null;
        suppressEndEvent = false;

        PlayerStateManager player = Object.FindFirstObjectByType<PlayerStateManager>();
        if (player != null)
        {
            player.EndInteraction();
        }
    }
}