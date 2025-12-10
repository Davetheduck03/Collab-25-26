using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingMenu : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider sfxSlider;

    [Header("Key Instructions")]
    public Transform keyInstructionContainer;
    public GameObject keyInstructionPrefab;

    [Header("Control Icons")]
    public Sprite iconLand;
    public Sprite iconW;
    public Sprite iconA;
    public Sprite iconS;
    public Sprite iconD;
    public Sprite iconF;
    public Sprite iconTab;
    public Sprite iconShift;
    public Sprite iconSpace;
    public Sprite iconR;
    public Sprite iconQ;
    public Sprite iconE;
    public Sprite iconNumbers;
    public Sprite iconBoat;

    private void Start()
    {
        LoadVolumes();
        GenerateKeyInstructions();
    }

    // ==================== VOLUME ====================
    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void LoadVolumes()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (masterSlider != null) masterSlider.value = master;
        if (sfxSlider != null) sfxSlider.value = sfx;

        AudioListener.volume = master;
    }

    // ==================== GENERATE CONTROLS ====================
    private void GenerateKeyInstructions()
    {
        if (keyInstructionContainer == null || keyInstructionPrefab == null)
        {
            Debug.LogError("[SettingMenu] keyInstructionContainer or keyInstructionPrefab is not assigned!");
            return;
        }

        ClearContainer();

        AddCategoryLabel("Land Mode (Top-Down Exploration)", iconLand);
        AddInstruction("W A S D", "Move Character", iconW, iconA, iconS, iconD);
        AddInstruction("F", "Interact / Collect / Trade", iconF);
        AddInstruction("Tab", "Open Inventory & Menu", iconTab);
        AddInstruction("Shift", "Sprint (Hold)", iconShift);
        AddInstruction("1 2 3", "Use Quick Slot Items", iconNumbers);

        AddCategoryLabel("Boat Mode (Side-Scroller Fishing)", iconBoat);
        AddInstruction("A D", "Move Boat Left / Right", iconA, iconD);
        AddInstruction("W S", "Ascend / Descend", iconW, iconS);
        AddInstruction("R", "Cast Fishing Line", iconR);
        AddInstruction("Q E", "Reel Left / Right", iconQ, iconE);
        AddInstruction("Space", "Parry Fish", iconSpace);
        AddInstruction("F", "Dock / Interact", iconF);
    }

    private void ClearContainer()
    {
        if (keyInstructionContainer == null) return;

        foreach (Transform child in keyInstructionContainer)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private void AddInstruction(string keyText, string description, params Sprite[] icons)
    {
        if (keyInstructionPrefab == null || keyInstructionContainer == null) return;

        GameObject obj = Instantiate(keyInstructionPrefab, keyInstructionContainer);
        Transform iconsContainer = obj.transform.Find("IconsContainer");
        if (iconsContainer == null) return;

        foreach (Transform child in iconsContainer)
            Destroy(child.gameObject);

        bool hasAnyIcon = false;

        if (icons != null && icons.Length > 0)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                Sprite sp = icons[i];
                if (sp == null) continue;

                hasAnyIcon = true;

                GameObject iconGO = new GameObject("Icon", typeof(Image));
                iconGO.transform.SetParent(iconsContainer, false);

                Image img = iconGO.GetComponent<Image>();
                img.sprite = sp;
                img.preserveAspect = true;

                RectTransform rt = iconGO.GetComponent<RectTransform>();

                // SPECIAL BIG SIZE FOR TAB, SHIFT, SPACE
                if (sp == iconTab || sp == iconShift || sp == iconSpace)
                {
                    rt.sizeDelta = new Vector2(80, 50);  // Big keys
                }
                else
                {
                    rt.sizeDelta = new Vector2(48, 48);  // Normal keys (W A S D F R etc.)
                }

                // SLASH between icons
                if (i < icons.Length - 1)
                {
                    GameObject slash = new GameObject("Slash", typeof(TextMeshProUGUI));
                    slash.transform.SetParent(iconsContainer, false);
                    TextMeshProUGUI tmp = slash.GetComponent<TextMeshProUGUI>();
                    tmp.text = " / ";
                    tmp.fontSize = 28;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = new Color(1f, 1f, 1f, 0.9f);

                    slash.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 48);
                }
            }
        }

        iconsContainer.gameObject.SetActive(hasAnyIcon);

        var keyTMP = obj.transform.Find("KeyText")?.GetComponent<TextMeshProUGUI>();
        var descTMP = obj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        if (keyTMP) keyTMP.text = keyText;
        if (descTMP) descTMP.text = description;
    }
    private void AddCategoryLabel(string title, Sprite icon = null)
    {
        if (keyInstructionPrefab == null || keyInstructionContainer == null) return;

        GameObject obj = Instantiate(keyInstructionPrefab, keyInstructionContainer);
        Transform iconsContainer = obj.transform.Find("IconsContainer");

        if (iconsContainer == null) return;

        // Clear
        foreach (Transform child in iconsContainer)
            Destroy(child.gameObject);

        if (icon != null)
        {
            GameObject iconGO = new GameObject("HeaderIcon", typeof(Image));
            iconGO.transform.SetParent(iconsContainer, false);
            Image img = iconGO.GetComponent<Image>();
            img.sprite = icon;
            img.preserveAspect = true;

            // FORCE SIZE HERE TOO!
            iconGO.GetComponent<RectTransform>().sizeDelta = new Vector2(56, 56); // slightly bigger for headers
        }

        iconsContainer.gameObject.SetActive(icon != null);

        TextMeshProUGUI keyTMP = obj.transform.Find("KeyText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descTMP = obj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();

        if (keyTMP != null)
        {
            keyTMP.text = title;
            keyTMP.fontSize = 36;
            keyTMP.fontStyle = FontStyles.Bold;
            keyTMP.color = new Color(1f, 0.85f, 0.4f); // gold
        }
        if (descTMP != null) descTMP.text = "";
    }
}