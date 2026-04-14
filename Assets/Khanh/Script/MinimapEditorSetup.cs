using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility: Fishing Game ▸ Setup UI
///   ① Create Minimap              — run in the FISHING scene
///   ② Create Quest Tracker HUD   — run in any scene (persists across loads)
///   ③ Create Item Context Menu   — run in the TOP DOWN scene
///   ④ Create Fish Shop UI        — run in the TOP DOWN scene
/// </summary>
public static class MinimapEditorSetup
{
    private const string PREFAB_FOLDER = "Assets/Khanh/Script/Minimap/Prefabs";

    // ─────────────────────────────────────────────────────────────────────────
    // ① MINIMAP
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("Fishing Game/Setup UI/① Create Minimap (Fishing Scene)")]
    public static void CreateMinimap()
    {
        EnsureFolder(PREFAB_FOLDER);

        GameObject blipPrefab = CreateAndSaveBlipPrefab();

        // Canvas
        var canvasGO = new GameObject("MinimapCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Minimap");

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight   = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel (bottom-right, 180×180)
        var panelGO = CreateUIObject("MinimapPanel", canvasGO.transform);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(1f, 0f);
        panelRT.anchorMax        = new Vector2(1f, 0f);
        panelRT.pivot            = new Vector2(1f, 0f);
        panelRT.sizeDelta        = new Vector2(180f, 180f);
        panelRT.anchoredPosition = new Vector2(-15f, 15f);

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color  = new Color(0.05f, 0.08f, 0.12f, 0.88f);
        panelImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        panelImg.type   = Image.Type.Sliced;

        panelGO.AddComponent<Mask>().showMaskGraphic = true;

        // Blip container
        var blipContainerGO = CreateUIObject("BlipContainer", panelGO.transform);
        var bcRT = blipContainerGO.GetComponent<RectTransform>();
        bcRT.anchorMin = Vector2.zero; bcRT.anchorMax = Vector2.one;
        bcRT.offsetMin = bcRT.offsetMax = Vector2.zero;
        bcRT.pivot     = new Vector2(0.5f, 0.5f);

        // "MAP" label
        var labelGO  = CreateUIObject("MapLabel", panelGO.transform);
        var labelRT  = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin        = new Vector2(0f, 1f);
        labelRT.anchorMax        = new Vector2(1f, 1f);
        labelRT.pivot            = new Vector2(0.5f, 1f);
        labelRT.sizeDelta        = new Vector2(0f, 18f);
        labelRT.anchoredPosition = new Vector2(0f, -2f);
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = "MAP";
        labelTMP.fontSize  = 10;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.color     = new Color(0.75f, 0.75f, 0.75f, 0.9f);

        // Wire MinimapController
        var controller = panelGO.AddComponent<MinimapController>();
        var so = new SerializedObject(controller);
        so.FindProperty("minimapRect").objectReferenceValue   = panelRT;
        so.FindProperty("blipContainer").objectReferenceValue = bcRT;
        so.FindProperty("blipPrefab").objectReferenceValue    = blipPrefab;
        so.ApplyModifiedProperties();

        EnsureEventSystem();
        Selection.activeGameObject = canvasGO;

        Debug.Log("[MinimapSetup] ✓ Minimap created! Open MinimapController and assign 'Boat Transform', 'World Center', and 'World Size'.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ② QUEST TRACKER HUD
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("Fishing Game/Setup UI/② Create Quest Tracker HUD (Persistent)")]
    public static void CreateQuestHUD()
    {
        EnsureFolder(PREFAB_FOLDER);

        GameObject rowPrefabGO = CreateAndSaveRowPrefab();

        // Canvas — NO GraphicRaycaster (display only)
        var canvasGO = new GameObject("QuestHUDCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Quest HUD");

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        // Quest Panel (top-left)
        var panelGO = CreateUIObject("QuestPanel", canvasGO.transform);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0f, 1f);
        panelRT.anchorMax        = new Vector2(0f, 1f);
        panelRT.pivot            = new Vector2(0f, 1f);
        panelRT.sizeDelta        = new Vector2(260f, 0f);
        panelRT.anchoredPosition = new Vector2(15f, -15f);

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color  = new Color(0.04f, 0.04f, 0.1f, 0.82f);
        panelImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        panelImg.type   = Image.Type.Sliced;

        var panelVLG = panelGO.AddComponent<VerticalLayoutGroup>();
        panelVLG.padding              = new RectOffset(8, 8, 6, 8);
        panelVLG.spacing              = 3f;
        panelVLG.childControlWidth    = true;
        panelVLG.childControlHeight   = true;
        panelVLG.childForceExpandWidth  = true;
        panelVLG.childForceExpandHeight = false;

        var panelCSF = panelGO.AddComponent<ContentSizeFitter>();
        panelCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Header
        var headerGO  = CreateUIObject("HeaderText", panelGO.transform);
        var headerTMP = headerGO.AddComponent<TextMeshProUGUI>();
        headerTMP.text      = "QUESTS";
        headerTMP.fontSize  = 13;
        headerTMP.fontStyle = FontStyles.Bold;
        headerTMP.color     = new Color(1f, 0.85f, 0.3f);
        headerTMP.alignment = TextAlignmentOptions.Left;
        var headerLE = headerGO.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 20f;

        // Divider
        var divGO  = CreateUIObject("Divider", panelGO.transform);
        var divImg = divGO.AddComponent<Image>();
        divImg.color = new Color(1f, 0.85f, 0.3f, 0.35f);
        var divLE = divGO.AddComponent<LayoutElement>();
        divLE.preferredHeight = 1f;

        // Row container
        var rowContainerGO = CreateUIObject("RowContainer", panelGO.transform);
        var rowContainerRT = rowContainerGO.GetComponent<RectTransform>();
        var rcVLG = rowContainerGO.AddComponent<VerticalLayoutGroup>();
        rcVLG.spacing             = 3f;
        rcVLG.childControlWidth   = true;
        rcVLG.childControlHeight  = true;
        rcVLG.childForceExpandWidth  = true;
        rcVLG.childForceExpandHeight = false;
        var rcCSF = rowContainerGO.AddComponent<ContentSizeFitter>();
        rcCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Empty label
        var emptyGO  = CreateUIObject("EmptyLabel", panelGO.transform);
        var emptyTMP = emptyGO.AddComponent<TextMeshProUGUI>();
        emptyTMP.text      = "No active quests.";
        emptyTMP.fontSize  = 10;
        emptyTMP.color     = new Color(0.55f, 0.55f, 0.55f);
        emptyTMP.fontStyle = FontStyles.Italic;
        emptyTMP.alignment = TextAlignmentOptions.Left;
        var emptyLE = emptyGO.AddComponent<LayoutElement>();
        emptyLE.preferredHeight = 16f;

        // Wire QuestTrackerHUD on CANVAS ROOT (never on the panel itself)
        var hud   = canvasGO.AddComponent<QuestTrackerHUD>();
        var hudSO = new SerializedObject(hud);
        hudSO.FindProperty("questPanel").objectReferenceValue     = panelGO;
        hudSO.FindProperty("rowContainer").objectReferenceValue   = rowContainerRT;
        hudSO.FindProperty("questRowPrefab").objectReferenceValue = rowPrefabGO.GetComponent<QuestTrackerRow>();
        hudSO.FindProperty("emptyLabel").objectReferenceValue     = emptyGO;
        hudSO.FindProperty("persistAcrossScenes").boolValue       = true;
        hudSO.ApplyModifiedProperties();

        EnsureEventSystem();
        Selection.activeGameObject = canvasGO;

        Debug.Log("[QuestHUD] ✓ Quest Tracker HUD created (persistAcrossScenes = true).");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ③ ITEM CONTEXT MENU
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("Fishing Game/Setup UI/③ Create Item Context Menu (Top Down Scene)")]
    public static void CreateItemContextMenu()
    {
        EnsureFolder(PREFAB_FOLDER);

        string btnPath = $"{PREFAB_FOLDER}/ContextMenuButton.prefab";
        GameObject buttonPrefab;
        if (File.Exists(btnPath))
        {
            buttonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(btnPath);
        }
        else
        {
            var btnGO  = new GameObject("ContextMenuButton");
            var btnRT  = btnGO.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(120f, 30f);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color  = new Color(0.15f, 0.15f, 0.2f);
            btnImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            btnImg.type   = Image.Type.Sliced;
            btnGO.AddComponent<Button>();

            var labelGO  = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var labelRT  = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(8f, 0f); labelRT.offsetMax = new Vector2(-8f, 0f);
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text      = "Action";
            labelTMP.fontSize  = 12;
            labelTMP.alignment = TextAlignmentOptions.Center;
            labelTMP.color     = Color.white;

            buttonPrefab = PrefabUtility.SaveAsPrefabAsset(btnGO, btnPath);
            Object.DestroyImmediate(btnGO);
        }

        // Always use a dedicated ContextMenuCanvas so it never lands in a wrong canvas
        GameObject canvasGO = GameObject.Find("ContextMenuCanvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("ContextMenuCanvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Context Menu Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        var rootGO = CreateUIObject("ItemContextMenuUI", canvasGO.transform);
        var rootRT = rootGO.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero; rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = rootRT.offsetMax = Vector2.zero;

        var panelGO = CreateUIObject("MenuPanel", rootGO.transform);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(130f, 0f);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color  = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        panelImg.type   = Image.Type.Sliced;
        var outline = panelGO.AddComponent<Outline>();
        outline.effectColor    = new Color(0f, 0f, 0f, 0.6f);
        outline.effectDistance = new Vector2(2f, -2f);

        var containerGO = CreateUIObject("ButtonContainer", panelGO.transform);
        var containerRT = containerGO.GetComponent<RectTransform>();
        containerRT.anchorMin = Vector2.zero; containerRT.anchorMax = Vector2.one;
        containerRT.offsetMin = new Vector2(4f, 4f); containerRT.offsetMax = new Vector2(-4f, -4f);
        var vlg = containerGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2f; vlg.childControlWidth = true; vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        var csf = panelGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        panelGO.SetActive(false);

        var ctxMenu = rootGO.AddComponent<ItemContextMenuUI>();
        ctxMenu.menuPanel       = panelGO;
        ctxMenu.buttonContainer = containerGO.transform;
        ctxMenu.buttonPrefab    = buttonPrefab;

        Undo.RegisterCreatedObjectUndo(rootGO, "Create Item Context Menu");
        Selection.activeGameObject = rootGO;
        Debug.Log("[ContextMenu] ✓ ItemContextMenuUI created.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ④ FISH SHOP UI
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("Fishing Game/Setup UI/④ Create Fish Shop UI (Top Down Scene)")]
    public static void CreateFishShopUI()
    {
        EnsureFolder(PREFAB_FOLDER);

        string rowPath = $"{PREFAB_FOLDER}/FishShopRow.prefab";
        GameObject rowPrefab = File.Exists(rowPath)
            ? AssetDatabase.LoadAssetAtPath<GameObject>(rowPath)
            : BuildFishShopRowPrefab(rowPath);

        GameObject shopCanvasGO = GameObject.Find("FishShopCanvas") ?? new GameObject("FishShopCanvas");
        Undo.RegisterCreatedObjectUndo(shopCanvasGO, "Create Fish Shop Canvas");

        var shopCanvas = shopCanvasGO.GetComponent<Canvas>() ?? shopCanvasGO.AddComponent<Canvas>();
        shopCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        shopCanvas.sortingOrder = 15;

        if (shopCanvasGO.GetComponent<CanvasScaler>() == null)
        {
            var scaler = shopCanvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
        }
        if (shopCanvasGO.GetComponent<GraphicRaycaster>() == null)
            shopCanvasGO.AddComponent<GraphicRaycaster>();

        // Shop panel (centered)
        var shopPanelGO = CreateUIObject("FishShopPanel", shopCanvasGO.transform);
        var shopRT      = shopPanelGO.GetComponent<RectTransform>();
        shopRT.anchorMin = shopRT.anchorMax = new Vector2(0.5f, 0.5f);
        shopRT.pivot     = new Vector2(0.5f, 0.5f);
        shopRT.sizeDelta = new Vector2(420f, 480f);
        shopRT.anchoredPosition = Vector2.zero;
        var shopBg = shopPanelGO.AddComponent<Image>();
        shopBg.color  = new Color(0.08f, 0.06f, 0.04f, 0.97f);
        shopBg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        shopBg.type   = Image.Type.Sliced;
        var panelVLG = shopPanelGO.AddComponent<VerticalLayoutGroup>();
        panelVLG.padding = new RectOffset(12, 12, 10, 12);
        panelVLG.spacing = 6f;
        panelVLG.childControlWidth  = true; panelVLG.childControlHeight  = false;
        panelVLG.childForceExpandWidth = true; panelVLG.childForceExpandHeight = false;

        // Header bar
        var headerBarGO  = CreateUIObject("HeaderBar", shopPanelGO.transform);
        var headerBarHLG = headerBarGO.AddComponent<HorizontalLayoutGroup>();
        headerBarHLG.childControlWidth = true; headerBarHLG.childControlHeight = true;
        headerBarHLG.childForceExpandWidth = false; headerBarHLG.childForceExpandHeight = true;
        headerBarHLG.spacing = 4f;
        var headerLE = headerBarGO.AddComponent<LayoutElement>();
        headerLE.preferredHeight = 36f;

        var titleGO  = CreateUIObject("Title", headerBarGO.transform);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "Fish Shop"; titleTMP.fontSize = 20; titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = new Color(1f, 0.85f, 0.3f); titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
        titleGO.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var goldGO  = CreateUIObject("GoldLabel", headerBarGO.transform);
        var goldTMP = goldGO.AddComponent<TextMeshProUGUI>();
        goldTMP.text = "Gold: 0"; goldTMP.fontSize = 13;
        goldTMP.color = new Color(1f, 0.9f, 0.4f); goldTMP.alignment = TextAlignmentOptions.MidlineRight;
        goldGO.AddComponent<LayoutElement>().preferredWidth = 100f;

        var closeBtnGO  = CreateUIObject("CloseButton", headerBarGO.transform);
        var closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.7f, 0.15f, 0.1f);
        closeBtnImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        closeBtnImg.type = Image.Type.Sliced;
        closeBtnGO.AddComponent<Button>();
        var closeLE = closeBtnGO.AddComponent<LayoutElement>();
        closeLE.preferredWidth = closeLE.preferredHeight = 30f;
        var closeLabelGO  = CreateUIObject("X", closeBtnGO.transform);
        var closeLabelRT  = closeLabelGO.GetComponent<RectTransform>();
        closeLabelRT.anchorMin = Vector2.zero; closeLabelRT.anchorMax = Vector2.one;
        closeLabelRT.offsetMin = closeLabelRT.offsetMax = Vector2.zero;
        var closeLabelTMP = closeLabelGO.AddComponent<TextMeshProUGUI>();
        closeLabelTMP.text = "X"; closeLabelTMP.fontSize = 16; closeLabelTMP.fontStyle = FontStyles.Bold;
        closeLabelTMP.color = Color.white; closeLabelTMP.alignment = TextAlignmentOptions.Center;

        // Divider
        var divGO = CreateUIObject("Divider", shopPanelGO.transform);
        divGO.AddComponent<Image>().color = new Color(1f, 0.85f, 0.3f, 0.3f);
        divGO.AddComponent<LayoutElement>().preferredHeight = 1f;

        // Scroll view
        var scrollGO = CreateUIObject("ScrollView", shopPanelGO.transform);
        scrollGO.AddComponent<LayoutElement>().flexibleHeight = 1f;

        var viewportGO  = CreateUIObject("Viewport", scrollGO.transform);
        var viewportRT  = viewportGO.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero; viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = viewportRT.offsetMax = Vector2.zero;
        viewportGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;

        var contentGO = CreateUIObject("Content", viewportGO.transform);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f); contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f); contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;
        var contentVLG = contentGO.AddComponent<VerticalLayoutGroup>();
        contentVLG.spacing = 4f; contentVLG.padding = new RectOffset(4, 4, 4, 4);
        contentVLG.childControlWidth = true; contentVLG.childControlHeight = true;
        contentVLG.childForceExpandWidth = true; contentVLG.childForceExpandHeight = false;
        contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scrollRectRT = scrollGO.GetComponent<RectTransform>();
        scrollRectRT.anchorMin = Vector2.zero; scrollRectRT.anchorMax = Vector2.one;
        scrollRectRT.offsetMin = scrollRectRT.offsetMax = Vector2.zero;
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.content = contentRT; scrollRect.viewport = viewportRT;
        scrollRect.horizontal = false; scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 20f; scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Footer
        var footerGO  = CreateUIObject("Footer", shopPanelGO.transform);
        var footerHLG = footerGO.AddComponent<HorizontalLayoutGroup>();
        footerHLG.childControlWidth = true; footerHLG.childControlHeight = true;
        footerHLG.childForceExpandWidth = false; footerHLG.childForceExpandHeight = true;
        footerHLG.spacing = 8f;
        footerGO.AddComponent<LayoutElement>().preferredHeight = 38f;

        var totalGO  = CreateUIObject("TotalValueText", footerGO.transform);
        var totalTMP = totalGO.AddComponent<TextMeshProUGUI>();
        totalTMP.text = "Total: 0g"; totalTMP.fontSize = 13; totalTMP.color = Color.white;
        totalTMP.alignment = TextAlignmentOptions.MidlineLeft;
        totalGO.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var feedbackGO  = CreateUIObject("FeedbackText", footerGO.transform);
        var feedbackTMP = feedbackGO.AddComponent<TextMeshProUGUI>();
        feedbackTMP.text = "+12g"; feedbackTMP.fontSize = 12; feedbackTMP.fontStyle = FontStyles.Italic;
        feedbackTMP.color = new Color(0.4f, 1f, 0.4f); feedbackTMP.alignment = TextAlignmentOptions.Midline;
        feedbackGO.AddComponent<LayoutElement>().preferredWidth = 80f;
        feedbackGO.SetActive(false);

        var sellAllGO  = CreateUIObject("SellAllButton", footerGO.transform);
        var sellAllImg = sellAllGO.AddComponent<Image>();
        sellAllImg.color = new Color(0.15f, 0.5f, 0.15f);
        sellAllImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sellAllImg.type = Image.Type.Sliced;
        sellAllGO.AddComponent<Button>();
        var sellAllLE = sellAllGO.AddComponent<LayoutElement>();
        sellAllLE.preferredWidth = 120f; sellAllLE.preferredHeight = 34f;
        var sellAllLabelGO  = CreateUIObject("Label", sellAllGO.transform);
        var sellAllLabelRT  = sellAllLabelGO.GetComponent<RectTransform>();
        sellAllLabelRT.anchorMin = Vector2.zero; sellAllLabelRT.anchorMax = Vector2.one;
        sellAllLabelRT.offsetMin = sellAllLabelRT.offsetMax = Vector2.zero;
        var sellAllLabelTMP = sellAllLabelGO.AddComponent<TextMeshProUGUI>();
        sellAllLabelTMP.text = "Sell All Fish"; sellAllLabelTMP.fontSize = 13;
        sellAllLabelTMP.fontStyle = FontStyles.Bold; sellAllLabelTMP.color = Color.white;
        sellAllLabelTMP.alignment = TextAlignmentOptions.Center;

        // Wire FishShopUI
        var shopUI  = shopPanelGO.AddComponent<FishShopUI>();
        var shopSO  = new SerializedObject(shopUI);
        shopSO.FindProperty("rowContainer").objectReferenceValue     = contentGO.transform;
        shopSO.FindProperty("rowPrefab").objectReferenceValue        = rowPrefab;
        shopSO.FindProperty("closeButton").objectReferenceValue      = closeBtnGO.GetComponent<Button>();
        shopSO.FindProperty("currentMoneyText").objectReferenceValue = goldTMP;
        shopSO.FindProperty("totalValueText").objectReferenceValue   = totalTMP;
        shopSO.FindProperty("sellAllButton").objectReferenceValue    = sellAllGO.GetComponent<Button>();
        shopSO.FindProperty("feedbackText").objectReferenceValue     = feedbackTMP;
        shopSO.ApplyModifiedProperties();

        shopPanelGO.SetActive(false);
        EnsureEventSystem();
        Selection.activeGameObject = shopPanelGO;
        Debug.Log("[FishShop] ✓ Fish Shop UI created (starts hidden).");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PREFAB BUILDERS
    // ─────────────────────────────────────────────────────────────────────────

    private static GameObject CreateAndSaveBlipPrefab()
    {
        string path = $"{PREFAB_FOLDER}/MinimapBlip.prefab";
        if (File.Exists(path)) return AssetDatabase.LoadAssetAtPath<GameObject>(path);

        var go  = new GameObject("MinimapBlip");
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(8f, 8f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        var img    = go.AddComponent<Image>();
        img.color  = Color.white;
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateAndSaveRowPrefab()
    {
        string path = $"{PREFAB_FOLDER}/QuestTrackerRow.prefab";
        if (File.Exists(path)) return AssetDatabase.LoadAssetAtPath<GameObject>(path);

        var rowGO  = new GameObject("QuestTrackerRow");
        rowGO.AddComponent<RectTransform>();
        var rowVLG = rowGO.AddComponent<VerticalLayoutGroup>();
        rowVLG.spacing = 2f; rowVLG.childControlWidth = true; rowVLG.childControlHeight = true;
        rowVLG.childForceExpandWidth = true; rowVLG.childForceExpandHeight = false;
        rowVLG.padding = new RectOffset(0, 0, 2, 2);
        rowGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Top row: objective + count
        var topRowGO  = new GameObject("TopRow");
        topRowGO.transform.SetParent(rowGO.transform, false);
        topRowGO.AddComponent<RectTransform>();
        var topHLG = topRowGO.AddComponent<HorizontalLayoutGroup>();
        topHLG.childControlWidth = true; topHLG.childControlHeight = true;
        topHLG.childForceExpandHeight = true; topHLG.childForceExpandWidth = false;
        topHLG.spacing = 4f;
        topRowGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var objGO  = new GameObject("ObjectiveText");
        objGO.transform.SetParent(topRowGO.transform, false);
        objGO.AddComponent<RectTransform>();
        var objTMP = objGO.AddComponent<TextMeshProUGUI>();
        objTMP.text = "Catch: Carp"; objTMP.fontSize = 11; objTMP.color = Color.white;
        objTMP.alignment = TextAlignmentOptions.Left;
        var objLE = objGO.AddComponent<LayoutElement>();
        objLE.flexibleWidth = 1f; objLE.preferredHeight = 16f;

        var progGO  = new GameObject("ProgressText");
        progGO.transform.SetParent(topRowGO.transform, false);
        progGO.AddComponent<RectTransform>();
        var progTMP = progGO.AddComponent<TextMeshProUGUI>();
        progTMP.text = "0 / 3"; progTMP.fontSize = 11; progTMP.color = Color.white;
        progTMP.alignment = TextAlignmentOptions.Right;
        var progLE = progGO.AddComponent<LayoutElement>();
        progLE.preferredWidth = 50f; progLE.preferredHeight = 16f;

        // Progress bar
        var barBgGO  = new GameObject("ProgressBarBg");
        barBgGO.transform.SetParent(rowGO.transform, false);
        barBgGO.AddComponent<RectTransform>();
        barBgGO.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        barBgGO.AddComponent<LayoutElement>().preferredHeight = 4f;

        var barFillGO  = new GameObject("ProgressBarFill");
        barFillGO.transform.SetParent(barBgGO.transform, false);
        var barFillRT  = barFillGO.AddComponent<RectTransform>();
        barFillRT.anchorMin = Vector2.zero; barFillRT.anchorMax = Vector2.one;
        barFillRT.offsetMin = barFillRT.offsetMax = Vector2.zero;
        var barFillImg = barFillGO.AddComponent<Image>();
        barFillImg.color      = new Color(1f, 0.85f, 0.3f);
        barFillImg.type       = Image.Type.Filled;
        barFillImg.fillMethod = Image.FillMethod.Horizontal;
        barFillImg.fillAmount = 0.4f;

        var row   = rowGO.AddComponent<QuestTrackerRow>();
        var rowSO = new SerializedObject(row);
        rowSO.FindProperty("objectiveText").objectReferenceValue = objTMP;
        rowSO.FindProperty("progressText").objectReferenceValue  = progTMP;
        rowSO.FindProperty("progressBar").objectReferenceValue   = barFillImg;
        rowSO.ApplyModifiedProperties();

        var prefab = PrefabUtility.SaveAsPrefabAsset(rowGO, path);
        Object.DestroyImmediate(rowGO);
        return prefab;
    }

    private static GameObject BuildFishShopRowPrefab(string path)
    {
        var rowGO = new GameObject("FishShopRow");
        rowGO.AddComponent<RectTransform>();
        var hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true; hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;
        hlg.spacing = 8f; hlg.padding = new RectOffset(6, 6, 4, 4);
        rowGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var rowBg = rowGO.AddComponent<Image>();
        rowBg.color = new Color(1f, 1f, 1f, 0.04f);
        rowBg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        rowBg.type = Image.Type.Sliced;
        rowGO.AddComponent<LayoutElement>().preferredHeight = 52f;

        var iconGO  = new GameObject("FishIcon"); iconGO.transform.SetParent(rowGO.transform, false);
        iconGO.AddComponent<RectTransform>();
        var iconImg = iconGO.AddComponent<Image>(); iconImg.preserveAspect = true;
        var iconLE  = iconGO.AddComponent<LayoutElement>();
        iconLE.preferredWidth = iconLE.preferredHeight = 44f;

        var infoGO  = new GameObject("InfoColumn"); infoGO.transform.SetParent(rowGO.transform, false);
        infoGO.AddComponent<RectTransform>();
        var infoVLG = infoGO.AddComponent<VerticalLayoutGroup>();
        infoVLG.childControlWidth = true; infoVLG.childControlHeight = true;
        infoVLG.childForceExpandWidth = true; infoVLG.childForceExpandHeight = false;
        infoVLG.spacing = 2f;
        infoGO.AddComponent<LayoutElement>().flexibleWidth = 1f;

        var nameGO  = new GameObject("FishName"); nameGO.transform.SetParent(infoGO.transform, false);
        nameGO.AddComponent<RectTransform>();
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Salmon"; nameTMP.fontSize = 13; nameTMP.fontStyle = FontStyles.Bold;
        nameTMP.color = Color.white; nameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        nameGO.AddComponent<LayoutElement>().preferredHeight = 20f;

        var priceGO  = new GameObject("PriceLabel"); priceGO.transform.SetParent(infoGO.transform, false);
        priceGO.AddComponent<RectTransform>();
        var priceTMP = priceGO.AddComponent<TextMeshProUGUI>();
        priceTMP.text = "10g"; priceTMP.fontSize = 11;
        priceTMP.color = new Color(1f, 0.85f, 0.3f); priceTMP.alignment = TextAlignmentOptions.MidlineLeft;
        priceGO.AddComponent<LayoutElement>().preferredHeight = 16f;

        var sellBtnGO  = new GameObject("SellButton"); sellBtnGO.transform.SetParent(rowGO.transform, false);
        sellBtnGO.AddComponent<RectTransform>();
        var sellBtnImg = sellBtnGO.AddComponent<Image>();
        sellBtnImg.color = new Color(0.15f, 0.45f, 0.15f);
        sellBtnImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sellBtnImg.type = Image.Type.Sliced;
        sellBtnGO.AddComponent<Button>();
        var sellBtnLE = sellBtnGO.AddComponent<LayoutElement>();
        sellBtnLE.preferredWidth = 60f; sellBtnLE.preferredHeight = 32f;
        var sellLabelGO  = new GameObject("Label"); sellLabelGO.transform.SetParent(sellBtnGO.transform, false);
        var sellLabelRT  = sellLabelGO.AddComponent<RectTransform>();
        sellLabelRT.anchorMin = Vector2.zero; sellLabelRT.anchorMax = Vector2.one;
        sellLabelRT.offsetMin = sellLabelRT.offsetMax = Vector2.zero;
        var sellLabelTMP = sellLabelGO.AddComponent<TextMeshProUGUI>();
        sellLabelTMP.text = "Sell"; sellLabelTMP.fontSize = 12; sellLabelTMP.fontStyle = FontStyles.Bold;
        sellLabelTMP.color = Color.white; sellLabelTMP.alignment = TextAlignmentOptions.Center;

        var rowComp = rowGO.AddComponent<FishShopRowUI>();
        var rowSO   = new SerializedObject(rowComp);
        rowSO.FindProperty("fishIcon").objectReferenceValue     = iconImg;
        rowSO.FindProperty("fishNameText").objectReferenceValue = nameTMP;
        rowSO.FindProperty("priceText").objectReferenceValue    = priceTMP;
        rowSO.FindProperty("sellButton").objectReferenceValue   = sellBtnGO.GetComponent<Button>();
        rowSO.ApplyModifiedProperties();

        var prefab = PrefabUtility.SaveAsPrefabAsset(rowGO, path);
        Object.DestroyImmediate(rowGO);
        return prefab;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UTILITIES
    // ─────────────────────────────────────────────────────────────────────────

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.localScale    = Vector3.one;
        rt.localPosition = Vector3.zero;
        return go;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        Undo.RegisterCreatedObjectUndo(esGO, "Create EventSystem");
    }
}
