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
    /// <summary>Fires whenever gold is successfully added. Used by QuotaManager to track run earnings.</summary>
    public static event Action<int> OnCurrencyAdded;

    private PlayerCurrencyData playerData;
    private string savePath;

    void Start()
    {
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
            SaveCurrency();  // Save after change
            Debug.Log("Added " + amount + ". New total: " + playerData.currency);
            OnCurrencyAdded?.Invoke(amount);
        }
    }

    public bool SpendCurrency(int amount)
    {
        if (amount > 0 && playerData.currency >= amount)
        {
            playerData.currency -= amount;
            SaveCurrency();  // Save after change
            Debug.Log("Spent " + amount + ". New total: " + playerData.currency);
            return true;
        }
        Debug.Log("Not enough currency!");
        return false;
    }
    // --- NEW: For forcibly removing gold when resetting a day ---
    public void RemoveCurrency(int amount)
    {
        if (amount > 0)
        {
            playerData.currency -= amount;

            // Safety check: don't let gold go into the negatives
            if (playerData.currency < 0)
            {
                playerData.currency = 0;
            }

            SaveCurrency();
            Debug.Log("Forcibly removed " + amount + " gold for day reset. New total: " + playerData.currency);
        }
    }
    // Optional: Call this on application quit to ensure save
    void OnApplicationQuit()
    {
        SaveCurrency();
    }
}
