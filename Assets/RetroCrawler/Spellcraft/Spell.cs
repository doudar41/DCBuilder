using UnityEngine;

[System.Serializable]
public class Spell 
{
    public SpellEffects spellEffect;
    public int amount = 0;
    public int agroPoints = 0;
    public int manaCost = 0;
    public int diceRollsNumber = 1;
    public int diceSides = 1;
    public int diceBonus = 0;
    public int numberOfTurns = -1; //-1 or zero means spell is infinite
    public MagicType magicType = MagicType.None;
    public MainStat changedMainStat = MainStat.None;
    public DependedStat changedDependedStat = DependedStat.None;
    public SkillsStat skillStatAdded = SkillsStat.None;
    public int restToBeAbleToCastAgainInTurns = 0;
    public string SpellDescription;
    public SkillsStat skillToCheckInCalculations = SkillsStat.None;
    public GameplayStatus stateToCure = GameplayStatus.None;
    public bool continuousSpell = false;
}

public enum SpellEffects
{
    PDmg, //Physical Damage
    MDmg, //Magical Damage
    MSMod,//Main Stat Modify
    DSMod,//Depended Stat Modify
    Recall,
    Mark,
    Paralize, //Modify turn order
    Restoration, //Debuff reset attached spells to hero
    Stone,//Modify turn order
    Death, //TurnOff hero
    WizardEye, //Map modification
    Waterwalk, // Modify blocks to return ground type to open and close access to them
    Identify, // Dictionary of items which describtion are opened
    ReadPortal, //Map modification
    LightARoom,
    Heal,
    ElementalResistance,
    ElementalWeapon,
    LavaWalk,
    Petrify,
    Immunity,
    Poison,
    LevelUp,
    Burn,
    Freeze,
    Vampirism,
    Equipment,
    Revive,
    Antidote,
    Createfood,
    CureState
}

public enum MagicType
{
    None,
    Fire,
    Water,
    Air,
    Earth,
    Light,
    Dark,
    Ice
}



