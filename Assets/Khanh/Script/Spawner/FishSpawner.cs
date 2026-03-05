using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;

    [Tooltip("Drag the Boat scene instance here (NOT the Boat prefab asset).")]
    public GameObject boat;

    public List<EnemySO> fishData;

    public Fish Spawn()
    {
        if (fishPrefab == null || fishData == null)
        {
            Debug.LogError("Missing component");
            return null;
        }
        GameObject fish = Instantiate(fishPrefab, transform.position, Quaternion.identity);
        Fish fishScript = fish.GetComponent<Fish>();
        int randomIndex = Random.Range(0, fishData.Count);
        fishScript.unitData = fishData[randomIndex];
        print("Spawned fish: " + fishScript.unitData.UnitName);

        // Inject the scene-instance reference BEFORE Initialize() so the fish
        // always targets the real Boat, not the un-initialized prefab asset.
        if (boat != null)
            fishScript.m_Boat = boat;

        fishScript.Initialize();
        return fishScript;
    }

    void Start()
    {
        Spawn();
    }

}