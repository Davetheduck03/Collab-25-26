using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

    public class MovementComponent : UnitComponent
    {
        public float movement_Speed;


        [Tooltip("Assign a Transform that represents the movement goal (e.g. the end waypoint).")]
        public Transform targetTransform;

        protected override void OnInitialize()
        {
            movement_Speed = data.speed;

        }
    }