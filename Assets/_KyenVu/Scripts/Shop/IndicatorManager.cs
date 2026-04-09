using System;
using System.Security.Cryptography;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class IndicatorManager : MonoBehaviour
{
    public static IndicatorManager Instance;

    [Header("References")]
    public Transform player;
    public GameObject pointerPrefab;

    [Tooltip("Drag your Main UI Canvas here so the arrows spawn on the UI!")]
    public Transform canvasParent; 

    [Header("Target Locations")]
    public Transform dockLocation;
    public Transform shopLocation;
    public Transform homeLocation;
    public Transform tavernLocation;

    private UIIndicatorPointer dockPointer;
    private UIIndicatorPointer shopPointer;
    private UIIndicatorPointer homePointer;
    private UIIndicatorPointer tavernPointer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Spawn the pointers orbiting the player
        dockPointer = CreatePointer(dockLocation, "Dock");
        shopPointer = CreatePointer(shopLocation, "Shop");
        homePointer = CreatePointer(homeLocation, "Home");
        tavernPointer = CreatePointer(tavernLocation, "Tavern");
    }

    private UIIndicatorPointer CreatePointer(Transform target, string label)
    {
        if (target == null) return null;

        // --- CHANGED: Spawn inside the UI Canvas! ---
        GameObject obj = Instantiate(pointerPrefab, canvasParent);

        // Note: Change 'IndicatorPointer' to 'UIIndicatorPointer' to match the new script
        UIIndicatorPointer pointer = obj.GetComponent<UIIndicatorPointer>();
        pointer.Initialize(player, target, label);

        return pointer;
    }
//``` *(Make sure to update the type of your `dockPointer`, `shopPointer`, etc.variables in the manager to use `UIIndicatorPointer` as well!)*

//### How to rebuild the Prefab for UI:
//Because we changed it to a UI element, your old prefab won't work anymore. Let's make a new, clean one!
//1.Right - click your main **UI Canvas** in the hierarchy -> **Create Empty**. Name it `UITargetPointer`.
//2. Add the **`UIIndicatorPointer`** script to it.
//3. Right-click `UITargetPointer` -> **UI -> Image**. Name it `Arrow`. Assign your arrow sprite here.
//4. Right-click `UITargetPointer` -> **UI -> Text - TextMeshPro**. Name it `Label` and drag it below the arrow.
//5. In the script on `UITargetPointer`, drag the `Arrow` into the **Arrow Graphic** and** Arrow Image** slots. Drag `Label` into the **Label Text** slot.
//6. Drag `UITargetPointer` into your project folder to make it a prefab, delete it from the canvas, and assign it to your `IndicatorManager`!

//***A Quick Unity Secret:** If you ever DO want to put text in the 3D/2D game world without it ruining performance by spawning a Canvas, don't use the `UI` menu. Instead, right-click -> `3D Object` -> `Text - TextMeshPro`. It acts exactly like a SpriteRenderer and uses 0 canvases! But for indicators like this, UI is absolutely the right choice.*

    private void OnEnable()
    {
        // Hook into your existing systems!
        TimeManager.OnTimeChanged += HandleTimeEvents;
        CastLineControl.OnFishingFinished += HandleFishingFinished;
    }

    private void OnDisable()
    {
        TimeManager.OnTimeChanged -= HandleTimeEvents;
        CastLineControl.OnFishingFinished -= HandleFishingFinished;
    }

    // =========================================================
    // --- HIGHLIGHT LOGIC ---
    // =========================================================

    private void HandleTimeEvents(string timeString)
    {
        if (TimeManager.Instance == null) return;
        int hour = TimeManager.Instance.currentHour;

        ClearAllHighlights();

        // 1. Dock opens at 7 AM
        if (hour == TimeManager.DOCK_OPEN_HOUR)
        {
            if (dockPointer != null) dockPointer.SetHighlight(true);
        }
        // 2. Shop closes at 7 PM (19) - Remind them to sell!
        else if (hour == 18)
        {
            if (shopPointer != null) shopPointer.SetHighlight(true);
        }
        // 3. Bedtime (e.g., 10 PM to 5 AM)
        else if (hour >= 22 || hour < 5)
        {
            if (homePointer != null) homePointer.SetHighlight(true);
        }
    }

    private void HandleFishingFinished(bool success, EnemySO caughtFish)
    {
        // If we caught a fish, heavily encourage going to the shop!
        if (success)
        {
            ClearAllHighlights();
            if (shopPointer != null) shopPointer.SetHighlight(true);
        }
    }

    public void ClearAllHighlights()
    {
        if (dockPointer != null) dockPointer.SetHighlight(false);
        if (shopPointer != null) shopPointer.SetHighlight(false);
        if (homePointer != null) homePointer.SetHighlight(false);
        if (tavernPointer != null) tavernPointer.SetHighlight(false);
    }
}