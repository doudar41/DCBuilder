using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SpellContainer")]
[System.Serializable]
public class SpellContainer : ScriptableObject
{
    public List<Spell> spells;
    public Sprite spellIcon;
    public bool AOE, OnlyEnemies, OnlyParty; //if AOE 
    public bool gameplaySpell;
    public bool battleSpell;
    public bool equipableSpell;
    public int minDistanceToEnemy = 2;
    public string spellName;
    public int spellPrice;
    public int spellLevel;
}
