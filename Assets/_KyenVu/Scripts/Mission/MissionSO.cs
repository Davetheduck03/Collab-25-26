using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum MissionType { Fish, Buy, Collect, Sell, Talk}

[System.Serializable]
public class MissionObjective
{
    public MissionType missionType;

    [Tooltip("Click the little circle to select a fish from your project files!")]
    public UnitSO targetFish;

    // TODO: If you use a different script for items later, just change "ItemData" to your new script name here:
    [Tooltip("Click the little circle to select an item to buy!")]
    public ItemData targetItem;

    [Tooltip("Type the name of the item to collect, sell, or NPC to talk to.")]
    public string targetName;

    public int targetAmount = 1;

    public string GetTargetID()
    {
        if (missionType == MissionType.Fish && targetFish != null)
            return targetFish.UnitName;

        // NEW: If it's a Buy mission, automatically use the Item's name!
        if (missionType == MissionType.Buy && targetItem != null)
            return targetItem.name;

        return targetName;
    }
}

[CreateAssetMenu(fileName = "New Mission", menuName = "Fishing Game/Mission")]
public class MissionSO : ScriptableObject
{
    public string missionID;
    public string missionName;
    [TextArea] public string description;

    [Header("Objectives")]
    public List<MissionObjective> objectives = new List<MissionObjective>();

    [Header("Rewards")]
    public int goldReward = 100;
}


// =========================================================================
// CUSTOM INSPECTOR CODE
// =========================================================================

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MissionObjective))]
public class MissionObjectiveDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Calculate the positions for 3 rows of data
        Rect typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect targetRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
        Rect amountRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 2, position.width, EditorGUIUtility.singleLineHeight);

        // Find the properties
        SerializedProperty typeProp = property.FindPropertyRelative("missionType");
        SerializedProperty fishProp = property.FindPropertyRelative("targetFish");
        SerializedProperty itemProp = property.FindPropertyRelative("targetItem"); // NEW: Find the item variable
        SerializedProperty nameProp = property.FindPropertyRelative("targetName");
        SerializedProperty amountProp = property.FindPropertyRelative("targetAmount");

        // 1. Draw the Mission Type dropdown
        EditorGUI.PropertyField(typeRect, typeProp);

        // 2. Conditionally draw the Target field based on the Dropdown choice!
        if (typeProp.enumValueIndex == (int)MissionType.Fish)
        {
            EditorGUI.PropertyField(targetRect, fishProp, new GUIContent("Target Fish"));
        }
        else if (typeProp.enumValueIndex == (int)MissionType.Buy) 
        {
            EditorGUI.PropertyField(targetRect, itemProp, new GUIContent("Target Item"));
        }
        else
        {
            EditorGUI.PropertyField(targetRect, nameProp, new GUIContent("Target Name"));
        }

        // 3. Draw the Amount field
        EditorGUI.PropertyField(amountRect, amountProp, new GUIContent("Amount Required"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2) * 3;
    }
}
#endif