
using Ami.BroAudio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;


public class OptionsMenu : MonoBehaviour
{
    [SerializeField] CameraOrder cameraOrder;
    [SerializeField] GameObject mainMenu, audioOptions;
    [SerializeField] Slider musicSlider, sfxSlider, uiSlider, moveSlider, rotateSlider;
    [SerializeField] BroAudioType musicType = BroAudioType.Music, sfxType = BroAudioType.SFX, uiType=BroAudioType.UI;
    [SerializeField] AudioMixerGroup sfxgroup;
    float musicVol, sfxVol, uiVol;

    private void Start()
    {
        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
        uiSlider.onValueChanged.AddListener(ChangeUIVolume);
        moveSlider.onValueChanged.AddListener(ChangePlayerMovementSpeed);
        rotateSlider.onValueChanged.AddListener(ChangePlayerRotationSpeed);
        GameInstance.LoadOptionsSaved();
        SaveOptionsData saveOptions = GameInstance.GetSaveOptionsData();
        if (saveOptions != null)
        {
            // print(saveOptions + " "+ saveOptions.musicVolume);
            BroAudio.SetVolume(BroAudioType.Music, saveOptions.musicVolume);
            BroAudio.SetVolume(BroAudioType.SFX, saveOptions.sfxVolume);
            GameInstance.playerController.SetPlayerSpeed(saveOptions.moveSpeed);
                GameInstance.playerController.SetPlayerRotationSpeed(saveOptions.rotationSpeed);
            

            musicSlider.value = saveOptions.musicVolume;
            sfxSlider.value = saveOptions.sfxVolume;
            uiSlider.value = saveOptions.uiVolume;
            moveSlider.value = saveOptions.moveSpeed;
            rotateSlider.value = saveOptions.rotationSpeed;


            sfxgroup.audioMixer.SetFloat("Volume", (saveOptions.sfxVolume - 1) * 80);
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
            newOptionsData.uiVolume = uiVol;
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
        SaveOptionsData newOptionsData = new SaveOptionsData();
        newOptionsData.musicVolume = musicVol;
        newOptionsData.sfxVolume = sfxVol;
        newOptionsData.uiVolume = uiVol;
        newOptionsData.moveSpeed = GameInstance.playerController.GetPlayerSpeed();
        newOptionsData.rotationSpeed = GameInstance.playerController.GetPlayerRotationSpeed();

        GameInstance.OptionsDataSaver(newOptionsData);
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
        sfxgroup.audioMixer.SetFloat("Volume", (vol-1) * 80);
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

    public void QuitGame()
    {
        Application.Quit();
    }

}
