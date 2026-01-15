using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.Events;
using TMPro;

public class TempleServices : MonoBehaviour
{
    [SerializeField] CameraOrder cameraOrder;
    [SerializeField] TextMeshProUGUI textOfShopState;
    [SerializeField] SpellContainer reviveSpell, restorationSpell;


    public UnityEvent closeShopPanel;
    private void Start()
    {

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

        if (GameInstance.party.SellBuyMoneyCheck(1000) >= 0)
        {
            foreach (Hero h in heroes)
            {
                h.ApplySpellToHero(restorationSpell);
                h.ApplySpellToHero(reviveSpell);

            }

            GameInstance.party.MoneyGoes(1000);
        }
    }


    public void CameraOut()
    {
        cameraOrder.BattleLogWithGameplay();
    }
    public void CloseShop()
    {
        closeShopPanel.Invoke();
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        gameObject.SetActive(false);
    }

}
