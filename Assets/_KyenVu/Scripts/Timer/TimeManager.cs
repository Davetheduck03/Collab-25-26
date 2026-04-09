using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("UI References")]
    public TMP_Text timeText;
    public Image timeIcon;
    public Sprite morningSprite;
    public Sprite nightSprite;

    [Header("Time Settings")]
    public int currentDay = 1;
    public int currentHour = 5;
    public int currentMinute = 0;

    // =======================================================
    // --- NEW: AUTO-TIME SETTINGS ---
    // =======================================================
    [Header("Auto-Time Settings")]
    [Tooltip("How many real-world seconds before time advances.")]
    public float realSecondsPerTick = 30f;
    [Tooltip("How many in-game minutes pass when the timer ticks.")]
    public int inGameMinutesPerTick = 30;

    private float autoTimeTimer = 0f;

    [Header("Schedule Limits")]
    public const int SHOP_OPEN_HOUR = 5;   // 5:00 AM
    public const int DOCK_OPEN_HOUR = 7;   // 7:00 AM
    public const int SHOP_CLOSE_HOUR = 19; // 7:00 PM (19:00)

    public static event Action<string> OnTimeChanged;
    public static event Action<int> OnDayChanged;
    public static event Action OnShopClosed;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        ForceUpdateUI();
        Debug.Log(IsShopOpen() ? "The Fish Shop is OPEN!" : "The Fish Shop is CLOSED!");
    }

    // =======================================================
    // --- NEW: THE TIMER TICK LOGIC ---
    // =======================================================
    private void Update()
    {
        // Check if we are outside of the 7:00 AM to 7:00 PM window
        if (currentHour < DOCK_OPEN_HOUR || currentHour >= SHOP_CLOSE_HOUR)
        {
            // Count up real-world seconds
            autoTimeTimer += Time.deltaTime;

            // If 30 seconds have passed...
            if (autoTimeTimer >= realSecondsPerTick)
            {
                autoTimeTimer = 0f; // Reset the timer
                AdvanceTime(inGameMinutesPerTick); // Add 30 in-game minutes!
                Debug.Log($"[TimeManager] Auto-advanced time by {inGameMinutesPerTick} mins.");
            }
        }
        else
        {
            // Keep the timer at 0 during the day so it doesn't instantly tick when 7 PM hits
            autoTimeTimer = 0f;
        }
    }

    public void AdvanceTime(int minutesPassed)
    {
        currentMinute += minutesPassed;

        while (currentMinute >= 60)
        {
            currentMinute -= 60;
            currentHour++;

            if (currentHour >= 24)
            {
                currentHour = 0;
            }

            // =======================================================
            // --- NEW: TIME-BASED NOTIFICATIONS ---
            // =======================================================

            // 1. Announce when the Dock opens (7:00 AM)
            if (currentHour == DOCK_OPEN_HOUR)
            {
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification("The dock is now OPEN!");
                }
            }

            // 2. Warning at 6:00 PM (18:00) that the shop is closing soon
            if (currentHour == 18)
            {
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification("It's 6:00 PM! The Fish Shop is closing soon, please come back!");
                }
            }

            // 3. Existing check for 7:00 PM
            if (currentHour == SHOP_CLOSE_HOUR)
            {
                Debug.Log("The Fish Shop is now CLOSED!");
                OnShopClosed?.Invoke();

            //Optional: You could also add a notification here!
                 if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowNotification("It's 7:00 PM. The Fish Shop is now CLOSED.");
            }
        }

        ForceUpdateUI();
    }

    public void SleepToNextDay()
    {
        Debug.Log("Going to sleep...");

        if (QuotaManager.Instance != null)
        {
            bool survived = QuotaManager.Instance.CheckQuotaAtEndOfDay();
            if (!survived)
            {
                Debug.Log("GAME OVER! You did not meet the quota.");
                return;
            }
        }

        currentDay++;
        currentHour = 5;
        currentMinute = 0;

        ForceUpdateUI();
        Debug.Log($"Woke up on Day {currentDay}!");
    }

    public bool IsShopOpen() => currentHour >= SHOP_OPEN_HOUR && currentHour < SHOP_CLOSE_HOUR;
    public bool IsDockOpen() => currentHour >= DOCK_OPEN_HOUR;

    public void ForceUpdateUI()
    {
        string timeString = GetTimeString();
        OnDayChanged?.Invoke(currentDay);
        OnTimeChanged?.Invoke(timeString);

        if (timeText != null)
        {
            timeText.text = timeString;
        }

        if (timeIcon != null)
        {
            if (currentHour >= 5 && currentHour < 12)
            {
                timeIcon.sprite = morningSprite;
            }
            else
            {
                timeIcon.sprite = nightSprite;
            }
        }
    }

    public string GetTimeString()
    {
        string ampm = currentHour >= 12 ? "PM" : "AM";
        int displayHour = currentHour > 12 ? currentHour - 12 : currentHour;
        if (displayHour == 0) displayHour = 12;

        return $"{displayHour:00}:{currentMinute:00} {ampm}";
    }
}