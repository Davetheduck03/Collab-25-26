using Phuc.SoundSystem;
using UnityEngine;

public class PlaySoundButton : MonoBehaviour
{
    private int playingBGM = 0;
    [SerializeField] private SoundManager _soundManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Other script can call this method with a parameter 
    public void OnChangingBGM(BgmSoundType name)
    {
        // if (playingBGM == 0)
        // {
        //     _soundManager.PlayBGM(name);
        // }
        // else if (playingBGM == 1)
        // {
        //     playingBGM = 2;
        //     SoundManager.PlaySound(BGMSoundType.Marketplace_demo3);
        // }
        // else if (playingBGM == 2)
        // {
        //     playingBGM = 0;
        //     SoundManager.PlaySound(BGMSoundType.Reel_em_in);
        // }

        // switch (playingBGM)
        // {
        //     case 0:
        //         _soundManager.PlayBGM(BgmSoundType.Forest_FishyWishy);
        //         playingBGM++;
        //         break;
        //     case 1:
        //         _soundManager.PlayBGM(BgmSoundType.Reel_em_in);
        //         playingBGM++;
        //         break;
        //     case 2:
        //         _soundManager.PlayBGM(BgmSoundType.Reel_em_in);
        //         break;
        // }
        switch (playingBGM)
        {
            case 0:
                // CAN BE CALLED FROM ANY SCRIPTS
                SoundManager.PlayBGM(BgmSoundType.Forest_FishyWishy);
                // ---------------------------------
                playingBGM++;
                break;
            case 1:
                SoundManager.PlayBGM(BgmSoundType.Reel_em_in);
                playingBGM++;
                break;
            case 2:
                SoundManager.PlayBGM(BgmSoundType.Reel_em_in);
                playingBGM = 0;
                break;
        }
    }

    public void TestPlaySFXButton(SfxSoundType type)
    {
        SoundManager.PlaySfx(type);
    }
    // If you want to stop playing 
    public void OnStoppingBGM()
    {
        SoundManager.StopBgmMusic();
    }
    // public void PlaySFX(AudioClip clip)
    // {
    //     
    // }
}
