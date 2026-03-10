using Phuc.SoundSystem;
using UnityEngine;

public class PlaySoundButton : MonoBehaviour
{
    private int playingBGM = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangingBGM()
    {
        if (playingBGM == 0)
        {
            playingBGM = 1;
            SoundManager.PlaySound(BGMSoundType.Forest_FishyWishy);
        }
        else if (playingBGM == 1)
        {
            playingBGM = 2;
            SoundManager.PlaySound(BGMSoundType.Marketplace_demo3);
        }
        else if (playingBGM == 2)
        {
            playingBGM = 0;
            SoundManager.PlaySound(BGMSoundType.Reel_em_in);
        }
    }

    public void OnStoppingBGM()
    {
        SoundManager.StopSound();
    }
    // public void PlaySFX(AudioClip clip)
    // {
    //     
    // }
}
