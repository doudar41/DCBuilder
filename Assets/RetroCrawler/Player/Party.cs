using Ami.BroAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Party : MonoBehaviour
{
    [SerializeField] List<Hero> heroes = new List<Hero>();

    public IHero activeHero;
    public UnityEvent RefreshUI;
    public UnityEvent SwitchHeroTraining;

    int moneyCollected = 0;
    int gemsCollected = 0;

    public List<UniqueDialogueName> currentUniqueDialogueNames = new List<UniqueDialogueName>(); 
    int partyLevel = 1;

    [SerializeField] int partyFood = 0; 

    [SerializeField] int experienceToNextLevel = 100;
    [SerializeField] float experienceCoeficient = 1.3f;
    [SerializeField] SoundID moneyGoesSound, gemsGoesSound;
    int currentexp = 0;
    int timeToLevelUp = 0;

    public int addExperiencePoints(int exp)
    {
        currentexp += exp;
        //print("party exp " + currentexp);
        if (currentexp >= experienceToNextLevel)
        {
            //print("next level");
            experienceToNextLevel = (int)(Mathf.Pow(experienceToNextLevel, experienceCoeficient));
            partyLevel++;
            foreach (var hero in heroes)
            {
                hero.AddExtraSkillPoints(10);
            }

            timeToLevelUp++;

            GameInstance.spellbook.battlelogEvent.Invoke(new List<string>() { "party level up " },
                                                            new List<ResultMsg> { new ResultMsg { msgType = "i", msgInt = partyLevel } } );
        }
        if(timeToLevelUp> 10000) return 0;

        if (currentexp > experienceToNextLevel) addExperiencePoints(0); 

        return currentexp;
    }

    public int GetPartyLevel()
    {
        return partyLevel;
    }

    private void OnEnable()
    {
        GameInstance.party = this;
    }

    private void Start()
    {

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

    public List<Hero> GetPartyMembers()  {  return heroes;  }

    public int CheckFoodSupply(int amount) { return partyFood - amount; }

    public void FoodGoes(int amount) { partyFood -= amount; if (partyFood < 0) partyFood = 0; }

    public void PartyHeroInit()
    {
        heroes[0].MakeHeroActive(true);
        activeHero = heroes[0].GetComponent<IHero>();
        GameInstance.spellbook.GetPagesReady();
        //RefreshUI.Invoke();
    }


    public void SetActiveHero(Hero hero)
    {
        if (GameInstance.spellbook.SpellWaiting()) return;
        if(hero == null)
        {
            hero = heroes[0];
        }
        foreach(Hero h in heroes)
        {
            if (hero == h) {
                h.MakeHeroActive(true);
                activeHero = h.GetComponent<IHero>();
                GameInstance.spellbook.GetPagesReady();
                GameInstance.inventory.GetEquipmentFromHero(activeHero.GetHeroEquipment());
                RefreshUI.Invoke();
                SwitchHeroTraining.Invoke();
            }
            else h.MakeHeroActive(false);
        }
    }

    public void GetItemFromEquipmentSlot(HeroInventoryItem heroInventoryItem, ItemType itemType)
    {
        if (heroInventoryItem == null)
        {
            activeHero.RemoveItemFromEquipment(itemType);
            RefreshUI.Invoke();
            return;
        }

        if (heroInventoryItem.container != -1)
        {
            activeHero.AddEquipmentToCharacter(heroInventoryItem);
           // GameInstance.SaveItemState(heroInventoryItem._GUID, SavedState.Equipment, heroInventoryItem);
        }
        else
        {
            activeHero.RemoveItemFromEquipment(itemType);
        }
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
                if (he.Value != null)
                { 
                    GameInstance.equipmentHeroesSavedWithGUID.Add(he.Value);
                    print("preloaded items "+ he.Value.container+" hero "+ he.Value.heroIndex);
                }
            }
        }
        //GameInstance.AddReplacedInventory();
    }

    public void LoadEquipment()
    {
        //print(" equipment storage = " + GameInstance.equipmentHeroesSavedWithGUID.Count);
        foreach(HeroInventoryItem he in GameInstance.equipmentHeroesSavedWithGUID)
        {
            print(" loading equipment " + he.container + " " +  " " + he.heroIndex);
            if (he == null) continue;
            if (he.container != -1) 
            {
                print(" loading equipment "+ he.container + " "+" "+ he.heroIndex);
                if (he.heroIndex >=0 ) heroes[he.heroIndex].AddEquipmentToCharacter(he); 
            }
        }
    }


    public void SaveHeroesSpells()
    {
        foreach(Hero h in heroes)
        {
            HeroSpellbookSaved heroSpellbookSaved = new HeroSpellbookSaved();
            heroSpellbookSaved.heroIndex = h.GetHeroIndex();
            heroSpellbookSaved.spells = h.GetActiveHeroSpellbook();
            GameInstance.spellbooksSaved.Add(heroSpellbookSaved);
        }
    }


    public int SellBuyMoneyCheck(int amount)
    {
        return moneyCollected - amount;
    }


    public int CheckGemsAmountForSell(int amount)
    {
        return gemsCollected - amount;
    }

    public void MoneyGoes(int amount)
    {
        moneyCollected -= amount; 
        if(amount !=0)
        BroAudio.Play(moneyGoesSound);
    }

    public void GemGoes(int amount)
    {
        gemsCollected -= amount;
        if (amount != 0)
            BroAudio.Play(gemsGoesSound);
    }

    public int CheckGems(int amount)
    {
        return gemsCollected - amount;
    }

    public List<int> GetCoinsForUI()
    {
        GameMoney gameMoney = new GameMoney();
        return gameMoney.ConvertCoins(moneyCollected);
    }


    public int GetPartyWeight()
    {
        int weight = 0;
        foreach(Hero h in heroes)
        {
            weight+=h.GetHeroWeight();
        }
        return weight+40;
    }

    public List<SavedSpellsAttached> GetSpellsAttached()
    {
        List<SavedSpellsAttached> spellsAttached = new List<SavedSpellsAttached>();
        foreach (Hero h in heroes)
        {
            SavedSpellsAttached spellattached = new SavedSpellsAttached();
            spellattached.heroID = h.GetHeroIndex();
            //print("spell attacted on hero " + h.GetSpellsAttached().Count);
            foreach (KeyValuePair< Spell,int> s in h.GetSpellsAttached())
            {
                spellattached.spell.Add(s.Key);
                spellattached.timesToFinish.Add(s.Value);
            }
            spellsAttached.Add(spellattached);
        }
        //print("spell attacted saved "+spellsAttached.Count);
        GameInstance.spellsAttachedToHeroes = spellsAttached;
        return spellsAttached;
    }

    public void RestoreSpellsAttached(List<SavedSpellsAttached> savedspellsattached) 
    {
        foreach(SavedSpellsAttached savedSpell in savedspellsattached)
        {
            for(int i =0; i<savedSpell.spell.Count;i++)
            {
                print(savedSpell.spell[i].spellEffect);
                heroes[savedSpell.heroID].AddSpellToSpellAttached(savedSpell.spell[i], savedSpell.timesToFinish[i]);
            }
        }
    }


    public bool CheckForDeadHeroes()
    {
        int health = 0;

        foreach(Hero hero in heroes)
        {
            health += hero.GetHeroHealth();
        }
        return health <= 0;

    }

    public void AddSomeFood(int amount)
    {
        partyFood += amount;
        RefreshUI.Invoke();
        foreach (Hero hero in heroes)
        {
            hero.FeedHero();
        }
    }

    public void LoadDialoguesFromInstance()
    {
        currentUniqueDialogueNames.Clear();

        foreach(UniqueDialogueName ud in  GameInstance.currentUniqueDialogueNames)
        {
            currentUniqueDialogueNames.Add(ud);
        }

    }

    public void SaveDialoguesToInstance()
    {
        GameInstance.currentUniqueDialogueNames.Clear();
        foreach (UniqueDialogueName ud in currentUniqueDialogueNames)
        {
            //print(ud);
            GameInstance.currentUniqueDialogueNames.Add(ud);
        }
    }


    public  List<MainStatsSave> ConvertHeroesMainStatsToSave()
    {
        List<MainStatsSave> newmainsave = new List<MainStatsSave>();

        
        for (int i=0;i<heroes.Count;i++)
        {
            foreach (KeyValuePair<MainStat, int> mainStat in heroes[i].GetMainStatsForUI())
            {
                MainStatsSave savemaintemp = new MainStatsSave();
                savemaintemp.heroIndex = i;
                savemaintemp.mainStat = mainStat.Key;
                savemaintemp.amount = mainStat.Value;
                newmainsave.Add(savemaintemp);
            }
        }

        return newmainsave;
    }


    public List<SkillStatSave> ConvertHeroesSkillsToSave()
    {
        List<SkillStatSave> newskillsSave = new List<SkillStatSave>();


        for (int i = 0; i < heroes.Count; i++)
        {
            foreach (KeyValuePair<SkillsStat, int> skillStat in heroes[i].GetSkillStatsForUI())
            {
                SkillStatSave saveskilltemp = new SkillStatSave();
                saveskilltemp.heroIndex = i;
                saveskilltemp.skill = skillStat.Key;
                saveskilltemp.amount = skillStat.Value;
                newskillsSave.Add(saveskilltemp);
            }
        }

        return newskillsSave;
    }

}
