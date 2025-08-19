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
}


public enum SpellEffects
{
    PhysicalDamage, //weapon use it
    MagicDamage,
    MainStatModify,
    DependedStatModify,
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
    Heal
}

public enum MagicType
{
    None,
    Fire,
    Water,
    Air,
    Earth,
    Light,
    Dark
}



