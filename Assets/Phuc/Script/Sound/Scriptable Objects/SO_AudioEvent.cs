using Phuc.SoundSystem;
using UnityEngine;

namespace Phuc.SoundSystem
{
    
    public enum SfxSoundType
    {
        Hooked_the_fish,
        Fish,
        Boat,
        Rod_casted,
        Player,
        Village_running,
        EXPERIMENTAL
        //Add more SFX type if needed
    }
    public enum BgmSoundType
    {
        //name matches the name of the file for easier reference
        Forest_FishyWishy,
        Reel_em_in,
        Marketplace_demo3
    }

    //abstract
    [CreateAssetMenu(fileName = "SO_AudioEvent", menuName = "AudioRelated/Abstract SO_AudioEvent")]
    public abstract class SO_AudioEvent : ScriptableObject
    {
    }

    // THESE TWO CLASSES INHERIT THE SO_EVENT

    
}