using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Phuc.SoundSystem
{
    public class SceneBGMController : MonoBehaviour
    {
        [System.Serializable]
        public class SceneMusic
        {
            public string sceneName;
            public SO_BGMEvent sceneMusic;
        }

        public static SceneBGMController Instance;

        [Header("Scene Music")] public List<SceneMusic> musicsList;

        private string lastSceneName;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);

            }
        }

        private void Start()
        {
            PlayMusicForScene(SceneManager.GetActiveScene().name);
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == lastSceneName)
            {
                //PREVENTS PLAYING THE SAME BGM
                return;
            }
            else
            {
                PlayMusicForScene(scene.name);
                // Debug.Log("reached");
                lastSceneName = scene.name;
            }
        }

        private void PlayMusicForScene(string sceneName)
        {
            foreach (var m in musicsList)
            {
                if (m.sceneName == sceneName)
                {
                    if (m.sceneMusic != null)
                    {
                        m.sceneMusic.Play();
                        // Debug.Log($"Playing BGM for: {sceneName}");
                    }
                    return;
                }
            }
            // Debug.LogWarning($"No music found for scene: {sceneName}");
        }
        
    }
}