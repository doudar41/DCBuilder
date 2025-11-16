using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.Events;
using TMPro;

public class TempleServices : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] TextMeshProUGUI textOfShopState;
    


    public UnityEvent closeShopPanel;
    private void Start()
    {
        //cam.depth = 2;

        textOfShopState.text = "Temple Services";
    }

    public void HealAllParty()
    {
        List<Hero> heroes= GameInstance.party.GetHeroList();

        if (GameInstance.party.SellBuyMoneyCheck(100) >= 0)
        {
            foreach (Hero h in heroes)
            {
               int currentHealth =  h.GetHeroHealth();
                if (currentHealth > 0)
                {
                    h.HealthDecrease(-(currentHealth + h.GetDependedStat(DependedStat.maxHealth)));
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

        if (GameInstance.party.SellBuyMoneyCheck(1000) >= 0)
        {
            foreach (Hero h in heroes)
            {
                int currentHealth = h.GetHeroHealth();
                if (currentHealth <= 0)
                {
                    h.HealthDecrease(-(currentHealth + h.GetDependedStat(DependedStat.maxHealth)));
                }


            }

            GameInstance.party.MoneyGoes(1000);
        }
    }
    public void CameraOut()
    {
        cam.depth = -2;
    }
    public void CloseShop()
    {
        closeShopPanel.Invoke();
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        gameObject.SetActive(false);
    }

}
