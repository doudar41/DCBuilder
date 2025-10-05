using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Database")]
public class DatabaseScriptable : ScriptableObject
{
    public List<ItemScriptableContainer> gameItemsBase = new List<ItemScriptableContainer>();
    public List<PortraitContainer> portraits = new List<PortraitContainer>();
    public List<SpellContainer> allSpells = new List<SpellContainer>();
    public List<DialogueDependencies> dialogues = new List<DialogueDependencies>();
}
