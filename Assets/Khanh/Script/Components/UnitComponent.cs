using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitComponent : MonoBehaviour
{
    protected BaseUnit unit;
    protected UnitSO data;
    protected PlayerStats playerData;

    public void Setup(BaseUnit unit, UnitSO data)
    {
        this.unit = unit;
        this.data = data;
        OnInitialize();
    }

    public void BoatSetUp()
    {
        OnBoatSetUp();
    }

    protected virtual void OnInitialize() { }
    protected virtual void OnBoatSetUp() { }
}