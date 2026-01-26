
using UnityEngine;
using UnityEngine.UI;
using Ami.BroAudio;

public class OptionsMainMenu : MonoBehaviour
{
    [SerializeField] Slider musicSlider, sfxSlider;
    [SerializeField] GameObject optionsMenu;
    [SerializeField] BroAudioType musicType = default, sfxType = default;
    [SerializeField] SoundID mainTheme = default;
    float musicVol = 1f, sfxVol = 1f;
    private void Start()
    {
        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
        GameInstance.LoadOptionsSaved();
        SaveOptionsData saveOptions = GameInstance.GetSaveOptionsData();
        if(saveOptions != null)
        {
           // print(saveOptions + " "+ saveOptions.musicVolume);
            BroAudio.SetVolume(BroAudioType.Music, saveOptions.musicVolume);
            BroAudio.SetVolume(BroAudioType.SFX, saveOptions.sfxVolume);
            musicSlider.value = saveOptions.musicVolume;
            sfxSlider.value = saveOptions.sfxVolume;

        }
        this.gameObject.SetActive(false);
        BroAudio.Play(mainTheme, 0.5f);

    }
    public void ChangeMusicVolume(float vol)
    {
        musicVol = vol;
        BroAudio.SetVolume(musicType, vol);
    }
    public void ChangeSFXVolume(float vol)
    {
        sfxVol = vol;
        BroAudio.SetVolume(sfxType, vol);
    }


    public void BackToMainMenu()
    {
        SaveOptionsData newOptionsData = new SaveOptionsData();
        newOptionsData.musicVolume = musicVol;
        newOptionsData.sfxVolume = sfxVol;
        GameInstance.OptionsDataSaver(newOptionsData);
        //print("saveing music " + newOptionsData.musicVolume);

        this.gameObject.SetActive(false);

    }

}
