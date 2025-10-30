using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
    [SerializeField] int initiative = 1, accuracy = 0, evasion = 0;
    [SerializeField] List<EnemyStatNumbers> enemyStatNumbers;

    [SerializeField] List<MagicType> immunityList = new List<MagicType>();
    [SerializeField] List<int> spellResistance = new List<int>();

    List<GameplayStatus> gameplayStatuses = new List<GameplayStatus>();

    int healthStarted;

    public UnityEvent<float> healthNormalized;
    public UnityEvent<SpellContainer> hitTargetEffecct;

    [SerializeField] SpellContainer immunityspell;


    Dictionary<EnemyStat, Vector3Int> rememberedNormalStats = new Dictionary<EnemyStat, Vector3Int>();

    Vector3 savedPosition;

    private void Start()
    {
        outlineRenderer.color = Color.clear;
        healthStarted = health;
        GameInstance.progress += Timepassed;
        foreach (EnemyStat es in System.Enum.GetValues(typeof(EnemyStat)))
        {
            rememberedNormalStats.Add(es, new Vector3Int(0, 0, 0));
        }

        rememberedNormalStats[EnemyStat.INITIATIVE] = new Vector3Int(initiative, 0, 0);
        rememberedNormalStats[EnemyStat.ACCURACY] = new Vector3Int(accuracy, 0, 0);
        rememberedNormalStats[EnemyStat.DEFENCE] = new Vector3Int(spellResistance[0], 0, 0);
        rememberedNormalStats[EnemyStat.HEALTH] = new Vector3Int(health, 0, 0);
        rememberedNormalStats[EnemyStat.EVASION] = new Vector3Int(evasion, 0, 0);
    }

    private void OnDestroy()
    {
        GameInstance.progress -= Timepassed;
    }

    public void HealthDamage(int amount)
    {
        health = Mathf.Clamp(health - amount, 0, int.MaxValue);

        if (health <= 0)
        {
            col.enabled = false;
            gameObject.tag = "Untagged";
            health = 0;
            StartCoroutine(SpriteFadeOut());
        }

        healthNormalized.Invoke(1-((float)health / (float)healthStarted));
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

    }

    IEnumerator WaitAndBack()
    {
        yield return new WaitForSeconds(0.2f);
        MoveBackAfterAttack();
    }

    public void MoveBackAfterAttack()
    {
        transform.localPosition = savedPosition;
        transform.localScale = new Vector3(1, 1, 1);
    }

    public SpellContainer enemyAttack()
    {
        return attackSpells[Random.Range(0, attackSpells.Count)];
    }

    IEnumerator AttackDelay()
    {
        GameInstance.battleManager.BattleEffect = true;
        yield return new WaitForSeconds(0.5f);
        GameInstance.battleManager.AttackEnding();
    }

    public List<string> ApplySpellToEnemy(SpellContainer spellToApply, GameObject spellcaster)
    {
        if (spellToApply == null) return null;
        List<string> results = new List<string>();
        int attackRoll = 0;
        int attackrollbonus = 0, magicbonus = 0, evaderoll = 0;

        IHero hero = spellcaster.GetComponent<IHero>();

        if (spellcaster.GetComponent<IHero>() != null) 
        {

            attackrollbonus = hero.GetDependedStat(DependedStat.accuracy);  // agillity modifier + endurance modifier of attacker
        }


        foreach (Spell _spell in spellToApply.spells)
        {

            if (immunityList.Contains(_spell.magicType)) // skip magic attack if enemy has a total immunity to it 
            {
                hitTargetEffecct.Invoke(immunityspell); // Event for showing immunity animation
                continue;
            }

            int dice = GameInstance.DiceRollingBiggestNumber(1, 20); //attackroll and evaderoll random generation
            attackRoll = dice + attackrollbonus;
            evaderoll = GameInstance.DiceRollingBiggestNumber(1, 20) + evasion;

            results.Add(attackRoll.ToString()); results.Add(evaderoll.ToString()); // attackroll and evaderoll added to list to be used in the battle log

            if (evaderoll > attackRoll) continue; //if evasion is successful and dice is not equal 20 spell is ignored

            int amount = CalculateIncomingDamage(_spell, dice); 


            switch (_spell.spellEffect) //spellEffects is enum used to list all spell effects 
            {
                case SpellEffects.PhysicalDamage: // Mostly weapon damage calculation

                    // Rolling dices according with stored in a spell scriptable object numbers + make it second time if dice is equal 20

                    if (spellToApply.minDistanceToEnemy > 2) //Add bonus to a range or melee attack
                    { 
                        amount += hero.GetDependedStat(DependedStat.rangeDamage); 
                    }
                    else 
                    { 
                        amount += hero.GetDependedStat(DependedStat.meleeDamage); 
                    }

                    // If weapon damage was change to a elemental checking if enemy has an immunity to it

                    if (immunityList.Contains(spellcaster.GetComponent<IHero>().GetWeaponMagicType())) 
                    {
                        hitTargetEffecct.Invoke(immunityspell); // Event for showing immunity animation
                        continue;
                    }

                    //First spellResistance index is defence amount
                    // If weapon enchanced with element all damage become an elemental damage

                    amount -= spellResistance[(int)spellcaster.GetComponent<IHero>().GetWeaponMagicType()]; 
                    HealthDamage(amount);
                    results.Add(amount.ToString());
                    break;

                case SpellEffects.MagicDamage: 

                    //if damage is magical Magic damage is calculating by rolling dice and add bonus of skill and stats (ex. + Elemental magic / 5)

                    amount += spellcaster.GetComponent<IHero>().MagicDamage(_spell.skillToCheckInCalculations);
                    amount -= spellResistance[(int)_spell.magicType];
                    HealthDamage(amount);
                    results.Add(amount.ToString());

                    break;

                case SpellEffects.DependedStatModify:

                    switch (_spell.changedDependedStat)
                    {
                        case DependedStat.maxMana: //if enemy is spellcaster and there is a limited mana pool
                            break;

                        case DependedStat.initiative:
                            rememberedNormalStats[EnemyStat.INITIATIVE] = new Vector3Int(initiative, Mathf.Clamp(initiative - amount, 0, int.MaxValue), _spell.numberOfTurns);
                            //initiative = Mathf.Clamp(initiative - amount, 0, int.MaxValue);

                            break;
                        case DependedStat.accuracy:
                            rememberedNormalStats[EnemyStat.ACCURACY] = new Vector3Int(initiative, Mathf.Clamp(initiative - amount, 0, int.MaxValue), _spell.numberOfTurns);

                            break;
                        case DependedStat.defence:
                            break;
                        case DependedStat.evasion:
                            break;
                        case DependedStat.FireResistance:
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
                            break;
                        case DependedStat.rangeDamage:
                            break;
                    }

                    break;

                case SpellEffects.Petrify:
                    if (!gameplayStatuses.Contains(GameplayStatus.Petrified))
                    {
                        gameplayStatuses.Add(GameplayStatus.Petrified);
                        if (enemySprites.GetStatePortrait(GameplayStatus.Petrified, out Sprite stateSpritePetrified)) enemyFace.sprite = stateSpritePetrified;
                    }

                    break;
            }
            results.Add("-1");
            if (evaderoll <= attackRoll || dice == 20) hitTargetEffecct.Invoke(spellToApply);
        }

        StartCoroutine(AttackDelay());
        return results;
    }

    private int CalculateIncomingDamage(Spell s, int dice)
    {
        
        int amount = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
        amount += s.diceBonus;
        HealthDamage(amount);
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
        if (health <= 0) return;
        if (outlineRenderer != null)
        {
            outlineRenderer.gameObject.SetActive(true);
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (health <= 0) return;
        if (outlineRenderer != null)
        {
            outlineRenderer.gameObject.SetActive(false);
        }

    }

    IEnumerator SpriteFadeOut()
    {
        if (outlineRenderer != null) outlineRenderer.color = Color.clear;
        for (byte f = 255; f > 0; f--)
        {

            Color32 b = new Color32(f,f,f,f);
            enemyFace.color = b;
            outlineRenderer.color = b;
            yield return new WaitForSeconds(0.1f*Time.deltaTime);
        }
        yield return null;
    }

    public string GetEnemyName()
    {
        return "";
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
        return initiative;
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

    }

    public void SetTransform(GameObject spawnPlace)
    {
        transform.position = spawnPlace.transform.position;
    }
    public int GetEnemyAccuracy()
    {
        return accuracy;
    }

    public List<GameplayStatus> GetEnemyStatus()
    {
        return gameplayStatuses;
    }



    void Timepassed(int minite)
    {
        List<int> indexesContinue = new List<int>();
        List<int> indexesFinish = new List<int>();
        foreach(EnemyStat es in System.Enum.GetValues(typeof(EnemyStat)))
        {
           if(rememberedNormalStats.ContainsKey(es)) rememberedNormalStats[es] = new Vector3Int(rememberedNormalStats[es].x, rememberedNormalStats[es].y,rememberedNormalStats[es].z-1);
        }
    }


}


public interface IEnemy
{
    public int GetEnemyRow();
    public List<int> GetEnemyPlace();
    public void SetEnemyPlaceSpace(int row, List<int> places);
    public void HealthDamage(int amount);
    public string GetEnemyName();
    public SpellContainer enemyAttack();
    public List<string> ApplySpellToEnemy(SpellContainer spellToApply, GameObject spellcaster);
    public List<int> CheckForPlaceMatch(List<int> listToCheck);

    public int GetEnemySize();

    public int GetEnemyHealth();
    public void SetTransform(GameObject spawnPlace);
    public int GetEnemyAccuracy();
    public List<GameplayStatus> GetEnemyStatus();
    public void MoveAheadOnAttack();
    public void MoveBackAfterAttack();
}


[System.Serializable]
public struct EnemyStatNumbers
{
    public      EnemyStat enemyStat;
    public int defaultAmount { get; set; }
    public      int modifiedAmount { get; set; }
    public      int TurnsToRestore { get; set; }
}


public enum EnemyStat
{
    INITIATIVE,
    HEALTH,
    DEFENCE,
    ACCURACY,
    EVASION
}