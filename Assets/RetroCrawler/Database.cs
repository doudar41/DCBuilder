using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



public class Database : MonoBehaviour
{
    [SerializeField] DatabaseScriptable databaseScriptable;

    private void Awake()
    {

        //DontDestroyOnLoad(this);
        GameInstance.dataBase = this;
    }

    public int GetItemIndexFromDataBase(ItemScriptableContainer container)
    {
        return databaseScriptable.gameItemsBase.IndexOf(container);
    }

    public ItemScriptableContainer GetItemFromBaseByIndex(int i)
    {
        return databaseScriptable.gameItemsBase[i];
    }

    public PortraitContainer GetPortraitFromDatabase(int index)
    {
        return databaseScriptable.portraits[index];
    }


    public List<ItemScriptableContainer> GetWholeItemDatabase()
    {
        return databaseScriptable.gameItemsBase;
    }

    public SpellContainer GetSpellByIndex(int index)
    {
        return databaseScriptable.allSpells[index];
    }

    public List<SpellContainer> GetAllSpells()
    {
        return databaseScriptable.allSpells;
    }

    public DialogueDependencies GetDialogue(UniqueDialogueName uniqueDialogueName)
    {
        foreach(DialogueDependencies dd in databaseScriptable.dialogues)
        {
            if(dd.uniqueName == uniqueDialogueName)
            {
                return dd;
            }
        }
        return null;
    }

    public List<DialogueDependencies> GetAllDialogues()
    {
        return databaseScriptable.dialogues;
    }


    public UniqueDialogueName CheckPriority(List<UniqueDialogueName> uniqueDialogueNames)
    {
        int priority = 0;
        UniqueDialogueName higherName = UniqueDialogueName.None;

        foreach (UniqueDialogueName un in uniqueDialogueNames)
        {
             if(GetDialogue(un).priorityIndex > priority)
            {
                if (GameInstance.party.currentUniqueDialogueNames.Contains(un))
                {
                    higherName = un;
                    priority = GetDialogue(un).priorityIndex;
                }
            }
        }

        return higherName;
    }


    public HeroInventoryItem HeroInventoryFromITemScriptable(ItemScriptableContainer item)
    {
        HeroInventoryItem heroInventoryItem = new HeroInventoryItem();
            heroInventoryItem.heroIndex =-1;
             heroInventoryItem.itemType = item.itemType;
             heroInventoryItem.container = GetItemIndexFromDataBase(item);
             heroInventoryItem.stackAmount = 1;
             heroInventoryItem.positionReplaced = Vector3.zero;
             heroInventoryItem.level = "Level01";
             heroInventoryItem.levelOfIdenifySaved = 0;

        return heroInventoryItem;
}

}
