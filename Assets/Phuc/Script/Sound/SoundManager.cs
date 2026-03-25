using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEngine.Audio;
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
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        private float fadeDuration = 1.0f;
        
        private Coroutine _bgmFadeCoroutine;
        public static SoundManager Instance { get; private set; }
        
        // [SerializeField] private SfxSoundItem[] sfxLibrary;
        [SerializeField] private List<SO_BGMEvent> so_bgmEvent;
        [SerializeField] private List<SO_SFXEvent> so_sfxEvent; 

        private static AudioSource Bgm_Source;
        private static AudioSource Sfx_Source;

        private Dictionary<BgmSoundType, SO_BGMEvent> _bgmDict = new();
        private Dictionary<SfxSoundType, SO_SFXEvent> _sfxDict = new();

        // saves timestamp
        private Dictionary<BgmSoundType, float> _bgmResumeTimer = new Dictionary<BgmSoundType, float>();
        private BgmSoundType _currentBgmType;
        [Header("volume control")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
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
            //------------------THIS LINE GOT ISSUES FOR NOW-------------------
            // DontDestroyOnLoad(gameObject);
            //-------------------------------------------
            InitializeSources();
            InitializeDictionaries();
        }

        private void InitializeSources()
        {
            Bgm_Source = gameObject.AddComponent<AudioSource>();
            Bgm_Source.outputAudioMixerGroup = bgmGroup;
            Sfx_Source = gameObject.AddComponent<AudioSource>();
            Sfx_Source.outputAudioMixerGroup = sfxGroup;
        }

        //Put all of the music in the array to the dictionary
        private void InitializeDictionaries()
        {
                _bgmDict.Clear();
            foreach (var bgm in so_bgmEvent)
            {
                // in dictionary, this key -> holds this sound
                _bgmDict[bgm.type] = bgm;
            }
            _sfxDict.Clear();
            foreach (var sfx in so_sfxEvent)
            {
                _sfxDict[sfx.type] = sfx;
            }
        }
        // Method to call from UI Sliders
        public void SetBGMVolume(float volume)
        {
            // AudioMixer uses a logarithmic scale (-80dB to 20dB)
            // Volume parameter from a slider is usually 0.0001 to 1
            //$$\text{dB} = \log_{10}(\text{Volume}) \times 20$$
            // dB = log10(Volume) * 20;
            SetMixerVolume("BGMVolume", volume);
            Debug.Log($"Bgm volume {volume}");
        }

        public void SetSFXVolume(float volume)
        {
            SetMixerVolume("SFXVolume", volume);
            Debug.Log($"Sfx volume {volume}");
        }
        private void SetMixerVolume(string parameterName, float sliderValue)
        {
            // Converts linear 0-1 to logarithmic dB scale
            float dB = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
            mainMixer.SetFloat(parameterName, dB);
        }
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }
        
        public static void PlayBGM(BgmSoundType type)
        {
            if (!Instance._bgmDict.TryGetValue(type, out var bgm))
            {
                return;
            }
            if (Bgm_Source.clip == bgm.clip)
            {
                return;
            }
            // 1. Save the progress of the CURRENTLY playing BGM before it stops
            if (Bgm_Source.clip != null)
            {
                Instance._bgmResumeTimer[Instance._currentBgmType] = Bgm_Source.time;
            }
            // 2. Set the new current type
            Instance._currentBgmType = type;
            
            if (Instance._bgmFadeCoroutine != null) Instance.StopCoroutine(Instance._bgmFadeCoroutine);
            Instance.StartCoroutine(Instance.FadeBGM(bgm));
        }

        private IEnumerator FadeBGM(SO_BGMEvent bgm)
        {
// Fade Out before fading in
            float startVolume = Bgm_Source.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                Bgm_Source.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }

            //
            Bgm_Source.clip = bgm.clip;
            // 3. CHECK FOR SAVED TIME
            if (_bgmResumeTimer.TryGetValue(_currentBgmType, out float savedTime))
            {
                // Wrap in try-catch or check length to prevent errors if clip changed length
                Bgm_Source.time = savedTime % bgm.clip.length; 
            }
            else
            {
                Bgm_Source.time = 0;
            }
            Bgm_Source.Play();

            // Fade In
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                //linear interpolation testing 1st time, may need to tweak here
                Bgm_Source.volume = Mathf.Lerp(0, 1.0f, t / fadeDuration);
                yield return null;
            }
            Bgm_Source.volume = 1.0f;
        }
//made non-static for testing, rememeber to set it to static back.
        public static void PlaySfx(SfxSoundType type)
        {
            if (Instance == null)
            {
                Debug.Log("soundmanager instance missing");
                return;
            }

            if (!Instance._sfxDict.TryGetValue(type, out var sfxSO))
            {
                return;
            }

            if (sfxSO.clips != null && sfxSO.clips.Length > 0)
            {
                int index = Random.Range(0, sfxSO.clips.Length);
                Sfx_Source.PlayOneShot(sfxSO.clips[index]);
            }
                // Instance.StartCoroutine(Instance.FadeAudio(1.0f, type));
        }

        // private IEnumerator FadeAudio(float targetVolume,SfxSoundType type )
        // {
            // if (!_sfxDict.TryGetValue(type, out var sfxSO))
            // {
            //     yield break;
            // }
            // float startVolume = Sfx_Source.volume;
            // float elapsedTime = 0;
            //
            // while (elapsedTime < fadeDuration)
            // {
            //     elapsedTime += Time.deltaTime;
            //     Sfx_Source.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
            //     yield return null;
            // }
            //
            // Sfx_Source.volume = targetVolume;
            //
            // if (targetVolume == 0) Sfx_Source.Stop(); // Stop playback when fully faded out
            // else if (!Sfx_Source.isPlaying)
            // {
            //     int index = Random.Range(0, sfxSO.clips.Length);
            //     Sfx_Source.Play(sfxSO.clips[index]);   
            //     //Sfx_Source.Play(); // Start playback when fading in
            // }
        //     
        //     if (!_sfxDict.TryGetValue(type, out var sfxSO)) yield break;
        //
        //     float startVolume = Sfx_Source.volume;
        //     float elapsedTime = 0;
        //
        //     // Optional: Assign the clip before starting the fade if it's not playing
        //     if (!Sfx_Source.isPlaying && sfxSO.clips.Length > 0)
        //     {
        //         int index = Random.Range(0, sfxSO.clips.Length);
        //         Sfx_Source.clip = sfxSO.clips[index];
        //         Sfx_Source.Play();
        //     }
        //
        //     while (elapsedTime < fadeDuration)
        //     {
        //         elapsedTime += Time.deltaTime;
        //         Sfx_Source.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / fadeDuration);
        //         yield return null;
        //     }
        //
        //     Sfx_Source.volume = targetVolume;
        //     if (targetVolume == 0) Sfx_Source.Stop();
        // }


        public static void StopBgmMusic()
        {
            Bgm_Source.Stop();
        }

        public static void StopSfx()
        {
            Sfx_Source.Stop();;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }

    
}
