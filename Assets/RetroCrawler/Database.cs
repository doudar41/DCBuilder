using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



public class Database : MonoBehaviour
{

    [SerializeField] List<ItemScriptableContainer> gameItemsBase = new List<ItemScriptableContainer>();
    private void Awake()
    {

        //if (GameInstance.dataBase != null) Destroy(gameObject);
        GameInstance.dataBase = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(gameObject);
    }

    public int GetItemIndexFromDataBase(ItemScriptableContainer container)
    {
        return gameItemsBase.IndexOf(container);
    }

    public ItemScriptableContainer GetItemFromBaseByIndex(int i)
    {
        return gameItemsBase[i];
    }
}
