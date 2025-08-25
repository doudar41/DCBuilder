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
    //public static BattleManager battleManager;
    public static Spellbook spellbook;
    public static List<string> levelNames;

    static Texture2D cursorTargetGraphics, cursorNormal;
    static CursorMode cursorMode = CursorMode.Auto;
    static Vector2 normalHotSpot = Vector2.zero, targetHotSpot = new Vector2(0.5f, 0.5f);


    public delegate void TimeProgress(int countdown);
    public static TimeProgress progress;
    static int timeProgress = 0;

    public static bool loadingLevel = false, levelEnter = false;

    // Heroes data
    public static Dictionary<int, Dictionary<MainStat, int>> mainHeroesStatsSaved = new Dictionary<int, Dictionary<MainStat, int>>();
    public static Dictionary<int, Dictionary<SkillsStat, int>> skillBonusHeroesStatsSaved = new Dictionary<int, Dictionary<SkillsStat, int>>();
    public static Dictionary<int, Dictionary<ItemType, ItemScriptableContainer>> equipmentHeroesSaved = new Dictionary<int, Dictionary<ItemType, ItemScriptableContainer>>();
    

    //SpellAttachedSaved 
    
    public static Vector3Int playerPositionSaved, nextLevelPosition;
    public static CardinalDirections playerRotationSaved, nextLevelRotation;

    public static Dictionary<string, SavedState> savedItemsState = new Dictionary<string, SavedState>();

    public static List<string> fileNamesList = new List<string>();
    static string currentLevelName = "";

    public static void LoadOrder()
    {
        if (loadingLevel)
        {
            //load from file or load from this class
            //position of player
            //state of the heroes, spells, equipment
            //load inventory stash 
            //enemies states
            //interactable objects states

        }
        else
        {

        }
    }

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

    public static int GameTime()
    {
        return 0;
    }


    public static void SetMouseCursor(Texture2D norm)
    {
        cursorNormal = norm;
        Cursor.SetCursor(cursorNormal, normalHotSpot, cursorMode);
    }

    public static List<IEnemy> FindEnemies()
    {
        GameObject[] gs = GameObject.FindGameObjectsWithTag("Enemy");
        List<IEnemy> enemies = new List<IEnemy>();
        foreach(GameObject g in gs)
        {
            enemies.Add(g.GetComponent<IEnemy>());
        }
        return enemies;
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
        currentLevelName = levelName;       
        if (party != null) party.SaveEquipment();
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

    public static void SaveItemState(string _guid, SavedState _state)
    {
        if (savedItemsState.ContainsKey(_guid)) savedItemsState[_guid] = _state;
        else  savedItemsState.Add(_guid, _state);
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
        
        saveData.playerPosition = playerController.GetcurrentPosition();
        saveData.playercardinalDirection = playerController.GetCurrentDirection();
        saveData.listOfItemsStates = new List<ItemDataSave>();
        saveData.heroesEquipment = HeroEquipmentConvertToSave();
        Debug.Log (" compare equipment "+HeroEquipmentConvertToSave().Count + " " + saveData.heroesEquipment.Count);
        saveData.listOfItemsStates = ItemsConvertToSave();
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

        equipmentHeroesSaved.Clear();
        equipmentHeroesSaved.Add(0, new Dictionary<ItemType, ItemScriptableContainer>());
        equipmentHeroesSaved.Add(1, new Dictionary<ItemType, ItemScriptableContainer>());
        equipmentHeroesSaved.Add(2, new Dictionary<ItemType, ItemScriptableContainer>());
        equipmentHeroesSaved.Add(3, new Dictionary<ItemType, ItemScriptableContainer>());
        if (result == SaveResult.Success)
        {

            Debug.Log(" load equipment "+saveData.heroesEquipment.Count);
            foreach(HeroEquipment he in saveData.heroesEquipment)
            {
                switch (he.heroIndex)
                {
                    case 0:
                        if (!equipmentHeroesSaved[0].ContainsKey(he.itemType)) equipmentHeroesSaved[0].Add(he.itemType, he.container);
                        else
                        {
                            equipmentHeroesSaved[0].Remove(he.itemType);
                            equipmentHeroesSaved[0].Add(he.itemType, he.container);
                        }
                        break;
                    case 1:
                        if(!equipmentHeroesSaved[1].ContainsKey(he.itemType)) equipmentHeroesSaved[1].Add(he.itemType, he.container);
                        else
                        {
                            equipmentHeroesSaved[1].Remove(he.itemType);
                            equipmentHeroesSaved[1].Add(he.itemType, he.container);
                        }
                        break;
                    case 2:
                        if (!equipmentHeroesSaved[2].ContainsKey(he.itemType)) equipmentHeroesSaved[2].Add(he.itemType, he.container);
                        else
                        {
                            equipmentHeroesSaved[2].Remove(he.itemType);
                            equipmentHeroesSaved[2].Add(he.itemType, he.container);
                        }
                        break;
                    case 3:
                        if (!equipmentHeroesSaved[3].ContainsKey(he.itemType)) equipmentHeroesSaved[3].Add(he.itemType, he.container);
                        else
                        {
                            equipmentHeroesSaved[3].Remove(he.itemType);
                            equipmentHeroesSaved[3].Add(he.itemType, he.container);
                        }
                        break;

                }
            }
            savedItemsState.Clear();
            foreach (ItemDataSave idata in saveData.listOfItemsStates)
            {
                savedItemsState.Add(idata.GUID, idata.states[0]);
            }

            nextLevelPosition = saveData.playerPosition;
            nextLevelRotation = saveData.playercardinalDirection;
            levelEnter = true;


            LoadNextLevel(saveData.levelName);
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

    private static List<HeroEquipment> HeroEquipmentConvertToSave()
    {
        List<HeroEquipment> equipment = new List<HeroEquipment>();
        foreach(KeyValuePair<int,Dictionary <ItemType, ItemScriptableContainer>> equipmentLoop in equipmentHeroesSaved )
        {
            foreach(KeyValuePair < ItemType, ItemScriptableContainer > e in equipmentLoop.Value)
            {
                if (e.Value == null) continue;
                HeroEquipment newItem = new HeroEquipment();
                newItem.heroIndex = equipmentLoop.Key;
                newItem.itemType = e.Key;
                newItem.container = e.Value;
                equipment.Add(newItem);
            }
        }
        return equipment;
    }
    private static List<ItemDataSave> ItemsConvertToSave()
    {
        List<ItemDataSave> items = new List<ItemDataSave>();
        foreach (KeyValuePair<string, SavedState > e in savedItemsState)
        {
                ItemDataSave newItem = new ItemDataSave();
                newItem.GUID = e.Key;
                newItem.states[0] = e.Value;
                items.Add(newItem);
        }
        return items;
    }

    public static List<string> GetFileNameList()
    {
        LoadFileNames();
        return fileNamesList;
    } 
        




}

[System.Serializable]
public enum SavedState
{
    None,
    Opened,
    Closed,
    Taken,
    Solved
}

[System.Serializable]
public class SaveData
{
    public string levelName = "Level01";
    public Vector3Int playerPosition;
    public CardinalDirections playercardinalDirection;
    //heroes stats
    // current heroes stats
    //heroes equipment
    public List<HeroEquipment> heroesEquipment = new List<HeroEquipment>();
    //inventory items
    public List<ItemDataSave> listOfItemsStates = new List<ItemDataSave>();
}

[System.Serializable]
public class ItemDataSave
{
    public string GUID = "";
    public List<SavedState> states = new List<SavedState>() { SavedState.None};
}

[System.Serializable] 
public class GameFileSaveNames
{
    public List<string> fileNames = new List<string>();
}
[System.Serializable]
public class HeroEquipment
{
    public int heroIndex = 0;
    public ItemType itemType;
    public ItemScriptableContainer container;
}