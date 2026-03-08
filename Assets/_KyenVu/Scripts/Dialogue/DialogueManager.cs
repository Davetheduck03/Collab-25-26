using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Required for Buttons

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Choices UI")]
    [SerializeField] private GameObject choicesContainer; // Parent object holding the buttons
    [SerializeField] private Button[] choiceButtons;      // Array of your 3 or 4 buttons
    [SerializeField] private TextMeshProUGUI[] choiceTexts; // The text inside those buttons

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.02f;

    private DialogueNode[] currentNodes;
    private int currentNodeIndex;
    private bool isTyping = false;

    public bool isDialogueActive { get; private set; }
    public bool isWaitingForChoice { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);
    }

    public void StartDialogue(string npcName, DialogueNode[] nodes)
    {
        isDialogueActive = true;
        isWaitingForChoice = false;
        dialoguePanel.SetActive(true);
        nameText.text = npcName;

        currentNodes = nodes;
        currentNodeIndex = 0; // Always start at Node 0

        DisplayNode(currentNodeIndex);
    }

    private void DisplayNode(int nodeIndex)
    {
        currentNodeIndex = nodeIndex;
        DialogueNode node = currentNodes[currentNodeIndex];

        choicesContainer.SetActive(false); // Hide buttons while typing
        StartCoroutine(TypeSentence(node.sentence));
    }

    public void DisplayNextSentence()
    {
        // Don't allow skipping if we are waiting for a button click!
        if (isWaitingForChoice) return;

        DialogueNode node = currentNodes[currentNodeIndex];

        // 1. If currently typing, press E to instantly finish text
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = node.sentence;
            isTyping = false;

            // If this node has choices, show them now that typing finished!
            if (node.choices.Length > 0) ShowChoices();
            return;
        }

        // 2. If it's a normal node with no choices, go to the next index
        if (node.choices.Length == 0)
        {
            if (node.nextNodeIndex == -1)
            {
                EndDialogue();
            }
            else
            {
                DisplayNode(node.nextNodeIndex);
            }
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
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

        DialogueNode node = currentNodes[currentNodeIndex];

        // Loop through our UI buttons and assign the choice data
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < node.choices.Length)
            {
                // We have a choice for this button
                choiceButtons[i].gameObject.SetActive(true);
                choiceTexts[i].text = node.choices[i].choiceText;

                int targetIndex = node.choices[i].nextNodeIndex;

                // Remove old clicks and add the new jump destination
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => MakeChoice(targetIndex));
            }
            else
            {
                // Hide extra buttons we don't need
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
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
        choicesContainer.SetActive(false);
        Debug.Log("End of conversation.");
    }
}