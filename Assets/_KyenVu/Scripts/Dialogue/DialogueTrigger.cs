using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int nextNodeIndex;
    public UnityEvent onChoiceSelected;
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(3, 5)]
    public string sentence;

    // NEW: The boolean tick box
    [Header("Mission Settings")]
    public bool isMission;

    [Tooltip("Assign a mission here to automatically replace the sentence with the mission's description, objectives, and rewards!")]
    public MissionSO missionDetails;

    public DialogueChoice[] choices;
    public int nextNodeIndex = -1;
}

// ... Keep your DialogueSet and DialogueTrigger classes exactly the same here ...


// =========================================================================
// CUSTOM INSPECTOR CODE (Paste this at the very bottom of DialogueTrigger.cs)
// =========================================================================
#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(DialogueNode))]
public class DialogueNodeDrawer : UnityEditor.PropertyDrawer
{
    public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
    {
        // If the array element is collapsed, just return a single line height
        if (!property.isExpanded)
            return UnityEditor.EditorGUIUtility.singleLineHeight;

        float height = UnityEditor.EditorGUIUtility.singleLineHeight + UnityEditor.EditorGUIUtility.standardVerticalSpacing;

        UnityEditor.SerializedProperty sentence = property.FindPropertyRelative("sentence");
        UnityEditor.SerializedProperty isMission = property.FindPropertyRelative("isMission");
        UnityEditor.SerializedProperty missionDetails = property.FindPropertyRelative("missionDetails");
        UnityEditor.SerializedProperty choices = property.FindPropertyRelative("choices");
        UnityEditor.SerializedProperty nextNodeIndex = property.FindPropertyRelative("nextNodeIndex");

        height += UnityEditor.EditorGUI.GetPropertyHeight(sentence, true) + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
        height += UnityEditor.EditorGUI.GetPropertyHeight(isMission, true) + UnityEditor.EditorGUIUtility.standardVerticalSpacing;

        // Only add height for missionDetails if the box is ticked!
        if (isMission.boolValue)
        {
            height += UnityEditor.EditorGUI.GetPropertyHeight(missionDetails, true) + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
        }

        height += UnityEditor.EditorGUI.GetPropertyHeight(choices, true) + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
        height += UnityEditor.EditorGUI.GetPropertyHeight(nextNodeIndex, true) + UnityEditor.EditorGUIUtility.standardVerticalSpacing;

        return height;
    }

    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
    {
        UnityEditor.EditorGUI.BeginProperty(position, label, property);

        Rect currentRect = new Rect(position.x, position.y, position.width, UnityEditor.EditorGUIUtility.singleLineHeight);

        // Draw the foldout arrow for the Array Element
        property.isExpanded = UnityEditor.EditorGUI.Foldout(currentRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            UnityEditor.EditorGUI.indentLevel++;
            currentRect.y += UnityEditor.EditorGUIUtility.singleLineHeight + UnityEditor.EditorGUIUtility.standardVerticalSpacing;

            UnityEditor.SerializedProperty sentence = property.FindPropertyRelative("sentence");
            UnityEditor.SerializedProperty isMission = property.FindPropertyRelative("isMission");
            UnityEditor.SerializedProperty missionDetails = property.FindPropertyRelative("missionDetails");
            UnityEditor.SerializedProperty choices = property.FindPropertyRelative("choices");
            UnityEditor.SerializedProperty nextNodeIndex = property.FindPropertyRelative("nextNodeIndex");

            float h = UnityEditor.EditorGUI.GetPropertyHeight(sentence, true);
            currentRect.height = h;
            UnityEditor.EditorGUI.PropertyField(currentRect, sentence, true);
            currentRect.y += h + UnityEditor.EditorGUIUtility.standardVerticalSpacing;

            h = UnityEditor.EditorGUI.GetPropertyHeight(isMission, true);
            currentRect.height = h;
            UnityEditor.EditorGUI.PropertyField(currentRect, isMission, true);
            currentRect.y += h + UnityEditor.EditorGUIUtility.standardVerticalSpacing;

            // Conditionally draw the Mission Details slot if ticked!
            if (isMission.boolValue)
            {
                h = UnityEditor.EditorGUI.GetPropertyHeight(missionDetails, true);
                currentRect.height = h;
                UnityEditor.EditorGUI.PropertyField(currentRect, missionDetails, true);
                currentRect.y += h + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
            }

            h = UnityEditor.EditorGUI.GetPropertyHeight(choices, true);
            currentRect.height = h;
            UnityEditor.EditorGUI.PropertyField(currentRect, choices, true);
            currentRect.y += h + UnityEditor.EditorGUIUtility.standardVerticalSpacing;

            h = UnityEditor.EditorGUI.GetPropertyHeight(nextNodeIndex, true);
            currentRect.height = h;
            UnityEditor.EditorGUI.PropertyField(currentRect, nextNodeIndex, true);

            UnityEditor.EditorGUI.indentLevel--;
        }
        UnityEditor.EditorGUI.EndProperty();
    }
}
#endif

[System.Serializable]
public class DialogueSet
{
    public string note = "Conversation";
    public int requiredEncounterLevel = 0;
    public DialogueNode[] dialogueNodes;

    // NEW: Fire an event when this specific conversation finishes!
    [Header("Events")]
    [Tooltip("Fires when the player closes this dialogue.")]
    public UnityEvent onDialogueEnd; 
}

public class DialogueTrigger : MonoBehaviour
{
    [Header("NPC Data")]
    public string npcName = "Mysterious Stranger";
    public Sprite npcPortrait;

    [Header("Conversations")]
    public DialogueSet[] dialogueSets;


    public void TriggerDialogue()
    {
        if (TimeManager.Instance != null && !TimeManager.Instance.IsShopOpen())
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification("The shop is closed, please come back at 5:00 AM.");
            }

           
            PlayerStateManager player = Object.FindFirstObjectByType<PlayerStateManager>();
            if (player != null)
            {
                player.EndInteraction();
            }
            return; 
        }

   
        int currentEncounter = MissionManager.Instance != null ? MissionManager.Instance.GetEncounterLevel(npcName) : 0;

        DialogueSet validSet = null;
        int highestValidLevel = -1;

        foreach (var set in dialogueSets)
        {
            if (set.requiredEncounterLevel <= currentEncounter && set.requiredEncounterLevel > highestValidLevel)
            {
                validSet = set;
                highestValidLevel = set.requiredEncounterLevel;
            }
        }

        if (validSet != null)
        {
            DialogueManager.Instance.StartDialogue(npcName, npcPortrait, validSet.dialogueNodes, validSet.onDialogueEnd);
        }
    }

}