using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.Events;
using TMPro;

public class TempleServices : MonoBehaviour
{
    [SerializeField] CameraOrder cameraOrder;

    [SerializeField] SpellContainer reviveSpell, restorationSpell;
    [SerializeField] GameObject templeGraphics, shopGraphics, shopButton, healButton, resurrectionButton;
    [SerializeField] int healCost = 100, resurrectionCost = 1000;

    public UnityEvent closeShopPanel;
    private void Start()
    {

    }


    public void initOpenTemple()
    {
        templeGraphics.SetActive(true);
        shopButton.SetActive(true);
        healButton.SetActive(true);
        resurrectionButton.SetActive(true);
        shopGraphics.SetActive(false);
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

                if (h.GetHeroStatus().Contains(GameplayStatus.Petrified))
                {
                
                }
            }

            GameInstance.party.MoneyGoes(100);
        }

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
    }


    public void OpenShop()
    {
        shopButton.SetActive(false);
        healButton.SetActive(false);
        resurrectionButton.SetActive(false);
        shopGraphics.SetActive(true);
    }


    public void CameraOut()
    {
        cameraOrder.BattleLogWithGameplay();
    }



    public void CloseShop()
    {
        if (!shopButton.activeSelf)
        {
            shopButton.SetActive(true);
            healButton.SetActive(true);
            resurrectionButton.SetActive(true);
            shopGraphics.SetActive(false);
            return;
        }

        templeGraphics.SetActive(false);
        closeShopPanel.Invoke();
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        //gameObject.SetActive(false);
    }

}
