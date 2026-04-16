#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine;

public class SaveDataCleaner
{
    // This creates a new clickable button in the top menu bar of Unity!
    [MenuItem("Tools/Clear All Save Data")]
    public static void ClearSaveData()
    {
        // 1. Delete ALL .json files in the persistent data path
        string[] jsonFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");
        foreach (string file in jsonFiles)
        {
            File.Delete(file);
        }

        // 2. Clear all PlayerPrefs (Volume settings, etc.)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("🧹 [SaveDataCleaner] All JSON saves and PlayerPrefs have been completely wiped!");
    }
}
#endif