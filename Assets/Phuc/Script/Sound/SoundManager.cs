using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.Pool;
namespace Phuc.SoundSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource _bgmSource;
        private SO_BGMEvent _currentBgm;
        //new object pooling unity
        private IObjectPool<AudioSource> _sfxPool;
        //
        private AudioSource _sfxSource;
        private Coroutine _bgmFadeCoroutine;

        // saves timestamp
        private Dictionary<SO_BGMEvent, float> _bgmResumeTimer = new Dictionary<SO_BGMEvent, float>();

        [SerializeField] private int sfxPoolSize = 20;
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
            DontDestroyOnLoad(gameObject);
            //-------------------------------------------
            InitializeBGMSource();
            SetUpAudioPool();
        }

        
        private void InitializeBGMSource()
        {
            if (_bgmSource == null)
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();    
            }
            _bgmSource.outputAudioMixerGroup = bgmGroup;
        }
        // test new Unity object pooling
        private void SetUpAudioPool()
        {
            // Setup the pool logic
            _sfxPool = new ObjectPool<AudioSource>(
                createFunc: CreateNewSource,       // How to make a new one
                actionOnGet: s => s.gameObject.SetActive(true),   // What to do when taken
                actionOnRelease: s => s.gameObject.SetActive(false), // What to do when returned
                actionOnDestroy: s => Destroy(s.gameObject),      // Clean up
                collectionCheck: false, 
                defaultCapacity: 10, 
                maxSize: 20
            );
        }
        private AudioSource CreateNewSource()
        {
            GameObject go = new GameObject("Pooled_SFX");
            go.transform.SetParent(transform);
            AudioSource s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.outputAudioMixerGroup = sfxGroup;
            return s;
        }
        


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        public void PlayBGM(SO_BGMEvent so_bgmEvent)
        {
            if (Instance == null || so_bgmEvent == null)
            {
                return;
            }
            
            //if playing the same audio, ignore
            if (Instance._currentBgm == so_bgmEvent && Instance._bgmSource.isPlaying)
            {
                return;
            }

            //Save the timestamps of the CURRENTLY playing BGM before it stops
            if (Instance._currentBgm != null && Instance._bgmSource.isPlaying)
            {
                Instance._bgmResumeTimer[Instance._currentBgm] = Instance._bgmSource.time;
            }

            //Set the new current BGM
            Instance._currentBgm = so_bgmEvent;

            if (Instance._bgmFadeCoroutine != null) Instance.StopCoroutine(Instance._bgmFadeCoroutine);
            Instance._bgmFadeCoroutine = Instance.StartCoroutine(Instance.FadeBGM(so_bgmEvent));
        }

        private IEnumerator FadeBGM(SO_BGMEvent bgm)
        {
            float fadeDuration = bgm.fadeDuration > 0 ? bgm.fadeDuration : 1.0f;
            //fade out before fade in
            if (_bgmSource.isPlaying)
            {
                float startVolume = _bgmSource.volume;
                for (float t = 0; t < fadeDuration; t += Time.deltaTime)
                {
                    _bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                    yield return null;
                }
            }

            _bgmSource.clip = bgm.clip;
            _bgmSource.loop = bgm.loop;

            // 3. CHECK FOR SAVED TIME
            if (_bgmResumeTimer.TryGetValue(bgm, out float savedTime))
            {
                _bgmSource.time = savedTime % bgm.clip.length;
            }
            else
            {
                _bgmSource.time = 0;
            }

            _bgmSource.Play();

            // Fade In
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                _bgmSource.volume = Mathf.Lerp(0, bgm.volume, t / fadeDuration);
                yield return null;
            }

            _bgmSource.volume = bgm.volume;
        }

        public void PlaySfx(SO_SFXEvent so, Vector3? position = null)
        {
            if (so == null) 
            {
                Debug.Log("From PlaySfx: data is null");
                return;
            }
            AudioSource source = _sfxPool.Get();
            
            //if you want to make the sound coming from a specific game object, pass the position
            if (position.HasValue)
            {
                source.transform.position = position.Value;
                source.spatialBlend = 1.0f; 
                source.minDistance = so.minDistance;
                source.maxDistance = so.maxDistance;
                source.rolloffMode = AudioRolloffMode.Logarithmic;
            }
            else
            {
                source.spatialBlend = 0.0f; //global sound effect
            }
            AudioClip clipToPlay = so.GetRandomClip();
            source.pitch = Random.Range(so.minPitch, so.maxPitch);
    
            // 4. PlayOneShot requires the clip as a parameter
            source.PlayOneShot(clipToPlay, so.volume);

            // 5. We still need to return the source to the pool when the clip ends!
            StartCoroutine(ReleaseSfxAfterPlaying(source, clipToPlay.length, source.pitch));
        }

        private IEnumerator ReleaseSfxAfterPlaying(AudioSource source, float length, float pitch)
        {
            yield return new WaitForSeconds(length / Mathf.Max(0.01f, pitch));
            _sfxPool.Release(source);
        }

        public void StopBgmMusic()
        {
            if (Instance != null)
            {
                Instance._bgmSource.Stop();
            }
        }

        // public void StopSfx()
        // {
        //     if (Instance != null)
        //     {
        //         Instance._sfxSource.Stop();
        //     }
        // }
        
        //Method to call from UI Sliders
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
    }
}
