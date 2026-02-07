using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Database")]
public class DatabaseScriptable : ScriptableObject
{
    public List<ItemScriptableContainer> gameItemsBase = new List<ItemScriptableContainer>();
    public List<PortraitContainer> portraits = new List<PortraitContainer>();
    public List<SpellContainer> allSpells = new List<SpellContainer>();
    public List<DialogueDependencies> dialogues = new List<DialogueDependencies>();

#if UNITY_EDITOR
    public void OnValidate()
    {
        gameItemsBase.Clear();
        AssetDatabase.Refresh();

        string[] guids1 = AssetDatabase.FindAssets("t:ItemScriptableContainer", null);
        foreach (string guid1 in guids1)
        {
            ItemScriptableContainer objectLoaded = AssetDatabase.LoadAssetAtPath<ItemScriptableContainer>(AssetDatabase.GUIDToAssetPath(guid1));

            gameItemsBase.Add(objectLoaded);

        }

    }
#endif

}


