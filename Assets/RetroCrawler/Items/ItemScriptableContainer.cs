using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="ScriptableObjects/ItemScriptable")]
public class ItemScriptableContainer : ScriptableObject
{
    public ItemType itemType;
    public string itemName;
    public Texture2D texture2DMouse;
    public Sprite InventorySprite, worldSprite;
    public GameObject prefab;
    public int weight;
    public SpellContainer spellContainer;
    public SkillsStat weaponType;
    public bool twoHanded = false;
    public int price;
}


