using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MovementComponent : UnitComponent
{
    public float movement_Speed;

    protected override void OnInitialize()
    {
        movement_Speed = data.speed;
    }


}