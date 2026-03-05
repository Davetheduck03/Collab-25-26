using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [Tooltip("The text the player will see on the button")]
    public string choiceText;
    [Tooltip("The Index of the Node this choice leads to. Set to -1 to end conversation.")]
    public int nextNodeIndex;
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(3, 5)]
    public string sentence;

    [Header("Branching (Optional)")]
    [Tooltip("If you add choices here, the game will wait for the player to click one.")]
    public DialogueChoice[] choices;

    [Tooltip("If there are NO choices, it automatically goes to this index next. (-1 ends conversation)")]
    public int nextNodeIndex = -1;
}

public class DialogueTrigger : MonoBehaviour
{
    [Header("NPC Data")]
    public string npcName = "Mysterious Stranger";

    [Header("Conversation Nodes")]
    [Tooltip("Index 0 is always the starting dialogue.")]
    public DialogueNode[] dialogueNodes;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(npcName, dialogueNodes);
    }
}