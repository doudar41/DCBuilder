
using UnityEngine;
using UnityEngine.UI;
using Ami.BroAudio;

public class OptionsMainMenu : MonoBehaviour
{
    [SerializeField] Slider musicSlider, sfxSlider, uiSlider;

    [SerializeField] BroAudioType musicType = default, sfxType = default, uiType = default;
    [SerializeField] SoundID mainTheme = default;
    float musicVol = 1f, sfxVol = 1f, uiVol = 1f;
    private void Start()
    {
        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
        uiSlider.onValueChanged.AddListener(ChangeUIVolume);
        GameInstance.LoadOptionsSaved();
        SaveOptionsData saveOptions = GameInstance.GetSaveOptionsData();
        if(saveOptions != null)
        {
           // print(saveOptions + " "+ saveOptions.musicVolume);
            BroAudio.SetVolume(BroAudioType.Music, saveOptions.musicVolume);
            BroAudio.SetVolume(BroAudioType.SFX, saveOptions.sfxVolume);
            musicSlider.value = saveOptions.musicVolume;
            sfxSlider.value = saveOptions.sfxVolume;
            uiSlider.value = saveOptions.uiVolume;


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
    public void ChangeUIVolume(float vol)
    {
        uiVol = vol;
        BroAudio.SetVolume(uiType, vol);
    }

    public void ChangePlayerMovementSpeed(float speed)
    {
        GameInstance.playerController.SetPlayerSpeed(speed);
    }

    public void ChangePlayerRotationSpeed(float speed)
    {
        GameInstance.playerController.SetPlayerRotationSpeed(speed);
    }


    public void BackToMainMenu()
    {
        SaveOptionsData newOptionsData = new SaveOptionsData();
        newOptionsData.musicVolume = musicVol;
        newOptionsData.sfxVolume = sfxVol;
        newOptionsData.uiVolume = uiVol;
        newOptionsData.moveSpeed = GameInstance.playerController.GetPlayerSpeed();
        newOptionsData.rotationSpeed = GameInstance.playerController.GetPlayerRotationSpeed();
        GameInstance.OptionsDataSaver(newOptionsData);

        this.gameObject.SetActive(false);

    }

}
