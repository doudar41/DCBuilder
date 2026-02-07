using Ami.BroAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevelOrder : MonoBehaviour
{
    [SerializeField]
    GameObject itemModelPrefab;
    [SerializeField] bool isDungeonLevel = false;
    void Start()
    {
        
        GameInstance.initItems();

        if (GameInstance.levelsVisited.Contains(GameInstance.GetLevelName()))
        {
            Dictionary<string, HeroInventoryItem> listToMake = new Dictionary<string, HeroInventoryItem>();
            foreach(KeyValuePair<string, HeroInventoryItem> h in GameInstance.itemsOnLevelSavedWithGUID)
            {
                //Debug.Log(" items on level containers "+ GameInstance.dataBase.GetItemFromBaseByIndex(h.Value.container).itemName);
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
        GameInstance.party.LoadEquipment();
        GameInstance.inventory.LoadKeys();

        GameInstance.checkWeight();

        GameInstance.party.addExperiencePoints(GameInstance.expPoints);
        GameInstance.party.MoneyGoes(-GameInstance.moneyCollected);
        GameInstance.party.GemGoes(-GameInstance.gemsCollected);
        GameInstance.party.AddSomeFoodInit(GameInstance.partyFood);

        GameInstance.party.RestoreSpellsAttached(GameInstance.spellsAttachedToHeroes);
        GameInstance.party.PartyHeroInit();
        GameInstance.spellbook.RestoreContinousSpells();
        GameInstance.party.LoadDialoguesFromInstance();
        //GameInstance.playerController.SetEncounter(GameInstance.noEncounter);
        if (!GameInstance.playerController.GetEncounterState()) 
        {
            if(GameInstance.savedTimeToEncounter>0 ) GameInstance.playerController.SetCountdownToEncounter(GameInstance.savedTimeToEncounter); 
        }

        GameInstance.inventory.BuildItemDatabase();

        GameInstance.dayNightChange.isDungeon = isDungeonLevel;
        GameInstance.dayNightChange.InitDayNightShift();
        StartCoroutine(GameInstance.TimeStep());


    }
}
