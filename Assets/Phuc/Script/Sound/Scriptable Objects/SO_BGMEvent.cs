using UnityEngine;

namespace Phuc.SoundSystem
{

    [CreateAssetMenu(fileName = "SO_BGMEvent", menuName = "AudioRelated/BGM Event")]
    public class SO_BGMEvent : SO_AudioEvent
    {
        public BgmSoundType type;
        public AudioClip clip;
    }
}