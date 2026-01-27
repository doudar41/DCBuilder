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

    //bool isNight = false;
    int refreshSellsTime = -100;

    public UnityEvent switchOnPanel;


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
            if(GameInstance.dayNightChange.isNight)
            {
                //print("shop closed for night");
                GameInstance.spellbook.BattleLogMessage(new List<string>() {"This shop is closed till 6.00 AM"}, null);
                shopToChoose[index].SetActive(false);
                buttonPanels[index].SetActive(false);
                cameraUI.BattleLogWithGameplay();
                GameInstance.playerController.shopIsOpened = false;
                return;
            }

            shopToChoose[index].GetComponent<ItemShop>().OpenShop();
            shopToChoose[index].GetComponent<ItemShop>().PlayerCoins( (GameInstance.party.SellBuyMoneyCheck(0)));
            shopToChoose[index].GetComponent<animateUIImage>().StartAnimation();
        }

        if (shopToChoose[index].GetComponent<SpellShop>() != null) 
        {
            shopToChoose[index].GetComponent<SpellShop>().OpenSpellShop();
            shopToChoose[index].GetComponent<SpellShop>().PlayerGems((GameInstance.party.SellBuyMoneyCheck(0)));
            shopToChoose[index].GetComponent<animateUIImage>().StartAnimation();
        }


        if(shopToChoose[index].GetComponent<TavernService>() != null)
        {
            shopToChoose[index].GetComponent<TavernService>().OpenTavern();
            shopToChoose[index].GetComponent<animateUIImage>().StartAnimation();
        }


        if (shopToChoose[index].GetComponent<TrainingShop>() != null)
        {
            shopToChoose[index].GetComponent<TrainingShop>().OpenTrainingShop();
        }

        if (shopToChoose[index].GetComponent<TempleServices>() != null)
        {
            shopToChoose[index].GetComponent<TempleServices>().initOpenTemple();
        }

    }




}
