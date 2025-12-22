using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ami.BroAudio;

public class ChooseShop : MonoBehaviour
{
    [SerializeField] List<GameObject> shopToChoose = new List<GameObject>();
    [SerializeField] List<GameObject> buttonPanels = new List<GameObject>();
    [SerializeField] CameraOrder cameraUI;
    [SerializeField] SoundID openShopSound = default, voicePhrase = default, closeShopPhrase = default;
    


    public UnityEvent switchOnPanel;

    public void ChooseShopOfType(int index)
    {

        foreach (GameObject g in shopToChoose)
        {
            g.SetActive(false);

        }
        cameraUI.ShopWithoutBattlelog();
        //switchOnPanel.Invoke();
        shopToChoose[index].SetActive(true);
        buttonPanels[index].SetActive(true);



        if (shopToChoose[index].GetComponent<ItemShop>() != null)
        {
            BroAudio.Stop(closeShopPhrase);
            BroAudio.Play(openShopSound);
            BroAudio.Play(voicePhrase);
            shopToChoose[index].GetComponent<ItemShop>().OpenShop();
            shopToChoose[index].GetComponent<ItemShop>().PlayerCoins( (GameInstance.party.SellBuyMoneyCheck(0)));
        }

        if (shopToChoose[index].GetComponent<SpellShop>() != null) 
        {
            BroAudio.Stop(closeShopPhrase);
            BroAudio.Play(openShopSound);
            BroAudio.Play(voicePhrase);
            shopToChoose[index].GetComponent<SpellShop>().OpenSpellShop();
            shopToChoose[index].GetComponent<SpellShop>().PlayerCoins((GameInstance.party.SellBuyMoneyCheck(0)));
        }

        if(shopToChoose[index].GetComponent<TavernService>() != null)
        {
            BroAudio.Stop(closeShopPhrase);
            BroAudio.Play(openShopSound);
            BroAudio.Play(voicePhrase);
            shopToChoose[index].GetComponent<TavernService>().OpenTavern();

        }

        if (shopToChoose[index].GetComponent<TrainingShop>() != null)
        {
            BroAudio.Stop(closeShopPhrase);
            BroAudio.Play(openShopSound);
            BroAudio.Play(voicePhrase);
            shopToChoose[index].GetComponent<TrainingShop>().OpenTrainingShop();

        }

    }



}
