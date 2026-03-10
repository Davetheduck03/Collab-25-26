using System;
using UnityEngine;

namespace Phuc.SoundSystem
{
    public enum BGMSoundType
    {
        //name matches the name of the file for easier reference
        Forest_FishyWishy,
        Reel_em_in,
        Marketplace_demo3
    }

    // public enum SFXSoundType
    // {
    //     
    // }
    [RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private BGMSoundItem[] bgmLibrary;
        private static SoundManager instance;
        private AudioSource audioSource;

        private void Awake()
        {
            instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

// volume is default to 1
        public static void PlaySound(BGMSoundType bgmSound, float volume = 1)
        {
            foreach (var item in instance.bgmLibrary)
            {
                if (item.type == bgmSound)
                {
                    instance.audioSource.Stop();
                    instance.audioSource.clip = item.clip;
                    instance.audioSource.loop = true;
                    instance.audioSource.Play();
                    return;
                }
            }
        }

        public static void StopSound()
        {
            instance.audioSource.Stop();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    //stores t he music file in the editor, it could need some adjustments for the SFX.
    [Serializable]
    public struct BGMSoundItem
    {
        public BGMSoundType type;
        public AudioClip clip;
    }
}
