
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
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
    [SerializeField] List<Spell> debuffSpells; //List of debuff spells which removed by restoration spell
    [SerializeField] HealthImage healthSlider, manaSlider; //Reference to health and Mana custom bars
    [SerializeField] List<SpellContainer> heroSpellbook = new List<SpellContainer>(); //This contains all spell owned by hero
    [SerializeField] SpellContainer unarmedSpell; //default hero attack without weapons
    [SerializeField] List<MagicType> immunityList = new List<MagicType>();
    [SerializeField] int foodConsumptionRate = 1; // How much food hero consumes per time unit
    int foodTimeConsumptionCounter = 0;
    SpellContainer lastSpell; //The last spell hero used canbe used by pressing "T" or menu button "Last Spell"
    int currentHealth = 100, currentMana = 100, currentHunger = 1440; //default health and mana parameters
    int heroID = 0; // hero ID is a hero index in the Party script which will be used for inventory management

    Dictionary<MainStat, int> mainStatContainer = new Dictionary<MainStat, int>();  // Container of main attributes held unmodified attributes of a hero
    Dictionary<DependedStat, int> dependedStatsDefault = new Dictionary<DependedStat, int>(); // Container of unmodified depended stats of a hero like health and mana 
    Dictionary<SkillsStat, int> skillsStatsCurrent = new Dictionary<SkillsStat, int>(); // Container of unmodified skills of a hero
    Dictionary<ItemType, SpellContainer> equipmentSpells = new Dictionary<ItemType, SpellContainer>(); // Spells on enchanted armor, weapons and accessories saved here
    Dictionary<Spell,int> spellsAttached = new Dictionary<Spell, int>(); // Other spells on player buffs and debuffs

    Dictionary<SkillsStat,int> skillsUsedInGameplay = new Dictionary<SkillsStat, int>(); // Skills selected at the beggining of the game

    //Battle manager var
    [SerializeField] int agroLevel = 1;  // recalculates Depend on damage and heal spell( spells can have an agro? ) 

    int currentInitiativeReduction = 0; // I think it's for Stun spell 

    public Dictionary<ItemType, HeroInventoryItem> equipmentWithGUID = new Dictionary<ItemType, HeroInventoryItem>(); // Container of equipment on hero

    [SerializeField] BuffPanels buffPanels; // panel above hero with buff and debuff spell icons

    List<GameplayStatus> gameplayStatuses = new List<GameplayStatus>(); // DeBuff states of a hero

    public UnityEvent<SpellContainer> hitTargetEffect;  

    MagicType weaponEnchanced = MagicType.None;

    int poisonDamage = 0;
    int skillPoints = 0;
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
        GameInstance.playerController.timeForward += TimePassBy;
    }
    private void OnDestroy()
    {
        GameInstance.battleManager.battlePassTime -= BattleTimeChanges;
        GameInstance.playerController.timeForward -= TimePassBy;
    }
    public void HeroInit()
    {
        
        foreach (KeyValuePair<DependedStat,int > dstat in HeroStatsDefault.GetFullDependedStats())
        {
            dependedStatsDefault.Add(dstat.Key,dstat.Value);
        }

        for (int i = 0; i < GameInstance.party.GetHeroList().Count; i++)
        {
            if (GameInstance.party.GetHeroList()[i] == this) heroID = i;
        }

        mainStatContainer = GameInstance.ConvertSavedMainStats(heroID);


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

/*        foreach(DependedStat dstat in System.Enum.GetValues(typeof(DependedStat)))
        {
            if (dependedStatsDefault.ContainsKey(dstat))
            {
                dependedStatsDefault[dstat] += GetDependedStatModificator(dstat);
            }
        }*/

        currentHealth = GetMaxDependedStat(DependedStat.maxHealth);
        currentMana = GetMaxDependedStat(DependedStat.maxMana);
        currentHunger = GetMaxDependedStat(DependedStat.Hunger);
        //print("hero "+ heroName+ "current health "+currentHealth + " max health "+ GetDependedStat(DependedStat.maxHealth));
        healthSlider.ProgressBarFill((float)currentHealth / (float)GetMaxDependedStat(DependedStat.maxHealth));
    }

    
    public Dictionary<Spell,int> GetSpellsAttached()
    {
        return spellsAttached;
    }

    public void AddSpellToSpellAttached(Spell spell, int timesToFinish)
    {
        //SingleSpellApply(spell, null, null,null);
       // spellsAttached.Add(spell, timesToFinish);
    }

    /*int GetDependedStatModificator(DependedStat dstat)
    {

        int statInt = 0;
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
                if (GetHeroWeight() < GetMaxDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += Mathf.Clamp(statInt - currentInitiativeReduction, 0, int.MaxValue);
                    statInt += (GetMainStat(MainStat.Agility) / 5);
                }
                break;
            case DependedStat.accuracy:
                statInt += (GetMainStat(MainStat.Agility) / 5) + (GetMainStat(MainStat.Endurance) / 5);
                break;
            case DependedStat.defence:
                statInt += (GetMainStat(MainStat.Endurance) / 5);
                break;
            case DependedStat.FireResistance:
                break;
            case DependedStat.CarryingCapacity:
                statInt += (GetMainStat(MainStat.Survival) / 5) + (GetMainStat(MainStat.Strength) / 5);
                break;
            case DependedStat.Hunger:
                statInt += (GetMainStat(MainStat.Survival) / 5) * 100; //Max hunger resistance depends on survival stat

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
                if (GetHeroWeight() < GetMaxDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += (GetMainStat(MainStat.Strength) / 5) + GetSkillsStat(GetWeaponType());
                }
                break;
            case DependedStat.rangeDamage:
                if (GetHeroWeight() < GetMaxDependedStat(DependedStat.CarryingCapacity))
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


    }*/

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


    public List<ResultMsg> ApplySpellToHero(SpellContainer spellToApply)
    {
        if (spellToApply == null) { StartCoroutine(AttackDelay()); return null; }
        List<ResultMsg> results = new List<ResultMsg>();

        foreach (Spell s in spellToApply.spells)
        {
            SingleSpellApply(s, spellToApply, results);
        }

        return results;
    }

    public List<ResultMsg> ApplySpellToHero(SpellContainer spellToApply, GameObject spellcaster)
    {
        if (spellToApply == null) { StartCoroutine(AttackDelay()); return null; }
        List<ResultMsg> results = new List<ResultMsg>();


        if (spellcaster.GetComponent<IHero>() != null)
        {
            IHero hero = spellcaster.GetComponent<IHero>();
            print("hero attack");

            foreach (Spell s in spellToApply.spells)
            {
                SingleSpellApply(s, spellToApply, results, spellcaster.GetComponent<IHero>());
            }
            //hitTargetEffect.Invoke(spellToApply);
            print("results "+results.Count);
            if (GameInstance.playerController.playerState == PlayerState.Battle)StartCoroutine(AttackDelay());
            return results;
        }

        if(spellcaster.GetComponent<IEnemy>() != null)
        {

            foreach (Spell s in spellToApply.spells)
            {
                SingleSpellApply(s, spellToApply, results, spellcaster.GetComponent<IEnemy>());
            }
            hitTargetEffect.Invoke(spellToApply);
            StartCoroutine(AttackDelay());
            return results;
        }



        return results;
    }

    private int CalculateDiceSumDamage(Spell s, int dice)
    {

        int amount = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
        amount += s.diceBonus;
        print("enemy damage from spell "+amount);
        //HealthDamage(amount);
        if (dice == 20)
        {
            amount = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
            amount += s.diceBonus;
        }

        return amount;
    }


    public List<ResultMsg> SingleSpellApply(Spell s, SpellContainer spellToApply, List<ResultMsg> results, IHero attacker)
    {
        switch (s.spellEffect)
        {

            case SpellEffects.Heal:
                if (currentHealth <= 0) break;
                int healroll = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
                healroll += GetSkillsStat(SkillsStat.LightMagic) + GetMainStat(MainStat.Mind) + s.diceBonus;
                currentHealth = Mathf.Clamp(currentHealth + healroll + (int)(Mathf.Pow((float)(GetMainStat(MainStat.Survival) / 4), 2)), 0, GetMaxDependedStat(DependedStat.maxHealth));

                results.Add(new() { msgType = "s", msgString = heroName + " healed " + healroll });
                ProgressBarChange();
                break;


            case SpellEffects.Restoration:


                foreach (Spell sc in debuffSpells)
                {
                    buffPanels.RemoveBuffFromList(sc);
                }

                foreach (GameplayStatus st in System.Enum.GetValues(typeof(GameplayStatus)))
                {
                    if (st != GameplayStatus.Dead)
                    {
                        gameplayStatuses.Remove(st);

                    }
                }

                if (portraits.GetStatePortrait(GameplayStatus.None, out Sprite stateSpriteWell)) portrait.sprite = stateSpriteWell;

                //HealHero(GetDependedStat(DependedStat.maxHealth));
                poisonDamage = 0;
                results.Add(new() { msgType = "s", msgString = heroName + " restored" });
                break;
        }
        return results;
    }

    public List<ResultMsg> SingleSpellApply(Spell s, SpellContainer spellToApply, List<ResultMsg> results)
    {
        switch (s.spellEffect)
        {

            case SpellEffects.Heal:
                if (currentHealth <= 0) break;
                int healroll = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);

                currentHealth = Mathf.Clamp(currentHealth+healroll, 0, GetMaxDependedStat(DependedStat.maxHealth));

                results.Add(new() { msgType = "s", msgString = heroName + " healed " + healroll });
                ProgressBarChange();
                break;


            case SpellEffects.Restoration:


                foreach (Spell sc in debuffSpells)
                {
                    buffPanels.RemoveBuffFromList(sc);
                }

                foreach (GameplayStatus st in System.Enum.GetValues(typeof(GameplayStatus)))
                {
                    if (st != GameplayStatus.Dead)
                    {
                        gameplayStatuses.Remove(st);

                    }
                }

                if (portraits.GetStatePortrait(GameplayStatus.None, out Sprite stateSpriteWell) && currentHealth > 0) 
                { 
                    portrait.sprite = stateSpriteWell; 
                }
                else
                {
                    if(currentHealth <= 0)
                    {
                        portraits.GetStatePortrait(GameplayStatus.Dead, out Sprite deadstate);
                        portrait.sprite = deadstate;
                    }
                }

                    //HealHero(GetDependedStat(DependedStat.maxHealth));
                    poisonDamage = 0;
                results.Add(new() { msgType = "s", msgString = heroName + " restored" });
                break;

                case SpellEffects.Revive:
                currentHealth = GetMaxDependedStat(DependedStat.maxHealth);
                ProgressBarChange();
                portraits.GetStatePortrait(GameplayStatus.None, out Sprite wellState);
                portrait.sprite = wellState;
                break;
        }
        return results;
    }


    public List<ResultMsg> SingleSpellApply(Spell s, SpellContainer spellToApply, List<ResultMsg> results, IEnemy attacker)
    {

        int attackRoll = 0;
        int evaderoll = 0;
        int attackrollbonus = 0;

        if (immunityList.Contains(s.magicType)) // skip magic attack if enemy has a total immunity to it 
        {
            //hitTargetEffect.Invoke(immunityspell); // Event for showing immunity animation
            return null;
        }
        attackrollbonus = attacker.GetCurrentStatValue(EnemyStat.ACCURACY);  // agillity modifier + endurance modifier of attacker

        int dice = GameInstance.DiceRollingBiggestNumber(1, 20); //attackroll and evaderoll random generation
        attackRoll = dice + attackrollbonus;
        evaderoll = GameInstance.DiceRollingBiggestNumber(1, 20) + GetMaxDependedStat(DependedStat.evasion);



        //results.Add(attackRoll.ToString()); results.Add(evaderoll.ToString()); // attackroll and evaderoll added to list to be used in the battle log
        results.Add(new() { msgType = "s", msgString = "AR " + s.spellEffect });
        results.Add(new() { msgType = "i", msgInt = attackRoll });

        results.Add(new() { msgType = "s", msgString = "/" });
        results.Add(new() { msgType = "i", msgInt = evaderoll });

        if (evaderoll > attackRoll) return results; //if evasion is successful and dice is not equal 20 spell is ignored

        int pureDamageAmount = CalculateDiceSumDamage(s, dice);
        bool applyEffectSpell = GameInstance.DiceRollingBiggestNumber(s.diceRollsNumber, s.diceSides) >= s.diceSides/2;

        switch (s.spellEffect)
        {
            case SpellEffects.PDmg:


                if(spellToApply.minDistanceToEnemy == 1) pureDamageAmount += GameInstance.DiceRollingBiggestNumber(2, attacker.GetCurrentStatValue(EnemyStat.MELEE_DAMAGE));
                if(spellToApply.minDistanceToEnemy > 1 && attacker.GetEnemyRow()>1) pureDamageAmount += GameInstance.DiceRollingBiggestNumber(2, attacker.GetCurrentStatValue(EnemyStat.RANGE_DAMAGE));
                if (spellToApply.minDistanceToEnemy > 1 && attacker.GetEnemyRow() < 1) 
                { 
                    pureDamageAmount -= GameInstance.DiceRollingBiggestNumber(2, attacker.GetCurrentStatValue(EnemyStat.RANGE_DAMAGE)); 
                    if (pureDamageAmount < 0) pureDamageAmount = 0;
                }
                
                
                int physicalDamage= pureDamageAmount - GetMaxDependedStat(DependedStat.defence);
                results.Add(new() { msgType = "s", msgString = " damage " + pureDamageAmount+" vs. defence "+ GetMaxDependedStat(DependedStat.defence) });
                //print("physical damage spell applied " + pureDamageAmount + "/ defence " + GetDependedStat(DependedStat.defence));

                results.Add(new() { msgType = "s", msgString = heroName + " damage " });
                results.Add(new() { msgType = "i", msgInt = physicalDamage }); // adding final damage amount to the results list
                if (physicalDamage < 0) physicalDamage = 0;
                HealthDecrease(physicalDamage);

                break;

            case SpellEffects.MDmg:
                switch (s.magicType)
                {
                    case MagicType.Fire:
                        if(immunityList.Contains(MagicType.Fire))
                        {
                            return null;
                        }
                         int fireDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.FireResistance) + (GetMainStat(MainStat.Willpower)/5));
                        HealthDecrease(fireDamage);
                        break;
                    case MagicType.Water:
                        if (immunityList.Contains(MagicType.Water))
                        {
                            return null;
                        }
                        int waterDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.WaterResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        HealthDecrease(waterDamage);

                        break;
                    case MagicType.Air:
                        if (immunityList.Contains(MagicType.Air))
                        {
                            return null;
                        }
                        int airDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.WaterResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        HealthDecrease(airDamage);
                        break;
                    case MagicType.Earth:
                        if (immunityList.Contains(MagicType.Earth))
                        {
                            return null;
                        }
                        int earthDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.EarthResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        HealthDecrease(earthDamage);
                        break;
                    case MagicType.Light:
                        if (immunityList.Contains(MagicType.Light))
                        {
                            return null;
                        }
                        int lightDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.DarkResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        HealthDecrease(lightDamage);

                        break;
                    case MagicType.Dark:
                        if (immunityList.Contains(MagicType.Dark))
                        {
                            return null;
                        }
                        int darkDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.DarkResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        HealthDecrease(darkDamage);
                        break;
                }
                break;

            case SpellEffects.MSMod:
                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, s.numberOfTurns);
                else spellsAttached[s] = s.numberOfTurns;
                if (buffPanels != null) 
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                    else buffPanels.AddBuffToList(s);
                }


                break;
            case SpellEffects.DSMod:
                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, s.numberOfTurns);
                else spellsAttached[s] = s.numberOfTurns;
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                    else buffPanels.AddBuffToList(s);
                }

                break;


            case SpellEffects.Restoration:
                
                break;


            case SpellEffects.Identify:
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

            case SpellEffects.ElementalResistance:

                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, s.numberOfTurns);
                else spellsAttached[s] = s.numberOfTurns;
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                    else buffPanels.AddBuffToList(s);
                }

                break;


            case SpellEffects.Poison:
                if (!gameplayStatuses.Contains(GameplayStatus.Poisoned))
                {
                    poisonDamage = pureDamageAmount - GetMaxDependedStat(DependedStat.DarkResistance);
                    if (applyEffectSpell)
                    {
                        gameplayStatuses.Add(GameplayStatus.Poisoned);
                        if (portraits.GetStatePortrait(GameplayStatus.Poisoned, out Sprite stateSpritePoisoned)) portrait.sprite = stateSpritePoisoned;
                        if (buffPanels != null)
                        {
                            if (!debuffSpells.Contains(s)) debuffSpells.Add(s);
                            if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                            else buffPanels.AddBuffToList(s);
                        }
                    }
                }
                break;

            case SpellEffects.Petrify:
                if (!gameplayStatuses.Contains(GameplayStatus.Petrified))
                {
                    if (applyEffectSpell)
                    {
                        gameplayStatuses.Add(GameplayStatus.Petrified); 
                        if (portraits.GetStatePortrait(GameplayStatus.Petrified, out Sprite stateSpritePetrified)) portrait.sprite = stateSpritePetrified;
                    
                    if (buffPanels != null)
                        {
                            if (!debuffSpells.Contains(s)) debuffSpells.Add(s);
                            if (spellToApply != null) buffPanels.AddBuffToList(spellToApply);
                            else buffPanels.AddBuffToList(s);
                        }

                    }
                    
                }

                break;


        }
        return results;
    }

    void ProgressBarChange()
    {
        healthSlider.ProgressBarFill((float)currentHealth / (float)GetMaxDependedStat(DependedStat.maxHealth));
       
    }

    IEnumerator AttackDelay()
    {
        //print("");
        yield return new WaitForSeconds(0.5f);

        GameInstance.battleManager.AttackEnding();
    }

    public void HealthDecrease(int amount)
    {

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, GetMaxDependedStat(DependedStat.maxHealth)); 
        healthSlider.ProgressBarFill((float)currentHealth / (float)GetMaxDependedStat(DependedStat.maxHealth));
        if (currentHealth <= 0) portrait.sprite = deadSprite;
        if(GameInstance.playerController.playerState != PlayerState.Battle)
        {
            if(GameInstance.party.CheckForDeadHeroes()) GameInstance.LoadGameMainMenu();
        }
        print(heroName + " health decrease amount " + amount + " current health"+currentHealth);
    }

    public void ManaDecrease(int amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, GetMaxDependedStat(DependedStat.maxMana));
        manaSlider.ProgressBarFill((float)currentMana / (float)GetMaxDependedStat(DependedStat.maxMana));
    }

    int GetMainStat(MainStat mainStat)
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

    public int GetMaxDependedStat(DependedStat dependedStat)
    {
        if (dependedStatsDefault.Count == 0) return 0;
        if (!dependedStatsDefault.ContainsKey(dependedStat)) return 0;
        int statInt = dependedStatsDefault[dependedStat];
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
                if (GetHeroWeight()<GetMaxDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += Mathf.Clamp(statInt - currentInitiativeReduction, 0, int.MaxValue);
                    statInt += (GetMainStat(MainStat.Agility) / 5);
                }
                break;
            case DependedStat.accuracy:
                statInt += (GetMainStat(MainStat.Agility) / 5) + (GetMainStat(MainStat.Endurance) / 5);
                break;
            case DependedStat.defence:
                statInt += (GetMainStat(MainStat.Endurance) / 5);
                break;
            case DependedStat.FireResistance:
                break;
            case DependedStat.CarryingCapacity:
                statInt += (GetMainStat(MainStat.Survival) / 5) + (GetMainStat(MainStat.Strength) / 5);
                break;
            case DependedStat.Hunger:
                statInt += (GetMainStat(MainStat.Survival) / 5) * 100; //Max hunger resistance depends on survival stat
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
                if (GetHeroWeight() < GetMaxDependedStat(DependedStat.CarryingCapacity))
                {
                    statInt += (GetMainStat(MainStat.Strength) / 5) + GetSkillsStat(GetWeaponType());
                }
                break;
            case DependedStat.rangeDamage:
                if (GetHeroWeight() < GetMaxDependedStat(DependedStat.CarryingCapacity))
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


    public void RecordSkillUsed(SkillsStat _skill)
    {
        print("record skill use " + _skill);
        if (!skillsUsedInGameplay.ContainsKey(_skill)) skillsUsedInGameplay.Add(_skill, 1);
        else skillsUsedInGameplay[_skill] = skillsUsedInGameplay[_skill] + 1;
    }



    public int GetSkillsStat(SkillsStat skillStat)
    {
        if (dependedStatsDefault.Count == 0) return 0;
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
        if (!dependedStatsDefault.TryAdd(dependedStat, amount))
        {
            dependedStatsDefault[dependedStat] = dependedStatsDefault[dependedStat] + amount;
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
            if(d != DependedStat.None)statListTemp.Add(d, GetMaxDependedStat(d));
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
        if(equipmentSpells.ContainsKey(itemType)) print("remove"+ equipmentSpells[itemType]);
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
        return GetMaxDependedStat(DependedStat.initiative);
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
            GameInstance.playerController.timeForward -= TimePassBy;
            GameInstance.battleManager.battlePassTime += BattleTimeChanges;
        }
        else
        {
            GameInstance.playerController.timeForward += TimePassBy;
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
        if (GameInstance.playerController.playerState != PlayerState.Battle)
        {
            foodTimeConsumptionCounter++;
            if (foodTimeConsumptionCounter >= foodConsumptionRate) // hero consumes food every hour
            {
                if (GameInstance.party.CheckFoodSupply(1) < 0)
                {
                    print("Check for food" + currentHunger);
                    currentHunger -=1 ;
                    if (currentHunger < 0) currentHunger = 0;
                    if (currentHunger <= 0)
                    {
                        HealthDecrease(1);
                    }
                }
                else
                {
                    GameInstance.party.FoodGoes(1);
                }

                foodTimeConsumptionCounter = 0;
                GameInstance.party.RefreshUI.Invoke();
            }
        }

        if (gameplayStatuses.Contains(GameplayStatus.Poisoned))
        {
            if (currentHealth > 0)
            {
                int _poisonDamage = GameInstance.DiceRollingBiggestNumber(1, poisonDamage);
                HealthDecrease(_poisonDamage);
                GameInstance.spellbook.ResultsToBattleLog(new() { "" }, new List<ResultMsg>() { new() { msgType = "s", msgString = heroName + " takes " + _poisonDamage.ToString() + " poison damage." } });

            }
        }

        //print("equipment spells "+ equipmentSpells.Count);
        foreach (ItemType it in equipmentSpells.Keys)
        {

            foreach (Spell s in equipmentSpells[it].spells)
            {
                if(s.spellEffect == SpellEffects.LightARoom)
                {
                    GameInstance.spellbook.CheckHeroesForLightSource(new KeyValuePair<int, bool>(heroID, true) );
                }
            }

        }

        if (equipmentSpells.Count == 0) GameInstance.spellbook.CheckHeroesForLightSource(new KeyValuePair<int, bool>(heroID, false));

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
 
                case SpellEffects.ElementalWeapon:
                    weaponEnchanced = MagicType.None;
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
        // if magic skills of a hero are high enough, he can get bonus to magic damage from mana pool
        //amount += (int)Mathf.Pow((float)(GetDependedStat(DependedStat.maxMana) / 50), 2);

        return amount;
    }

    int HungerLevel()
    {
        int hungerToAdd = GetMaxDependedStat(DependedStat.Hunger) - currentHunger;
        if (hungerToAdd < 0) hungerToAdd = 0;
        return hungerToAdd;
    }


    public void FeedHero() 
    {         
        if (dependedStatsDefault.ContainsKey(DependedStat.Hunger))
        {
            int hungerToAdd = HungerLevel();

            if (hungerToAdd >0)
            {
                int foodEaten = GameInstance.party.CheckFoodSupply(hungerToAdd);
                if (foodEaten < 0)
                {
                    currentHunger = hungerToAdd + foodEaten;

                }
                else
                {
                    currentHunger= GetMaxDependedStat(DependedStat.Hunger);

                }
                GameInstance.party.FoodGoes(hungerToAdd);
            }
            GameInstance.party.RefreshUI.Invoke() ;
        }
    }

    public float GetHungerLevelPercents()
    {
        print("hunger level " + GetMaxDependedStat(DependedStat.Hunger)+" - "+ dependedStatsDefault[DependedStat.Hunger]);
        return (float)currentHunger/(float)GetMaxDependedStat(DependedStat.Hunger);
    }

    public int GetSkillPoints()
    {
        return skillPoints;
    }

    public void AddExtraSkillPoints(int amount)
    {
        skillPoints += amount + GameInstance.GetAdditionalSkillPoints(skillsUsedInGameplay, out List<SkillsStat> skillList);
    }

    public void SetSkillPoints(int amount)
    {
        skillPoints = amount;
    }

    public void SetSKillStat(SkillsStat _skillStat, int amount)
    {
        skillsStatsCurrent[_skillStat] = amount;
    }


    public void GetPureStats(out Dictionary<MainStat,int> _mainStats, out Dictionary<DependedStat, int> _dependStats, out Dictionary<SkillsStat, int> _skillStats)
    {
        _mainStats = new Dictionary<MainStat, int>();
        foreach (KeyValuePair<MainStat,int> ms in mainStatContainer) { _mainStats.Add(ms.Key, ms.Value); }
        _dependStats = new Dictionary<DependedStat, int>();
        foreach (KeyValuePair<DependedStat, int> ds in dependedStatsDefault) { _dependStats.Add(ds.Key, ds.Value); }

        _skillStats = new Dictionary<SkillsStat, int>();
        foreach (KeyValuePair<SkillsStat, int> sks in skillsStatsCurrent) { _skillStats.Add(sks.Key, sks.Value); }

    }


}


public interface IHero
{
    public List<SpellContainer> GetActiveHeroSpellbook();
    public bool AddEquipmentToCharacter(HeroInventoryItem heroInventoryItem);
    public void RemoveItemFromEquipment(ItemType itemType);
    public void MakeHeroActive(bool active);
    public List<ResultMsg> ApplySpellToHero(SpellContainer spellToApply);
    public List<ResultMsg> ApplySpellToHero(SpellContainer spellToApply, GameObject spellcaster);
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
    public int GetMaxDependedStat(DependedStat dependedStat);
    public int GetSkillsStat(SkillsStat skillStat);

    public int MagicDamageModifier(SkillsStat skillStat);
    public List<GameplayStatus> GetHeroStatus();

    public MagicType GetWeaponMagicType();

    public int GetHeroIndex();
    public int GetHeroWeight();
    public void HealthDecrease(int amount);
    public float GetHungerLevelPercents();
    public void RecordSkillUsed(SkillsStat _skill);
    public int GetSkillPoints();
    public void AddExtraSkillPoints(int amount);

    public void SetSKillStat(SkillsStat _skillStat, int amount);
    public void SetMainStat(MainStat _mainStat, int amount);
    public void SetSkillPoints(int amount);


    public void GetPureStats(out Dictionary<MainStat, int> _mainStats, out Dictionary<DependedStat, int> _dependStats, out Dictionary<SkillsStat, int> _skillStats);
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

[System.Serializable]
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