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

}
