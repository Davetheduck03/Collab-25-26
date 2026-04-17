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

    [Header("Auto-Time Settings")]
    [Tooltip("How many real-world seconds before time advances.")]
    public float realSecondsPerTick = 30f;
    [Tooltip("How many in-game minutes pass when the timer ticks.")]
    public int inGameMinutesPerTick = 30;

    private float autoTimeTimer = 0f;

    [Header("Schedule Limits")]
    public const int SHOP_OPEN_HOUR = 5;   // 5:00 AM
    public const int DOCK_OPEN_HOUR = 7;   // 7:00 AM
    public const int SHOP_CLOSE_HOUR = 24; // 24 = Midnight

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

    private void Update()
    {
        // --- NEW: If time hit midnight, stop ALL automatic ticking completely ---
        if (currentHour >= 24) return;

        // Auto-tick time during specific hours (e.g., before the dock opens)
        if (currentHour < DOCK_OPEN_HOUR)
        {
            autoTimeTimer += Time.deltaTime;

            if (autoTimeTimer >= realSecondsPerTick)
            {
                autoTimeTimer = 0f;
                AdvanceTime(inGameMinutesPerTick);
                Debug.Log($"[TimeManager] Auto-advanced time by {inGameMinutesPerTick} mins.");
            }
        }
        else
        {
            autoTimeTimer = 0f;
        }
    }

    public void AdvanceTime(int minutesPassed)
    {
        // --- NEW: Block any time from passing if it is already midnight ---
        if (currentHour >= 24) return;

        currentMinute += minutesPassed;

        while (currentMinute >= 60)
        {
            currentMinute -= 60;
            currentHour++;

            // =======================================================
            // --- NEW: MIDNIGHT LIMIT AND LATE NIGHT WARNINGS ---
            // =======================================================

            // 1. Cap time exactly at Midnight (24:00)
            if (currentHour >= 24)
            {
                currentHour = 24;
                currentMinute = 0; // Force it to 12:00 AM exactly

                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification("It's Midnight! You are exhausted, please go to sleep.");
                }

                OnShopClosed?.Invoke();
                break; // Exit the loop so time stops advancing completely
            }

            // 2. Late Night Fishing Warnings
            if (currentHour == 22) // 10:00 PM
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowNotification("It's 10:00 PM! It's getting very late.");
            }
            if (currentHour == 23) // 11:00 PM
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowNotification("It's 11:00 PM! You need to head back to town soon!");
            }

            // 3. Dock Opening
            if (currentHour == DOCK_OPEN_HOUR)
            {
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowNotification("The dock is now OPEN!");
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

            if (survived)
            {
                // YOU WON TODAY! Show the Win screen!
                if (DailyResultUI.Instance != null) DailyResultUI.Instance.ShowWinScreen();
            }
            else
            {
                // YOU FAILED THE QUOTA! Show the Game Over screen!
                if (DailyResultUI.Instance != null) DailyResultUI.Instance.ShowLoseScreen();
            }
        }
    }

    // This is called by the "Next Day" button after the screen fades to black!
    public void ExecuteSleepRoutine()
    {
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
            if (currentHour >= 5 && currentHour < 18) // Switch to night icon at 6 PM
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
        // --- NEW: Fix AM/PM logic so 24:00 correctly says "12:00 AM" instead of PM! ---
        string ampm = (currentHour >= 12 && currentHour < 24) ? "PM" : "AM";
        int displayHour = currentHour > 12 ? currentHour - 12 : currentHour;

        if (displayHour == 0) displayHour = 12;

        return $"{displayHour:00}:{currentMinute:00} {ampm}";
    }
}