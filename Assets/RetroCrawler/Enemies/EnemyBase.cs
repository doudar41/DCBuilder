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
    [SerializeField] int health =100;
    [SerializeField] SpriteRenderer enemyFace, outlineRenderer;
    [SerializeField] PortraitContainer enemySprites;

    [SerializeField] List<SpellContainer> attackSpells = new List<SpellContainer>();
    [SerializeField] Collider col;
    [SerializeField] int enemySize;
    [SerializeField] int initiative = 1, defence = 1, magicDefence = 0, accuracy = 0, evasion =0;
    [SerializeField] List<MagicType> immunityList = new List<MagicType>();

    [SerializeField] Sprite EffectSprite;

    List<GameplayStatus> gameplayStatuses = new List<GameplayStatus>();

    int healthStarted;
    bool isDead = false;

    public UnityEvent<float> healthNormalized;
    public UnityEvent<SpellContainer> hitTargetEffecct;
    [SerializeField] SpellContainer immunityspell;
    [SerializeField] List<int> spellResistance = new List<int>();
    private void Start()
    {
        outlineRenderer.color = Color.clear;
        healthStarted = health;
    }

    public void HealthDamage(int amount)
    {
        health = Mathf.Clamp(health - amount, 0, int.MaxValue);
        if (health <= 0)
        {
            col.enabled = false;
            gameObject.tag = "Untagged";
            health = 0;
            isDead = true;
            StartCoroutine(SpriteFadeOut());
        }
        //print ("enemy health " + health + " - " + (float)health / (float)healthStarted);
        healthNormalized.Invoke(1-((float)health / (float)healthStarted));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (health > 0) GameInstance.spellbook.spellTargetEvent.Invoke(this.gameObject);
    }
    public SpellContainer enemyAttack()
    {
        //StartCoroutine(AttackDelay());
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
        int attackrollbonus = 0, damagebonus = 0, magicbonus =0, evaderoll = 0;
        IHero hero = spellcaster.GetComponent<IHero>();
        if (spellcaster.GetComponent<IHero>() != null) 
        {

            attackrollbonus = hero.GetDependedStat(DependedStat.accuracy);

        }


        foreach (Spell s in spellToApply.spells)
        {
            int dice = GameInstance.DiceRollingBiggestNumber(1, 20);
            attackRoll = dice + attackrollbonus;
            evaderoll = GameInstance.DiceRollingBiggestNumber(1, 20) + evasion;
            results.Add(attackRoll.ToString()); results.Add(evaderoll.ToString());
            if (immunityList.Contains(s.magicType)) 
            {
                hitTargetEffecct.Invoke(immunityspell);
                continue; 
            }

            switch (s.spellEffect)
            {
                case SpellEffects.PhysicalDamage:
                    if (evaderoll <= attackRoll || dice == 20)
                    {
                        int amount = CalculateIncomingDamage(s, dice);

                        if (spellToApply.minDistanceToEnemy>2) amount += hero.GetDependedStat(DependedStat.rangeDamage);
                        else amount += hero.GetDependedStat(DependedStat.meleeDamage);

                        if (immunityList.Contains(spellcaster.GetComponent<IHero>().GetWeaponMagicType()))
                        {
                            hitTargetEffecct.Invoke(immunityspell);
                            continue;
                        }
                        switch (spellcaster.GetComponent<IHero>().GetWeaponMagicType())
                        {

                            case MagicType.None:
                                amount -= defence;
                                HealthDamage(amount);
                                break;

                            case MagicType.Fire:
                                if (evaderoll <= attackRoll || dice == 20)
                                {
                                    amount -= spellResistance[1];
                                    HealthDamage(amount);
                                    results.Add(amount.ToString());
                                }
                                break;
                            case MagicType.Water:
                                if (evaderoll <= attackRoll || dice == 20)
                                {
                                    amount -= spellResistance[2];
                                    HealthDamage(amount);
                                    results.Add(amount.ToString());
                                }
                                break;

                            case MagicType.Ice:
                                if (evaderoll <= attackRoll || dice == 20)
                                {
                                    amount -= spellResistance[3];
                                    HealthDamage(amount);
                                    results.Add(amount.ToString());
                                }
                                break;
                            case MagicType.Air:
                                if (evaderoll <= attackRoll || dice == 20)
                                {
                                    amount -= spellResistance[4];
                                    HealthDamage(amount);
                                    results.Add(amount.ToString());
                                }
                                break;
                            case MagicType.Earth:
                                if (evaderoll <= attackRoll || dice == 20)
                                {
                                    amount -= spellResistance[5];
                                    HealthDamage(amount);
                                    results.Add(amount.ToString());
                                }
                                break;
                            case MagicType.Light:
                                if (evaderoll <= attackRoll || dice == 20)
                                {
                                    amount -= spellResistance[6];
                                    HealthDamage(amount);
                                    results.Add(amount.ToString());
                                }
                                break;
                            case MagicType.Dark:
                                if (evaderoll <= attackRoll || dice == 20)
                                {
                                    amount -= spellResistance[7];
                                    HealthDamage(amount);
                                    results.Add(amount.ToString());
                                }
                                break;
                        }
                        amount -= defence;

                        HealthDamage(amount);
                        results.Add(amount.ToString());
                    }

                    break;
                case SpellEffects.MagicDamage:
                    switch (s.magicType)
                    {
                        case MagicType.Fire:
                            if (evaderoll <= attackRoll || dice == 20)
                            {
                                int amount = CalculateIncomingDamage(s, dice);

                                HealthDamage(amount);
                                results.Add(amount.ToString());
                            }
                            break;
                        case MagicType.Water:
                            if (evaderoll <= attackRoll || dice == 20)
                            {
                                int amount = CalculateIncomingDamage(s, dice);
                                HealthDamage(amount);
                                results.Add(amount.ToString());
                            }
                            break;

                        case MagicType.Ice:
                            if (evaderoll <= attackRoll || dice == 20)
                            {
                                int amount = CalculateIncomingDamage(s, dice);

                                HealthDamage(amount);
                                results.Add(amount.ToString());
                            }
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
                    break;
                case SpellEffects.DependedStatModify:
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
            outlineRenderer.color = Color.white;
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (health <= 0) return;
        if (outlineRenderer != null)
        {
            outlineRenderer.color = Color.clear;
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
}