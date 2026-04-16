using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Serializable]
public class PlayerCurrencyData
{
    public int currency;
}
public class CurrencyManager : GameSingleton<CurrencyManager>
{
    public static event Action<int> OnCurrencyAdded;
    // --- NEW: This tells the UI to update whenever gold changes (up OR down!) ---
    public static event Action<int> OnCurrencyChanged;

    private PlayerCurrencyData playerData;
    private string savePath;

    protected override void Awake()
    {
        base.Awake(); // Sets up the GameSingleton

        savePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        LoadCurrency();
    }

    private void LoadCurrency()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            playerData = JsonUtility.FromJson<PlayerCurrencyData>(json);
            Debug.Log("Loaded currency: " + playerData.currency);
        }
        else
        {
            playerData = new PlayerCurrencyData { currency = 0 };  // Starting amount
            SaveCurrency();
            Debug.Log("New player data created with starting currency: 0");
        }
    }

    public void SaveCurrency()
    {
        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(savePath, json);
        Debug.Log("Saved currency: " + playerData.currency);
    }

    public int GetCurrency()
    {
        return playerData.currency;
    }

    public void AddCurrency(int amount)
    {
        if (amount > 0)
        {
            playerData.currency += amount;
            SaveCurrency();
            Debug.Log("Added " + amount + ". New total: " + playerData.currency);

            OnCurrencyAdded?.Invoke(amount);
            OnCurrencyChanged?.Invoke(playerData.currency); // <-- NEW
        }
    }
    public void AddFreeCurrency(int amount)
    {
        if (amount > 0)
        {
            playerData.currency += amount;
            SaveCurrency();
            Debug.Log("Added FREE gold: " + amount + ". New total: " + playerData.currency);

            // NOTICE: We do NOT fire OnCurrencyAdded here! 
            // This makes the QuotaManager completely ignore this money.

            // We DO fire this so the UI text still updates on the screen:
            OnCurrencyChanged?.Invoke(playerData.currency);
        }
    }
    public bool SpendCurrency(int amount)
    {
        if (amount > 0 && playerData.currency >= amount)
        {
            playerData.currency -= amount;
            SaveCurrency();
            Debug.Log("Spent " + amount + ". New total: " + playerData.currency);

            OnCurrencyChanged?.Invoke(playerData.currency); // <-- NEW
            return true;
        }
        Debug.Log("Not enough currency!");
        return false;
    }

    public void RemoveCurrency(int amount)
    {
        if (amount > 0)
        {
            playerData.currency -= amount;
            if (playerData.currency < 0) playerData.currency = 0;

            SaveCurrency();
            Debug.Log("Forcibly removed " + amount + " gold. New total: " + playerData.currency);

            OnCurrencyChanged?.Invoke(playerData.currency); // <-- NEW
        }
    }
    // Optional: Call this on application quit to ensure save
    void OnApplicationQuit()
    {
        SaveCurrency();
    }
}
