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
    public void GetItemFromEquipmentSlot(HeroInventoryItem heroInventoryItem, ItemType itemType)
    {
        if (heroInventoryItem == null) 
        { 
            activeHero.RemoveItemFromEquipment(itemType); 
            GameInstance.inventory.UpdatePartyWeight();
            RefreshUI.Invoke();
            return; 
        }
        if (heroInventoryItem.container != null)
        {
            activeHero.AddEquipmentToCharacter(heroInventoryItem);
            GameInstance.SaveItemState(heroInventoryItem._GUID, SavedState.Equipment, heroInventoryItem);
        }
        else
        {
            activeHero.RemoveItemFromEquipment(heroInventoryItem.itemType);
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
        GameInstance.equipmentHeroesSavedWithGUID.Clear();
        for (int i=0; i < heroes.Count; i++)
        {
            foreach(KeyValuePair<ItemType, HeroInventoryItem> he in heroes[i].equipmentWithGUID)
            {
                GameInstance.equipmentHeroesSavedWithGUID.Add(he.Value);
            }
        }
        GameInstance.AddReplacedInventory();
    }

    public void LoadEquipment()
    {
        //print(" equipment storage = " + GameInstance.equipmentHeroesSavedWithGUID.Count);
        foreach(HeroInventoryItem he in GameInstance.equipmentHeroesSavedWithGUID)
        {
/*            print(" loading equipment " + he.container + " " + he._GUID + " " + he.heroIndex);*/
            if (he == null) continue;
            if (he.container != null) 
            {
                print(" loading equipment "+ he.container + " "+ he._GUID+" "+ he.heroIndex);
                if (he.heroIndex >=0 ) heroes[he.heroIndex].AddEquipmentToCharacter(he); 
            }
        }

        
    }
}
