using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerStats : ScriptableObject
{

    [Serializable]
    public class UpgradeEntry
    {
        public UpgradeType type;
        public float baseValue;
        public float incrementPerLevel;
        public int maxLevel = 10;// Default max, override per entry
        [Header("Cost Settings")]
        public int baseCost = 100;
        public int costIncrement = 50;
        public float costMultiplier = 1.5f;

    }
    public List<UpgradeEntry> upgrades = new List<UpgradeEntry>
    {
        new UpgradeEntry { type = UpgradeType.Health,baseValue = 100f, incrementPerLevel = 12.5f,maxLevel =10,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.Mana,baseValue = 50f, incrementPerLevel = 5f,maxLevel =7,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.Stamina,baseValue = 100f, incrementPerLevel = 10f,maxLevel =3,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.Defense,baseValue = 5f, incrementPerLevel = 1.5f,maxLevel =6,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.Attack,baseValue = 10f, incrementPerLevel = 1.5f,maxLevel =7,baseCost = 200},
        
    };
    public UpgradeEntry GetUpgrade(UpgradeType type)
    {
        return upgrades.Find(u => u.type == type);
    }
}
