using Ami.BroAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class ChooseShop : MonoBehaviour
{
    [SerializeField] List<GameObject> shopToChoose = new List<GameObject>();
    [SerializeField] List<GameObject> buttonPanels = new List<GameObject>();
    [SerializeField] CameraOrder cameraUI;
    [SerializeField] SoundID openShopSound = default, voicePhrase = default, closeShopPhrase = default;
    bool isNight = false;
    int refreshSellsTime = -100;

    public UnityEvent switchOnPanel;

    private void Awake()
    {
        GameInstance.progress += NightClosed;
    }

    private void OnDestroy()
    {
        GameInstance.progress -= NightClosed;
    }

    public void ChooseShopOfType(int index)
    {

        foreach (GameObject g in shopToChoose)
        {
            g.SetActive(false);
        }
        if (GameInstance.GetUnformattedTime() > refreshSellsTime)
        {


            foreach (GameObject g in shopToChoose)
            {
                if (g.GetComponent<ItemShop>() != null)
                {
                    g.SetActive(true);
                    g.GetComponent<ItemShop>().NewItemsToSell();
                    g.SetActive(false);
                }
                if (g.GetComponent<SpellShop>() != null)
                {
                    g.GetComponent<SpellShop>().RefreshSoldSpells();
                }
            }
            refreshSellsTime = GameInstance.GetUnformattedTime() + 1440;
        }
        cameraUI.ShopWithoutBattlelog();
        shopToChoose[index].SetActive(true);
        buttonPanels[index].SetActive(true);




        if (shopToChoose[index].GetComponent<ItemShop>() != null)
        {
            if(isNight)
            {
                //print("shop closed for night");
                GameInstance.spellbook.BattleLogMessage(new List<string>() {"This shop is closed till 6.00 AM"}, null);
                shopToChoose[index].SetActive(false);
                buttonPanels[index].SetActive(false);
                cameraUI.BattleLogWithGameplay();
                GameInstance.playerController.shopIsOpened = false;

                return;
            }
            BroAudio.Stop(closeShopPhrase);
            BroAudio.Play(openShopSound);
            BroAudio.Play(voicePhrase);
            shopToChoose[index].GetComponent<ItemShop>().OpenShop();
            shopToChoose[index].GetComponent<ItemShop>().PlayerCoins( (GameInstance.party.SellBuyMoneyCheck(0)));
            shopToChoose[index].GetComponent<animateUIImage>().StartAnimation();
        }

        if (shopToChoose[index].GetComponent<SpellShop>() != null) 
        {
            BroAudio.Stop(closeShopPhrase);
            BroAudio.Play(openShopSound);
            //BroAudio.Play(voicePhrase);
            shopToChoose[index].GetComponent<SpellShop>().OpenSpellShop();
            shopToChoose[index].GetComponent<SpellShop>().PlayerGems((GameInstance.party.SellBuyMoneyCheck(0)));
            shopToChoose[index].GetComponent<animateUIImage>().StartAnimation();
        }


        if(shopToChoose[index].GetComponent<TavernService>() != null)
        {
            BroAudio.Stop(closeShopPhrase);
            BroAudio.Play(openShopSound);
            //BroAudio.Play(voicePhrase);
            shopToChoose[index].GetComponent<TavernService>().OpenTavern();
            shopToChoose[index].GetComponent<animateUIImage>().StartAnimation();

        }


        if (shopToChoose[index].GetComponent<TrainingShop>() != null)
        {
            BroAudio.Stop(closeShopPhrase);
            BroAudio.Play(openShopSound);
            //BroAudio.Play(voicePhrase);
            shopToChoose[index].GetComponent<TrainingShop>().OpenTrainingShop();

        }

        if (shopToChoose[index].GetComponent<TempleServices>() != null)
        {
            BroAudio.Play(openShopSound);
            shopToChoose[index].GetComponent<TempleServices>().initOpenTemple();
        }

    }

    void NightClosed(int count)
    {
        //print(GameInstance.GetNormalTime()[1].ToString() + ":" + GameInstance.GetNormalTime()[2].ToString()+":"+GameInstance.GetNormalTime()[3].ToString());
        if (GameInstance.GetNormalTime()[1]%24 >= 6 && GameInstance.GetNormalTime()[1]%24 < 20)
        {
            isNight = false;
        }
        else
        {
            isNight = true;

        }
    }


}
