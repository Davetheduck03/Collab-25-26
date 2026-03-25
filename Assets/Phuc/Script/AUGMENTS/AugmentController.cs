using UnityEngine;

namespace Phuc.Augments
{
    public class AugmentController : MonoBehaviour
    {
        [SerializeField] private AugmentData _sodatabase;
        [SerializeField] private PhucPlayerStats _soplayerStats;

        private void OnEnable() => AugmentUIController.OnEventOpenAugmentSelection += ChooseAugment;
        private void OnDisable() => AugmentUIController.OnEventOpenAugmentSelection -= ChooseAugment;

        private void ChooseAugment()
        {
            // FOR NOW THE AUGMENT IS AUTOMATICALLY CHOSEN
            // I WOULD NEED SOME HELP IMPLEMENTING THE CHOICE SYSTEM LIKE IN SOUL KNIGHT
            // var picked =  GetRandomAugment();
            AugmentEntry picked = GetRandomAugment();
            ApplyAugmentEffect(picked);
            Debug.Log($"picked augment: {picked.augmentName} and {picked}");
        }

        //use AugmentData instead of void, then the code will work
        private AugmentEntry GetRandomAugment()
        {
            if (_sodatabase == null || _sodatabase.allAugments.Count == 0)
            {
                Debug.Log("No augments found");
            }
            else
            {
                //RANDOMIZER 
                float totalWeight = 0;
                foreach (var augment in _sodatabase.allAugments)
                {
                    totalWeight += augment.weight;
                }
                
                //gen a random number
                float randomNumber = Random.Range(0, totalWeight);
                float currentWeight = 0;
                //unless the randomNumber is smaller or equals to the weight, it picks that augment
                foreach (var augment in _sodatabase.allAugments)
                {
                    currentWeight += augment.weight;
                    if (randomNumber <= currentWeight)
                    {
                        Debug.Log($"{augment} is called");
                        return augment;
                    }
                }
            }
            return _sodatabase.allAugments[0];
        }

        private void ApplyAugmentEffect(AugmentEntry entry)
        {
            switch (entry.augmentName)
            {
                // --- COMMON ---
                case "Are we a Tank or a Truck?!":
                    Debug.Log($"{entry.augmentName} is chosen INCREASE BOAT LOAD BY 2 AND HP BY 10");
                    break;
                case "My salesman is legit":
                    Debug.Log($"{entry.augmentName} is chosen INCREASE ROD'S DURABILITY BY 5");
                    break;
                case "Who dropped the wallet?":
                    Debug.Log($"{entry.augmentName} is chosen GAIN 1~ 18 GOLD");
                    break;
                case "Enchantments?":
                    Debug.Log($"{entry.augmentName} is chosen INCREASE ROD'S ATTACK MULTIPLIER BY 0.1");
                    break;
                case "Happy Birthday":
                    Debug.Log($"{entry.augmentName} is chosen IMMEDIATELY GAIN 1 EXP");
                    break;
                case "Repair kit":
                    Debug.Log($"{entry.augmentName} is chosen HEALS BOAT HP BY 5");
                    break;
                case "Sparkling Water on sales":
                    Debug.Log($"{entry.augmentName} is chosen HEAL STAMINA BY 15");
                    break;
                case "Where's my tea?!":
                    Debug.Log($"{entry.augmentName} is chosen TEMPORARILY GAIN 20 STAMINA FOR 60 SECONDS. IF CURRENT STAMINA == MAX STAMINA, OVERCAP");
                    break;
                case "Cloth Armour":
                    Debug.Log($"{entry.augmentName} is chosen INCREASE DEFENSE BY 0.5");
                    break;
                
                // --- UNCOMMON ---
                case "The One Piece is real?!":
                    // Gold + 36
                    break;
                case "MotorBoat":
                    // Boat Speed + 0.5
                    break;
                case "Graduation":
                    // EXP + 5
                    break;
                case "Skyborn fish":
                    // 1 Uncommon or 2 Common fish
                    break;
                case "Bandage":
                    // Player HP + 5
                    break;

                // --- RARE ---
                case "Adrenaline rush":
                    // Restore Stamina to 40% (Single use)
                    break;
                case "Mature":
                    // EXP + 10
                    break;
                case "Shady business":
                    // Trade 25% fish for full HP
                    break;
                case "Chainmail Vest":
                    // Defense + 1
                    break;

                // --- EPIC ---
                case "Critical Strike":
                    // 15% Fish Health Damage
                    break;
                case "I AM THE ASCENDED":
                    // Heal 20 HP at low health (Single use)
                    break;
                case "Mini Doctor":
                    // Heal based on missing health (Single use)
                    break;
                case "Full Armour":
                    // Defense + 2, Attack + 1
                    break;

                // --- MYTHIC ---
                case "I AM GOD":
                    // All Stats + 10%
                    break;
            }
            
            Time.timeScale = 1; // Resume Game
        }
    }
}