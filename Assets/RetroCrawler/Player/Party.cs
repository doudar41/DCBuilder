using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Party : MonoBehaviour
{
    [SerializeField] List<Hero> heroes = new List<Hero>();

    public IHero activeHero;
    public UnityEvent RefreshUI;



    private void OnEnable()
    {
        GameInstance.party = this;
    }
    private void Start()
    {
        SetActiveHero(heroes[0]);
        StartCoroutine(GameInstance.TimeStep());
        SetTimerForHeroes(false);

    }

    public  void SetTimerForHeroes(bool battleOnOf)
    {
        foreach (Hero h in heroes)
        {
            h.SetBattleTimeOnOff(battleOnOf);
        }
    }

    private void OnDestroy()
    {

    }

    public List<Hero> GetPartyMembers()
    {
        return heroes;
    }

    public void SetActiveHero(Hero hero)
    {
        if (GameInstance.spellbook.SpellWaiting()) return;
        foreach(Hero h in heroes)
        {
            if (hero == h) { 
                h.MakeHeroActive(true);
                activeHero = h.GetComponent<IHero>();
                GameInstance.spellbook.GetPagesReady();
                GameInstance.inventory.GetEquipmentFromHero(activeHero.GetHeroEquipment());
                RefreshUI.Invoke();
            }
            else h.MakeHeroActive(false);
        }
    }
    public void GetItemFromEquipmentSlot(ItemType itemType, ItemScriptableContainer item)
    {
        if (item != null)
        {
            activeHero.AddEquipmentToCharacter(item.itemType, item);
        }
        else
        {
            activeHero.RemoveItemFromEquipment(itemType);
        }

        GameInstance.inventory.UpdatePartyWeight();
        RefreshUI.Invoke();
    }


    public void heroEquipmentToInventory()
    {
        if (GameInstance.inventory != null)
        {
            GameInstance.inventory.GetEquipmentFromHero(activeHero.GetHeroEquipment());
        }
        RefreshUI.Invoke();
    }

    public List<Hero> GetHeroList()
    {
        return heroes;
    }
    public void BattleHeroSwitch(Hero hero)
    {
        for (int i = 0; i < heroes.Count; i++)
        {
            if (heroes[i] == hero)
            {
                SetActiveHero(hero);
            }
        }
    }

    public void SaveEquipment()
    {
        for(int i=0; i < heroes.Count; i++)
        {
            //print("saved");
            GameInstance.equipmentHeroesSaved[i] = heroes[i].equipment;
        }
    }

    public void LoadEquipment()
    {
        for (int i = 0; i < heroes.Count; i++)
        {
            foreach(ItemType itype in System.Enum.GetValues(typeof(ItemType)))
            {
                if (GameInstance.equipmentHeroesSaved.ContainsKey(i)) 
                {
                    if (GameInstance.equipmentHeroesSaved[i].ContainsKey(itype))
                    {
                        heroes[i].AddEquipmentToCharacter(itype, GameInstance.equipmentHeroesSaved[i][itype]);
                    }

                }
            }

        }
    }
}
