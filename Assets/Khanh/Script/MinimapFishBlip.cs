using UnityEngine;

/// <summary>
/// Attach to every fish prefab in the fishing scene.
/// Automatically registers with MinimapController so a blip appears on the map.
/// </summary>
[RequireComponent(typeof(Fish))]
public class MinimapFishBlip : MonoBehaviour
{
    public string FishName { get; private set; }

    private void Start()
    {
        var fish = GetComponent<Fish>();
        if (fish != null && fish.unitData != null)
            FishName = fish.unitData.UnitName;

        MinimapController.Instance?.RegisterFish(this);
    }

    private void OnDestroy()
    {
        MinimapController.Instance?.UnregisterFish(this);
    }
}
