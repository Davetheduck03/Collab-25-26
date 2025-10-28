using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public UnitSO unitData;

    protected List<UnitComponent> components = new List<UnitComponent>();

    public virtual void Awake()
    {
        // Get all components that inherit from UnitComponent
        GetComponents(components);

        foreach (var comp in components)
        {
            comp.Setup(this, unitData);
        }
    }
}