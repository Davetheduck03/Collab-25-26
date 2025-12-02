using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class SettingMenu : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider sfxSlider;
    [Header("Key Instruction")]
    public Transform keyInstructionContainer;
    public GameObject keyInstructionPrefab;
    // NEW: assign your icons here in the Inspector (drag & drop)
    [Header("Control Icons")]
    public Sprite iconWASD;
    public Sprite iconF;
    public Sprite iconTab;
    public Sprite iconShift;
    public Sprite iconNumbers;
    public Sprite iconAD;
    public Sprite iconWS;
    public Sprite iconR;
    public Sprite iconQE;
    public Sprite iconSpace;
    public Sprite iconBoat;
    private void Start()
    {
        LoadVolumes();
        GenerateKeyInstructions();
    }
    // ================ VOLUME (unchanged) ================ //
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
    // ================ KEY INSTRUCTIONS WITH ICONS ================ //
    private void GenerateKeyInstructions()
    {
        foreach (Transform child in keyInstructionContainer)
            Destroy(child.gameObject);
        AddCategoryLabel("Land Mode (Top-Down Exploration)", iconBoat); // you can use a land icon if you have one
        AddInstruction("W / A / S / D", "Move Up / Down / Left / Right", iconWASD);
        AddInstruction("F", "Interact (NPCs, Trade, Collect, Upgrade)", iconF);
        AddInstruction("Tab / I", "Open Inventory / Menu", iconTab);
        AddInstruction("Shift (Hold)", "Sprint (Increase Movement Speed)", iconShift);
        AddInstruction("1–3", "Use / Consume Items from Slots", iconNumbers);
        AddCategoryLabel("Boat Mode (Side-Scroller Fishing)", iconBoat);
        AddInstruction("A / D", "Move Boat Left / Right", iconAD);
        AddInstruction("W / S", "Ascend / Descend (Depth Control)", iconWS);
        AddInstruction("R", "Cast Fishing Line", iconR);
        AddInstruction("Q / E", "Reel Left / Right", iconQE);
        AddInstruction("Space", "Parry Lunging Fish", iconSpace);
        AddInstruction("F", "Dock / Interact with Hotspots", iconF);
    }
    private void AddInstruction(string key, string description, Sprite icon)
    {
        GameObject newItem = Instantiate(keyInstructionPrefab, keyInstructionContainer);
        newItem.transform.Find("IconImage").GetComponent<Image>().sprite = icon;
        newItem.transform.Find("KeyText").GetComponent<TextMeshProUGUI>().text = key;
        newItem.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = description;
    }
    private void AddCategoryLabel(string title, Sprite icon = null)
    {
        GameObject newItem = Instantiate(keyInstructionPrefab, keyInstructionContainer);
        var iconImg = newItem.transform.Find("IconImage").GetComponent<Image>();
        if (icon != null)
            iconImg.sprite = icon;
        else
            iconImg.enabled = false; // hide icon on headers
        var keyTMP = newItem.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
        keyTMP.text = title;
        keyTMP.fontSize = 36;
        keyTMP.fontStyle = FontStyles.Bold;
        keyTMP.color = new Color(1f, 0.85f, 0.4f);
        newItem.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = "";
    }
}