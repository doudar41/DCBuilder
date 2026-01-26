
using Ami.BroAudio;
using UnityEngine;
using UnityEngine.UI;


public class OptionsMenu : MonoBehaviour
{
    [SerializeField] CameraOrder cameraOrder;
    [SerializeField] GameObject mainMenu, audioOptions;
    [SerializeField] Slider musicSlider, sfxSlider;
    [SerializeField] BroAudioType musicType = default, sfxType = default;
    float musicVol, sfxVol;

    private void Start()
    {
        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);

        GameInstance.LoadOptionsSaved();
        SaveOptionsData saveOptions = GameInstance.GetSaveOptionsData();
        if (saveOptions != null)
        {
            // print(saveOptions + " "+ saveOptions.musicVolume);
            BroAudio.SetVolume(BroAudioType.Music, saveOptions.musicVolume);
            BroAudio.SetVolume(BroAudioType.SFX, saveOptions.sfxVolume);
            musicSlider.value = saveOptions.musicVolume;
            sfxSlider.value = saveOptions.sfxVolume;

        }
    }

    public void BackToMainMenu()
    {
        GameInstance.LoadGameMainMenu();
    }


    public void OpenInGameMenu(bool onOff)
    {
        if (onOff)
        {
            cameraOrder.ShopWithoutBattlelog();
            mainMenu.SetActive(true);
        }
        else
        {
            SaveOptionsData newOptionsData = new SaveOptionsData();
            newOptionsData.musicVolume = musicVol;
            newOptionsData.sfxVolume = sfxVol;
            GameInstance.OptionsDataSaver(newOptionsData);

            cameraOrder.BattleLogWithGameplay();
            mainMenu.SetActive(false);
        }
    }


    public void OpenAudioOptions()
    {
        audioOptions.SetActive(true);
    }

    public void BackToMainOptionsMenu()
    {
        audioOptions.SetActive(false);
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
    public void QuitGame()
    {
        Application.Quit();
    }

}
