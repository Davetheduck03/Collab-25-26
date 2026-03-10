using UnityEngine;
using System;

namespace Phuc.Augments
{
    public class AugmentUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _augmentPanel;
        public static event Action OnEventOpenAugmentSelection;

        private void OnEnable() => LevelUp.OnEventLevelUp += ActivateAugmentUI;
        private void OnDisable() => LevelUp.OnEventLevelUp -= ActivateAugmentUI;

        private void ActivateAugmentUI()
        {
            _augmentPanel.SetActive(true);
            Time.timeScale = 0; // Pause Game
            OnEventOpenAugmentSelection?.Invoke();
        }
    }
}