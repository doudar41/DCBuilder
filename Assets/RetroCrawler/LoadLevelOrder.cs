using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevelOrder : MonoBehaviour
{
    [SerializeField]
    GameObject itemModelPrefab;

    void Start()
    {
        
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

        foreach(HeroInventoryItem h in GameInstance.inventoryItemsSaved)
        {
            //print(GameInstance.inventoryItemsSaved.Count);
            GameInstance.inventory.FindEmptySlotAndPutItem(h, h.stackAmount);
        }

        GameInstance.playerController.InitWallAccess();

        foreach (visitedBlock v in GameInstance.visitedBlocks)
        {
            if(v.level == GameInstance.GetLevelName())
            {
                if (GameInstance.playerController.GetBlockByCoordinatesOnStart(v.coordinates) != null) 
                { 
                    GameInstance.playerController.GetBlockByCoordinatesOnStart(v.coordinates).ShowOnMap(true); 
                }
            }

        }
        GameInstance.playerController.CheckIfLevelLoaded();


        foreach (Hero h in GameInstance.party.GetHeroList())
        {
            h.HeroInit();
        }

        GameInstance.inventory.LoadKeys();

        GameInstance.checkWeight();
    }




}
