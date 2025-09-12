using Gley.AllPlatformsSave;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameInstance
{
    public static PlayerController playerController;
    public static Inventory inventory;
    public static Party party;
    public static BattleManager battleManager;
    public static Spellbook spellbook;
    public static Database dataBase;

    static Texture2D cursorTargetGraphics, cursorNormal;
    static CursorMode cursorMode = CursorMode.Auto;
    static Vector2 normalHotSpot = Vector2.zero, targetHotSpot = new Vector2(0.5f, 0.5f);

    public delegate void TimeProgress(int countdown);
    public static TimeProgress progress;
    static int timeProgress = 0;

    public delegate void InitItems();
    public static InitItems initItems;

    public delegate bool GetInventoryItem();
    public static GetInventoryItem getInventoryItem;

    public static bool loadingLevel = false, levelChange = false;

    // Heroes data



    public static List<HeroInventoryItem> equipmentHeroesSavedWithGUID = new List<HeroInventoryItem>();
    public static List<HeroInventoryItem> inventoryItemsSaved = new List<HeroInventoryItem>();
    public static Dictionary<string, HeroInventoryItem> itemsOnLevelSavedWithGUID = new Dictionary<string,HeroInventoryItem>();

    public static List<string> levelsVisited = new List<string>();
    public static List<visitedBlock> visitedBlocks = new List<visitedBlock>();
    //SpellAttachedSaved 

    public static Vector3Int playerPositionSaved, nextLevelPosition;
    public static CardinalDirections playerRotationSaved, nextLevelRotation;

    public static Dictionary<string, SavedState> savedItemsState = new Dictionary<string, SavedState>();

    //public static List<string> itemsFound = new List<string>();

    public static List<string> fileNamesList = new List<string>();
    static string currentLevelName = "";

    //Heroes stats

    public static List<MainStatsSave> mainStatsAdded = new List<MainStatsSave>();
    public static List<SkillStatSave> skillStatSaves = new List<SkillStatSave>();
    public static List<int> heroesPortraits = new List<int>();
    public static List<string> heroesNames = new List<string>();
    public static List<HeroSpellbookSaved> spellbooksSaved = new List<HeroSpellbookSaved>(); 

    public static int DiceRollingBiggestNumber(int diceNumber, int diceSides)
    {
        List<int> dices = new List<int>();
        for (int i = 1; i < diceNumber + 1; i++)
        {
            dices.Add(Random.Range(1, diceSides+1));
        }
        dices.Sort();

        int result = dices[dices.Count - 1]; // choosing biggest number

        return result;
    }

    public static int DiceRollingSum(int diceNumber, int diceSides)
    {
        List<int> dices = new List<int>();
        for (int i = 1; i < diceNumber + 1; i++)
        {
            dices.Add(Random.Range(1, diceSides + 1));
        }

        int result = 1;
        foreach (int i in dices)
        {
            result += i;
        }

        return result; //sum of all random numbers from dice
    }


    public static void SetMouseCursor(Texture2D norm)
    {
        cursorNormal = norm;
        Cursor.SetCursor(cursorNormal, normalHotSpot, cursorMode);
    }

    public static void LoadGameMainMenu()
    {
        SceneManager.LoadScene("StartGameMenu", LoadSceneMode.Single);
    }
    public static void LoadGameFromStart()
    {
        currentLevelName = "Level01";
        SceneManager.LoadScene("Level01", LoadSceneMode.Single);
    }

    public static void LoadNextLevel(string levelName)
    {
        if (party != null) party.SaveEquipment();
        inventoryItemsSaved.Clear();
        getInventoryItem();
        foreach(visitedBlock v in playerController.GetVisitedBlocksCooordinates())
        {
            if(!visitedBlocks.Contains(v)) visitedBlocks.Add(v);
        }
        spellbooksSaved.Clear();
        party.SaveHeroesSpells();
        currentLevelName = levelName;
        levelChange = true;

        SceneManager.LoadScene(levelName, LoadSceneMode.Single);
    }


    public static IEnumerator TimeStep()
    {
        while (playerController.playerState != PlayerState.Battle)
        {
            timeProgress++;
            progress(timeProgress);
            yield return new WaitForSeconds(1);
        }
        yield return null;
    }


    /// <summary>
    /// This is save game part 
    /// </summary>


    public static void AddItemFromLevel(string _guid,HeroInventoryItem heroInventoryItem)
    {
        if (!itemsOnLevelSavedWithGUID.ContainsKey(_guid)) itemsOnLevelSavedWithGUID.Add(_guid, heroInventoryItem);
    }

    public static void RemoveItemFromLevel(string _guid, HeroInventoryItem heroInventoryItem)
    {
        itemsOnLevelSavedWithGUID.Remove(_guid);
    }


    public static void CheckAllItemsOnLevel()
    {
        initItems();
    }




    public static void SaveItemState(string _guid, SavedState _state, HeroInventoryItem heroInventoryItem)
    {

        if (savedItemsState.ContainsKey(_guid)) 
        {
            savedItemsState[_guid] = _state;
            Debug.Log(" item guid " + _guid + savedItemsState[_guid]);
        }
        else savedItemsState.Add(_guid, _state);

    }
    

    public static void ClearAllSaves()
    {
        fileNamesList.Clear();
        Gley.AllPlatformsSave.API.ClearAllData(Application.persistentDataPath + "/");
    }

    public static void SaveFile(string fileName)
    {
        party.SaveEquipment();

        SaveData saveData = new SaveData();
        if (currentLevelName == "") saveData.levelName = SceneManager.GetActiveScene().name;
        else saveData.levelName = currentLevelName;


        foreach( KeyValuePair<string, SavedState> sdata in savedItemsState)
        {
            if (sdata.Value == SavedState.Replaced || sdata.Value == SavedState.Equipment || sdata.Value == SavedState.Inventory) continue;
            else
            {
                InteractablesStates newstate;
                newstate._guid = sdata.Key;
                newstate._state = sdata.Value;
                saveData.interactablesStates.Add(newstate);
            } 
        }
        saveData.visitedLevels = levelsVisited;
        saveData.itemsOnLevel = ConvertItemsOnLevel(itemsOnLevelSavedWithGUID);
        saveData.playerPosition = playerController.GetCurrentPosition();
        saveData.playercardinalDirection = playerController.GetCurrentDirection();
        saveData.heroesEquipment = equipmentHeroesSavedWithGUID;
        saveData.inventoryItemsSaved = inventoryItemsSaved;
        saveData.visitedBlocks = visitedBlocks;

        saveData.mainStatsAdded = mainStatsAdded;
        saveData.skillStatSaves = skillStatSaves;
        saveData.heroesPortraits = heroesPortraits;
        saveData.heroesNames = heroesNames;
        saveData.spellbooksSaved = spellbooksSaved;

        string path = Application.persistentDataPath + "/" + fileName;
        Gley.AllPlatformsSave.API.Save(saveData, path, DataWasSaved, false);
    }

    public static void LoadFile(string fileName)
    {
        SaveData saveData = new SaveData();
        string path = Application.persistentDataPath + "/" + fileName;
        Gley.AllPlatformsSave.API.Load<SaveData>(path, DataWasLoaded, false);
    }


    private static void DataWasLoaded(SaveData saveData, SaveResult result, string message)
    {
        if (result == SaveResult.EmptyData || result == SaveResult.Error)
        {
            Debug.Log("No Data File Found -> Creating new data...");
            saveData = new SaveData();
        }

        if (result == SaveResult.Success)
        {

            equipmentHeroesSavedWithGUID = saveData.heroesEquipment;
            savedItemsState.Clear();

            foreach(InteractablesStates i in saveData.interactablesStates)
            {
                savedItemsState.Add(i._guid, i._state);
            }
            itemsOnLevelSavedWithGUID = ConvertLevelItemsBack(saveData.itemsOnLevel);
            Debug.Log(" items on levels "+itemsOnLevelSavedWithGUID.Count);
            levelsVisited = saveData.visitedLevels;
            nextLevelPosition = saveData.playerPosition;
            nextLevelRotation = saveData.playercardinalDirection;
            inventoryItemsSaved = saveData.inventoryItemsSaved;
            visitedBlocks = saveData.visitedBlocks ;
            levelChange = true;
            SceneManager.LoadScene(saveData.levelName, LoadSceneMode.Single); 
        }
    }

    private static void DataWasSaved(SaveResult result, string message)
    {
        if (result == SaveResult.Error)
        {
            Debug.Log("Error saving data");
        }
    }

    public static void AddNewFileName(string fileName)
    {
        LoadFileNames();
        GameFileSaveNames saveNames = new GameFileSaveNames();
        string path = Application.persistentDataPath + "/" + "LocalFileNames";
        fileNamesList.Add(fileName);
        saveNames.fileNames = fileNamesList; 
        Gley.AllPlatformsSave.API.Save(saveNames, path, DataWasSaved, false);
    }

    public static void RemoveFileName( string fileName)
    {
        LoadFileNames();
        GameFileSaveNames saveNames = new GameFileSaveNames();
        string path = Application.persistentDataPath + "/" + "LocalFileNames";
        fileNamesList.Remove(fileName);
        Gley.AllPlatformsSave.API.Save(saveNames, path, DataWasSaved, false);
        Gley.AllPlatformsSave.API.ClearFile(Application.persistentDataPath + "/" + fileName);
    }


    public static void LoadFileNames()
    {
        GameFileSaveNames saveNames = new GameFileSaveNames();
        string path = Application.persistentDataPath + "/" + "LocalFileNames";
        Gley.AllPlatformsSave.API.Load<GameFileSaveNames>(path, FileNamesLoaded, false);
    }


    private static void FileNamesLoaded(GameFileSaveNames data, SaveResult result, string arg2)
    {
        if (result == SaveResult.EmptyData || result == SaveResult.Error)
        {
            Debug.Log("No Data File Found -> No Files Saved");
        }
        if (result == SaveResult.Success)
        {
            fileNamesList = data.fileNames;
        }
    }

    public static List<string> GetFileNameList()
    {
        LoadFileNames();
        return fileNamesList;
    } 


    public static string GetLevelName()
    {
        return SceneManager.GetActiveScene().name;
    }


    static List<ItemOnLevel> ConvertItemsOnLevel(Dictionary<string,HeroInventoryItem> itemsToConvert)
    {
        List<ItemOnLevel> itemsOnLevel = new List<ItemOnLevel>();

        foreach(KeyValuePair<string, HeroInventoryItem> h in itemsToConvert)
        {
            ItemOnLevel newitem = new ItemOnLevel();
            newitem.Key = h.Key;
            newitem.heroInventoryItem = new HeroInventoryItem();
            newitem.heroInventoryItem = h.Value;
            itemsOnLevel.Add(newitem);
        }

        return itemsOnLevel;
    }

    static Dictionary<string, HeroInventoryItem> ConvertLevelItemsBack(List<ItemOnLevel> itemsOnLevel)
    {
        Dictionary<string, HeroInventoryItem> itemsToConvert = new Dictionary<string, HeroInventoryItem>();
        foreach(ItemOnLevel i in itemsOnLevel)
        {
            itemsToConvert.Add(i.Key, i.heroInventoryItem);
        }

        return itemsToConvert;
    }


    public static void AddInventoryItem(HeroInventoryItem heroInventoryItem)
    {
        if(heroInventoryItem !=null) inventoryItemsSaved.Add(heroInventoryItem);
    }


    public static List<visitedBlock> ConvertVisitedBlocks(List<Vector3Int> vBlocks)
    {
        //List<visitedBlock> tempBlocks = new List<visitedBlock>();
        foreach (Vector3Int v in vBlocks)
        {
            visitedBlock block;
            block.level = GetLevelName();
            block.coordinates = v;
            if(!visitedBlocks.Contains(block)) visitedBlocks.Add(block);
        }
        return visitedBlocks;
    }

    public static Dictionary<MainStat,int> ConvertSavedMainStats(int heroIndex)
    {
        Dictionary<MainStat, int> heroMainStats = new Dictionary<MainStat, int>();


        foreach(MainStat m in System.Enum.GetValues(typeof(MainStat)))
        {
            if (m == MainStat.None) continue;
            foreach (MainStatsSave ms in mainStatsAdded)
            {
                if (m == ms.mainStat && ms.heroIndex == heroIndex)
                {
                    heroMainStats.Add(ms.mainStat,  ms.amount);
                }
            }
            if(!heroMainStats.ContainsKey(m)) heroMainStats.Add(m, HeroStatsDefault.GetFullMinStats()[m]);
        }
        return heroMainStats;
    }

    public static List<MainStatsSave> ConvertMainStatsToSave(Dictionary<int, Dictionary<MainStat, int>> mainStatToConvert)
    {
        List<MainStatsSave> newmainsave = new List<MainStatsSave>();

        foreach(KeyValuePair<int, Dictionary<MainStat, int>> keypairMain in mainStatToConvert)
        {
            foreach(KeyValuePair<MainStat, int> mainStat in keypairMain.Value)
            {
                MainStatsSave savemaintemp = new MainStatsSave();
                savemaintemp.heroIndex = keypairMain.Key;
                savemaintemp.mainStat = mainStat.Key;
                savemaintemp.amount = mainStat.Value;
                newmainsave.Add(savemaintemp);
            }
        }
        Debug.Log("main stat check " +newmainsave[0].mainStat +" "+ newmainsave[0].amount);
        return newmainsave;
    }

    public static List<MainStatsSave> ConvertMainStatsToSave(Dictionary<MainStat, int> mainStatToConvert, int heroIndex)
    {
        List<MainStatsSave> newmainsave = new List<MainStatsSave>();

        foreach (KeyValuePair<MainStat, int> mainStat in mainStatToConvert)
        {
            MainStatsSave savemaintemp = new MainStatsSave();
            savemaintemp.heroIndex = heroIndex;
            savemaintemp.mainStat = mainStat.Key;
            savemaintemp.amount = mainStat.Value;
            newmainsave.Add(savemaintemp);
        }
        return newmainsave;
    }





}

[System.Serializable]
public enum SavedState
{
    None,
    Opened,
    Closed,
    Cursor,
    Solved,
    Replaced,
    Inventory,
    Equipment
}

[System.Serializable]
public class SaveData
{
    public string levelName = "Level01";
    public Vector3Int playerPosition;
    public CardinalDirections playercardinalDirection;
    public List<InteractablesStates> interactablesStates = new List<InteractablesStates>();
    public List<HeroInventoryItem> heroesEquipment = new List<HeroInventoryItem>();
    public List<HeroInventoryItem> inventoryItemsSaved = new List<HeroInventoryItem>();
    public List<ItemOnLevel> itemsOnLevel = new List<ItemOnLevel>();
    public List<string> visitedLevels = new List<string>();
    public List<visitedBlock> visitedBlocks = new List<visitedBlock>();

    public List<MainStatsSave> mainStatsAdded = new List<MainStatsSave>();
    public List<SkillStatSave> skillStatSaves = new List<SkillStatSave>();
    public List<int> heroesPortraits = new List<int>();
    public List<string> heroesNames = new List<string>();
    public List<HeroSpellbookSaved> spellbooksSaved = new List<HeroSpellbookSaved>();
}

[System.Serializable]
public struct visitedBlock
{
    public string level;
    public Vector3Int coordinates;
}


[System.Serializable] 
public class GameFileSaveNames
{
    public List<string> fileNames = new List<string>();
}
[System.Serializable]
public struct InteractablesStates
{
    public string _guid;
    public SavedState _state;

}

[System.Serializable]
public class ItemOnLevel
{
    public string Key = "";
    public HeroInventoryItem heroInventoryItem = new HeroInventoryItem();
}

[System.Serializable]
public class HeroInventoryItem
{    
    public int heroIndex = 0;
    public ItemType itemType = ItemType.LOOT;
    public int container = -1;
    public int stackAmount = 1;
    public Vector3 positionReplaced = Vector3.zero;
    public string level = "Level01";
}


[System.Serializable]
public class MainStatsSave
{
    public int heroIndex;
    public MainStat mainStat;
    public int amount;
}

[System.Serializable]
public class SkillStatSave
{
    public int heroIndex;
    public SkillsStat skill;
    public int amount;
}


[System.Serializable]
public class HeroSpellbookSaved
{
    public int heroIndex;
    public List<SpellContainer> spells = new List<SpellContainer>();
}
