using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class BoatData : MonoBehaviour
{
    public List<UnitComponent> components;

    private void Start()
    {
        Initialtize();
    }

    private void Initialtize()
    {
        GetComponents(components);
        foreach(var component in components)
        {
            component.BoatSetUp();
        }
    }
}
