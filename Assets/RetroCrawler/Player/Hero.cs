using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hero is a Image on a lower panel of game play menu which receive damage and can attack. Clicking on image will make hero active it will shows equipped weapons, spells 
/// hunger level, status. During the battle battleManager switching between heroes waiting for player input to attack or to cast a spell. 
/// </summary>




public class Hero : MonoBehaviour, IPointerClickHandler, IHero, IBattle
{

    [SerializeField] string heroName = "";
    [SerializeField] Image portrait; 
    [SerializeField] Sprite deadSprite; // The same for all players
    [SerializeField] PortraitContainer portraits; // Each hero can have a picture representing state their in, poisoned, stunned etc.
    [SerializeField] List<SpellContainer> debuffSpells; //List of debuff spells which removed by restoration spell
    [SerializeField] HealthImage healthSlider, manaSlider; //Reference to health and Mana custom bars
    [SerializeField] List<SpellContainer> heroSpellbook = new List<SpellContainer>(); //This contains all spell owned by hero
    [SerializeField] SpellContainer unarmedSpell; //default hero attack without weapons
    [SerializeField] List<MagicType> immunityList = new List<MagicType>();
    SpellContainer lastSpell; //The last spell hero used canbe used by pressing "T" or menu button "Last Spell"
    int currentHealth = 100, currentMana = 100; //default health and mana parameters
    int heroID = 0; // hero ID is a hero index in the Party script which will be used for inventory management

    Dictionary<MainStat, int> mainStatContainer = new Dictionary<MainStat, int>();  // Container of main attributes held unmodified attributes of a hero
    Dictionary<DependedStat, int> dependedStatsCurrent = new Dictionary<DependedStat, int>(); // Container of unmodified depended stats of a hero like health and mana 
    Dictionary<SkillsStat, int> skillsStatsCurrent = new Dictionary<SkillsStat, int>(); // Container of unmodified skills of a hero
    Dictionary<ItemType, SpellContainer> equipmentSpells = new Dictionary<ItemType, SpellContainer>(); // Spells on enchanted armor, weapons and accessories saved here
    Dictionary<Spell,int> spellsAttached = new Dictionary<Spell, int>(); // Other spells on player buffs and debuffs


    //Battle manager var
    [SerializeField] int agroLevel = 1;  // recalculates Depend on damage and heal spell( spells can have an agro? ) 

    int currentInitiativeReduction = 0; // I think it's for Stun spell 

    public Dictionary<ItemType, HeroInventoryItem> equipmentWithGUID = new Dictionary<ItemType, HeroInventoryItem>(); // Container of equipment on hero

    [SerializeField] BuffPanels buffPanels; // panel above hero with buff and debuff spell icons

    List<GameplayStatus> gameplayStatuses = new List<GameplayStatus>(); // DeBuff states of a hero

    public UnityEvent<SpellContainer> hitTargetEffect;  

    MagicType weaponEnchanced = MagicType.None;

    private void Awake()
    {
        if (!GameInstance.levelChange)
        {

            foreach (ItemType sk in System.Enum.GetValues(typeof(ItemType)))
            {

                if (!equipmentWithGUID.ContainsKey(sk)) equipmentWithGUID.Add(sk, null);
            }
        }
    }
    private void Start()
    {
        GameInstance.progress += TimePassBy;
    }

    public void HeroInit()
    {
        
        foreach (KeyValuePair<DependedStat,int > dstat in HeroStatsDefault.GetFullDependedStats())
        {
            dependedStatsCurrent.Add(dstat.Key,dstat.Value);
        }

        for (int i = 0; i < GameInstance.party.GetHeroList().Count; i++)
        {
            if (GameInstance.party.GetHeroList()[i] == this) heroID = i;
        }

        mainStatContainer = GameInstance.ConvertSavedMainStats(heroID);
        currentHealth = GetDependedStat(DependedStat.maxHealth);
        currentMana = GetDependedStat(DependedStat.maxMana);

        foreach (SkillsStat s in System.Enum.GetValues(typeof(SkillsStat)))
        {
            if (s != SkillsStat.None) skillsStatsCurrent.Add(s, 0);
        }


        if (GameInstance.heroesPortraits.Contains(heroID))
        {
            portraits = GameInstance.dataBase.GetPortraitFromDatabase(GameInstance.heroesPortraits[heroID]);
            portrait.sprite = portraits.portraits[0].sprite;
        }


        foreach (SkillStatSave savedskill in GameInstance.skillStatSaves)
        {
            if(savedskill.heroIndex == heroID)
            {
                skillsStatsCurrent[savedskill.skill] = savedskill.amount;
            }
        }
        if (GameInstance.heroesNames.Count> heroID)  heroName = GameInstance.heroesNames[heroID];

        heroSpellbook.Clear();
        foreach(HeroSpellbookSaved hsb in GameInstance.spellbooksSaved)
        {
            if(hsb.heroIndex == heroID)
            {
                heroSpellbook = hsb.spells;
            }
        }

        foreach(DependedStat dstat in System.Enum.GetValues(typeof(DependedStat)))
        {
            if (dependedStatsCurrent.ContainsKey(dstat))
            {
                dependedStatsCurrent[dstat] += GetDependedStatModificator(dstat);
            }

        }
    }

    
    public Dictionary<Spell,int> GetSpellsAttached()
    {
        return spellsAttached;
    }

    public void AddSpellToSpellAttached(Spell spell, int timesToFinish)
    {
        SingleSpellApply(spell, null);
       // spellsAttached.Add(spell, timesToFinish);

    }

    int GetDependedStatModificator(DependedStat dstat)
    {

        int statInt = dependedStatsCurrent[dstat];
        switch (dstat)
        {
            case DependedStat.heroLevel:
                break;
            case DependedStat.maxHealth:
                statInt += (GetMainStat(MainStat.Strength) / 5) * 10;
                break;
            case DependedStat.maxMana:
                statInt += (GetMainStat(MainStat.Mind) / 5) * 10;
                break;
            case DependedStat.initiative:
                if (GetHeroWeight() < GetDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += Mathf.Clamp(statInt - currentInitiativeReduction, 0, int.MaxValue);
                    statInt += (GetMainStat(MainStat.Agility) / 5);
                }
                break;
            case DependedStat.accuracy:
                statInt += (GetMainStat(MainStat.Agility) / 5) + (GetMainStat(MainStat.Endurance) / 5);
                break;
            case DependedStat.defence:
                statInt += 10 + (GetMainStat(MainStat.Endurance) / 5);
                break;
            case DependedStat.FireResistance:
                break;
            case DependedStat.CarryingCapacity:
                statInt += (GetMainStat(MainStat.Survival) / 5) + (GetMainStat(MainStat.Strength) / 5);
                break;
            case DependedStat.Hunger:
                statInt += (GetMainStat(MainStat.Survival) / 5) * 100;

                break;
            case DependedStat.None:
                break;
            case DependedStat.evasion:
                break;
            case DependedStat.WaterResistance:
                break;
            case DependedStat.EarthResistance:
                break;
            case DependedStat.AirResistance:
                break;
            case DependedStat.DarkResistance:
                break;
            case DependedStat.meleeDamage:
                if (GetHeroWeight() < GetDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += (GetMainStat(MainStat.Strength) / 5) + GetSkillsStat(GetWeaponType());
                }
                break;
            case DependedStat.rangeDamage:
                if (GetHeroWeight() < GetDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += (GetMainStat(MainStat.Agility) / 5) + GetSkillsStat(GetWeaponType());
                }
                break;
        }

        foreach (KeyValuePair<ItemType, SpellContainer> k in equipmentSpells)
        {
            foreach (Spell s in k.Value.spells)
            {

                if (s.changedDependedStat == dstat)
                {
                    statInt += s.amount;
                }
            }
        }


        foreach (KeyValuePair<Spell, int> s in spellsAttached)
        {
            if (s.Key.changedDependedStat == dstat)
            {
                statInt += s.Key.amount;
            }
        }

        return Mathf.Clamp(statInt, 0, int.MaxValue);


    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameInstance.party.SetActiveHero(this);
        GameInstance.spellbook.spellTargetEvent.Invoke(this.gameObject);

    }

    public void MakeHeroActive(bool active)
    {
        healthSlider.SetActiveBar(active);
        manaSlider.SetActiveBar(active);
    }

    public List<SpellContainer> GetActiveHeroSpellbook()
    {
        return heroSpellbook;
    }

    public void ApplySpellToHero(SpellContainer spellToApply, GameObject spellcaster)
    {
        //print("spell on hero "+ spellToApply);
        if (spellToApply == null) { StartCoroutine(AttackDelay()); return; }

       // print("Applying spell to hero " + heroName + " spell name " + spellToApply.spellName+ "hero health  "+currentHealth);
        foreach (Spell s in spellToApply.spells)
        {
            SingleSpellApply(s, spellToApply);
        }
        hitTargetEffect.Invoke(spellToApply);
        if (GameInstance.playerController.playerState == PlayerState.Battle && !spellToApply.AOE) StartCoroutine(AttackDelay());
    }

    private int CalculateIncomingDamage(Spell s, int dice)
    {

        int amount = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
        amount += s.diceBonus;
       //HealthDamage(amount);
        if (dice == 20)
        {
            amount = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
            amount += s.diceBonus;
        }

        return amount;
    }

    public void SingleSpellApply(Spell s, SpellContainer spellToApply)
    {


        int attackRoll = 0;
        int evaderoll = 0;
        int attackrollbonus = 0;

        if (immunityList.Contains(s.magicType)) // skip magic attack if enemy has a total immunity to it 
        {
            //hitTargetEffect.Invoke(immunityspell); // Event for showing immunity animation
            return;
        }

        int dice = GameInstance.DiceRollingBiggestNumber(1, 20); //attackroll and evaderoll random generation
        attackRoll = dice + attackrollbonus;
        evaderoll = GameInstance.DiceRollingBiggestNumber(1, 20) + GetDependedStat(DependedStat.evasion);

        //results.Add(attackRoll.ToString()); results.Add(evaderoll.ToString()); // attackroll and evaderoll added to list to be used in the battle log

        if (evaderoll > attackRoll) return; //if evasion is successful and dice is not equal 20 spell is ignored

        int pureDamageAmount = CalculateIncomingDamage(s, dice);

        //if (GetDependedStat(DependedStat.evasion) >= attackRoll) return;

        switch (s.spellEffect)
        {
            case SpellEffects.PhysicalDamage:

                int amount = pureDamageAmount - GetDependedStat(DependedStat.defence);
                healthDecrease(amount);

                break;

            case SpellEffects.MagicDamage:
                switch (s.magicType)
                {
                    case MagicType.Fire:

                        break;
                    case MagicType.Water:
                        break;
                    case MagicType.Air:
                        break;
                    case MagicType.Earth:
                        break;
                    case MagicType.Light:
                        break;
                    case MagicType.Dark:
                        break;
                }

                break;
            case SpellEffects.MainStatModify:
                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, s.numberOfTurns);
                else spellsAttached[s] = s.numberOfTurns;
                if (buffPanels != null) 
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                    else buffPanels.AddBuffToList(s);
                }


                break;
            case SpellEffects.DependedStatModify:
                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, s.numberOfTurns);
                else spellsAttached[s] = s.numberOfTurns;
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                    else buffPanels.AddBuffToList(s);
                }

                break;
            case SpellEffects.Restoration:

                foreach(SpellContainer sc in debuffSpells)
                {
                    if (buffPanels != null) 
                    { 
                        foreach(Spell sp in sc.spells)
                        {
                            buffPanels.RemoveBuffFromList(sp);
                        }

                    }
                }

                foreach (GameplayStatus st in System.Enum.GetValues(typeof(GameplayStatus)))
                {
                    if (st != GameplayStatus.Dead)
                    {
                        gameplayStatuses.Remove(st);

                    }
                }

                if (portraits.GetStatePortrait(GameplayStatus.None, out Sprite stateSpriteWell)) portrait.sprite = stateSpriteWell;
                break;


            case SpellEffects.Identify:
                break;

            case SpellEffects.Heal:
                if (currentHealth <= 0) break;
                int healroll = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
                healroll += GetSkillsStat(SkillsStat.LightMagic) + GetMainStat(MainStat.Mind) + s.diceBonus + s.amount;
                HealHero(healroll);
                break;
            case SpellEffects.ElementalWeapon:
                weaponEnchanced = s.magicType;
                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, s.numberOfTurns);
                else spellsAttached[s] = s.numberOfTurns;
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                    else buffPanels.AddBuffToList(s);
                }

                break;
            case SpellEffects.Poison:

                break;
            case SpellEffects.ElementalResistance:

                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, s.numberOfTurns);
                else spellsAttached[s] = s.numberOfTurns;
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                    else buffPanels.AddBuffToList(s);
                }

                break;
            case SpellEffects.Petrify:
                if (!gameplayStatuses.Contains(GameplayStatus.Petrified))
                {
                    gameplayStatuses.Add(GameplayStatus.Petrified);
                    if (portraits.GetStatePortrait(GameplayStatus.Petrified, out Sprite stateSpritePetrified)) portrait.sprite = stateSpritePetrified;
                }
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                    else buffPanels.AddBuffToList(s);
                }
                break;
        }

    }



    void HealHero(int amount)
    {
        //print("heal " + amount);
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, GetDependedStat(DependedStat.maxHealth));
        healthSlider.ProgressBarFill((float)currentHealth / (float)GetDependedStat(DependedStat.maxHealth));
        //if (currentHealth <= 0) portrait.sprite = deadSprite;
    }

    IEnumerator AttackDelay()
    {
        //print("");
        yield return new WaitForSeconds(0.5f);

        GameInstance.battleManager.AttackEnding();
    }

    public void healthDecrease(int amount)
    {

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, GetDependedStat(DependedStat.maxHealth)); 
        healthSlider.ProgressBarFill((float)currentHealth / (float)GetDependedStat(DependedStat.maxHealth));
        if (currentHealth <= 0) portrait.sprite = deadSprite;
        if(GameInstance.playerController.playerState != PlayerState.Battle)
        {
            if(GameInstance.party.CheckForDeadHeroes()) GameInstance.LoadGameMainMenu();
        }
        print(heroName + " health decrease amount " + amount + " current health"+currentHealth);
    }

    public void ManaDecrease(int amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, GetDependedStat(DependedStat.maxMana));
        manaSlider.ProgressBarFill((float)currentMana / (float)GetDependedStat(DependedStat.maxMana));
    }



    public int GetMainStat(MainStat mainStat)
    {
        if (mainStatContainer.Count <= 0) return 0;

        int statInt = mainStatContainer[mainStat];

        foreach (KeyValuePair<ItemType, SpellContainer> k in equipmentSpells)
        {
            foreach (Spell s in k.Value.spells)
            {
                if (s.changedMainStat == mainStat)
                {
                    statInt += s.amount;
                }
            }
        }

        foreach (KeyValuePair< Spell,int> s in spellsAttached)
        {

            if (s.Key.changedMainStat == mainStat)
            {
                statInt += s.Key.amount;
            }
        }

        return Mathf.Clamp(statInt, 0, int.MaxValue);
    }



    public void SetMainStat(MainStat mainStat, int amount)
    {
        if (!mainStatContainer.TryAdd(mainStat, amount))
        {
            mainStatContainer[mainStat] = mainStatContainer[mainStat] + amount;
        }
    }

    public int GetDependedStat(DependedStat dependedStat)
    {
        if (dependedStatsCurrent.Count == 0) return 0;
        if (!dependedStatsCurrent.ContainsKey(dependedStat)) return 0;
        int statInt = dependedStatsCurrent[dependedStat];
        switch (dependedStat)
        {
            case DependedStat.heroLevel:
                break;
            case DependedStat.maxHealth:
                statInt += (GetMainStat(MainStat.Strength) / 5)*10;
                break;
            case DependedStat.maxMana:
                statInt += (GetMainStat(MainStat.Mind) / 5)*10;
                break;
            case DependedStat.initiative:
                if (GetHeroWeight()<GetDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += Mathf.Clamp(statInt - currentInitiativeReduction, 0, int.MaxValue);
                    statInt += (GetMainStat(MainStat.Agility) / 5);
                }
                break;
            case DependedStat.accuracy:
                statInt += (GetMainStat(MainStat.Agility) / 5) + (GetMainStat(MainStat.Endurance) / 5);
                break;
            case DependedStat.defence:
                statInt += 10 + (GetMainStat(MainStat.Endurance) / 5);
                break;
            case DependedStat.FireResistance:
                break;
            case DependedStat.CarryingCapacity:
                statInt += (GetMainStat(MainStat.Survival) / 5) + (GetMainStat(MainStat.Strength) / 5);
                break;
            case DependedStat.Hunger:
                break;
            case DependedStat.None:
                break;
            case DependedStat.evasion:
                break;
            case DependedStat.WaterResistance:
                break;
            case DependedStat.EarthResistance:
                break;
            case DependedStat.AirResistance:
                break;
            case DependedStat.DarkResistance:
                break;
            case DependedStat.meleeDamage:
                if (GetHeroWeight() < GetDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += (GetMainStat(MainStat.Strength) / 5) + GetSkillsStat(GetWeaponType());
                }
                break;
            case DependedStat.rangeDamage:
                if (GetHeroWeight() < GetDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += (GetMainStat(MainStat.Agility) / 5) + GetSkillsStat(GetWeaponType());
                }
                break;
        }


        foreach (KeyValuePair<ItemType, SpellContainer> k in equipmentSpells)
        {
            foreach (Spell s in k.Value.spells)
            {

                if (s.changedDependedStat == dependedStat)
                {
                    statInt += s.amount;
                }
            }
        }


        foreach (KeyValuePair<Spell, int> s in spellsAttached)
        {
            if (s.Key.changedDependedStat == dependedStat)
            {
                statInt += s.Key.amount;
            }
        }

        return Mathf.Clamp(statInt,0,int.MaxValue);
    }

    public int GetSkillsStat(SkillsStat skillStat)
    {
        if (dependedStatsCurrent.Count == 0) return 0;
        skillsStatsCurrent.TryGetValue(skillStat, out int st);
        int statInt = 0; 
        statInt += st;
        switch (skillStat)
        {
            case SkillsStat.BluntWeapons:
                break;
            case SkillsStat.BladedWeapons:
                break;
            case SkillsStat.Polearms:
                break;
            case SkillsStat.RangedWeapons:
                break;
            case SkillsStat.HeavyArmour:
                break;
            case SkillsStat.LightArmour:
                break;
            case SkillsStat.LightMagic:
                break;
            case SkillsStat.DarkMagic:
                break;
            case SkillsStat.ElementalMagic:
                break;
            case SkillsStat.Identify:
                break;
            case SkillsStat.SpotSecret:
                break;
        }


        foreach (KeyValuePair<ItemType, SpellContainer> k in equipmentSpells)
        {
            foreach (Spell s in k.Value.spells)
            {
                if (s.skillStatAdded == skillStat)
                {
                    statInt += s.amount;
                }
            }
        }

        foreach (KeyValuePair<Spell, int> s in spellsAttached)
        {

            if (s.Key.skillStatAdded == skillStat)
            {
                statInt += s.Key.amount;
            }
        }

        return Mathf.Clamp(statInt, 0, int.MaxValue);
    }

    public void SetDependedStat(DependedStat dependedStat, int amount)
    {
        if (!dependedStatsCurrent.TryAdd(dependedStat, amount))
        {
            dependedStatsCurrent[dependedStat] = dependedStatsCurrent[dependedStat] + amount;
        }
    }

    public Dictionary<MainStat,int> GetMainStatsForUI()
    {
        Dictionary<MainStat, int> statListTemp = new Dictionary<MainStat, int>();

        statListTemp.Add(MainStat.Strength, GetMainStat(MainStat.Strength));
        statListTemp.Add(MainStat.Agility, GetMainStat(MainStat.Agility));
        statListTemp.Add(MainStat.Mind,GetMainStat(MainStat.Mind));
        statListTemp.Add(MainStat.Endurance,GetMainStat(MainStat.Endurance));
        statListTemp.Add(MainStat.Willpower,GetMainStat(MainStat.Willpower));
        statListTemp.Add(MainStat.Survival, GetMainStat(MainStat.Survival));
        return statListTemp;
    }

    public Dictionary<DependedStat, int> GetDependedStatsForUI()
    {
        Dictionary<DependedStat, int> statListTemp = new Dictionary<DependedStat, int>();

        foreach (DependedStat d in System.Enum.GetValues(typeof(DependedStat)))
        {
            if(d != DependedStat.None)statListTemp.Add(d, GetDependedStat(d));
        }

        return statListTemp;
    }

    public Dictionary<SkillsStat, int> GetSkillStatsForUI()
    {
        Dictionary<SkillsStat, int> statListTemp = new Dictionary<SkillsStat, int>();

        foreach (SkillsStat d in System.Enum.GetValues(typeof(SkillsStat)))
        {
            if(d != SkillsStat.None)statListTemp.Add(d, GetSkillsStat(d));
        }

        return statListTemp;
    }


  
    public bool AddEquipmentToCharacter(HeroInventoryItem heroInventoryItem)
    {

        if (heroInventoryItem == null) return false;

        //heroInventoryItem.savedState = SavedState.Equipment;

        if (!equipmentWithGUID.TryAdd(GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).itemType, heroInventoryItem))
        {
            heroInventoryItem.heroIndex = heroID;
            equipmentWithGUID[GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).itemType] = heroInventoryItem;
            
        }
        else
        {
            return false;
        }

        if (!equipmentSpells.TryAdd(GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).itemType, GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer))
        {
            equipmentSpells[GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).itemType] = GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer;
            return true;
        }
        else return false;
    }

    public void RemoveItemFromEquipment(ItemType itemType)
    {
        equipmentSpells.Remove(itemType);
        equipmentWithGUID.Remove(itemType);
    }

    public Dictionary<ItemType, HeroInventoryItem> GetHeroEquipment()
    {
        return equipmentWithGUID;
    }

    public int GetHeroHealth()
    {
        return currentHealth;
    }

    public int GetInitiativeInBattle()
    {
        return GetDependedStat(DependedStat.initiative);
    }

    public List<GameObject> GetOpponents()
    {
        return null;
    }
    public Hero GetThisHero()
    {
        return this;
    }

    public int GetHeroAgro()
    {
        return agroLevel;
    }
    public void ChangeArgo(int amount)
    {
        agroLevel = Mathf.Clamp(agroLevel + amount, 0, int.MaxValue);
    }

    public SpellContainer GetWeaponSpell()
    {
        if(equipmentSpells.TryGetValue(ItemType.WEAPON, out SpellContainer w))
        {

        }
        else
        {
            return unarmedSpell;
        }
        return w;
    }

    public SkillsStat GetWeaponType()
    {
        if(!equipmentWithGUID.ContainsKey(ItemType.WEAPON)) return SkillsStat.None;
        if (equipmentWithGUID[ItemType.WEAPON] != null) return GameInstance.dataBase.GetItemFromBaseByIndex(equipmentWithGUID[ItemType.WEAPON].container).weaponType;
        return SkillsStat.None;
    }

    public string HeroName()
    {
        return heroName;
    }


    public void SetBattleTimeOnOff(bool onOff)
    {
        if (onOff)
        {
            GameInstance.progress -= TimePassBy;
            GameInstance.battleManager.battlePassTime += BattleTimeChanges;
        }
        else
        {
            GameInstance.progress += TimePassBy;
            GameInstance.battleManager.battlePassTime -= BattleTimeChanges;
        }
    }


    void BattleTimeChanges(int count)
    {
        if (GameInstance.playerController.playerState != PlayerState.Battle) return;
        TimeChanges(count);
    }


    void TimePassBy(int count)
    {
        if (GameInstance.playerController.playerState == PlayerState.Battle) return;
        TimeChanges(count);
    }


    void TimeChanges(int count)
    {

       // print(GameInstance.GetNormalTime()[0]%60+"/"+ GameInstance.GetNormalTime()[1]+ "/"+GameInstance.GetNormalTime()[2]);

        if (GameInstance.playerController.playerState != PlayerState.Battle)
        {

            if (dependedStatsCurrent.ContainsKey(DependedStat.Hunger))
            {
                dependedStatsCurrent[DependedStat.Hunger] = dependedStatsCurrent[DependedStat.Hunger] - 1;
                if (dependedStatsCurrent[DependedStat.Hunger] <= 0)
                {
                    healthDecrease(1);
                }
            }
        }

        //print("hero time changes");
        if (spellsAttached.Count <= 0) return;
        List<Spell> listToDelete = new List<Spell>();
        List<Spell> listToChange = new List<Spell>();
        foreach (KeyValuePair<Spell, int> s in spellsAttached)
        {
            if (spellsAttached[s.Key] > 0) { listToChange.Add(s.Key); }
            else listToDelete.Add(s.Key);
        }

        foreach (Spell s in listToChange)
        {
            int x = spellsAttached[s];
            spellsAttached[s] = x - 1;
        }


        foreach (Spell s in listToDelete)
        {
            switch (s.spellEffect)
            {
                case SpellEffects.PhysicalDamage:
                    break;
                case SpellEffects.MagicDamage:
                    break;
                case SpellEffects.MainStatModify:
                    break;
                case SpellEffects.DependedStatModify:
                    break;
                case SpellEffects.Recall:
                    break;
                case SpellEffects.Mark:
                    break;
                case SpellEffects.Paralize:
                    break;
                case SpellEffects.Restoration:
                    break;
                case SpellEffects.Stone:
                    break;
                case SpellEffects.Death:
                    break;
                case SpellEffects.WizardEye:
                    break;
                case SpellEffects.Waterwalk:
                    break;
                case SpellEffects.Identify:
                    break;
                case SpellEffects.ReadPortal:
                    break;
                case SpellEffects.LightARoom:
                    break;
                case SpellEffects.Heal:
                    break;
                case SpellEffects.ElementalResistance:
                    break;
                case SpellEffects.ElementalWeapon:
                    weaponEnchanced = MagicType.None;
                    break;
                case SpellEffects.LavaWalk:
                    break;
                case SpellEffects.Petrify:
                    break;
                case SpellEffects.Immunity:
                    break;
                case SpellEffects.Poison:
                    break;
            }
            if (buffPanels != null)buffPanels.RemoveBuffFromList(s);
            spellsAttached.Remove(s);
        }
    }



    public SpellContainer GetInfusedWeaponSpell()
    {


        return null;
    }

    public List<GameplayStatus> GetHeroStatus()
    {
        return gameplayStatuses;
    }

    private void OnDestroy()
    {
        GameInstance.progress -= TimePassBy;
        GameInstance.battleManager.battlePassTime -= BattleTimeChanges;
    }

    public MagicType GetWeaponMagicType()
    {
        return weaponEnchanced;
    }

    public int GetHeroIndex()
    {
        return heroID;
    }

    public void SetDefaultSpell(SpellContainer spellContainer)
    {
        lastSpell = spellContainer;
    }

    public SpellContainer GetDefaultSpell()
    {

        return lastSpell;
    }


    public int GetHeroWeight()
    {
        int weight = 0;
        foreach (KeyValuePair<ItemType, HeroInventoryItem> equi in equipmentWithGUID)
        {
            if (equi.Value != null)
            {
                weight += GameInstance.dataBase.GetItemFromBaseByIndex(equi.Value.container).weight;
            }
        }
        return weight;

    }


    public int MagicDamageModifier(SkillsStat skillStat) // calculate magic damage from skills and main stat mind
    {
        int amount = 0;
        amount += GetSkillsStat(skillStat) / 5; 
        amount += GetMainStat(MainStat.Mind) / 5;
        amount += GetDependedStat(DependedStat.maxMana) / 5;

        return amount;
    }

}


public interface IHero
{
    public List<SpellContainer> GetActiveHeroSpellbook();
    public bool AddEquipmentToCharacter(HeroInventoryItem heroInventoryItem);
    public void RemoveItemFromEquipment(ItemType itemType);
    public void MakeHeroActive(bool active);
    public void ApplySpellToHero(SpellContainer spellToApply, GameObject spellcaster);
    public void ManaDecrease(int amount);

    public Dictionary<MainStat, int> GetMainStatsForUI();
    public Dictionary<DependedStat, int> GetDependedStatsForUI();
    public Dictionary<SkillsStat, int> GetSkillStatsForUI();
    public Dictionary<ItemType, HeroInventoryItem> GetHeroEquipment();

    public int GetHeroHealth();
    public Hero GetThisHero();
    public int GetHeroAgro();
    public void ChangeArgo(int amount);

    public SpellContainer GetWeaponSpell();
    public SpellContainer GetInfusedWeaponSpell();
    public string HeroName();
    public int GetDependedStat(DependedStat dependedStat);
    public int GetSkillsStat(SkillsStat skillStat);

    public int MagicDamageModifier(SkillsStat skillStat);
    public List<GameplayStatus> GetHeroStatus();

    public MagicType GetWeaponMagicType();

    public int GetHeroIndex();
    public int GetHeroWeight();
}

public enum MainStat
{
    None,
    Strength,
    Agility,
    Mind,
    Endurance,
    Willpower,
    Survival
}

public enum DependedStat
{
    None,
    maxHealth,
    maxMana,
    heroLevel,
    initiative,
    accuracy,
    defence,
    evasion,
    FireResistance,
    WaterResistance,
    EarthResistance,
    AirResistance,
    DarkResistance,
    CarryingCapacity,
    Hunger,
    meleeDamage,
    rangeDamage,
    IceResistance
}

public enum SkillsStat
{
    None,
    BluntWeapons,
    BladedWeapons,
    Polearms,
    RangedWeapons,
    HeavyArmour,
    LightArmour,
    LightMagic,
    DarkMagic,
    ElementalMagic,
    Identify,
    SpotSecret
}


public enum GameplayStatus
{
    None,
    Frozen,
    Burning,
    Poisoned,
    Stunned,
    Petrified,
    Dead,
    Stoned,
    Paralized
}

[System.Serializable]
public struct MainStatClass
{
    public MainStat mainStat;
    public int amount;
}

[System.Serializable]
public struct DependedStatClass
{
    public DependedStat dependedStat;
    public int amount;
}

[System.Serializable]
public struct SkillStatClass
{
    public SkillsStat skillStat;
    public int amount;
}