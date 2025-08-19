using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : MonoBehaviour
{

   
}

public interface IInteractable
{
   public void  ApplySpellToItem(SpellContainer spellWaitToRelease);
}


public enum ItemType
{
    WEAPON,
    AMMUNITION,
    TORSO_ARMOR,
    HELM,
    GLOVES,
    AMULET,
    BOOT,
    BELT,
    SHIELD,
    RING,
    RING2, 
    RING3, 
    RING4, 
    RING5, 
    RING6,
    CONSUMABLE,
    QUEST,
    LOOT,
    InfusedWeapon
}