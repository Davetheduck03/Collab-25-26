using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitComponent : MonoBehaviour
{
    protected BaseUnit unit;
    protected UnitSO data;
    protected PlayerStats playerData;

    /// <summary>
    /// Called by BaseUnit on Awake.
    /// </summary>
    public void Setup(BaseUnit unit, UnitSO data)
    {
        this.unit = unit;
        this.data = data;
        OnInitialize();
    }

    public void BoatSetUp(UnitSO _)
    {
        this.data = _;
        OnBoatSetUp();
    }

    /// <summary>
    /// Override this in child components to initialize from data.
    /// </summary>
    protected virtual void OnInitialize() { }
    protected virtual void OnBoatSetUp() { }
}