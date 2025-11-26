using Ami.BroAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TavernService : MonoBehaviour
{
    [SerializeField] Image backGroundImage;
    [SerializeField] GameObject closeButton;
    [SerializeField] GameObject tavernButtons;
    [SerializeField] SoundID closeShopSound, voicePhrase, openShopPhrase, background, backgroundMusic;
    [SerializeField] int coinsForDrink = 5, rentForRoom = 10, buyFood = 10;
    public UnityEvent exitTavern;


    public void OpenTavern()
    {
        backGroundImage.enabled = true;
        tavernButtons.SetActive(true);
        BroAudio.Play(background);
        closeButton.SetActive(true);
        BroAudio.SetVolume(backgroundMusic, 0.0f, 0.5f);
    }

    public void BuyADrink()
    {
       if(GameInstance.party.SellBuyMoneyCheck(coinsForDrink) >= 0)
        {
            // add gossip tip to journal or buff
        }
    }

    public void RentARoom()
    {
        if (GameInstance.party.SellBuyMoneyCheck(rentForRoom) >= 0)
        {
            //forward time and heal party mass heal
        }
    }

    public void BuyFood()
    {
        if (GameInstance.party.SellBuyMoneyCheck(buyFood) >= 0)
        {
            //add food to inventory
        }
    }

    public void CloseTavern()
    {
        backGroundImage.enabled = false;
        BroAudio.Play(closeShopSound);
        BroAudio.Play(voicePhrase).SetVelocity(4);
        BroAudio.Stop(openShopPhrase);
        tavernButtons.SetActive(false);
        exitTavern.Invoke();
        BroAudio.Stop(background, 0.5f);
        closeButton.SetActive(false);
        GameInstance.playerController.shopIsOpened = false;
        BroAudio.SetVolume(backgroundMusic, 1.0f, 0.5f);
    }

}
