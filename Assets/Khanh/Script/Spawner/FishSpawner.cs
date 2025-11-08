using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;          

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
        fishScript.Initialize();
        return fishScript;
    }

    void Start()
    {
        Spawn();
    }
    
}