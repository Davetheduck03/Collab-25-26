using UnityEngine;

namespace Phuc.SoundSystem
{
    [CreateAssetMenu(fileName = "SO_BGMEvent", menuName = "AudioRelated/BGM Event")]
    public class SO_BGMEvent : ScriptableObject
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.001f, 2.0f)]public float fadeDuration = 1f;
        public bool loop = true;

        public void Play()
        {
            if (clip == null)
            {
                Debug.Log($"BGM Event {name} is missing an audio clip!");
                return;
            }

            SoundManager.Instance.PlayBGM(this);
        }
    }
}