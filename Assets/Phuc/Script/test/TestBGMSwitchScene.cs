using UnityEngine;
using UnityEngine.SceneManagement;

namespace Phuc.SoundSystem
{
    public class TestBGMSwitchScene : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}