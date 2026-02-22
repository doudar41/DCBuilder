
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    [SerializeField] animateUIImage portraitOverlayAnimation;
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
    Dictionary<Spell,Vector3Int> spellsAttached = new Dictionary<Spell, Vector3Int>(); // Other spells on player buffs and debuffs

    Dictionary<SkillsStat,int> skillsUsedInGameplay = new Dictionary<SkillsStat, int>(); // Skills selected at the beggining of the game

    //Battle manager var
    [SerializeField] int agroLevel = 1;  // recalculates Depend on damage and heal spell( spells can have an agro? ) 

    int currentInitiativeReduction = 0; // I think it's for Stun spell 

    public Dictionary<ItemType, HeroInventoryItem> equipmentWithGUID = new Dictionary<ItemType, HeroInventoryItem>(); // Container of equipment on hero

    [SerializeField] BuffPanels buffPanels; // panel above hero with buff and debuff spell icons
    [SerializeField] DebuffIcons debuffPanel;
    List<GameplayStates> gameplayStatuses = new List<GameplayStates>(); // DeBuff states of a hero

    public UnityEvent<SpellContainer> hitTargetEffect;  

    MagicType weaponEnchanced = MagicType.None;

    int poisonDamageRate = 0, burningRate = 0;
    int skillPoints = 0;
    bool showDamageEffect = false;
    
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
        GameInstance.battleManager.battlePassTime += BattleTimeChanges;
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


        if (GameInstance.heroesPortraits.Count>heroID)
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
        //print(heroName + " hero init called id " + heroID);
        heroSpellbook.Clear();

        foreach(HeroSpellbookSaved hsb in GameInstance.spellbooksSaved)
        {
            if(hsb.heroIndex == heroID)
            {
                heroSpellbook = hsb.spells;
            }
        }

        if(GameInstance.heroesCurrentData.Count !=0)
        {
            foreach(HeroSavedCurrentData hcd in GameInstance.heroesCurrentData)
            {
                if(hcd.heroIndex == heroID)
                {
                    currentHealth = hcd.currentHealth;
                    currentMana = hcd.currentMana;
                    currentHunger = hcd.currentHunger;

                }
            }
        }
        else
        {
            currentHealth = GetMaxDependedStat(DependedStat.maxHealth);
            currentMana = GetMaxDependedStat(DependedStat.maxMana);
            currentHunger = GetMaxDependedStat(DependedStat.Hunger);
        }

        healthSlider.ProgressBarFill((float)currentHealth / (float)GetMaxDependedStat(DependedStat.maxHealth));
    }
    
    public Dictionary<Spell,Vector3Int> GetSpellsAttached()
    {
        return spellsAttached;
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


    public List<ResultMsg> ApplySpellToHero(SpellContainer spellToApply)
    {
        if (spellToApply == null) { StartCoroutine(AttackDelay()); return null; }
        List<ResultMsg> results = new List<ResultMsg>();

        foreach (Spell s in spellToApply.spells)
        {
            //print("apply spell to hero "+s.spellEffect);
            SingleSpellApply(s, spellToApply, results);
        }
        if (GameInstance.playerController.playerState == PlayerState.Battle) StartCoroutine(AttackDelay());
        return results;
    }

    public List<ResultMsg> ApplySpellToHero(SpellContainer spellToApply, GameObject spellcaster)
    {
        if (spellToApply == null) { StartCoroutine(AttackDelay()); return null; }
        List<ResultMsg> results = new List<ResultMsg>();

        if (spellcaster.GetComponent<IHero>() != null)
        {
            IHero hero = spellcaster.GetComponent<IHero>();

            foreach (Spell s in spellToApply.spells)
            {
                SingleSpellApply(s, spellToApply, results, spellcaster.GetComponent<IHero>());
            }
            
            hitTargetEffect.Invoke(spellToApply);

            if (GameInstance.playerController.playerState == PlayerState.Battle) StartCoroutine(AttackDelay());
            return results;
        }

        if(spellcaster.GetComponent<IEnemy>() != null)
        {

            foreach (Spell s in spellToApply.spells)
            {
                SingleSpellApply(s, spellToApply, results, spellcaster.GetComponent<IEnemy>());
            }
            //hitTargetEffect.Invoke(spellToApply);
            StartCoroutine(AttackDelay());
            return results;
        }

        return results;
    }

    private int CalculateDiceSumDamage(Spell s, int dice)
    {

        int amount = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
        amount += s.diceBonus;
        //print("enemy damage from spell "+amount);
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
        int attackRoll = 0;
        int evaderoll = 0;
        int attackrollbonus = 0;

        if (immunityList.Contains(s.magicType)) // skip magic attack if enemy has a total immunity to it 
        {
            //hitTargetEffect.Invoke(immunityspell); // Event for showing immunity animation
            results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " hero immune ti " + s.magicType });
            return results;
        }
        attackrollbonus = attacker.GetMaxDependedStat(DependedStat.accuracy);  // agillity modifier + endurance modifier of attacker
        if (attacker.GetHeroStatus().Contains(GameplayStates.Blind))
        {
            attackrollbonus = -5;
        }
        int dice = GameInstance.DiceRollingBiggestNumber(1, 20); //attackroll and evaderoll random generation
        attackRoll = dice + attackrollbonus;
        if (attackRoll <= 0) attackRoll = 1;

        evaderoll = GameInstance.DiceRollingBiggestNumber(1, 20) + GetMaxDependedStat(DependedStat.evasion);

        results.Add(new() { msgType = "s", msgString = "Roll for " + s.spellEffect });
        results.Add(new() { msgType = "i", msgInt = attackRoll });

        results.Add(new() { msgType = "s", msgString = "/" });
        results.Add(new() { msgType = "i", msgInt = evaderoll });

        if (evaderoll > attackRoll)
        {
            results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " missed" });
            return results;
        } //if evasion is successful and dice is not equal 20 spell is ignored

        int pureDamageAmount = CalculateDiceSumDamage(s, dice);
        bool applyEffectSpell = GameInstance.DiceRollingBiggestNumber(s.diceRollsNumber, s.diceSides) >= s.diceSides / 2;

        if (gameplayStatuses.Contains(GameplayStates.Sleep))
        {
            gameplayStatuses.Remove(GameplayStates.Sleep);
            if (gameplayStatuses.Count == 0)
            {
                if (currentHealth > 0) if (portraits.GetStatePortrait(GameplayStates.None, out Sprite stateNorm)) portrait.sprite = stateNorm;
                    else
                    {
                        portrait.sprite = deadSprite;
                    }
            }
            else
            {
                if (currentHealth > 0) if (portraits.GetStatePortrait(gameplayStatuses[gameplayStatuses.Count - 1], out Sprite stateLast)) portrait.sprite = stateLast;
            }
        }


        switch (s.spellEffect)
        {
            case SpellEffects.PDmg:

                pureDamageAmount += GameInstance.DiceRollingBiggestNumber(2, attacker.GetMaxDependedStat(DependedStat.meleeDamage));

                int physicalDamage = pureDamageAmount - GetMaxDependedStat(DependedStat.defence);
                results.Add(new() { msgType = "s", msgString = " damage " + pureDamageAmount + " vs. defence " + GetMaxDependedStat(DependedStat.defence) });

                results.Add(new() { msgType = "s", msgString = heroName + " damage " });
                results.Add(new() { msgType = "i", msgInt = physicalDamage }); // adding final damage amount to the results list
                if (physicalDamage < 0) physicalDamage = 0;
                HealthDecrease(physicalDamage);

                break;

            case SpellEffects.MDmg:

                switch (s.magicType)
                {

                    case MagicType.Fire:
                        if (immunityList.Contains(MagicType.Fire))
                        {
                            results.Add(new() { msgType = "s", msgString = heroName + " immune to Fire magic " });
                            return results;
                        }
                        if (gameplayStatuses.Contains(GameplayStates.Frozen))
                        {
                            gameplayStatuses.Remove(GameplayStates.Frozen);
                            debuffPanel.AddDebuffIcon(GameplayStates.Frozen, false);
                            UpdatePortraitState();
                            //additional damage ???
                        }
                        int fireDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.FireResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        fireDamage = fireDamage < 0 ? 0 : fireDamage;
                        HealthDecrease(fireDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " Hit " + heroName + " with Fire " + fireDamage });
                        break;
                    case MagicType.Water:
                        if (immunityList.Contains(MagicType.Water))
                        {
                            results.Add(new() { msgType = "s", msgString = heroName + " immune to Water magic " });
                            return results;
                        }
                        if (gameplayStatuses.Contains(GameplayStates.Burning))
                        {
                            gameplayStatuses.Remove(GameplayStates.Burning);
                            debuffPanel.AddDebuffIcon(GameplayStates.Burning, false);
                            UpdatePortraitState();
                            portraitOverlayAnimation.StopFXAnimation();
                            //additional damage ???
                        }
                        int waterDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.WaterResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        waterDamage = waterDamage < 0 ? 0 : waterDamage;
                        HealthDecrease(waterDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " Hit " + heroName + " with water " + waterDamage });
                        break;
                    case MagicType.Air:
                        if (immunityList.Contains(MagicType.Air))
                        {
                            results.Add(new() { msgType = "s", msgString = heroName + " immune to Air magic " });
                            return results;
                        }
                        int airDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.WaterResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        airDamage = airDamage < 0 ? 0 : airDamage;
                        HealthDecrease(airDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " Hit " + heroName + " with air " + airDamage });
                        break;
                    case MagicType.Earth:
                        if (immunityList.Contains(MagicType.Earth))
                        {
                            return null;
                        }
                        int earthDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.EarthResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        earthDamage = earthDamage < 0 ? 0 : earthDamage;
                        HealthDecrease(earthDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " Hit " + heroName + " with Earth magic " + earthDamage });
                        break;
                    case MagicType.Light:
                        if (immunityList.Contains(MagicType.Light))
                        {
                            return null;
                        }
                        int lightDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.DarkResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        lightDamage = lightDamage < 0 ? 0 : lightDamage;
                        results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " Hit " + heroName + " with Light magic " + lightDamage });
                        HealthDecrease(lightDamage);

                        break;


                    case MagicType.Dark:
                        if (immunityList.Contains(MagicType.Dark))
                        {
                            return null;
                        }
                        int darkDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.DarkResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        darkDamage = darkDamage < 0 ? 0 : darkDamage;
                        HealthDecrease(darkDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " Hit " + heroName + " with Dark magic " + darkDamage });
                        break;
                    case MagicType.Ice:
                        if (immunityList.Contains(MagicType.Ice))
                        {
                            results.Add(new() { msgType = "s", msgString = heroName + " immune to Ice magic " });
                            return results;
                        }
                        if (gameplayStatuses.Contains(GameplayStates.Burning))
                        {
                            gameplayStatuses.Remove(GameplayStates.Burning);
                            debuffPanel.AddDebuffIcon(GameplayStates.Burning, false);
                            UpdatePortraitState();
                            portraitOverlayAnimation.StopFXAnimation();
                            //additional damage ???
                        }
                        int iceDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.WaterResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        iceDamage = iceDamage < 0 ? 0 : iceDamage;
                        HealthDecrease(iceDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " Hit " + heroName + " with water " + iceDamage });

                        break;
                }
                break;


            case SpellEffects.Heal:
                if (currentHealth <= 0) break;
                if (s.continuousSpell)
                {
                    gameplayStatuses.Add(GameplayStates.Regenerating);
                    
                    int diceResult01 = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides) + attacker.GetSkillsStat(s.skillToCheckInCalculations, false) / 3;
                    int regenerateRate = GameInstance.DiceRollingBiggestNumber(s.diceRollsNumber, s.diceSides) + attacker.GetSkillsStat(s.skillToCheckInCalculations,false) / 3;
                    if (!spellsAttached.ContainsKey(s))
                    {
                        spellsAttached.Add(s, new Vector3Int(diceResult01, regenerateRate, 0));
                    }
                    else
                    {
                        spellsAttached[s] = new Vector3Int(diceResult01, regenerateRate, 0); 
                    }
                    if (buffPanels != null)
                    {
                        if (spellToApply != null) buffPanels.AddBuffToList(spellToApply, diceResult01);

                    }
                    
                    results.Add(new() { msgType = "s", msgString = heroName + " regenerate state " });
                    break;
                }
                int healroll = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
                healroll += GetSkillsStat(SkillsStat.LightMagic) + GetMainStat(MainStat.Mind) + s.diceBonus;
                currentHealth = Mathf.Clamp(currentHealth + healroll + (int)(Mathf.Pow((float)(GetMainStat(MainStat.Survival) / 4), 2)), 0, GetMaxDependedStat(DependedStat.maxHealth));

                results.Add(new() { msgType = "s", msgString = heroName + " healed " + healroll });
                ProgressBarChange();
                break;

            case SpellEffects.Restoration:

                foreach (GameplayStates st in System.Enum.GetValues(typeof(GameplayStates)))
                {
                    if (st != GameplayStates.Petrified && st != GameplayStates.Frozen && st != GameplayStates.Burning && st != GameplayStates.Regenerating && st != GameplayStates.MagicMantle)
                    {
                        gameplayStatuses.Remove(st);
                        debuffPanel.AddDebuffIcon(st, false);
                        UpdatePortraitState();
                    }
                }


                results.Add(new() { msgType = "s", msgString = heroName + " restored " });
                break;

            case SpellEffects.DSMod:
                    int sumdice = GameInstance.DiceRollingWithSkill(true, s,attacker.GetThisHero().gameObject, EnemyStat.INITIATIVE,3);
                    int dependedStatBonus = GameInstance.DiceRollingSum(s.diceRollsNumber,s.diceSides)+  attacker.GetSkillsStat(s.skillToCheckInCalculations, false) / 3;
                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(sumdice, dependedStatBonus, 0));
                    else spellsAttached[s] = new Vector3Int(sumdice, dependedStatBonus, 0);
                    if (buffPanels != null)
                    {
                        if (spellToApply != null) buffPanels.AddBuffToList(spellToApply, sumdice);

                    }
                    SetDependedStat(s.changedDependedStat, (sumdice + attacker.GetSkillsStat(s.skillToCheckInCalculations, false) / 3) + GetMaxDependedStat(s.changedDependedStat));
                break;
                

            case SpellEffects.CureState:
                if (gameplayStatuses.Contains(s.targetGamestate))
                {
                    if(GameInstance.DiceRollingBiggestNumber(s.diceRollsNumber, s.diceSides) + GetSkillsStat(SkillsStat.LightMagic)/5 < s.diceSides/2)
                    {
                        results.Add(new() { msgType = "s", msgString = attacker.HeroName() + " failed to cure " + s.targetGamestate });
                        break;
                    }
                    gameplayStatuses.Remove(s.targetGamestate);
                    if (buffPanels != null)
                    {
                        if (debuffSpells.Contains(s)) debuffSpells.Remove(s);
                        if (spellToApply != null) buffPanels.AddBuffToList(spellToApply,0);
                    }
                    if (s.targetGamestate == GameplayStates.Poisoned) poisonDamageRate = 0;

                    if (gameplayStatuses.Count == 0)
                    {
                        if (portraits.GetStatePortrait(GameplayStates.None, out Sprite stateSpriteNormal)) portrait.sprite = stateSpriteNormal;
                    }
                    else
                    {
                        if (portraits.GetStatePortrait(gameplayStatuses[0], out Sprite stateSpriteNew)) portrait.sprite = stateSpriteNew;
                    }
                }
                break;
                case SpellEffects.CauseState:
                if(s.targetGamestate== GameplayStates.MagicMantle)
                {
                    int timeOfMantle = GameInstance.DiceRollingWithSkill(true, s, attacker.GetThisHero().gameObject, EnemyStat.INITIATIVE, 3);
                    gameplayStatuses.Add(GameplayStates.MagicMantle);
                    if (buffPanels != null)
                    {
                        if (!debuffSpells.Contains(s)) debuffSpells.Add(s);
                        if (spellToApply != null) buffPanels.AddBuffToList(spellToApply, timeOfMantle);
                    }
                }
                
                break;


            case SpellEffects.ElementalWeapon:
                weaponEnchanced = s.magicType;
                int numberOfTurnsWeapon = GameInstance.DiceRollingWithSkill(true, s, gameObject,EnemyStat.INITIATIVE, 3);
                
                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(numberOfTurnsWeapon, 0,0));
                else spellsAttached[s] = new Vector3Int(numberOfTurnsWeapon, 0, 0);
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply, numberOfTurnsWeapon);

                }
                break;

            case SpellEffects.ElementalResistance:

                int diceResult = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides) + attacker.GetSkillsStat(s.skillToCheckInCalculations, false) / 3;
                int resistance = GameInstance.DiceRollingBiggestNumber(s.diceRollsNumber, s.diceSides) + attacker.GetSkillsStat(s.skillToCheckInCalculations, false) / 3;

                print("apply resistance spell "+resistance);
                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(diceResult,resistance,0));
                else spellsAttached[s] = new Vector3Int(diceResult, resistance, 0);
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply, diceResult);
                }

                break;



        }
        return results;
    }


    // This method exist for applying potions, scrolls 
    public List<ResultMsg> SingleSpellApply(Spell s, SpellContainer spellToApply, List<ResultMsg> results)
    {
        print("no one attacks with spell");
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

                foreach (GameplayStates st in System.Enum.GetValues(typeof(GameplayStates)))
                {
                    if (st == GameplayStates.Slow || st == GameplayStates.Blind || 
                        st == GameplayStates.Confused || st == GameplayStates.Poisoned)
                    {
                        gameplayStatuses.Remove(st);
                        debuffPanel.AddDebuffIcon(st, false);
                        UpdatePortraitState();
                    }
                }


                results.Add(new() { msgType = "s", msgString = heroName + " restored " });
                break;

                case SpellEffects.Revive:
                currentHealth = GetMaxDependedStat(DependedStat.maxHealth);
                ProgressBarChange();
                UpdatePortraitState();
                break;
            case SpellEffects.MSMod:
                AddToMainStat(s.changedMainStat, s.amount);
                results.Add(new() { msgType = "s", msgString = heroName + " " + s.changedMainStat + " increased by " + s.amount });

                break;
            case SpellEffects.DSMod:
                SetDependedStat(s.changedDependedStat, s.amount + GetMaxDependedStat(s.changedDependedStat));
                break;

            case SpellEffects.Antidote:


                gameplayStatuses.Remove(GameplayStates.Poisoned);
                poisonDamageRate = 0;

                UpdatePortraitState();
                results.Add(new() { msgType = "s", msgString = heroName + " cured poison" });
                break;

            case SpellEffects.CureState:
                {

                    gameplayStatuses.Remove(s.targetGamestate);
                    spellsAttached.Remove(s);
                    debuffPanel.AddDebuffIcon(s.targetGamestate, false);
                    UpdatePortraitState();
                    results.Add(new() { msgType = "s", msgString = heroName + " cured state " + s.targetGamestate });
                    break;

                }
        }
        hitTargetEffect.Invoke(spellToApply);
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
        if(attacker.GetEnemyPermanentStatus().ContainsKey(GameplayStates.Blind))
        {
            attackrollbonus = -5; 
        }
        int dice = GameInstance.DiceRollingBiggestNumber(1, 20); //attackroll and evaderoll random generation
        attackRoll = dice + attackrollbonus;
        if(attackRoll <= 0) attackRoll = 1;

        evaderoll = GameInstance.DiceRollingBiggestNumber(1, 20) + GetMaxDependedStat(DependedStat.evasion);

        results.Add(new() { msgType = "s", msgString = "Roll for " + s.spellEffect });
        results.Add(new() { msgType = "i", msgInt = attackRoll });

        results.Add(new() { msgType = "s", msgString = "/" });
        results.Add(new() { msgType = "i", msgInt = evaderoll });

        if (evaderoll > attackRoll) 
        {
            results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName()+ " missed" });
            return results; 
        } //if evasion is successful and dice is not equal 20 spell is ignored

        int pureDamageAmount = CalculateDiceSumDamage(s, dice);
        bool applyEffectSpell = GameInstance.DiceRollingBiggestNumber(s.diceRollsNumber, s.diceSides) >= s.diceSides/2;

        if (gameplayStatuses.Contains(GameplayStates.Sleep))
        {
            gameplayStatuses.Remove(GameplayStates.Sleep);
            if (gameplayStatuses.Count == 0)
            {
                if (currentHealth > 0) if (portraits.GetStatePortrait(GameplayStates.None, out Sprite stateNorm)) portrait.sprite = stateNorm;
                    else
                    {
                        portrait.sprite = deadSprite;
                    }
            }
            else
            {
                if (currentHealth > 0) if (portraits.GetStatePortrait(gameplayStatuses[gameplayStatuses.Count - 1], out Sprite stateLast)) portrait.sprite = stateLast;
            }
        }

        switch (s.spellEffect)
        {
            case SpellEffects.PDmg:

                if (spellToApply.minDistanceToEnemy == 1) pureDamageAmount += GameInstance.DiceRollingBiggestNumber(2, attacker.GetCurrentStatValue(EnemyStat.MELEE_DAMAGE));
                if (spellToApply.minDistanceToEnemy > 1 && attacker.GetEnemyRow() > 1) pureDamageAmount += GameInstance.DiceRollingBiggestNumber(2, attacker.GetCurrentStatValue(EnemyStat.RANGE_DAMAGE));
                if (spellToApply.minDistanceToEnemy > 1 && attacker.GetEnemyRow() < 1)
                {
                    pureDamageAmount -= GameInstance.DiceRollingBiggestNumber(2, attacker.GetCurrentStatValue(EnemyStat.RANGE_DAMAGE));
                    if (pureDamageAmount < 0) pureDamageAmount = 0;
                }

                int physicalDamage = pureDamageAmount - GetMaxDependedStat(DependedStat.defence);
                results.Add(new() { msgType = "s", msgString = " damage " + pureDamageAmount + " vs. defence " + GetMaxDependedStat(DependedStat.defence) });

                results.Add(new() { msgType = "s", msgString = heroName + " damage " });
                results.Add(new() { msgType = "i", msgInt = physicalDamage }); // adding final damage amount to the results list
                if (physicalDamage < 0) physicalDamage = 0;
                HealthDecrease(physicalDamage);

                break;

            case SpellEffects.MDmg:

                switch (s.magicType)
                {

                    case MagicType.Fire:
                        if (immunityList.Contains(MagicType.Fire))
                        {
                            results.Add(new() { msgType = "s", msgString = heroName + " immune to Fire magic " });
                            return results;
                        }
                        if (gameplayStatuses.Contains(GameplayStates.Frozen))
                        {
                            gameplayStatuses.Remove(GameplayStates.Frozen);
                            debuffPanel.AddDebuffIcon(GameplayStates.Frozen, false);
                            UpdatePortraitState();
                            //additional damage ???
                        }
                        int fireDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.FireResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        fireDamage = fireDamage < 0 ? 0 : fireDamage;
                        HealthDecrease(fireDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Hit " + heroName + " with Fire " + fireDamage });
                        break;
                    case MagicType.Water:
                        if (immunityList.Contains(MagicType.Water))
                        {
                            results.Add(new() { msgType = "s", msgString = heroName + " immune to Water magic " });
                            return results;
                        }
                        if (gameplayStatuses.Contains(GameplayStates.Burning))
                        {
                            gameplayStatuses.Remove(GameplayStates.Burning);
                            debuffPanel.AddDebuffIcon(GameplayStates.Burning, false);
                            UpdatePortraitState();
                            portraitOverlayAnimation.StopFXAnimation();
                            //additional damage ???
                        }
                        int waterDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.WaterResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        waterDamage = waterDamage < 0 ? 0 : waterDamage;
                        HealthDecrease(waterDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Hit " + heroName + " with water " + waterDamage });
                        break;
                    case MagicType.Air:
                        if (immunityList.Contains(MagicType.Air))
                        {
                            results.Add(new() { msgType = "s", msgString = heroName + " immune to Air magic " });
                            return results;
                        }
                        int airDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.WaterResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        airDamage = airDamage < 0 ? 0 : airDamage;
                        HealthDecrease(airDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Hit " + heroName + " with air " + airDamage });
                        break;
                    case MagicType.Earth:
                        if (immunityList.Contains(MagicType.Earth))
                        {
                            return null;
                        }
                        int earthDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.EarthResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        earthDamage = earthDamage < 0 ? 0 : earthDamage;
                        HealthDecrease(earthDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Hit " + heroName + " with Earth magic " + earthDamage });
                        break;
                    case MagicType.Light:
                        if (immunityList.Contains(MagicType.Light))
                        {
                            return null;
                        }
                        int lightDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.DarkResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        lightDamage = lightDamage < 0 ? 0 : lightDamage;
                        results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Hit " + heroName + " with Light magic " + lightDamage });
                        HealthDecrease(lightDamage);

                        break;
                    case MagicType.Dark:
                        if (immunityList.Contains(MagicType.Dark))
                        {
                            return null;
                        }
                        int darkDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.DarkResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        darkDamage = darkDamage < 0 ? 0 : darkDamage;
                        HealthDecrease(darkDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Hit " + heroName + " with Dark magic " + darkDamage });
                        break;
                        case MagicType.Ice:
                        if (immunityList.Contains(MagicType.Ice))
                        {
                            results.Add(new() { msgType = "s", msgString = heroName + " immune to Ice magic " });
                            return results;
                        }
                        if (gameplayStatuses.Contains(GameplayStates.Burning))
                        {
                            gameplayStatuses.Remove(GameplayStates.Burning);
                            debuffPanel.AddDebuffIcon(GameplayStates.Burning, false);
                            UpdatePortraitState();
                            portraitOverlayAnimation.StopFXAnimation();
                            //additional damage ???
                        }
                        int iceDamage = pureDamageAmount - (GetMaxDependedStat(DependedStat.WaterResistance) + (GetMainStat(MainStat.Willpower) / 5));
                        iceDamage = iceDamage < 0 ? 0 : iceDamage;
                        HealthDecrease(iceDamage);
                        results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Hit " + heroName + " with water " + iceDamage });

                        break;
                }
            break;

            case SpellEffects.MSMod:


                if(attacker.GetCurrentStatValue(EnemyStat.DARK_DAMAGE)< GetMaxDependedStat(DependedStat.DarkResistance))
                {
                    results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Failed to decrease main stats "  });
                    break;
                }

                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(s.numberOfTurns, -s.amount, 0));
                else spellsAttached[s] = new Vector3Int(s.numberOfTurns, s.amount, 0);
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply, s.numberOfTurns);
                }
            break;

            case SpellEffects.DSMod:
                if (attacker.GetCurrentStatValue(EnemyStat.DARK_DAMAGE) < GetMaxDependedStat(DependedStat.DarkResistance))
                {
                    results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Failed to decrease depended stats " });
                    break;
                }

                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(s.numberOfTurns, -s.amount, 0));
                else spellsAttached[s] = new Vector3Int(s.numberOfTurns, -s.amount, 0);
                if (buffPanels != null)
                {
                    if (spellToApply != null) buffPanels.AddBuffToList(spellToApply, s.numberOfTurns);

                }

            break;



            case SpellEffects.CauseState:

                if (!gameplayStatuses.Contains(s.targetGamestate))
                {

                    if(gameplayStatuses.Contains(GameplayStates.MagicMantle))
                    {
                        if(GameInstance.DiceRollingBiggestNumber(s.numberOfTurns, s.diceSides)+GetSkillsStat(SkillsStat.DarkMagic)/5 < s.diceSides / 2)
                        {
                            results.Add(new() { msgType = "s", msgString = heroName + " magic mantle protected from " + s.targetGamestate });
                            break;
                        }

                    }
                    int diceResult = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides) + s.diceBonus;
                    int resistance = GameInstance.DiceRollingBiggestNumber(s.diceRollsNumber, s.diceSides) + s.diceBonus;

                    switch (s.targetGamestate)
                    {
                    case GameplayStates.Burning:
                        if (applyEffectSpell)
                        {
                                if (gameplayStatuses.Contains(GameplayStates.Frozen))
                                {
                                    gameplayStatuses.Remove(GameplayStates.Frozen);
                                    UpdatePortraitState();
                                    debuffPanel.AddDebuffIcon(GameplayStates.Frozen, false);
                                    //additional damage ???
                                }

                                gameplayStatuses.Add(GameplayStates.Burning);
                                if (portraits.IsAnimatedState(GameplayStates.Burning, out List<Sprite> listSprites))
                                {
                                    portraitOverlayAnimation.FillSpriteList("Burning",listSprites);
                                    portraitOverlayAnimation.StartFXAnimation("Burning");
                                }
                                else
                                {
                                    if (portraits.GetStatePortrait(GameplayStates.Burning, out Sprite stateSpriteBurning)) portrait.sprite = stateSpriteBurning;
                                }
                                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(diceResult, resistance, 0));
                                else spellsAttached[s] = new Vector3Int(diceResult, resistance, 0);
                                debuffPanel.AddDebuffIcon(GameplayStates.Burning, true);
                        }
                    break;

                    case GameplayStates.Frozen:
                        if (applyEffectSpell)
                        {
                                if (gameplayStatuses.Contains(GameplayStates.Burning))
                                {
                                    gameplayStatuses.Remove(GameplayStates.Burning);
                                    UpdatePortraitState();
                                    debuffPanel.AddDebuffIcon(GameplayStates.Burning, false);
                                    portraitOverlayAnimation.StopFXAnimation();
                                }

                                gameplayStatuses.Add(GameplayStates.Frozen);
                            if (portraits.GetStatePortrait(GameplayStates.Frozen, out Sprite stateSpriteFrozen)) portrait.sprite = stateSpriteFrozen;

                                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(diceResult, resistance, 0));
                                else spellsAttached[s] = new Vector3Int(diceResult, resistance, 0);
                            }
                            debuffPanel.AddDebuffIcon(GameplayStates.Frozen, true);
                        break;

                    case GameplayStates.Petrified:

                            if (gameplayStatuses.Contains(GameplayStates.Frozen))
                            {
                                gameplayStatuses.Remove(GameplayStates.Frozen);
                                debuffPanel.AddDebuffIcon(GameplayStates.Frozen, false);
                                UpdatePortraitState();
                            }
                            if (gameplayStatuses.Contains(GameplayStates.Burning))
                            {
                                gameplayStatuses.Remove(GameplayStates.Burning);
                                UpdatePortraitState();
                                debuffPanel.AddDebuffIcon(GameplayStates.Burning, false);
                                portraitOverlayAnimation.StopFXAnimation();
                            }
                            if (gameplayStatuses.Contains(GameplayStates.Sleep))
                            {
                                gameplayStatuses.Remove(GameplayStates.Sleep);
                                UpdatePortraitState();
                                debuffPanel.AddDebuffIcon(GameplayStates.Sleep, false);
                            }

                            if (!gameplayStatuses.Contains(GameplayStates.Petrified))
                            {

                            if (applyEffectSpell)
                            {
                                gameplayStatuses.Add(GameplayStates.Petrified);
                                if (portraits.GetStatePortrait(GameplayStates.Petrified, out Sprite stateSpritePetrified)) portrait.sprite = stateSpritePetrified;

                            }
                                debuffPanel.AddDebuffIcon(GameplayStates.Petrified, true);
                            }

                        break;
                        case GameplayStates.Poisoned:
                            if (!gameplayStatuses.Contains(GameplayStates.Poisoned))
                            {
                                if (applyEffectSpell)
                                {
                                    gameplayStatuses.Add(GameplayStates.Poisoned);
                                    if (portraits.GetStatePortrait(GameplayStates.Poisoned, out Sprite stateSpritePoisoned)) portrait.sprite = stateSpritePoisoned;

                                    if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(diceResult, resistance, 0));
                                    else spellsAttached[s] = new Vector3Int(diceResult, resistance, 0);
                                    debuffPanel.AddDebuffIcon(GameplayStates.Poisoned, true);
                                }
                            }
                        break;
                        case GameplayStates.Sleep:
                            if (!gameplayStatuses.Contains(GameplayStates.Sleep))
                            {
                                if (applyEffectSpell)
                                {
                                    gameplayStatuses.Add(GameplayStates.Sleep);
                                    if (portraits.GetStatePortrait(GameplayStates.Sleep, out Sprite stateSpriteSleep)) portrait.sprite = stateSpriteSleep;

                                    debuffPanel.AddDebuffIcon(GameplayStates.Sleep, true);
                                }
                            }
                        break;

                        case GameplayStates.Blind:

                            if (!gameplayStatuses.Contains(GameplayStates.Blind))
                            {

                                if (applyEffectSpell)
                                {
                                    gameplayStatuses.Add(GameplayStates.Blind);
                                    if (portraits.GetStatePortrait(GameplayStates.Blind, out Sprite stateSpriteSleep)) portrait.sprite = stateSpriteSleep;

                                    debuffPanel.AddDebuffIcon(GameplayStates.Blind, true);
                                }
                            }
                            break;

                        case GameplayStates.Slow:

                            if (attacker.GetCurrentStatValue(EnemyStat.DARK_DAMAGE) < GetMaxDependedStat(DependedStat.DarkResistance))
                            {
                                results.Add(new() { msgType = "s", msgString = attacker.GetEnemyName() + " Failed to decrease depended stats " });
                                break;
                            }
                            int slowTurnAmount = GameInstance.DiceRollingWithSkill(true, s, attacker.GetEnemyGameObject().gameObject, EnemyStat.RANGE_DAMAGE, 3);
                            int slowAmount = GameInstance.DiceRollingWithSkill(true, s, attacker.GetEnemyGameObject().gameObject, EnemyStat.RANGE_DAMAGE, 3);

                            if (applyEffectSpell)
                            {
                                gameplayStatuses.Add(GameplayStates.Slow);
                                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(s.numberOfTurns, Mathf.Clamp(-s.amount, 0, int.MaxValue), 0));
                                else spellsAttached[s] = new Vector3Int(s.numberOfTurns, Mathf.Clamp(-s.amount, 0, int.MaxValue), 0);

                                debuffPanel.AddDebuffIcon(GameplayStates.Sleep, true);
                            }
                            break;
                        case GameplayStates.Confused:
                            

                            if (applyEffectSpell)
                            {
                                int confusedTurnAmount = GameInstance.DiceRollingWithSkill(true, s, attacker.GetEnemyGameObject().gameObject, EnemyStat.RANGE_DAMAGE, 3);
                               gameplayStatuses.Add(GameplayStates.Confused);
                                if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, new Vector3Int(confusedTurnAmount+20, 0, 0));
                                else spellsAttached[s] = new Vector3Int(confusedTurnAmount+20, 0, 0);

                                debuffPanel.AddDebuffIcon(GameplayStates.Confused, true);
                            }

                            break;
                    }
                }
            break;
        }
        if(showDamageEffect) hitTargetEffect.Invoke(spellToApply);
        showDamageEffect = false;
        return results;
    }



    void ProgressBarChange()
    {
        healthSlider.ProgressBarFill((float)currentHealth / (float)GetMaxDependedStat(DependedStat.maxHealth));
    }

    IEnumerator AttackDelay()
    {
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
        if (amount > 0)
        {
            showDamageEffect = true;
        }
    }

    public void ManaDecrease(int amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, GetMaxDependedStat(DependedStat.maxMana));
        manaSlider.ProgressBarFill((float)currentMana / (float)GetMaxDependedStat(DependedStat.maxMana));
    }

    int GetMainStat(MainStat mainStat, bool purestat = false)
    {
        if (mainStatContainer.Count <= 0) return 0;

        int statInt = mainStatContainer[mainStat];

        if (purestat) { return Mathf.Clamp(statInt, 0, int.MaxValue); }

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

        foreach (KeyValuePair< Spell,Vector3Int> s in spellsAttached)
        {

            if (s.Key.changedMainStat == mainStat)
            {
                statInt += s.Value.y;
            }
        }
        return Mathf.Clamp(statInt, 0, int.MaxValue);
    }



    public void AddToMainStat(MainStat mainStat, int amount)
    {
        if (!mainStatContainer.TryAdd(mainStat, amount))
        {
            mainStatContainer[mainStat] = mainStatContainer[mainStat] + amount;
        }

    }

     int IncreasingCoeficientFromSkillLearned(bool physicSkill)
    {
        int skillsum = 0;

        if (physicSkill)
        {
            skillsum += GetSkillsStat(SkillsStat.BluntWeapons, true);
            skillsum += GetSkillsStat(SkillsStat.BladedWeapons, true);
            skillsum += GetSkillsStat(SkillsStat.RangedWeapons, true);
            skillsum += GetSkillsStat(SkillsStat.Polearms, true);
            skillsum += GetSkillsStat(SkillsStat.HeavyArmour, true);
            skillsum += GetSkillsStat(SkillsStat.LightArmour, true);
            return (int)skillsum / 6;
        }
        else
        {
            skillsum += GetSkillsStat(SkillsStat.LightMagic, true);
            skillsum += GetSkillsStat(SkillsStat.ElementalMagic, true);
            skillsum += GetSkillsStat(SkillsStat.DarkMagic, true);
            skillsum += GetSkillsStat(SkillsStat.Identify, true);
            skillsum += GetSkillsStat(SkillsStat.SpotSecret, true);
  
            return (int)skillsum / 5;
        }
    }

    public int GetMaxDependedStat(DependedStat dependedStat)
    {
        if (dependedStatsDefault.Count == 0) return 0;
        if (!dependedStatsDefault.ContainsKey(dependedStat)) return 0;
        int statInt = dependedStatsDefault[dependedStat];
        if (dependedStat == DependedStat.meleeDamage || dependedStat == DependedStat.rangeDamage) print("base stat " + dependedStat + " " + statInt);
        switch (dependedStat)
        {
            case DependedStat.maxHealth:
                statInt += ((GetMainStat(MainStat.Strength) / 5) + (GameInstance.party.GetPartyLevel()/5) + IncreasingCoeficientFromSkillLearned(true)) * 10;
                break;
            case DependedStat.maxMana:
                statInt += ((GetMainStat(MainStat.Mind) / 5) + (GameInstance.party.GetPartyLevel() / 5) + IncreasingCoeficientFromSkillLearned(false)) * 10;
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

            case DependedStat.meleeDamage:
                if (GetHeroWeight() < GetMaxDependedStat(DependedStat.CarryingCapacity))
                {                    
                    //print ("melee damage stat increased by main stat and skill " + statInt +" "+ (GetMainStat(MainStat.Strength) / 5)+" "+ GetSkillsStat(GetWeaponType()));
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
       //if(dependedStat == DependedStat.meleeDamage || dependedStat == DependedStat.rangeDamage  ) print("modified stat " + dependedStat + " " + statInt);

        foreach (KeyValuePair<ItemType, SpellContainer> k in equipmentSpells)
        {
            int x = statInt;
            foreach (Spell s in k.Value.spells)
            {

                if (s.changedDependedStat == dependedStat)
                {
                    statInt += s.amount;
                }

            }
            //if (statInt != x) print("depended stat " + dependedStat + " changed by spell " + statInt + " - " + k.Value.spellName);
        }


        foreach (KeyValuePair<Spell, Vector3Int> s in spellsAttached)
        {
            //print("depended stat before spell  " + dependedStat + " " + statInt);
            int x = statInt;


            if (s.Key.changedDependedStat == dependedStat)
            {
                if(s.Value.y != 0) statInt += s.Value.y;

            }
             if(statInt != x) print("depended stat " + dependedStat + " changed by spell " + statInt + " - " + s.Key.spellEffect);
        }

        return Mathf.Clamp(statInt,0,int.MaxValue);
    }


    public void RecordSkillUsed(SkillsStat _skill)
    {
        if (!skillsUsedInGameplay.ContainsKey(_skill)) skillsUsedInGameplay.Add(_skill, 1);
        else skillsUsedInGameplay[_skill] = skillsUsedInGameplay[_skill] + 1;
    }

    public int GetSkillsStat(SkillsStat skillStat, bool pureStat = false)
    {
        if (dependedStatsDefault.Count == 0) return 0;
        if (skillStat == SkillsStat.None) return 0;
        skillsStatsCurrent.TryGetValue(skillStat, out int st);
        int statInt = 0;
        statInt += st;

        if (!pureStat) { 
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

            foreach (KeyValuePair<Spell, Vector3Int> s in spellsAttached)
            {

                if (s.Key.skillStatAdded == skillStat)
                {
                    statInt += s.Value.y;
                }
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
        //print("Mind stat " + GetMainStat(MainStat.Mind));
        statListTemp.Add(MainStat.Strength, GetMainStat(MainStat.Strength));
        statListTemp.Add(MainStat.Agility,  GetMainStat(MainStat.Agility));
        statListTemp.Add(MainStat.Mind,     GetMainStat(MainStat.Mind));
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
            if(d != DependedStat.None) statListTemp.Add(d, GetMaxDependedStat(d));

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


  
    public bool AddEquipmentToCharacter(HeroInventoryItem heroInventoryItem, ItemType _itemType)
    {

        if (heroInventoryItem == null) return false;

        //heroInventoryItem.savedState = SavedState.Equipment;

        if (!equipmentWithGUID.TryAdd(_itemType, heroInventoryItem))
        {
            heroInventoryItem.heroIndex = heroID;
            equipmentWithGUID[_itemType] = heroInventoryItem;

        }

        if(equipmentSpells.TryGetValue(_itemType, out SpellContainer _spell))
        {
            if (_spell == GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer) return true;
        }
        
        if (!equipmentSpells.TryAdd(_itemType, GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer))
        {
                //print("light added " + GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer.gameplaySpell); 

            equipmentSpells[_itemType] = GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer;
            if (GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer.gameplaySpell)
            {
                GameInstance.spellbook.CastSpell(GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer);
                foreach(Spell _s in GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer.spells)
                {
                    spellsAttached.Add(_s, new Vector3Int( _s.numberOfTurns,0,0));
                }
            }
        }
        else
        {
            if (GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer.gameplaySpell)
            {
                GameInstance.spellbook.CastSpell(GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer);
                foreach (Spell _s in GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).spellContainer.spells)
                {
                    spellsAttached.Add(_s, new Vector3Int(_s.numberOfTurns, 0, 0));
                }
            }
        }
        return true;
    }

    public void RemoveItemFromEquipment(ItemType itemType)
    {
        if(equipmentSpells.ContainsKey(itemType)) print("remove "+ equipmentSpells[itemType]);
        if (equipmentSpells.ContainsKey(itemType))
        {
            foreach (Spell spell in equipmentSpells[itemType].spells)
            {
                if (spell.spellEffect == SpellEffects.LightARoom)
                {

                    spellsAttached.Remove(spell);
                    print("remove light " + GameInstance.spellbook.CheckHeroesForLightSource());
                    if (!GameInstance.spellbook.CheckHeroesForLightSource()) GameInstance.spellbook.LightOff();

                }
            }
        }

        equipmentSpells.Remove(itemType);
        equipmentWithGUID.Remove(itemType);
        GameInstance.party.RefreshUI.Invoke();
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
        if (equipmentWithGUID[ItemType.WEAPON] != null) 
        { 
            return GameInstance.dataBase.GetItemFromBaseByIndex(equipmentWithGUID[ItemType.WEAPON].container).weaponType; 
        }
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
                    currentHunger -= 1;
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


        if (gameplayStatuses.Contains(GameplayStates.Regenerating))
        {
            if (currentHealth > 0)
            {
                int regen = 0;
                foreach (Spell s in spellsAttached.Keys)
                {
                    if (s.spellEffect == SpellEffects.Heal && s.continuousSpell)
                    {
                        regen = GameInstance.DiceRollingBiggestNumber(1, spellsAttached[s].y);
                    }
                }
                print("regen "  +regen);
                HealthDecrease(-regen);
            }
        }

            if (spellsAttached.Count <= 0) return;
            List<Spell> listToDelete = new List<Spell>();
            List<Spell> listToChange = new List<Spell>();

            foreach (KeyValuePair<Spell, Vector3Int> s in spellsAttached)
            {
                if (spellsAttached[s.Key].x > 0) 
                { 
                    listToChange.Add(s.Key); 
                    if(s.Key.targetGamestate == GameplayStates.Burning)
                    {
                    print("burning damage " + spellsAttached[s.Key].y);
                        HealthDecrease(Random.Range(0,spellsAttached[s.Key].y)); 
                    }
                    if (s.Key.targetGamestate == GameplayStates.Poisoned)
                    {
                       if(!gameplayStatuses.Contains(GameplayStates.Petrified)) HealthDecrease(Random.Range(0, spellsAttached[s.Key].y));
                    }
            }
                else listToDelete.Add(s.Key);
            }

            foreach (Spell s in listToChange)
            {
                int x = spellsAttached[s].x;
                spellsAttached[s] = new Vector3Int (x - 1, spellsAttached[s].y,0) ;
            }

            foreach (Spell s in listToDelete)
        {
            switch (s.spellEffect)
            {

                case SpellEffects.ElementalWeapon:
                    weaponEnchanced = MagicType.None;
                    break;

                case SpellEffects.LightARoom:
                    RemoveItemFromEquipment(ItemType.SHIELD);

                    break;
                case SpellEffects.ElementalResistance:


                    break;
                case SpellEffects.Heal:

                    gameplayStatuses.Remove(GameplayStates.Regenerating);
                    break;

            }


            gameplayStatuses.Remove(s.targetGamestate);
            portraitOverlayAnimation.StopFXAnimation();
            debuffPanel.AddDebuffIcon(s.targetGamestate, false);

            if (currentHealth <= 0)
            {
                foreach(GameplayStates gs in gameplayStatuses)
                {
                    debuffPanel.AddDebuffIcon(gs, false);
                }
                gameplayStatuses.Clear();
            }

            UpdatePortraitState();

            spellsAttached.Remove(s);
        }


    }

    private void UpdatePortraitState()
    {
        if (gameplayStatuses.Count == 0)
        {
            if (currentHealth > 0) 
            { 
                if (portraits.GetStatePortrait(GameplayStates.None, out Sprite stateNorm)) portrait.sprite = stateNorm;
                debuffPanel.ClearAllDebuffs();
                portraitOverlayAnimation.StopFXAnimation();
            }
            else
            {
                portrait.sprite = deadSprite;
            }
        }
        else
        {
            if (currentHealth > 0) { if (portraits.GetStatePortrait(gameplayStatuses[0], out Sprite stateLast)) portrait.sprite = stateLast; }
            else portrait.sprite = deadSprite;
            if (gameplayStatuses.Contains(GameplayStates.Burning)) portraitOverlayAnimation.StartFXAnimation("Burning");
        }
    }

    public SpellContainer GetInfusedWeaponSpell()
    {
        return null;
    }

    public List<GameplayStates> GetHeroStatus()
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
        if(lastSpell == null)
        {
            GameInstance.spellbook.ResultsToBattleLog(new() { "" }, new List<ResultMsg>() { new() { msgType = "s", msgString = heroName + " has no spell to cast! " } });
            return null;
        }
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
        currentHunger= GetMaxDependedStat(DependedStat.Hunger);
        GameInstance.party.RefreshUI.Invoke();
    }

    public void FeedHeroInit()
    {
        currentHunger = GetMaxDependedStat(DependedStat.Hunger);
    }


    public float GetHungerLevelPercents()
    {
       // print("hunger level " + GetMaxDependedStat(DependedStat.Hunger)+" - "+ dependedStatsDefault[DependedStat.Hunger]);
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
    public int GetHeroMana() { 
        return currentMana;
    }
    public int GetHeroHunger()
    {
        return currentHunger;
    }


    public void AddSpellToSpellAttached(Spell savedspells, int timeToFinish)
    {

    }

}


public interface IHero
{
    public List<SpellContainer> GetActiveHeroSpellbook();
    public bool AddEquipmentToCharacter(HeroInventoryItem heroInventoryItem, ItemType _itemType);

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
    public int GetHeroMana();
    public int GetHeroHunger();
    public Hero GetThisHero();
    public int GetHeroAgro();
    public void ChangeArgo(int amount);

    public SpellContainer GetWeaponSpell();
    public SpellContainer GetInfusedWeaponSpell();
    public string HeroName();
    public int GetMaxDependedStat(DependedStat dependedStat);
    public int GetSkillsStat(SkillsStat skillStat, bool pureStat);

    public int MagicDamageModifier(SkillsStat skillStat);
    public List<GameplayStates> GetHeroStatus();

    public MagicType GetWeaponMagicType();

    public int GetHeroIndex();
    public int GetHeroWeight();
    public void HealthDecrease(int amount);
    public float GetHungerLevelPercents();
    public void RecordSkillUsed(SkillsStat _skill);
    public int GetSkillPoints();
    public void AddExtraSkillPoints(int amount);

    public void SetSKillStat(SkillsStat _skillStat, int amount);
    public void AddToMainStat(MainStat _mainStat, int amount);
    public void SetSkillPoints(int amount);


    public void GetPureStats(out Dictionary<MainStat, int> _mainStats, out Dictionary<DependedStat, int> _dependStats, out Dictionary<SkillsStat, int> _skillStats);
    public Dictionary<Spell, Vector3Int> GetSpellsAttached();

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


public enum GameplayStates
{
    None,
    Frozen,//
    Burning,//
    Poisoned,//
    Stunned,
    Petrified,//
    Dead,
    Stoned,
    Paralized,
    Blind,//
    Regenerating,  
    Confused,//
    MagicMantle, 
    Sleep,//
    Slow//
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