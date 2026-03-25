using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the level-up augment selection panel.
/// • Uses weighted random selection (AugmentSO.weight).
/// • Each augment can only be picked once per run ("ALL AUGMENTS CAN ONLY BE PICKED ONCE").
/// • Calls AugmentApplier.Apply() when a card is chosen.
/// </summary>
public class AugmentPanelManager : GameSingleton<AugmentPanelManager>
{
    public static event Action OnPanelOpened;
    public static event Action OnPanelClosed;

    [Header("Panel")]
    [SerializeField] private GameObject augmentPanel;
    [SerializeField] private AugmentCardUI[] cards; // typically 3 cards

    [Header("Augment Pool")]
    [Tooltip("Drag all your AugmentSO assets here.")]
    [SerializeField] private List<AugmentSO> allAugments;

    // Tracks which augments have already been picked this run
    private readonly HashSet<AugmentSO> pickedAugments = new HashSet<AugmentSO>();

    // Raycasters that were disabled when we opened the panel (restored on close)
    private readonly List<GraphicRaycaster> disabledRaycasters = new List<GraphicRaycaster>();

    // ── Unity ─────────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        if (augmentPanel != null)
            augmentPanel.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void OpenPanel()
    {
        if (augmentPanel == null)
        {
            Debug.LogWarning("[AugmentPanelManager] augmentPanel is not assigned!");
            return;
        }

        // Build pool of unpicked augments
        var available = allAugments.FindAll(a => !pickedAugments.Contains(a));
        if (available.Count == 0)
        {
            Debug.Log("[AugmentPanelManager] All augments have been picked — skipping panel.");
            return;
        }

        if (cards == null || cards.Length == 0)
        {
            Debug.LogError("[AugmentPanelManager] cards array is empty — drag your AugmentCardUI objects into the Cards array in the Inspector!");
            return;
        }

        // Pick weighted-random choices (up to cards.Length)
        var choices = WeightedPickWithoutReplacement(available, cards.Length);

        for (int i = 0; i < cards.Length; i++)
        {
            bool hasChoice = i < choices.Count;
            cards[i].gameObject.SetActive(hasChoice);
            if (hasChoice)
            {
                Debug.Log($"[AugmentPanelManager] Setting up card {i} with {choices[i].displayName}");
                cards[i].Setup(choices[i], OnAugmentChosen);
            }
        }

        augmentPanel.SetActive(true);

        // Disable ALL other GraphicRaycasters so Screen Space Overlay canvases
        // (like the game HUD) cannot intercept clicks meant for our panel.
        var myRaycaster = augmentPanel.GetComponentInChildren<GraphicRaycaster>(true);
        disabledRaycasters.Clear();
        foreach (var gr in FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None))
        {
            if (gr != myRaycaster && gr.enabled)
            {
                gr.enabled = false;
                disabledRaycasters.Add(gr);
                Debug.Log($"[AugmentPanelManager] Disabled raycaster on '{gr.gameObject.name}'.");
            }
        }

        Time.timeScale = 0f;
        OnPanelOpened?.Invoke();
        Debug.Log("[AugmentPanelManager] Panel opened.");
    }

    public void ClosePanel()
    {
        if (augmentPanel == null) return;
        augmentPanel.SetActive(false);

        // Restore all raycasters we disabled when opening
        foreach (var gr in disabledRaycasters)
            if (gr != null) gr.enabled = true;
        disabledRaycasters.Clear();

        Time.timeScale = 1f;
        OnPanelClosed?.Invoke();
        Debug.Log("[AugmentPanelManager] Panel closed.");
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnAugmentChosen(AugmentSO chosen)
    {
        pickedAugments.Add(chosen);
        AugmentApplier.Apply(chosen);
        ClosePanel();
    }

    /// <summary>
    /// Picks <paramref name="count"/> distinct items from <paramref name="pool"/>
    /// using each item's weight as its relative probability.
    /// </summary>
    private List<AugmentSO> WeightedPickWithoutReplacement(List<AugmentSO> pool, int count)
    {
        var result = new List<AugmentSO>();
        var remaining = new List<AugmentSO>(pool);

        for (int i = 0; i < count && remaining.Count > 0; i++)
        {
            int totalWeight = 0;
            foreach (var a in remaining) totalWeight += Mathf.Max(1, a.weight);

            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;

            for (int j = 0; j < remaining.Count; j++)
            {
                cumulative += Mathf.Max(1, remaining[j].weight);
                if (roll < cumulative)
                {
                    result.Add(remaining[j]);
                    remaining.RemoveAt(j);
                    break;
                }
            }
        }

        return result;
    }
}
