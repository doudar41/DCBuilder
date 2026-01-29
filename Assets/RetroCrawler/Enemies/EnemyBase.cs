using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Ami.BroAudio;

public class EnemyBase : MonoBehaviour, IEnemy, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBattle
{
    [SerializeField] string enemyName;
    [SerializeField] int rowIndex;
    [SerializeField] List<int> placeIndexes;
    [SerializeField] int health = 100;
    [SerializeField] SpriteRenderer enemyFace, outlineRenderer;
    [SerializeField] PortraitContainer enemySprites;

    [SerializeField] List<SpellContainer> attackSpells = new List<SpellContainer>();
    [SerializeField] Collider col;
    [SerializeField] int enemySize;

    [SerializeField] 
    List<EnemyStatInitAmount> enemyStatInitAmounts = new List<EnemyStatInitAmount>() {
    new () { enemyStat = EnemyStat.INITIATIVE, initialAmount = 1 },
    new () { enemyStat = EnemyStat.ACCURACY, initialAmount = 0 },
    new () { enemyStat = EnemyStat.EVASION, initialAmount = 1 },
    new () { enemyStat = EnemyStat.DEFENCE, initialAmount = 0 },
    new () { enemyStat = EnemyStat.FIRE_RESISTANCE, initialAmount = 0 },
    new () { enemyStat = EnemyStat.WATER_RESISTANCE, initialAmount = 0 },
    new () { enemyStat = EnemyStat.EARTH_RESISTANCE, initialAmount = 0 },
    new () { enemyStat = EnemyStat.AIR_RESISTANCE, initialAmount = 0 },
    new () { enemyStat = EnemyStat.DARK_RESISTANCE, initialAmount = 0 },
    new () { enemyStat = EnemyStat.ICE_RESISTANCE, initialAmount = 0 },
    new () { enemyStat = EnemyStat.MELEE_DAMAGE, initialAmount = 0 },
    new () { enemyStat = EnemyStat.RANGE_DAMAGE, initialAmount = 0 }

    };

    [SerializeField] List<MagicType> magicTypeImmunityList = new List<MagicType>();
    [SerializeField] List<SpellEffects> spellEffectImmunityList = new List<SpellEffects>();


    Dictionary<SpellEffects,int> appliedPermanentDebuffs= new Dictionary<SpellEffects, int>();

    int healthStarted;

    public UnityEvent<float> healthNormalized;
    public UnityEvent<SpellContainer> hitTargetEffect;
    public UnityEvent<string> playStatusAnimation;

    [SerializeField] SpellContainer immunityspell;
    [SerializeField] SoundID upFrontSound, attackSound;
    [SerializeField] int experienceReward = 50;
    [SerializeField] List<ItemScriptableContainer>  itemScriptableContainers = new List<ItemScriptableContainer>();
    [SerializeField] List<int> itemsStackAmout;
    [SerializeField] Vector2Int moneyRandomRange, gemsRandomRange, randomLootPossibility;

    Dictionary<EnemyStat, Vector3Int> currentStats = new Dictionary<EnemyStat, Vector3Int>();

    Vector3 savedPosition;



    private void Awake()
    {
        foreach (EnemyStatInitAmount es in enemyStatInitAmounts)
        {
            currentStats.Add(es.enemyStat, new Vector3Int(es.initialAmount, es.initialAmount, 0));
        }
    }

    private void Start()
    {


        outlineRenderer.color = Color.clear;
        healthStarted = health;
        GameInstance.battleManager.battlePassTime += Timepassed;

    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        GameInstance.battleManager.battlePassTime -= Timepassed;
    }

    public int HealthDamage(int amount)
    {
        health = Mathf.Clamp(health - amount, 0, int.MaxValue);

        healthNormalized.Invoke(1-((float)health / (float)healthStarted));

        if (health <= 0)
        {
            col.enabled = false;
            gameObject.tag = "Untagged";
            health = 0;
            if(enemySprites.GetStatePortrait(GameplayStatus.Dead, out Sprite deadSprite))
            {
                outlineRenderer.gameObject.SetActive(false);
                enemyFace.gameObject.SetActive(true);
                enemyFace.sprite = deadSprite;
            }
        }
        return health;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (health > 0) GameInstance.spellbook.spellTargetEvent.Invoke(this.gameObject);
    }


    public void MoveAheadOnAttack()
    {

        savedPosition = transform.localPosition;
        transform.localPosition = new Vector3(savedPosition.x, savedPosition.y, savedPosition.z - 0.05f);
        transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        StartCoroutine(WaitAndBack());
        GameInstance.soundManagerInGame.ProtectedPlay(upFrontSound);
        GameInstance.soundManagerInGame.ProtectedPlay(attackSound);
        //animator.SetTrigger("Attack");

    }

    IEnumerator WaitAndBack()
    {
        yield return new WaitForSeconds(1f);
        MoveBackAfterAttack();
       // animator.SetBool("Idle", true);
    }

    public void MoveBackAfterAttack()
    {
        transform.localPosition = savedPosition;
        transform.localScale = new Vector3(1, 1, 1);
    }

    public SpellContainer enemyAttack(int distanceToHero)
    {
        foreach(SpellContainer sc in attackSpells)
        {
            if(sc.minDistanceToEnemy >= distanceToHero)
            {
                return sc;
            }
        }

        return attackSpells[Random.Range(0, attackSpells.Count)];
    }

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(0.5f);
        //print("stop attacking enemy");
        GameInstance.battleManager.AttackEnding();
        if(health <= 0)
        {
            SetEnemyRowAndPlace(-1, new List<int> { });
            if(gameObject!=null) GameInstance.battleManager.RemoveDeadEnemy(gameObject);
        }

    }

    public List<ResultMsg> ApplySpellToEnemy(SpellContainer spellToApply, GameObject spellcaster) // applying spell to enemy and calculating damage and effects
    {
        if (spellToApply == null) { if (!spellToApply.AOE) StartCoroutine(AttackDelay()); return null; }
        List<ResultMsg> results = new List<ResultMsg>();
        int attackRoll = 0;
        int attackrollbonus = 0, evaderoll = 0;

        IHero hero = spellcaster.GetComponent<IHero>();

        if (hero != null) 
        {
            attackrollbonus = hero.GetMaxDependedStat(DependedStat.accuracy);  // agillity modifier + endurance modifier of attacker
        }


        foreach (Spell _spell in spellToApply.spells)
        {

            if (magicTypeImmunityList.Contains(_spell.magicType)) // skip magic attack if enemy has a total immunity to it 
            {
                hitTargetEffect.Invoke(immunityspell); // Event for showing immunity animation
                continue;
            }

            int dice = GameInstance.DiceRollingBiggestNumber(1, 20); //attackroll and evaderoll random generation
            attackRoll = dice + attackrollbonus;
            evaderoll = GameInstance.DiceRollingBiggestNumber(1, 20) + currentStats[EnemyStat.EVASION].y;

            results.Add(new() { msgType = "s", msgString = "AR "+ _spell.spellEffect});
            results.Add(new() { msgType = "i", msgInt = attackRoll});

            results.Add(new() { msgType = "s", msgString = "/" });
            results.Add(new() { msgType = "i", msgInt = evaderoll });  // attackroll and evaderoll added to list to be used in the battle log

            if (evaderoll > attackRoll) 
            {
                results.Add(new() { msgType = "s", msgString = "missed"});
                continue; 
            }//if evasion is successful and dice is not equal 20 spell is ignored


            int damageAmountDiceSumResult = CalculateIncomingDamage(_spell, dice); // calculating damage according to dice rolls
            
            int diceToCompare = GameInstance.DiceRollingBiggestNumber(_spell.diceRollsNumber, _spell.diceSides); // dice to compare with spell effect application chance


            switch (_spell.spellEffect) //spellEffects is enum used to list all spell effects 
            {
                case SpellEffects.PDmg: // Most weapon damage calculation

                    // Rolling dices according with stored in a spell scriptable object numbers + make it second time if dice is equal 20

                    if (spellToApply.minDistanceToEnemy > 1) //Add bonus to a range or melee attack
                    {
                        if(rowIndex > 1)
                        {
                            damageAmountDiceSumResult += hero.GetMaxDependedStat(DependedStat.rangeDamage);// if enemy is in the back rows range damage is used
                        }
                        else
                        {
                            damageAmountDiceSumResult -= hero.GetMaxDependedStat(DependedStat.rangeDamage);// if enemy is in the front row range damage is subtracted from total damage
                            if(damageAmountDiceSumResult<0) damageAmountDiceSumResult = 0;
                        }
                    }
                    else
                    {
                        damageAmountDiceSumResult += hero.GetMaxDependedStat(DependedStat.meleeDamage); // if enemy is in the front row melee damage is used, Spellbook preventing from using melee spells on back rows enemies
                    }

                    // If weapon damage was change to a elemental checking if enemy has an immunity to it

                    if (magicTypeImmunityList.Contains(spellcaster.GetComponent<IHero>().GetWeaponMagicType()))
                    {
                        hitTargetEffect.Invoke(immunityspell); // Event for showing immunity animation
                        continue;
                    }

                    // If weapon enchanced with element all damage become an elemental damage
                    damageAmountDiceSumResult += GameInstance.DiceRollingBiggestNumber(1, spellcaster.GetComponent<IHero>().GetSkillsStat(_spell.skillToCheckInCalculations));

                    damageAmountDiceSumResult = ApplyMagicResistanceToWeapon(spellcaster, damageAmountDiceSumResult);

                    results.Add(new() { msgType = "s", msgString = enemyName + " damaged " });
                    results.Add(new() { msgType = "i", msgInt = damageAmountDiceSumResult }); // adding final damage amount to the results list
                    HealthDamage(damageAmountDiceSumResult); // applying damage to enemy health
                    spellcaster.GetComponent<IHero>().RecordSkillUsed(_spell.skillToCheckInCalculations);
                    break;

                case SpellEffects.MDmg:

                    damageAmountDiceSumResult = MagicDamageApply(spellcaster, _spell, damageAmountDiceSumResult);
                    results.Add(new() { msgType = "i", msgInt = damageAmountDiceSumResult });
                    spellcaster.GetComponent<IHero>().RecordSkillUsed(_spell.skillToCheckInCalculations);
                    break;

                case SpellEffects.DSMod: // if spell is modifying depended stat of enemy (ex. accuracy, evasion, resistances etc.)

                    if(damageAmountDiceSumResult < (_spell.diceRollsNumber * _spell.diceSides)/2)
                    {
                        continue; // if rolled damage is less than half of maximum possible damage debuff is not applied
                    }
                    damageAmountDiceSumResult += spellcaster.GetComponent<IHero>().MagicDamageModifier(_spell.skillToCheckInCalculations);
                    print("damage calculated " + damageAmountDiceSumResult);
                    switch (_spell.changedDependedStat)
                    {
                        case DependedStat.initiative:
                            currentStats[EnemyStat.INITIATIVE] = new Vector3Int(currentStats[EnemyStat.INITIATIVE].x, currentStats[EnemyStat.INITIATIVE].x - damageAmountDiceSumResult, _spell.numberOfTurns);
                            print("slow enemy "+ currentStats[EnemyStat.INITIATIVE].x +"/"+ currentStats[EnemyStat.INITIATIVE].y);
                            results.Add(new() { msgType = "s", msgString = enemyName + " slowdown " });

                            break;
                        case DependedStat.accuracy:
                            currentStats[EnemyStat.ACCURACY] = new Vector3Int(currentStats[EnemyStat.ACCURACY].x, Mathf.Clamp( currentStats[EnemyStat.ACCURACY].x - damageAmountDiceSumResult,0,int.MaxValue), _spell.numberOfTurns);
                            results.Add(new() { msgType = "s", msgString = enemyName + " accuracy decreased " });
                            break;
                        case DependedStat.defence:
                            currentStats[EnemyStat.DEFENCE] = new Vector3Int(currentStats[EnemyStat.DEFENCE].x, Mathf.Clamp(currentStats[EnemyStat.DEFENCE].x - damageAmountDiceSumResult, 0, int.MaxValue), _spell.numberOfTurns);
                            
                            break;
                        case DependedStat.evasion:
                            currentStats[EnemyStat.EVASION] = new Vector3Int(currentStats[EnemyStat.EVASION].x, Mathf.Clamp(currentStats[EnemyStat.EVASION].x - damageAmountDiceSumResult, 0, int.MaxValue), _spell.numberOfTurns);
                            break;
                        case DependedStat.FireResistance:
                            currentStats[EnemyStat.FIRE_RESISTANCE] = new Vector3Int(currentStats[EnemyStat.FIRE_RESISTANCE].x, currentStats[EnemyStat.FIRE_RESISTANCE].x - damageAmountDiceSumResult, _spell.numberOfTurns);
                            break;
                        case DependedStat.WaterResistance:
                            currentStats[EnemyStat.WATER_RESISTANCE] = new Vector3Int(currentStats[EnemyStat.WATER_RESISTANCE].x, currentStats[EnemyStat.WATER_RESISTANCE].x - damageAmountDiceSumResult, _spell.numberOfTurns);
                            break;
                        case DependedStat.EarthResistance:
                            currentStats[EnemyStat.EARTH_RESISTANCE] = new Vector3Int(currentStats[EnemyStat.EARTH_RESISTANCE].x, currentStats[EnemyStat.EARTH_RESISTANCE].x - damageAmountDiceSumResult, _spell.numberOfTurns);
                            break;
                        case DependedStat.AirResistance:
                            currentStats[EnemyStat.AIR_RESISTANCE] = new Vector3Int(currentStats[EnemyStat.AIR_RESISTANCE].x, currentStats[EnemyStat.AIR_RESISTANCE].x - damageAmountDiceSumResult, _spell.numberOfTurns);
                            break;
                        case DependedStat.DarkResistance:
                            currentStats[EnemyStat.DARK_RESISTANCE] = new Vector3Int(currentStats[EnemyStat.DARK_RESISTANCE].x, currentStats[EnemyStat.DARK_RESISTANCE].x - damageAmountDiceSumResult, _spell.numberOfTurns);
                            break;

                        case DependedStat.meleeDamage:
                            currentStats[EnemyStat.MELEE_DAMAGE] = new Vector3Int(currentStats[EnemyStat.MELEE_DAMAGE].x, Mathf.Clamp(currentStats[EnemyStat.MELEE_DAMAGE].x - damageAmountDiceSumResult, 0, int.MaxValue), _spell.numberOfTurns);
                            break;
                        case DependedStat.rangeDamage:
                            currentStats[EnemyStat.RANGE_DAMAGE] = new Vector3Int(currentStats[EnemyStat.RANGE_DAMAGE].x, Mathf.Clamp(currentStats[EnemyStat.RANGE_DAMAGE].x - damageAmountDiceSumResult, 0, int.MaxValue), _spell.numberOfTurns);
                            break;
                            case DependedStat.IceResistance:
                            break;
                    }

                    break;


                case SpellEffects.Stone:
                    if(spellEffectImmunityList.Contains(SpellEffects.Stone)) continue; // skip stone effect if enemy has a total immunity to it
                    diceToCompare += Mathf.Clamp(spellcaster.GetComponent<IHero>().GetSkillsStat(SkillsStat.ElementalMagic) / 10, 0, _spell.diceSides); // adding bonus to stone chance from elemental magic skill 
                    if ( diceToCompare == _spell.diceSides)
                    {
                        appliedPermanentDebuffs.TryAdd(SpellEffects.Stone, 0);
                        if (enemySprites.GetStatePortrait(GameplayStatus.Stoned, out Sprite stateSpriteStoned)) enemyFace.sprite = stateSpriteStoned;
                    }
                    break;

                case SpellEffects.Paralize:
                    if (spellEffectImmunityList.Contains(SpellEffects.Paralize)) continue; // skip paralize effect if enemy has a total immunity to it
                    if ( diceToCompare == _spell.diceSides)
                    {
                        appliedPermanentDebuffs.TryAdd(SpellEffects.Paralize, 0);
                        if (enemySprites.GetStatePortrait(GameplayStatus.Paralized, out Sprite stateSpriteParalized)) enemyFace.sprite = stateSpriteParalized;
                    }
                    break;


                case SpellEffects.Poison:
                    if (spellEffectImmunityList.Contains(SpellEffects.Poison)) continue; // skip poison effect if enemy has a total immunity to it
                    if (diceToCompare > _spell.diceSides / 2)
                    {
                        appliedPermanentDebuffs.TryAdd(SpellEffects.Poison, damageAmountDiceSumResult);
                        if (enemySprites.GetStatePortrait(GameplayStatus.Poisoned, out Sprite stateSpritePetrified)) enemyFace.sprite = stateSpritePetrified;
                        playStatusAnimation.Invoke("Poisoned");
                    }

                    break;

                case SpellEffects.Burn:
                    if (spellEffectImmunityList.Contains(SpellEffects.Burn)) continue; // skip burn effect if enemy has a total immunity to it
                    if (diceToCompare > _spell.diceSides / 2)
                    {

                        appliedPermanentDebuffs.TryAdd(SpellEffects.Burn, damageAmountDiceSumResult);
                        playStatusAnimation.Invoke("BurnStatus");
                    }
                    results.Add(new() { msgType="s", msgString = "burn"});
                    break;

                case SpellEffects.Freeze:
                    if (spellEffectImmunityList.Contains(SpellEffects.Freeze)) continue; // skip freeze effect if enemy has a total immunity to it
                    if (diceToCompare > _spell.diceSides / 2)
                {
                    appliedPermanentDebuffs.TryAdd(SpellEffects.Freeze, 0);
                        playStatusAnimation.Invoke("Freeze");
                    }
                    results.Add(new() { msgType = "s", msgString = "freeze" });
                    break;
                case SpellEffects.Vampirism:
                    if (spellEffectImmunityList.Contains(SpellEffects.Vampirism)) continue; // skip vampirism effect if enemy has a total immunity to it
                    HealthDamage(damageAmountDiceSumResult); 
                    spellcaster.GetComponent<IHero>().HealthDecrease(-damageAmountDiceSumResult); // healing spellcaster by vampirism amount
                    break;

            }
            //results.Add(new() { msgType = "s", msgString = "freeze" });
            if (evaderoll <= attackRoll || dice == 20) hitTargetEffect.Invoke(spellToApply);
        }

        if(!spellToApply.AOE)StartCoroutine(AttackDelay());
        return results;
    }

    private int MagicDamageApply(GameObject spellcaster,  Spell _spell, int damageAmountDiceSumResult)
    {
        damageAmountDiceSumResult += spellcaster.GetComponent<IHero>().MagicDamageModifier(_spell.skillToCheckInCalculations);
        damageAmountDiceSumResult = ApplyMagicResistanceToSpell(_spell, damageAmountDiceSumResult, spellcaster);
        HealthDamage(damageAmountDiceSumResult);
        return damageAmountDiceSumResult;
    }

    private int ApplyMagicResistanceToWeapon(GameObject spellcaster, int amount)
    {
        switch (spellcaster.GetComponent<IHero>().GetWeaponMagicType())
        {
            case MagicType.None:
                amount -= currentStats[EnemyStat.DEFENCE].y;
                break;
            case MagicType.Fire:
                amount -= currentStats[EnemyStat.FIRE_RESISTANCE].y;
                break;
            case MagicType.Water:
                amount -= currentStats[EnemyStat.WATER_RESISTANCE].y;
                break;
            case MagicType.Air:
                amount -= currentStats[EnemyStat.AIR_RESISTANCE].y;
                break;
            case MagicType.Earth:
                amount -= currentStats[EnemyStat.EARTH_RESISTANCE].y;
                break;
            case MagicType.Light:
                amount -= currentStats[EnemyStat.LIGHT_RESISTANCE].y;
                break;
            case MagicType.Dark:
                amount -= currentStats[EnemyStat.DARK_RESISTANCE].y;
                break;
            case MagicType.Ice:
                amount -= currentStats[EnemyStat.ICE_RESISTANCE].y;
                break;
        }

        return amount;
    }
    private int ApplyMagicResistanceToSpell(Spell _spell, int amount, GameObject spellcaster)
    {
        int elementalSkill = spellcaster.GetComponent<IHero>().GetSkillsStat(SkillsStat.ElementalMagic);

        switch (_spell.magicType)
        {
            case MagicType.None:
                amount -= currentStats[EnemyStat.DEFENCE].y;
                break;
            case MagicType.Fire:
                amount = CompareElementalSkillAndResistance(amount, elementalSkill, EnemyStat.FIRE_RESISTANCE);
                if(appliedPermanentDebuffs.ContainsKey(SpellEffects.Freeze))
                {
                    appliedPermanentDebuffs.Remove(SpellEffects.Freeze); // removing freeze debuff if fire spell is applied
                    playStatusAnimation.Invoke("Idle");
                }
                break;
            case MagicType.Water:
                amount = CompareElementalSkillAndResistance(amount, elementalSkill, EnemyStat.WATER_RESISTANCE);
                if (appliedPermanentDebuffs.ContainsKey(SpellEffects.Burn))
                {
                    appliedPermanentDebuffs.Remove(SpellEffects.Burn); // removing freeze debuff if fire spell is applied
                    playStatusAnimation.Invoke("Idle");
                }
                break;
            case MagicType.Air:
                amount = CompareElementalSkillAndResistance(amount, elementalSkill, EnemyStat.AIR_RESISTANCE);
                if (appliedPermanentDebuffs.ContainsKey(SpellEffects.Freeze))
                {
                    //appliedPermanentDebuffs.Remove(SpellEffects.Freeze); // removing freeze debuff if fire spell is applied
                    MagicDamageApply(spellcaster,  _spell, GameInstance.DiceRollingSum(_spell.diceRollsNumber, _spell.diceSides));
                }
                break;
            case MagicType.Earth:
                amount = CompareElementalSkillAndResistance(amount, elementalSkill, EnemyStat.EARTH_RESISTANCE);
                break;
            case MagicType.Light:
                amount = CompareElementalSkillAndResistance(amount, elementalSkill, EnemyStat.LIGHT_RESISTANCE);
                break;
            case MagicType.Dark:
                amount = CompareElementalSkillAndResistance(amount, elementalSkill, EnemyStat.DARK_RESISTANCE);
                break;
            case MagicType.Ice:
                amount = CompareElementalSkillAndResistance(amount, elementalSkill, EnemyStat.ICE_RESISTANCE);
                if (appliedPermanentDebuffs.ContainsKey(SpellEffects.Burn))
                {
                    appliedPermanentDebuffs.Remove(SpellEffects.Burn); // removing freeze debuff if fire spell is applied
                    playStatusAnimation.Invoke("Idle");
                }
                break;
        }

        return amount;
    }

    private int CompareElementalSkillAndResistance(int amount, int elementalSkill, EnemyStat enemyStat)
    {
        if (elementalSkill < currentStats[enemyStat].y)
        {
            amount -= currentStats[enemyStat].y;
        }
        else
        {
            amount -= (int)(currentStats[enemyStat].y * 0.5f);// if skill is higher than resistance only half resistance is applied
        }

        return amount;
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

    public void SetEnemyRowAndPlace(int row, List<int> place)
    {

        rowIndex = row;
        placeIndexes = place;
    }

    public int GetEnemyRow()
    {
        return rowIndex;
    }
    public List<int> GetEnemyPlace()
    {
        return placeIndexes;
    }

    public List<int> CheckForPlaceMatch(List<int> listToCheck)
    {
        List<int> c = new List<int>();
        foreach(int i in listToCheck)
        {
            if (placeIndexes.Contains(i)) c.Add(i);
        }
        return c;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameInstance.playerController.playerState == PlayerState.Explore) return;
        if (health <= 0) return;
        if (outlineRenderer != null)
        {
            enemyFace.gameObject.SetActive(false);
            outlineRenderer.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (health <= 0) return;
        if (outlineRenderer != null)
        {
            outlineRenderer.gameObject.SetActive(false);
            enemyFace.gameObject.SetActive(true);
        }

    }



    public string GetEnemyName()
    {
        return enemyName;
    }
    public int GetEnemySize()
    {
        return enemySize;
    }

    public int GetEnemyHealth()
    {
        return health;
    }

    public int GetInitiativeInBattle()
    {
        return currentStats[EnemyStat.INITIATIVE].y;
    }

    public List<GameObject> GetOpponents()
    {
        return null;
    }

    public void SetEnemyPlaceSpace(int row, List<int> places)
    {
        rowIndex = row;

        //placeIndexes.Clear();
        foreach (int i in places)
        {
            if(!placeIndexes.Contains(i)) placeIndexes.Add(i);
        }
        SetEnemyRowAndPlace(row, places);
    }

    public void SetTransform(GameObject spawnPlace)
    {
        
        //transform.position = spawnPlace.transform.position;

        transform.parent = spawnPlace.transform;
        transform.localPosition = Vector3.zero;

    }
    public int GetEnemyAccuracy()
    {
        return currentStats[EnemyStat.ACCURACY].y;
    }

    public Dictionary<SpellEffects,int> GetEnemyPermanentStatus()
    {
        return appliedPermanentDebuffs;
    }


    void Timepassed(int minite)
    {
        
        foreach(EnemyStat es in System.Enum.GetValues(typeof(EnemyStat)))
        {



            if (currentStats.ContainsKey(es) && currentStats[es].z > 0)
            {  
                print("battle time passes" + currentStats[EnemyStat.INITIATIVE].y);
                currentStats[es] = new Vector3Int(currentStats[es].x, currentStats[es].y, currentStats[es].z - 1);

                if (currentStats.ContainsKey(es))
                {
                    if (currentStats[es].z <= 0)
                    {
                        print("back to normal ");
                        currentStats[es] = new Vector3Int(currentStats[es].x, currentStats[es].x, 0); // resetting stat to initial value when debuff time is over
                    }
                }
            }

        }
        
        if(appliedPermanentDebuffs.ContainsKey(SpellEffects.Poison))
        {
            HealthDamage(appliedPermanentDebuffs[SpellEffects.Poison]);
        }
        if (appliedPermanentDebuffs.ContainsKey(SpellEffects.Burn))
        {
            print("burnig amount  " + appliedPermanentDebuffs[SpellEffects.Burn]);
            HealthDamage(appliedPermanentDebuffs[SpellEffects.Burn]);

        }
        if (health <= 0)
        {
            SetEnemyRowAndPlace(-1, new List<int> { });
            if (gameObject != null) GameInstance.battleManager.RemoveDeadEnemy(gameObject);
            playStatusAnimation.Invoke("Idle");
        }
    }

    public int GetCurrentStatValue(EnemyStat enemyStat)
    {
        return currentStats[enemyStat].y;
    }


    public List<SpellContainer> GetEnemyAttackSpell() { return attackSpells; }
    public int ExperienceReward() { return experienceReward; }


    public GameObject GetEnemyGameObject() { return this.gameObject; }

    public void GetEnemyLoot(out int money, out int gems, out List<HeroInventoryItem> items)
    {
        items = new List<HeroInventoryItem>();
        money = Random.Range(moneyRandomRange.x, moneyRandomRange.y);
        gems = Random.Range(gemsRandomRange.x, gemsRandomRange.y);

        if (itemScriptableContainers.Count == 0) return;

        foreach(ItemScriptableContainer isc in itemScriptableContainers)
        {
            int i = Random.Range(randomLootPossibility.x, randomLootPossibility.y);
            if (i == 1)
            {

                HeroInventoryItem hii = new HeroInventoryItem();
                hii = GameInstance.dataBase.HeroInventoryFromITemScriptable(isc);
                if(itemsStackAmout.Count !=0) hii.stackAmount = itemsStackAmout[itemScriptableContainers.IndexOf(isc)];
                else hii.stackAmount = 1;
                items.Add(hii);
            }
        }


    }
}


public interface IEnemy
{
    public int GetEnemyRow();
    public List<int> GetEnemyPlace();
    public void SetEnemyPlaceSpace(int row, List<int> places);
    public int HealthDamage(int amount);
    public string GetEnemyName();
    public SpellContainer enemyAttack(int distanceToHero);
    public List<ResultMsg> ApplySpellToEnemy(SpellContainer spellToApply, GameObject spellcaster);
    public List<int> CheckForPlaceMatch(List<int> listToCheck);

    public int GetEnemySize();

    public int GetEnemyHealth();
    public void SetTransform(GameObject spawnPlace);
    public int GetEnemyAccuracy();
    public Dictionary<SpellEffects,int> GetEnemyPermanentStatus();
    public int GetCurrentStatValue(EnemyStat enemyStat);
    public void MoveAheadOnAttack();
    public void MoveBackAfterAttack();
    public List<SpellContainer> GetEnemyAttackSpell();
    public int ExperienceReward();
    public GameObject GetEnemyGameObject();
    public void GetEnemyLoot(out int money, out int gems, out List<HeroInventoryItem> item );
}

[System.Serializable]
public enum EnemyStat
{
    INITIATIVE,
    HEALTH,
    DEFENCE,
    ACCURACY,
    EVASION,
    FIRE_RESISTANCE,
    WATER_RESISTANCE,
    EARTH_RESISTANCE,
    ICE_RESISTANCE,
    AIR_RESISTANCE,
    DARK_RESISTANCE,
    LIGHT_RESISTANCE,
    MELEE_DAMAGE,
    RANGE_DAMAGE,
    BURN
}

[System.Serializable]
public class EnemyStatInitAmount
{
    public EnemyStat enemyStat;
    public int initialAmount;
}

public struct ResultMsg
{
    public string msgType;
    public int msgInt;
    public float msgFloat;
    public string msgString;
}