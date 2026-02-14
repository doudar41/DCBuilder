using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.Events;
using TMPro;

public class TempleServices : MonoBehaviour
{
    [SerializeField] CameraOrder cameraOrder;

    [SerializeField] SpellContainer reviveSpell, restorationSpell;
    [SerializeField] GameObject templeGraphics, shopGraphics, serviceButton, shopButton, healButton, resurrectionButton, heroesMoney, serviceOffer;
    [SerializeField] int healCost = 100, resurrectionCost = 1000;
    [SerializeField] List<TextMeshProUGUI> heroesCoinsText;


    public UnityEvent closeShopPanel;

    private void Start()
    {

    }


    public void initOpenTemple()
    {
        templeGraphics.SetActive(true);
        shopButton.SetActive(true);
        healButton.SetActive(false);
        resurrectionButton.SetActive(false);
        shopGraphics.SetActive(false);
        GetComponent<animateUIImage>().StartAnimation();
        GameInstance.soundManagerInGame.DuckExploreMusicSwitchToAmbience(RoomSpaces.TempleShop);
        heroesMoney.SetActive(true);
        serviceOffer.SetActive(true);
        GetPlayersCoins();
    }


    public void ShowServiceOffer(string desc)
    {
        serviceOffer.GetComponent<TextMeshProUGUI>().text = desc;
    }



    public void GetPlayersCoins()
    {
        var money = GameInstance.party.GetCoinsForUI();
        for (int i = 0; i < money.Count; i++)
        {
            heroesCoinsText[i].text = money[i].ToString();
        }
    }

    public void HealAllParty()
    {
        List<Hero> heroes= GameInstance.party.GetHeroList();

        if (GameInstance.party.SellBuyMoneyCheck(healCost) >= 0)
        {
            foreach (Hero h in heroes)
            {
               int currentHealth =  h.GetHeroHealth();
                if (currentHealth > 0)
                {
                    h.HealthDecrease(-(currentHealth + h.GetMaxDependedStat(DependedStat.maxHealth)));
                }

                if (h.GetHeroStatus().Contains(GameplayStates.Petrified))
                {
                
                }
            }

            GameInstance.party.MoneyGoes(100);
        }
        GetPlayersCoins();
    }


    public void Resurrection()
    {
        List<Hero> heroes = GameInstance.party.GetHeroList();

        if (GameInstance.party.SellBuyMoneyCheck(resurrectionCost) >= 0)
        {
            foreach (Hero h in heroes)
            {
                h.ApplySpellToHero(restorationSpell);
                h.ApplySpellToHero(reviveSpell);

            }

            GameInstance.party.MoneyGoes(1000);
        }
        GetPlayersCoins();
    }


    public void OpenShop()
    {
        shopButton.SetActive(false);
        healButton.SetActive(false);
        resurrectionButton.SetActive(false);
        shopGraphics.SetActive(true);
        serviceButton.SetActive(false);
    }


    public void CameraOut()
    {
        cameraOrder.BattleLogWithGameplay();
    }

    public void OpenServices()
    {
        shopButton.SetActive(false);
        healButton.SetActive(true);
        resurrectionButton.SetActive(true);
        shopGraphics.SetActive(false);
        serviceButton.SetActive(false);
    }


    public void CloseShop()
    {
        if (shopGraphics.activeSelf)
        {
            shopButton.SetActive(true);
            healButton.SetActive(false);
            resurrectionButton.SetActive(false);
            serviceButton.SetActive(true);
            shopGraphics.SetActive(false);
            return;
        }
        if (healButton.activeSelf) 
        {
            shopButton.SetActive(true);
            healButton.SetActive(false);
            resurrectionButton.SetActive(false);
            serviceButton.SetActive(true);
            shopGraphics.SetActive(false);
            return;
        }

        templeGraphics.SetActive(false);
        closeShopPanel.Invoke();
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        GameInstance.soundManagerInGame.UnDuckExploreMusicSwitchToAmbience();
        GetComponent<animateUIImage>().StopAnimation();

    }

}
