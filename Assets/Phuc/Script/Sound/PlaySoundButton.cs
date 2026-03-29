using Phuc.SoundSystem;
using UnityEngine;

namespace Phuc.SoundSystem
{
    public class PlaySoundButton : MonoBehaviour
    {

        [Header("Audio location test")] 
        [SerializeField] private Vector2 GameObject_position = new Vector2();

        // Drag your ScriptableObject assets into these slots in the Inspector
        [Header("BGM Library")] [SerializeField]
        private SO_BGMEvent forestMusic;

        [SerializeField] private SO_BGMEvent marketMusic;

        [Header("SFX Library")] [SerializeField]
        private SO_SFXEvent clickSfx;

        private int _bgmToggle = 0;

        public void OnChangingBGM()
        {
            // Use the Instance to call the methods
            if (_bgmToggle == 0)
            {
                SoundManager.Instance.PlayBGM(forestMusic);
                _bgmToggle = 1;
            }
            else
            {
                SoundManager.Instance.PlayBGM(marketMusic);
                _bgmToggle = 0;
            }
        }

        public void TestPlaySFXButton()
        {
            // Play as 2D (Global/UI)
            SoundManager.Instance.PlaySfx(clickSfx);

            // OR Play as 3D (at this button's position)

        }

        public void TestPlaySFXButtonWithPosition()
        {
            SoundManager.Instance.PlaySfx(clickSfx, GameObject_position);
        }

        public void OnStoppingBGM()
        {
            SoundManager.Instance.StopBgmMusic();
        }
    }
}