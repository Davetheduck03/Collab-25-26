using UnityEngine;

namespace Phuc.SoundSystem
{
    [CreateAssetMenu(fileName = "SO_SFXEvent", menuName = "AudioRelated/SFX Event")]
    public class SO_SFXEvent : ScriptableObject
    {
        [Header("Audio Configuration")] public AudioClip[] clips;

        [Range(0f, 1f)] public float volume = 0.8f;
        [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
        [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;

        [Header("Max volume")]
        public float minDistance = 1f;
        [Header("Min volume")]
        public float maxDistance = 20f;

        public void Play(Vector3? position = null)
        {
            if (clips == null || clips.Length == 0)
            {
                Debug.Log($"SFX Event {name} is missing audio clips!");
                return;
            }
            //tell the SoundManager.cs to play the sfx
            SoundManager.Instance.PlaySfx(this, position);
        }

        public AudioClip GetRandomClip() => clips[Random.Range(0, clips.Length)];
    }
}