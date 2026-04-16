using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Self-building equipment selection panel shown before entering the fishing scene.
/// Add this script to an EMPTY GameObject in your Top-Down scene.
/// It creates the entire canvas hierarchy at runtime — no setup script or inspector
/// wiring needed.  Call PreFishingMenuUI.Instance.Open(sceneToLoad) to open.
/// </summary>
public class PreFishingMenuUI : MonoBehaviour
{
    public static PreFishingMenuUI Instance { get; private set; }

    public enum EquipSlotType { Rod, Hook, Boat, Zone }

    [Header("Font")]
    [Tooltip("Assign a TMP font asset here to use it for all text in the menu. " +
             "Leave empty to fall back to the TMP default font.")]
    public TMP_FontAsset menuFont;

    // ── Runtime-created UI refs ────────────────────────────────────────────
    private GameObject    panel;
    private CanvasGroup   cg;
    private TMP_Text      listHeaderText;
    private Transform     itemListContainer;
    private Button        goBtn, cancelBtn;
    private PreFishingEquipSlotUI rodSlot, hookSlot, boatSlot, zoneSlot;

    // ── State ──────────────────────────────────────────────────────────────
    private string       pendingScene;
    private EquipSlotType activeSlot = EquipSlotType.Rod;

    // ── Colours  (parchment / warm-paper palette) ──────────────────────────
    private static readonly Color C_Dim        = new Color(0.18f, 0.11f, 0.05f, 0.72f); // warm brown overlay
    private static readonly Color C_CardBg     = new Color(0.93f, 0.87f, 0.73f, 1.00f); // parchment
    private static readonly Color C_HeaderBg   = new Color(0.85f, 0.77f, 0.60f, 1.00f); // slightly darker parchment
    private static readonly Color C_FooterBg   = new Color(0.85f, 0.77f, 0.60f, 1.00f);
    private static readonly Color C_SlotBg     = new Color(0.88f, 0.81f, 0.66f, 1.00f); // mid parchment
    private static readonly Color C_SlotHL     = new Color(0.38f, 0.58f, 0.28f, 1.00f); // muted green (active)
    private static readonly Color C_SlotDim    = new Color(0.75f, 0.66f, 0.50f, 1.00f); // tan border (inactive)
    private static readonly Color C_Accent     = new Color(0.30f, 0.50f, 0.22f, 1.00f); // earthy green
    private static readonly Color C_TextWhite  = new Color(0.22f, 0.14f, 0.06f, 1.00f); // dark brown (primary text)
    private static readonly Color C_TextGrey   = new Color(0.45f, 0.35f, 0.22f, 1.00f); // warm mid-brown (secondary)
    private static readonly Color C_GoGreen    = new Color(0.30f, 0.48f, 0.20f, 1.00f); // muted green button
    private static readonly Color C_CancelRed  = new Color(0.58f, 0.26f, 0.18f, 1.00f); // terracotta button
    private static readonly Color C_RowSel     = new Color(0.35f, 0.55f, 0.25f, 0.30f); // green-tinted selection
    private static readonly Color C_RowNorm    = new Color(0.00f, 0.00f, 0.00f, 0.06f); // subtle dark tint on rows

    // ── Lifecycle ──────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }

        BuildUI();
        panel.SetActive(false);
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void Open(string sceneToLoad)
    {
        pendingScene = sceneToLoad;
        panel.SetActive(true);
        Time.timeScale = 0f;
        RefreshSlots();
        ShowSlotList(EquipSlotType.Rod);
        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f, 0.18f));
    }

    public void ShowSlotList(EquipSlotType slot)
    {
        activeSlot = slot;

        foreach (Transform child in itemListContainer)
            Destroy(child.gameObject);

        if (listHeaderText != null)
            listHeaderText.text = slot == EquipSlotType.Rod  ? "Select Rod"
                                : slot == EquipSlotType.Hook ? "Select Hook"
                                : slot == EquipSlotType.Zone ? "Select Zone"
                                :                              "Select Boat";

        rodSlot? .SetHighlight(slot == EquipSlotType.Rod);
        hookSlot?.SetHighlight(slot == EquipSlotType.Hook);
        boatSlot?.SetHighlight(slot == EquipSlotType.Boat);
        zoneSlot?.SetHighlight(slot == EquipSlotType.Zone);

        // ── Zone slot: entirely different data source ──────────────────────────
        if (slot == EquipSlotType.Zone)
        {
            var prog   = ZoneProgressionManager.Instance;
            var zones  = prog?.GetUnlockedZones();
            int selIdx = prog?.SelectedZoneIndex ?? 0;

            if (zones == null || zones.Count == 0)
            {
                SpawnEmptyRow("No zones available — assign zones to ZoneProgressionManager.");
            }
            else
            {
                for (int i = 0; i < zones.Count; i++)
                    SpawnZoneRow(zones[i], i, i == selIdx);
            }

            RelayoutItemList();
            return;
        }

        var inv = InventoryController.Instance;
        var em  = EquipmentManager.Instance;

        EquippableData equipped =
            slot == EquipSlotType.Rod  ? (EquippableData)em?.GetEquippedRod()
          : slot == EquipSlotType.Hook ? (EquippableData)em?.GetEquippedHook()
          :                              (EquippableData)em?.GetEquippedBoat();

        // Only show items the player actually owns
        bool found = false;
        if (inv != null)
        {
            foreach (var inv_item in inv.items)
            {
                if (inv_item?.data == null) continue;
                bool match = (slot == EquipSlotType.Rod  && inv_item.data is RodItemData)
                          || (slot == EquipSlotType.Hook && inv_item.data is HookItemData)
                          || (slot == EquipSlotType.Boat && inv_item.data is BoatItemData);
                if (!match) continue;
                found = true;
                SpawnRow(inv_item, inv_item.data == equipped);
            }
        }

        if (!found) SpawnEmptyRow($"No {slot} in inventory");

        // Manually lay out all rows so they are correctly positioned immediately,
        // without relying on Unity's layout system timing.
        RelayoutItemList();
    }

    /// <summary>Manually stack all rows in itemListContainer top-to-bottom.</summary>
    private void RelayoutItemList()
    {
        const float PAD     = 4f;
        const float SPACING = 3f;
        float yOff   = -PAD;
        float totalH =  PAD;

        for (int i = 0; i < itemListContainer.childCount; i++)
        {
            var childRT = itemListContainer.GetChild(i) as RectTransform;
            if (childRT == null) continue;

            float h = childRT.sizeDelta.y;
            childRT.anchoredPosition = new Vector2(0f, yOff);
            Debug.Log($"[PreFishing] row[{i}] h={h} pos={childRT.anchoredPosition}");
            yOff   -= h + SPACING;
            totalH += h + SPACING;
        }

        totalH += PAD;
        var cRT = itemListContainer as RectTransform;
        if (cRT != null)
        {
            cRT.sizeDelta = new Vector2(0f, totalH);
            Debug.Log($"[PreFishing] Content sized to h={totalH}");
        }
    }

    public void EquipItem(InventoryItem item)
    {
        if (item?.data is EquippableData e) e.OnEquip();
        RefreshSlots();
        ShowSlotList(activeSlot);
    }

    public void SelectZone(int index)
    {
        var prog = ZoneProgressionManager.Instance;
        if (prog != null)
        {
            prog.SelectedZoneIndex = index;
            prog.SaveSelectedZone();
        }
        RefreshSlots();
        ShowSlotList(EquipSlotType.Zone);
    }

    // ── Buttons ────────────────────────────────────────────────────────────

    private void OnCancel()  { StopAllCoroutines(); StartCoroutine(FadeClose()); }
    private void OnGoFish()  { StopAllCoroutines(); StartCoroutine(FadeLoad());  }

    private IEnumerator FadeClose()
    {
        yield return Fade(cg.alpha, 0f, 0.15f);
        panel.SetActive(false);
        Time.timeScale = 1f;
        // Release the player from the frozen InteractState
        FindObjectOfType<PlayerStateManager>()?.EndInteraction();
    }

    private IEnumerator FadeLoad()
    {
        yield return Fade(cg.alpha, 0f, 0.25f);
        panel.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(pendingScene);
    }

    private IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f; cg.alpha = from;
        while (t < dur) { t += Time.unscaledDeltaTime; cg.alpha = Mathf.Lerp(from, to, t / dur); yield return null; }
        cg.alpha = to;
    }

    // ── Slot refresh ───────────────────────────────────────────────────────

    private void RefreshSlots()
    {
        var em = EquipmentManager.Instance;
        if (em != null)
        {
            rodSlot? .Refresh(em.GetEquippedRod());
            hookSlot?.Refresh(em.GetEquippedHook());
            boatSlot?.Refresh(em.GetEquippedBoat());
        }

        var prog = ZoneProgressionManager.Instance;
        zoneSlot?.RefreshZone(prog?.GetZone(prog?.SelectedZoneIndex ?? 0));
    }

    // ══════════════════════════════════════════════════════════════════════
    // UI CONSTRUCTION
    // ══════════════════════════════════════════════════════════════════════

    private void BuildUI()
    {
        // Canvas on THIS GameObject
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 25;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        cg = gameObject.AddComponent<CanvasGroup>();

        // Dim overlay  (= the "panel" we toggle)
        panel = MakeGO("DimOverlay", transform);
        Img(panel, C_Dim);
        Fill(panel);

        // Card  820 × 540  centred
        const float CW = 820f, CH = 540f;
        const float HDR = 50f, FTR = 52f;
        const float LEFT_W = 215f;

        var card = MakeGO("Card", panel.transform);
        Img(card, C_CardBg);
        var cardRT = RT(card);
        cardRT.anchorMin = cardRT.anchorMax = cardRT.pivot = new Vector2(0.5f, 0.5f);
        cardRT.sizeDelta = new Vector2(CW, CH);
        cardRT.anchoredPosition = Vector2.zero;

        // ── Header ─────────────────────────────────────────────────────────
        var hdr = MakeGO("Header", card.transform);
        Img(hdr, C_HeaderBg);
        var hdrRT = RT(hdr);
        hdrRT.anchorMin = new Vector2(0f, 1f); hdrRT.anchorMax = new Vector2(1f, 1f);
        hdrRT.pivot = new Vector2(0.5f, 1f);
        hdrRT.sizeDelta = new Vector2(0f, HDR);
        hdrRT.anchoredPosition = Vector2.zero;

        var titleT = TMP(hdr, "Prepare for Fishing", 18f, C_TextWhite);
        Fill(titleT.gameObject); titleT.alignment = TextAlignmentOptions.Center; titleT.fontStyle = FontStyles.Bold;

        // ── Left column ────────────────────────────────────────────────────
        var left = MakeGO("LeftColumn", card.transform);
        Img(left, new Color(0.87f, 0.80f, 0.64f, 1f));
        var leftRT = RT(left);
        leftRT.anchorMin = new Vector2(0f, 0f); leftRT.anchorMax = new Vector2(0f, 1f);
        leftRT.pivot = new Vector2(0f, 0.5f);
        leftRT.offsetMin = new Vector2(0f, FTR);
        leftRT.offsetMax = new Vector2(LEFT_W, -HDR);

        var eqLbl = TMP(left, "EQUIPMENT", 9f, C_Accent);
        eqLbl.fontStyle = FontStyles.Bold; eqLbl.alignment = TextAlignmentOptions.Center;
        var eqRT = RT(eqLbl.gameObject);
        eqRT.anchorMin = new Vector2(0f,1f); eqRT.anchorMax = new Vector2(1f,1f);
        eqRT.pivot = new Vector2(0.5f,1f); eqRT.sizeDelta = new Vector2(0f,18f); eqRT.anchoredPosition = new Vector2(0f,-8f);

        rodSlot  = SlotButton(left.transform, "ROD",  -30f);
        hookSlot = SlotButton(left.transform, "HOOK", -128f);
        boatSlot = SlotButton(left.transform, "BOAT", -226f);
        zoneSlot = SlotButton(left.transform, "ZONE", -324f);
        rodSlot .Init(EquipSlotType.Rod,  this);
        hookSlot.Init(EquipSlotType.Hook, this);
        boatSlot.Init(EquipSlotType.Boat, this);
        zoneSlot.Init(EquipSlotType.Zone, this);

        // Thin separator
        var sep = MakeGO("Sep", card.transform);
        Img(sep, new Color(0.55f, 0.42f, 0.25f, 0.60f));
        var sepRT = RT(sep);
        sepRT.anchorMin = new Vector2(0f,0f); sepRT.anchorMax = new Vector2(0f,1f);
        sepRT.pivot = new Vector2(0f,0.5f);
        sepRT.offsetMin = new Vector2(LEFT_W, FTR);
        sepRT.offsetMax = new Vector2(LEFT_W + 1f, -HDR);

        // ── Right column ───────────────────────────────────────────────────
        var right = MakeGO("RightColumn", card.transform);
        var rightRT = RT(right);
        rightRT.anchorMin = new Vector2(0f,0f); rightRT.anchorMax = new Vector2(1f,1f);
        rightRT.offsetMin = new Vector2(LEFT_W + 1f, FTR);
        rightRT.offsetMax = new Vector2(-4f, -HDR);

        // List header
        const float LH_H = 28f;
        var listHdr = TMP(right, "Select Rod", 14f, C_Accent);
        listHdr.fontStyle = FontStyles.Bold; listHdr.alignment = TextAlignmentOptions.MidlineLeft;
        var lhRT = RT(listHdr.gameObject);
        lhRT.anchorMin = new Vector2(0f,1f); lhRT.anchorMax = new Vector2(1f,1f);
        lhRT.pivot = new Vector2(0.5f,1f); lhRT.sizeDelta = new Vector2(-8f, LH_H); lhRT.anchoredPosition = new Vector2(4f,-2f);
        listHeaderText = listHdr;

        // Scroll view
        var scrollGO = MakeGO("Scroll", right.transform);
        Img(scrollGO, new Color(0.82f, 0.74f, 0.58f, 1f));
        var scrollRT = RT(scrollGO);
        scrollRT.anchorMin = new Vector2(0f,0f); scrollRT.anchorMax = new Vector2(1f,1f);
        scrollRT.offsetMin = new Vector2(4f,4f); scrollRT.offsetMax = new Vector2(-4f, -(LH_H + 4f));

        var sr = scrollGO.AddComponent<ScrollRect>(); sr.horizontal = false;

        var vp = MakeGO("Viewport", scrollGO.transform);
        Fill(vp);
        // Mask needs an opaque Image for the stencil buffer — Color.clear causes it to clip everything.
        // showMaskGraphic = false hides the white rectangle visually while keeping the mask active.
        Img(vp, Color.white);
        var vpMask = vp.AddComponent<Mask>(); vpMask.showMaskGraphic = false;
        sr.viewport = RT(vp);

        var content = MakeGO("Content", vp.transform);
        var contentRT = RT(content);
        contentRT.anchorMin = new Vector2(0f,1f); contentRT.anchorMax = new Vector2(1f,1f);
        contentRT.pivot = new Vector2(0.5f,1f); contentRT.sizeDelta = Vector2.zero;
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 3f;
        vlg.childControlWidth  = true;
        vlg.childControlHeight = false; // rows set their own height explicitly
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(4,4,4,4);
        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = contentRT;
        itemListContainer = contentRT;

        // ── Footer ─────────────────────────────────────────────────────────
        var ftr = MakeGO("Footer", card.transform);
        Img(ftr, C_FooterBg);
        var ftrRT = RT(ftr);
        ftrRT.anchorMin = new Vector2(0f,0f); ftrRT.anchorMax = new Vector2(1f,0f);
        ftrRT.pivot = new Vector2(0.5f,0f); ftrRT.sizeDelta = new Vector2(0f, FTR);
        ftrRT.anchoredPosition = Vector2.zero;

        cancelBtn = FootBtn(ftr.transform, "Cancel",     C_CancelRed, new Vector2(-108f, 0f), new Vector2(130f,36f));
        goBtn     = FootBtn(ftr.transform, "Go Fishing!", C_GoGreen,   new Vector2(  72f, 0f), new Vector2(170f,36f));
        cancelBtn.onClick.AddListener(OnCancel);
        goBtn    .onClick.AddListener(OnGoFish);
    }

    // ── Slot button builder ────────────────────────────────────────────────

    private PreFishingEquipSlotUI SlotButton(Transform parent, string catLabel, float yOffset)
    {
        const float H = 90f;
        var go = MakeGO(catLabel + "Slot", parent);
        Img(go, C_SlotBg);
        var rt = RT(go);
        rt.anchorMin = new Vector2(0f,1f); rt.anchorMax = new Vector2(1f,1f);
        rt.pivot = new Vector2(0.5f,1f);
        rt.sizeDelta = new Vector2(-4f, H);
        rt.anchoredPosition = new Vector2(0f, yOffset);

        // Highlight border (full stretch, behind everything)
        var bdr = MakeGO("Border", go.transform);
        Img(bdr, C_SlotDim);
        Fill(bdr);

        // Category label (top-left)
        var cat = TMP(go, catLabel, 9f, C_Accent);
        cat.fontStyle = FontStyles.Bold; cat.alignment = TextAlignmentOptions.MidlineLeft;
        var catRT = RT(cat.gameObject);
        catRT.anchorMin = new Vector2(0f,1f); catRT.anchorMax = new Vector2(1f,1f);
        catRT.pivot = new Vector2(0f,1f); catRT.sizeDelta = new Vector2(-12f,18f); catRT.anchoredPosition = new Vector2(8f,-4f);

        // Item name
        var nm = TMP(go, $"— No {catLabel} —", 12f, C_TextWhite);
        nm.fontStyle = FontStyles.Bold; nm.alignment = TextAlignmentOptions.MidlineLeft;
        nm.overflowMode = TextOverflowModes.Ellipsis;
        var nmRT = RT(nm.gameObject);
        nmRT.anchorMin = new Vector2(0f,0.5f); nmRT.anchorMax = new Vector2(1f,1f);
        nmRT.offsetMin = new Vector2(8f, 0f); nmRT.offsetMax = new Vector2(-8f,-22f);

        // Stats
        var st = TMP(go, "", 10f, C_TextGrey);
        st.alignment = TextAlignmentOptions.MidlineLeft;
        var stRT = RT(st.gameObject);
        stRT.anchorMin = new Vector2(0f,0f); stRT.anchorMax = new Vector2(1f,0.5f);
        stRT.offsetMin = new Vector2(8f,4f); stRT.offsetMax = new Vector2(-8f,0f);

        // Button
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1.15f,1.15f,1.15f,1f);
        colors.pressedColor     = new Color(0.85f,0.85f,0.85f,1f);
        btn.colors = colors;

        var slot = go.AddComponent<PreFishingEquipSlotUI>();
        slot.nameText       = nm;
        slot.statsText      = st;
        slot.button         = btn;
        slot.highlightBorder = bdr.GetComponent<Image>();
        return slot;
    }

    // ── Footer button builder ──────────────────────────────────────────────

    private Button FootBtn(Transform parent, string label, Color bg, Vector2 pos, Vector2 size)
    {
        var go = MakeGO(label, parent);
        Img(go, bg);
        var rt = RT(go);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        var txt = TMP(go, label, 13f, C_TextWhite);
        txt.fontStyle = FontStyles.Bold; txt.alignment = TextAlignmentOptions.Center;
        Fill(txt.gameObject);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1.15f,1.15f,1.15f,1f);
        colors.pressedColor     = new Color(0.80f,0.80f,0.80f,1f);
        btn.colors = colors;
        return btn;
    }

    // ── Row builders (called at runtime) ──────────────────────────────────

    private void SpawnRow(InventoryItem item, bool selected)
    {
        var row = MakeGO("Row", itemListContainer);
        Img(row, selected ? C_RowSel : C_RowNorm);

        // Pin height explicitly so it's correct before the LayoutGroup ever runs.
        // anchorMin/Max both Y=1 → height is sizeDelta.y directly (no parent-stretch).
        var rowRT = RT(row);
        rowRT.anchorMin = new Vector2(0f, 1f);
        rowRT.anchorMax = new Vector2(1f, 1f);
        rowRT.pivot     = new Vector2(0.5f, 1f);
        rowRT.sizeDelta = new Vector2(0f, 58f);

        var le = row.AddComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = 58f;

        // Icon
        var ico = MakeGO("Icon", row.transform);
        var icoRT = RT(ico);
        icoRT.anchorMin = new Vector2(0f,0f); icoRT.anchorMax = new Vector2(0f,1f);
        icoRT.pivot = new Vector2(0f,0.5f); icoRT.sizeDelta = new Vector2(48f,-8f); icoRT.anchoredPosition = new Vector2(6f,0f);
        var icoImg = ico.AddComponent<Image>(); icoImg.preserveAspect = true;
        if (item.data.Sprite != null) icoImg.sprite = item.data.Sprite;
        else icoImg.color = Color.clear;

        // Name
        var nm = TMP(row, item.data.displayName, 13f, C_TextWhite);
        nm.fontStyle = FontStyles.Bold; nm.alignment = TextAlignmentOptions.MidlineLeft; nm.overflowMode = TextOverflowModes.Ellipsis;
        var nmRT = RT(nm.gameObject);
        nmRT.anchorMin = new Vector2(0f,0.5f); nmRT.anchorMax = new Vector2(1f,1f);
        nmRT.offsetMin = new Vector2(62f,0f); nmRT.offsetMax = new Vector2(-8f,-4f);

        // Stats
        var st = TMP(row, StatLine(item), 10f, C_TextGrey);
        st.alignment = TextAlignmentOptions.MidlineLeft;
        var stRT = RT(st.gameObject);
        stRT.anchorMin = new Vector2(0f,0f); stRT.anchorMax = new Vector2(1f,0.5f);
        stRT.offsetMin = new Vector2(62f,4f); stRT.offsetMax = new Vector2(-8f,0f);

        // Blue dot if selected
        if (selected)
        {
            var dot = MakeGO("Dot", row.transform);
            var dotRT = RT(dot);
            dotRT.anchorMin = dotRT.anchorMax = new Vector2(1f,1f);
            dotRT.pivot = new Vector2(1f,1f); dotRT.sizeDelta = new Vector2(12f,12f); dotRT.anchoredPosition = new Vector2(-4f,-4f);
            Img(dot, new Color(0.3f,0.7f,1f));
        }

        // Full-row invisible button
        var btnGO = MakeGO("Btn", row.transform);
        Fill(btnGO); Img(btnGO, Color.clear);
        var btn = btnGO.AddComponent<Button>();
        var cap = item;
        btn.onClick.AddListener(() => EquipItem(cap));
    }

    private void SpawnZoneRow(ZoneSO zone, int index, bool selected)
    {
        var row = MakeGO("ZoneRow", itemListContainer);
        Img(row, selected ? C_RowSel : C_RowNorm);

        var rowRT = RT(row);
        rowRT.anchorMin = new Vector2(0f, 1f);
        rowRT.anchorMax = new Vector2(1f, 1f);
        rowRT.pivot     = new Vector2(0.5f, 1f);
        rowRT.sizeDelta = new Vector2(0f, 58f);

        var le = row.AddComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = 58f;

        // Zone name
        var nm = TMP(row, zone.zoneName, 13f, C_TextWhite);
        nm.fontStyle = FontStyles.Bold; nm.alignment = TextAlignmentOptions.MidlineLeft; nm.overflowMode = TextOverflowModes.Ellipsis;
        var nmRT = RT(nm.gameObject);
        nmRT.anchorMin = new Vector2(0f, 0.5f); nmRT.anchorMax = new Vector2(1f, 1f);
        nmRT.offsetMin = new Vector2(12f, 0f);  nmRT.offsetMax = new Vector2(-8f, -4f);

        // Stats line
        string statsLine = $"×{zone.currencyMultiplier:0.0} Gold  ×{zone.expMultiplier:0.0} XP  " +
                           $"Common {zone.commonWeight:0}%  Rare {zone.rareWeight:0}%  Epic {zone.epicWeight:0}%";
        var st = TMP(row, statsLine, 10f, C_TextGrey);
        st.alignment = TextAlignmentOptions.MidlineLeft;
        var stRT = RT(st.gameObject);
        stRT.anchorMin = new Vector2(0f, 0f); stRT.anchorMax = new Vector2(1f, 0.5f);
        stRT.offsetMin = new Vector2(12f, 4f); stRT.offsetMax = new Vector2(-8f, 0f);

        // Selected dot
        if (selected)
        {
            var dot   = MakeGO("Dot", row.transform);
            var dotRT = RT(dot);
            dotRT.anchorMin = dotRT.anchorMax = new Vector2(1f, 1f);
            dotRT.pivot = new Vector2(1f, 1f); dotRT.sizeDelta = new Vector2(12f, 12f); dotRT.anchoredPosition = new Vector2(-4f, -4f);
            Img(dot, new Color(0.3f, 0.7f, 1f));
        }

        // Full-row invisible button
        var btnGO = MakeGO("Btn", row.transform);
        Fill(btnGO); Img(btnGO, Color.clear);
        var btn = btnGO.AddComponent<Button>();
        int cap = index;
        btn.onClick.AddListener(() => SelectZone(cap));
    }

    private void SpawnEmptyRow(string msg)
    {
        var row = MakeGO("RowEmpty", itemListContainer);
        Img(row, new Color(1f,1f,1f,0.03f));
        var rowRT = RT(row);
        rowRT.anchorMin = new Vector2(0f, 1f);
        rowRT.anchorMax = new Vector2(1f, 1f);
        rowRT.pivot     = new Vector2(0.5f, 1f);
        rowRT.sizeDelta = new Vector2(0f, 50f);
        var le = row.AddComponent<LayoutElement>(); le.minHeight = le.preferredHeight = 50f;
        var txt = TMP(row, msg, 12f, C_TextGrey);
        txt.fontStyle = FontStyles.Italic; txt.alignment = TextAlignmentOptions.Center;
        Fill(txt.gameObject);
    }

    private static string StatLine(InventoryItem item)
    {
        if (item.data is RodItemData  rod)  return $"Line {rod.lineLength}m   Reel ×{rod.reelSpeed:0.0}   ATK ×{rod.attackMult:0.0}";
        if (item.data is HookItemData hook) return $"Chance {hook.hookChance:0}%   Rare +{hook.rareBoost:0.0}";
        if (item.data is BoatItemData boat) return $"Speed {boat.speed:0.0}   Cap {boat.capacity}   HP +{boat.hp}";
        return "";
    }

    // ══════════════════════════════════════════════════════════════════════
    // TINY UI HELPERS
    // ══════════════════════════════════════════════════════════════════════

    private static GameObject MakeGO(string n, Transform p)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        return go;
    }

    private static RectTransform RT(GameObject go)
        => go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();

    private static Image Img(GameObject go, Color c)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = c; return img;
    }

    private static void Fill(GameObject go)
    {
        var r = RT(go);
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    // ── TMP font lookup (multiple fallbacks so text is never invisible) ──────

    private TMP_FontAsset GetTMPFont()
    {
        // Inspector-assigned font takes priority
        if (menuFont != null) return menuFont;

        // 1. TMP Settings asset (works if TMP Essential Resources were imported)
        var f = TMP_Settings.defaultFontAsset;
        if (f != null) { Debug.Log("[PreFishing] TMP font: TMP_Settings"); return f; }

        // 2. Standard LiberationSans that ships with TMP Essential Resources
        f = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (f != null) { Debug.Log("[PreFishing] TMP font: LiberationSans SDF"); return f; }

        // 3. Any TMP font already loaded in memory
        var all = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        if (all != null && all.Length > 0) { Debug.Log($"[PreFishing] TMP font: {all[0].name}"); return all[0]; }

        Debug.LogError("[PreFishing] Could not find ANY TMP font asset — text will be invisible. Import TMP Essential Resources (Window > TextMeshPro > Import TMP Essential Resources).");
        return null;
    }

    private TMP_Text TMP(GameObject parent, string text, float size, Color col)
    {
        var go  = new GameObject("T", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var t   = go.AddComponent<TextMeshProUGUI>();
        var fnt = GetTMPFont();
        if (fnt != null) t.font = fnt;
        t.text  = text; t.fontSize = size; t.color = col;
        t.overflowMode = TextOverflowModes.Overflow;
        return t;
    }
}
