using UnityEngine;
using UnityEngine.UI;
using Phuc.SoundSystem;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        // DO NOT SET MINVALUE TO 0
        bgmSlider.minValue = 0.0001f;
        bgmSlider.maxValue = 1f;
        sfxSlider.minValue = 0.0001f;
        sfxSlider.maxValue = 1f;

        bgmSlider.onValueChanged.AddListener(delegate { OnBGMChanged(); });
        sfxSlider.onValueChanged.AddListener(delegate { OnSFXChanged(); });
    }

    public void OnBGMChanged()
    {
        // This calls the "Industry Standard" Log10 method we wrote
        SoundManager.Instance.SetBGMVolume(bgmSlider.value);
    }

    public void OnSFXChanged()
    {
        SoundManager.Instance.SetSFXVolume(sfxSlider.value);
    }
}