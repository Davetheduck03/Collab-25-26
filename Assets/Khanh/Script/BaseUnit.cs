using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public UnitSO unitData;

    protected List<UnitComponent> components = new List<UnitComponent>();
}