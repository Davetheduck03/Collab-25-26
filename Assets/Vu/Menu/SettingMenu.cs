using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SettingMenu : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider sfxSlider;

    [Header("Key Instruction")]
    public Transform keyInstructionContainer;
    public GameObject keyInstructionPrefab;

    private void Start()
    {
        LoadVolumes();
        GenerateKeyInstructions();
    }

    //================ VOLUME =================//
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

        masterSlider.value = master;
        sfxSlider.value = sfx;

        AudioListener.volume = master;
    }

    //================= KEY INSTRUCTIONS UI =================//
    private void GenerateKeyInstructions()
    {
        // Clear old children (if any)
        foreach (Transform child in keyInstructionContainer)
            Destroy(child.gameObject);

        //---------------- LAND MODE ----------------//
        AddCategoryLabel("Land Mode (Top-Down Exploration)");

        AddInstruction("W / A / S / D", "Move Up / Down / Left / Right");
        AddInstruction("F", "Interact (NPCs, Trade, Collect, Upgrade)");
        AddInstruction("Tab / I", "Open Inventory / Menu");
        AddInstruction("Shift (Hold)", "Sprint (Increase Movement Speed)");
        AddInstruction("1–3", "Use / Consume Items from Slots");

        //---------------- BOAT MODE ----------------//
        AddCategoryLabel("Boat Mode (Side-Scroller Fishing)");

        AddInstruction("A / D", "Move Boat Left / Right");
        AddInstruction("W / S", "Ascend / Descend (Depth Control)");
        AddInstruction("R", "Cast Fishing Line");
        AddInstruction("Q", "Reel Left (Counter Fish Pull)");
        AddInstruction("E", "Reel Right (Counter Fish Pull)");
        AddInstruction("Space", "Parry Lunging Fish");
        AddInstruction("F", "Dock / Interact with Hotspots");

    }

    private void AddInstruction(string key, string description)
    {
        GameObject newItem = Instantiate(keyInstructionPrefab, keyInstructionContainer);

        newItem.transform.Find("KeyText")
            .GetComponent<TextMeshProUGUI>().text = key;

        newItem.transform.Find("DescriptionText")
            .GetComponent<TextMeshProUGUI>().text = description;
    }

    private void AddCategoryLabel(string title)
    {
        GameObject newItem = Instantiate(keyInstructionPrefab, keyInstructionContainer);

        var keyTMP = newItem.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
        var descTMP = newItem.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();

        keyTMP.text = title;
        descTMP.text = "";

        // Make it look like a section header
        keyTMP.fontSize = 36;
        keyTMP.fontStyle = FontStyles.Bold;
        keyTMP.color = new Color(1f, 0.8f, 0.4f); // nice gold/orange color (optional)

        // Optional: add a bit of top spacing so it feels separated
        var layout = newItem.GetComponent<HorizontalLayoutGroup>();
        if (layout) layout.padding.top = 30;
    }
}
