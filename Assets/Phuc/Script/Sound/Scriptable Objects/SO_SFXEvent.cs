using UnityEngine;

namespace Phuc.SoundSystem
{
    
    [CreateAssetMenu(fileName = "SO_SFXEvent", menuName = "AudioRelated/SFX Event")]
    public class SO_SFXEvent : SO_AudioEvent
    {
        public SfxSoundType type;
        public AudioClip[] clips;

    }
}