using System;
using UnityEngine;
using System.Collections.Generic;

namespace Phuc.Augments
{
    [Serializable]
    public class AugmentEntry
    {
        public string augmentName;
        public string effectDescription;
        public float weight;
        
        // --------------------- LATER ------------
        // public enum Rarity { Common, Uncommon, Rare, Epic, Mythic }
        // public Rarity rarity;
    }

    [CreateAssetMenu(fileName = "AugmentData", menuName = "AugmentRelated/AugmentData")]
    public class AugmentData : ScriptableObject
    {
        [SerializeField]
        public List<AugmentEntry> allAugments = new List<AugmentEntry>()
        {
            // --- Common ---
            new AugmentEntry { augmentName = "Are we a Tank or a Truck?!", effectDescription = "Increase Boat Load by 2 and HP by 10", weight = 100 },
            new AugmentEntry { augmentName = "My salesman is legit", effectDescription = "Increase rod's durability by 5", weight = 99 },
            new AugmentEntry { augmentName = "Who dropped the wallet?", effectDescription = "Gain 1~ 18 gold", weight = 98 },
            new AugmentEntry { augmentName = "Enchantments?", effectDescription = "Increase Rod's attack multiplier by 0.1", weight = 97 },
            new AugmentEntry { augmentName = "Happy Birthday", effectDescription = "Immediately gain 1 EXP", weight = 96 },
            new AugmentEntry { augmentName = "Repair kit", effectDescription = "Heals boat HP by 5", weight = 95 },
            new AugmentEntry { augmentName = "Sparkling Water on sales", effectDescription = "Heal stamina by 15", weight = 94 },
            new AugmentEntry { augmentName = "Where's my tea?!", effectDescription = "Temporarily gain 20 stamina for 60 seconds", weight = 93 },
            new AugmentEntry { augmentName = "Cloth Armour", effectDescription = "Increase defense by 0.5", weight = 92 },

            // --- Uncommon ---
            new AugmentEntry { augmentName = "The One Piece is real?!", effectDescription = "Gain 36 gold", weight = 50 },
            new AugmentEntry { augmentName = "MotorBoat", effectDescription = "Increase Boat Speed by 0.5", weight = 49 },
            new AugmentEntry { augmentName = "Graduation", effectDescription = "Immediately gain 5 EXP", weight = 48 },
            new AugmentEntry { augmentName = "Skyborn fish", effectDescription = "Gain 1 uncommon fish OR 2 common fish", weight = 47 },
            new AugmentEntry { augmentName = "Bandage", effectDescription = "Heal player's HP by 5", weight = 46 },

            // --- Rare ---
            new AugmentEntry { augmentName = "Adrenaline rush", effectDescription = "Restore Stamina to 40% when it hits 0. Single use only", weight = 20 },
            new AugmentEntry { augmentName = "Mature", effectDescription = "Immediately gain 10 EXP", weight = 19 },
            new AugmentEntry { augmentName = "Shady business", effectDescription = "Trade 25% of current fish caught for full HP", weight = 18 },
            new AugmentEntry { augmentName = "Chainmail Vest", effectDescription = "Increase defense by 1", weight = 17 },

            // --- Epic ---
            new AugmentEntry { augmentName = "Critical Strike", effectDescription = "Immediately deals damage to the fish equals to 15% of its health", weight = 7 },
            new AugmentEntry { augmentName = "I AM THE ASCENDED", effectDescription = "If player's HP drop to 10, heals 20 HP. Single use only", weight = 6 },
            new AugmentEntry { augmentName = "Mini Doctor", effectDescription = "Immediately heals player HP based on missing health. Trigger only once", weight = 5 },
            new AugmentEntry { augmentName = "Full Armour", effectDescription = "Increase defense by 2, attack by 1", weight = 4 },

            // --- Mythic ---
            new AugmentEntry { augmentName = "I AM GOD", effectDescription = "Increase all current stats by 10% (this function only runs once)", weight = 1 }
        };
    }
}