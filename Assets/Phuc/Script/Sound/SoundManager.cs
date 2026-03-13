using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Phuc.SoundSystem
{
    // public enum BgmSoundType
    // {
    //     //name matches the name of the file for easier reference
    //     Forest_FishyWishy,
    //     Reel_em_in,
    //     Marketplace_demo3
    // }
    // public enum SfxSoundType
    // {
    //     Hook,
    //     Fish,
    //     Boat,
    //     Rod,
    //     Player
    //     //Add more SFX type if needed
    // }
    //stores t he music file in the editor, it could need some adjustments for the SFX.
    // [Serializable]
    // public struct BgmSoundItem
    // {
    //     public BgmSoundType type;
    //     public AudioClip clip;
    // }
    // [Serializable]
    // public struct SfxSoundItem
    // {
    //     public SfxSoundType type;
    //     public AudioClip[] clips;
    // }
    
    
    /*Flow: create an array in the inspector, while in runtime, it converts that into a dictionary
     for better optimization*/
    [RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        
        // [SerializeField] private SfxSoundItem[] sfxLibrary;
        [SerializeField] private List<SO_BGMEvent> so_bgmEvent;
        [SerializeField] private List<SO_SFXEvent> so_sfxEvent; 

        private static AudioSource Bgm_Source;
        private static AudioSource Sfx_Source;

        private static Dictionary<BgmSoundType, SO_BGMEvent> _bgmDict = new();
        private static Dictionary<SfxSoundType, SO_SFXEvent> _sfxDict = new();

        private void Awake()
        {
            // --------------- FAILSAFE, DO NOT DELETE -----------------
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            //------------------------------------------------------
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSources();
            InitializeDictionaries();
        }

        private void InitializeSources()
        {
            Bgm_Source = gameObject.AddComponent<AudioSource>();
            Sfx_Source = gameObject.AddComponent<AudioSource>();
        }

        //Put all of the music in the array to the dictionary
        private void InitializeDictionaries()
        {
            foreach (var bgm in so_bgmEvent)
            {
                // in dictionary, this key -> holds this sound
                _bgmDict[bgm.type] = bgm;
            }

            foreach (var sfx in so_sfxEvent)
            {
                _sfxDict[sfx.type] = sfx;
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public static void PlayBGM(BgmSoundType type)
        {
            if (!_bgmDict.TryGetValue(type, out var bgm))
            {
                return;
            }
            if (Bgm_Source.clip == bgm.clip)
            {
                return;
            }
            Bgm_Source.clip = bgm.clip;
            Bgm_Source.loop = true;
            Bgm_Source.Play();
        }

        public static void PlaySfx(SfxSoundType type)
        {
            
            if (!_sfxDict.TryGetValue(type, out var sfxSO))
            {
                return;
            }
            int index = Random.Range(0, sfxSO.clips.Length);
            Sfx_Source.PlayOneShot(sfxSO.clips[index]);
        }

        public static void StopBgmMusic()
        {
            Bgm_Source.Stop();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    
}
