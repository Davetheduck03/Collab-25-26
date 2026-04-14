using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Quest Tracker HUD — place this on the CANVAS ROOT, NOT on the QuestPanel child.
/// If placed on QuestPanel, SetActive(false) will kill Update() and the panel can
/// never re-enable itself.
/// </summary>
public class QuestTrackerHUD : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("The child panel to show/hide based on quest state")]
    [SerializeField] private GameObject questPanel;
    [Tooltip("Transform inside QuestPanel where quest rows are spawned")]
    [SerializeField] private RectTransform rowContainer;
    [Tooltip("Prefab with a QuestTrackerRow component")]
    [SerializeField] private QuestTrackerRow questRowPrefab;
    [Tooltip("'No active quests' label inside QuestPanel")]
    [SerializeField] private GameObject emptyLabel;

    [Header("Persistence")]
    [SerializeField] private bool persistAcrossScenes = true;

    private readonly List<QuestTrackerRow> activeRows = new();

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (!persistAcrossScenes) return;

        // Duplicate guard — only one HUD survives scene loads
        var existing = FindObjectsByType<QuestTrackerHUD>(FindObjectsSortMode.None);
        foreach (var other in existing)
        {
            if (other != this)
            {
                Destroy(transform.root.gameObject);
                return;
            }
        }
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void OnEnable()  => Refresh();
    private void Update()    => Refresh();   // lightweight — only redraws when counts change

    // ─────────────────────────────────────────────────────────────────────────

    public void Refresh()
    {
        if (questRowPrefab == null)
        {
            TryLoadPrefabFallback();
            if (questRowPrefab == null) return;
        }

        // Collect fish objectives from active missions
        var objectives = new List<ObjectiveProgress>();
        if (MissionManager.Instance != null)
        {
            foreach (var mission in MissionManager.Instance.activeMissions)
                foreach (var progress in mission.progressList)
                    if (progress.objective.missionType == MissionType.Fish)
                        objectives.Add(progress);
        }

        bool hasQuests = objectives.Count > 0;

        // Toggle panel visibility
        if (questPanel != null)
            questPanel.SetActive(hasQuests);

        if (emptyLabel != null)
            emptyLabel.SetActive(!hasQuests);

        if (!hasQuests)
        {
            ClearRows();
            return;
        }

        // Rebuild rows only when count changes (avoid every-frame allocation)
        if (activeRows.Count == objectives.Count)
        {
            for (int i = 0; i < activeRows.Count; i++)
                activeRows[i].Populate(objectives[i]);
            return;
        }

        ClearRows();
        foreach (var obj in objectives)
        {
            var row = Instantiate(questRowPrefab, rowContainer);
            row.Populate(obj);
            activeRows.Add(row);
        }
    }

    private void ClearRows()
    {
        foreach (var row in activeRows)
            if (row != null) Destroy(row.gameObject);
        activeRows.Clear();
    }

    private void TryLoadPrefabFallback()
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("QuestTrackerRow t:Prefab");
        if (guids.Length == 0) return;
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go != null) questRowPrefab = go.GetComponent<QuestTrackerRow>();
#endif
    }
}
