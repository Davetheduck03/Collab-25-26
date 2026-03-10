using UnityEngine;
using System;

namespace Phuc.Augments
{
    public class LevelUp : MonoBehaviour
    { 
        private int curentLevel = 1;
        private int maxLevel = 10;
        public static event Action OnEventLevelUp;

        public void LeveledUp()
        {
            if(curentLevel < maxLevel)
            {
                curentLevel++;
                OnEventLevelUp?.Invoke();
            }
        }
    }
}