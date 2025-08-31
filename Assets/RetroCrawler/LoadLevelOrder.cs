using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevelOrder : MonoBehaviour
{
    [SerializeField]
    GameObject itemModelPrefab;

    void Start()
    {
        print("start loading ");
        GameInstance.initItems();
        if (GameInstance.levelsVisited.Contains(GameInstance.GetLevelName()))
        {
            Dictionary<string, HeroInventoryItem> listToMake = new Dictionary<string, HeroInventoryItem>();
            foreach(KeyValuePair<string, HeroInventoryItem> h in GameInstance.itemsOnLevelSavedWithGUID)
            {
                Debug.Log(" items on level containers "+ GameInstance.dataBase.GetItemFromBaseByIndex(h.Value.container).itemName);
                if(h.Value.level == GameInstance.GetLevelName())
                {                    
                    listToMake.Add(h.Key,h.Value); 

                }
            }
            foreach(KeyValuePair<string, HeroInventoryItem> h in listToMake)
            {
                GameObject item = Instantiate(itemModelPrefab);

                IItem iItem = item.GetComponent<IItem>();
                iItem.SetGUID(h.Key);
                iItem.SetPrefab(GameInstance.dataBase.GetItemFromBaseByIndex(h.Value.container));
                iItem.SetItemsAmount(h.Value.stackAmount);
                iItem.PlaceCreatedItem(h.Value.positionReplaced);
                iItem.RemoveFromParent();
            }

        }
        else
        {
            GameInstance.levelsVisited.Add(GameInstance.GetLevelName());
        }
        GameInstance.party.LoadEquipment();


    }


}
