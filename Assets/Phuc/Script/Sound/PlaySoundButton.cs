using Phuc.SoundSystem;
using UnityEngine;

public class PlaySoundButton : MonoBehaviour
{
    private int playingBGM = 0;
    private int playingSFX = 0;
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
    public void OnChangingBGM()
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
                SoundManager.PlayBGM(BgmSoundType.Marketplace_demo3);
                playingBGM = 0;
                break;
        }
    }

    public void TestPlaySFXButton()
    {
        // switch (playingBGM)
        // {
        //     case 0:
        //         // CAN BE CALLED FROM ANY SCRIPTS
        //         SoundManager.PlaySfx(SfxSoundType.Boat);
        //         // ---------------------------------
        //         playingSFX++;
        //         break;
        //     case 1:
        //         SoundManager.PlaySfx(SfxSoundType.Village_running);
        //         playingSFX++;
        //         break;
        //     case 2:
        //         SoundManager.PlaySfx(SfxSoundType.Rod_casted);
        //         playingSFX = 0;
        //         break;
        // }
        SoundManager.PlaySfx(SfxSoundType.EXPERIMENTAL);
    }
    // If you want to stop playing 
    public void OnStoppingBGM()
    {
        SoundManager.StopBgmMusic();
    }

    public void OnStoppingSfx()
    {
        SoundManager.StopSfx();
    }
    // public void PlaySFX(AudioClip clip)
    // {
    //     
    // }
}
