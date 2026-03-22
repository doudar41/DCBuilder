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
    [SerializeField] SpellContainer wakeUpSpell;
    int currentexp = 0;
    int timeToLevelUp = 0;

    private void Awake()
    {
        activeHero = heroes[0].gameObject.GetComponent<IHero>();
    }


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
        print("party timer " + battleOnOf);
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
        //if (GameInstance.playerController.playerState == PlayerState.Battle && GameInstance.spellbook.IsSpellBookOpened()) return;
        if (GameInstance.playerController.playerState == PlayerState.Battle && !GameInstance.inventory.IsOpen()) return;
        if (hero == null)
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


    void SetActiveBattleHero(Hero hero)
    {
        if (hero == null)
        {
            hero = heroes[0];
        }
        foreach (Hero h in heroes)
        {
            if (hero == h)
            {
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

            activeHero.AddEquipmentToCharacter(heroInventoryItem, itemType);
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

    public List<IHero> GetIHeroes()
    {
        List<IHero> iheroes = new List<IHero>();
        foreach(Hero h in heroes)
        {
            iheroes.Add(h.GetComponent<IHero>());
        }
        return iheroes;
    }

    public void BattleHeroSwitch(Hero hero)
    {
        for (int i = 0; i < heroes.Count; i++)
        {
            if (heroes[i] == hero)
            {
                SetActiveBattleHero(hero);
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
        int countRings = 0;
        foreach(HeroInventoryItem he in GameInstance.equipmentHeroesSavedWithGUID)
        {
            print(" loading equipment " + he.container + " " +  " " + he.heroIndex);
            if (he == null) continue;
            if (he.container != -1)
            {
                print(" loading equipment " + GameInstance.dataBase.GetItemFromBaseByIndex( he.container).name + " " + " " + he.heroIndex);
                if (he.itemType != ItemType.RING) 
                { 
                if (he.heroIndex >= 0) heroes[he.heroIndex].AddEquipmentToCharacter(he, he.itemType);
                }
                else
                {
                    switch (countRings)
                    {
                        case 0: if (he.heroIndex >= 0) heroes[he.heroIndex].AddEquipmentToCharacter(he, ItemType.RING); break;
                        case 1: if (he.heroIndex >= 0) heroes[he.heroIndex].AddEquipmentToCharacter(he, ItemType.RING2); break;
                        case 2: if (he.heroIndex >= 0) heroes[he.heroIndex].AddEquipmentToCharacter(he, ItemType.RING3); break;
                        case 3: if (he.heroIndex >= 0) heroes[he.heroIndex].AddEquipmentToCharacter(he, ItemType.RING4); break;
                        case 4: if (he.heroIndex >= 0) heroes[he.heroIndex].AddEquipmentToCharacter(he, ItemType.RING5); break;
                        case 5: if (he.heroIndex >= 0) heroes[he.heroIndex].AddEquipmentToCharacter(he, ItemType.RING6); break;
                    }
                    countRings++;
                }
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
        GameInstance.soundManagerInGame.ProtectedPlay(moneyGoesSound);
    }

    public void GemGoes(int amount)
    {
        gemsCollected -= amount;
        if (amount != 0)
            GameInstance.soundManagerInGame.ProtectedPlay(gemsGoesSound);
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
            foreach (KeyValuePair< Spell,Vector3Int> s in h.GetSpellsAttached())
            {
                spellattached.spell.Add(s.Key);
                spellattached.timesToFinish.Add(s.Value.x);
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

    public void AddSomeFoodInit(int amount)
    {
        partyFood += amount;
        foreach (Hero hero in heroes)
        {
            hero.FeedHeroInit();
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
            heroes[i].GetPureStats(out Dictionary<MainStat, int> mainstats, out Dictionary<DependedStat, int> dependStats, out Dictionary<SkillsStat, int> skillstats);
            foreach (KeyValuePair<MainStat, int> mainStat in mainstats)
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
            foreach (SkillsStat skillStat in Enum.GetValues(typeof(SkillsStat)))
            {
                int s = heroes[i].GetSkillsStat(skillStat, true);
                SkillStatSave saveskilltemp = new SkillStatSave();
                saveskilltemp.heroIndex = i;
                saveskilltemp.skill = skillStat;
                saveskilltemp.amount = s;
                newskillsSave.Add(saveskilltemp);
            }
        }

        return newskillsSave;
    }

    public void TrapDamage(SpellContainer _spell, int trapComplexity, bool allParty)
    {
        if (allParty) {
            int damage = 0;
            foreach (Hero hero in heroes) 
            { 
                int _skill = hero.GetSkillsStat(SkillsStat.SpotSecret, false);
                if (_skill <= trapComplexity)
                {
                    hero.ApplySpellToHero(_spell);
                }
            }
        }
        else
        {
            Hero hero = heroes[UnityEngine.Random.Range(0, heroes.Count)];
            int _skill = hero.GetSkillsStat(SkillsStat.SpotSecret, false);
            if (_skill <= trapComplexity)
            {
                hero.ApplySpellToHero(_spell);
            }
        }


    }

    public void PartyAfterFullRest()
    {
        foreach (Hero hero in heroes)
        {
            if (hero.GetHeroHealth() <= 0) continue;
            if (hero.GetHeroStatus().Contains(GameplayStates.Petrified) || hero.GetHeroStatus().Contains(GameplayStates.Stoned)) continue;
            hero.HealthDecrease(-hero.GetMaxDependedStat(DependedStat.maxHealth));
            hero.ManaDecrease(-hero.GetMaxDependedStat(DependedStat.maxMana));
            hero.ApplySpellToHero(wakeUpSpell);
        }
    }

}
