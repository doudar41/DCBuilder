using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Hero : MonoBehaviour, IPointerClickHandler, IHero, IBattle
{
    
    [SerializeField] string heroName = "";
    [SerializeField] Image portrait;
    [SerializeField] Sprite deadSprite, portraitSprite;
    //[SerializeField] List<Sprite> portraitSprite;
    [SerializeField] HealthImage healthSlider, manaSlider;
    [SerializeField] List<SpellContainer> heroSpellbook = new List<SpellContainer>();
    [SerializeField]  int rowIndex = 1;
    [SerializeField] SpellContainer unarmedSpell;
    int currentHealth = 100, currentMana = 100;

    Dictionary<MainStat, int> mainStatContainer = new Dictionary<MainStat, int>();
    Dictionary<DependedStat, int> dependedStatsCurrent = new Dictionary<DependedStat, int>();
    Dictionary<SkillsStat, int> skillsStatsCurrent = new Dictionary<SkillsStat, int>();
    Dictionary<ItemType, SpellContainer> equipmentSpells = new Dictionary<ItemType, SpellContainer>();
    Dictionary<Spell,int> spellsAttached = new Dictionary<Spell, int>();

    [SerializeField] List<DependedStatClass> dependedStatsInitList = new List<DependedStatClass>();

    //Battle manager var
    [SerializeField] int agroLevel = 1;  // recalculates Depend on damage and heal spell( spells can have an agro? ) 

    int currentInitiativeReduction = 0;

    public Dictionary<ItemType, ItemScriptableContainer> equipment = new Dictionary<ItemType, ItemScriptableContainer>();

    int currentTimeSnap;
    [SerializeField] BuffPanels buffPanels;

    private void Start()
    {
        FillMainStats(null);
        FillDependedStatsInit();
        currentHealth = GetDependedStatFromList(DependedStat.maxHealth, dependedStatsInitList);
        currentMana = GetDependedStatFromList(DependedStat.maxMana, dependedStatsInitList);

        foreach(ItemType sk in System.Enum.GetValues(typeof(ItemType)))
        {
            equipment.Add(sk, null);
        }
        foreach(SkillsStat s in System.Enum.GetValues(typeof(SkillsStat)))
        {
            if(s != SkillsStat.None) skillsStatsCurrent.Add(s, 0);
        }
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameInstance.playerController.playerState != PlayerState.Battle)
        {
            GameInstance.party.SetActiveHero(this);
            GameInstance.spellbook.spellTargetEvent.Invoke(this.gameObject);
        }
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

        foreach (Spell s in spellToApply.spells)
        {
            switch (s.spellEffect)
            {
                case SpellEffects.PhysicalDamage:

                    int attackRoll = GameInstance.DiceRollingBiggestNumber(1, 20);
                    if (GetDependedStat(DependedStat.defense) <= attackRoll) 
                    {
                        int amount = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
                        healthDecrease(amount); 
                    }
                    
                    break;
                case SpellEffects.MagicDamage:


                    break;
                case SpellEffects.MainStatModify:
                    if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, s.numberOfTurns);
                    else spellsAttached[s] = s.numberOfTurns;
                    if (buffPanels != null) buffPanels.AddBuffToList(spellToApply);


                    break;
                case SpellEffects.DependedStatModify:
                    if (!spellsAttached.ContainsKey(s)) spellsAttached.Add(s, s.numberOfTurns);
                    else spellsAttached[s] = s.numberOfTurns;
                    if (buffPanels != null) buffPanels.AddBuffToList(spellToApply);

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
                    if (currentHealth <= 0) break;
                    int healroll = GameInstance.DiceRollingSum(s.diceRollsNumber, s.diceSides);
                    healroll += GetSkillsStat(SkillsStat.LightMagic) + GetMainStat(MainStat.Mind)+s.diceBonus+s.amount;
                    HealHero(healroll);
                    break;
            }
        }
        if (GameInstance.playerController.playerState == PlayerState.Battle && !spellToApply.AOE) StartCoroutine(AttackDelay());
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
        yield return new WaitForSeconds(0.5f);
        GameInstance.battleManager.AttackEnding();
    }

    public void healthDecrease(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, GetDependedStat(DependedStat.maxHealth)); 
        healthSlider.ProgressBarFill((float)currentHealth / (float)GetDependedStat(DependedStat.maxHealth));
        if (currentHealth <= 0) portrait.sprite = deadSprite;
    }
    public void ManaDecrease(int amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, GetDependedStat(DependedStat.maxMana));
        manaSlider.ProgressBarFill((float)currentMana / (float)GetDependedStat(DependedStat.maxMana));
    }

    public int GetRowIndex()
    {
        return rowIndex;
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
        int statInt = dependedStatsCurrent[dependedStat];
        switch (dependedStat)
        {
            case DependedStat.heroLevel:
                break;
            case DependedStat.maxHealth:
                statInt += (GetMainStat(MainStat.Strength) / 5);
                break;
            case DependedStat.maxMana:
                statInt += (GetMainStat(MainStat.Mind) / 5);
                break;
            case DependedStat.initiative:
                statInt += Mathf.Clamp(statInt - currentInitiativeReduction, 0, int.MaxValue);
                statInt += (GetMainStat(MainStat.Agility) / 5);
                break;
            case DependedStat.accuracy:
                statInt += (GetMainStat(MainStat.Agility) / 5) + (GetMainStat(MainStat.Endurance) / 5);
                break;
            case DependedStat.defense:
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
                statInt +=  (GetMainStat(MainStat.Strength) / 5) + GetSkillsStat(GetWeaponType());
                break;
            case DependedStat.rangeDamage:
                statInt += (GetMainStat(MainStat.Agility) / 5 ) + GetSkillsStat(GetWeaponType()); 
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
                print(s.Key.changedDependedStat + " "+ s.Key.amount);
                statInt += s.Key.amount;
            }
        }

        return Mathf.Clamp(statInt,0,int.MaxValue);
    }

    public int GetSkillsStat(SkillsStat skillStat)
    {
        if (dependedStatsCurrent.Count == 0) return 0;
        skillsStatsCurrent.TryGetValue(skillStat, out int st);
        int statInt = 0; statInt += st;
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


    void FillMainStats(List<int> mainStatsInit)
    {
        if (mainStatsInit == null)
        {
            mainStatContainer.Add(MainStat.Strength, 4);
            mainStatContainer.Add(MainStat.Agility, 4);
            mainStatContainer.Add(MainStat.Mind, 4);
            mainStatContainer.Add(MainStat.Endurance, 4);
            mainStatContainer.Add(MainStat.Willpower, 4);
            mainStatContainer.Add(MainStat.Survival, 4);
        }

    }


    //Fill with numbers which need to be start 
    void FillDependedStatsInit()
    {
        foreach(DependedStat d in System.Enum.GetValues(typeof(DependedStat)))
        {
         dependedStatsCurrent.Add(d,GetDependedStatFromList(d,dependedStatsInitList));
        }

    }

    public bool AddEquipmentToCharacter(ItemType itemType, ItemScriptableContainer item)
    {
        if (!equipment.TryAdd(itemType, item))
        {
            equipment[itemType] = item;
        }
        if (!equipmentSpells.TryAdd(itemType, item.spellContainer))
        {
            equipmentSpells[itemType] = item.spellContainer;
            return true;
        }
        else return false;
    }

    public void RemoveItemFromEquipment(ItemType itemType)
    {
        equipmentSpells.Remove(itemType);
        equipment.Remove(itemType);
    }

    public Dictionary<ItemType, ItemScriptableContainer> GetHeroEquipment()
    {
        return equipment;
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
        if (equipment[ItemType.WEAPON] != null) return equipment[ItemType.WEAPON].weaponType;
        return SkillsStat.None;
    }

    public string HeroName()
    {
        return heroName;
    }

    int GetDependedStatFromList(DependedStat ds ,List<DependedStatClass> list)
    {
        foreach(DependedStatClass d in list)
        {
            if (d.dependedStat == ds) return d.amount;
        }
        return 0;
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
        TimeChanges();
    }


    void TimePassBy(int count)
    {
        if (GameInstance.playerController.playerState == PlayerState.Battle) return;
        TimeChanges();
    }


    void TimeChanges()
    {
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
            if(buffPanels != null)buffPanels.RemoveBuffFromList(s);
            spellsAttached.Remove(s);
        }
    }



    public SpellContainer GetInfusedWeaponSpell()
    {


        return null;
    }


    private void OnDestroy()
    {
        GameInstance.progress -= TimePassBy;
        StopCoroutine(GameInstance.TimeStep());
        GameInstance.battleManager.battlePassTime -= BattleTimeChanges;
    }
}


public interface IHero
{
    public List<SpellContainer> GetActiveHeroSpellbook();
    public bool AddEquipmentToCharacter(ItemType itemType, ItemScriptableContainer item);
    public void RemoveItemFromEquipment(ItemType itemType);
    public void MakeHeroActive(bool active);
    public void ApplySpellToHero(SpellContainer spellToApply, GameObject spellcaster);
    public void ManaDecrease(int amount);
    public int GetRowIndex();
    public Dictionary<MainStat, int> GetMainStatsForUI();
    public Dictionary<DependedStat, int> GetDependedStatsForUI();
    public Dictionary<SkillsStat, int> GetSkillStatsForUI();
    public Dictionary<ItemType, ItemScriptableContainer> GetHeroEquipment();

    public int GetHeroHealth();
    public Hero GetThisHero();
    public int GetHeroAgro();
    public void ChangeArgo(int amount);

    public SpellContainer GetWeaponSpell();
    public SpellContainer GetInfusedWeaponSpell();
    public string HeroName();
    public int GetDependedStat(DependedStat dependedStat);
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
    defense,
    evasion,
    FireResistance,
    WaterResistance,
    EarthResistance,
    AirResistance,
    DarkResistance,
    CarryingCapacity,
    Hunger,
    meleeDamage,
    rangeDamage
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


public enum GameplayStatuses
{
    None,
    Frozen,
    Burning,
    Poisoned,
    Stunned, 
    Dead
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