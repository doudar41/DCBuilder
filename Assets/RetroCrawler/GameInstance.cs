
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
    public static DialoguePanel dialoguePanel;
    public static GameJournal gameJournal;
    public static SoundManagerInGame soundManagerInGame;
    public static DayNightChange dayNightChange;

    static Texture2D cursorTargetGraphics, cursorNormal;
    static CursorMode cursorMode = CursorMode.Auto;
    static Vector2 normalHotSpot = Vector2.zero, targetHotSpot = new Vector2(0.5f, 0.5f);

    public delegate void TimeProgress(int countdown);
    public static TimeProgress progress;
    static int timeProgress = 1;
    static int timeStamp = 0;
    static float gameTimeFrame = 0.5f;
    public static int[] gameTimeInNormalTime = new int[3];
    static List<int> savedGameTimeInNormalTime = new List<int> (){0,0,0 };


    public delegate void InitItems();
    public static InitItems initItems;

    public delegate void CheckWeight();
    public static CheckWeight checkWeight;

    public delegate bool GetInventoryItem();
    public static GetInventoryItem getInventoryItem;

    public static bool loadingLevel = false, levelChange = false;

    // Heroes data

    public static List<HeroInventoryItem> equipmentHeroesSavedWithGUID = new List<HeroInventoryItem>();
    public static List<HeroInventoryItem> inventoryItemsSaved = new List<HeroInventoryItem>();
    public static Dictionary<int, HeroInventoryItem> identifiedItems = new Dictionary<int, HeroInventoryItem>(); 
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
    public static List<KeyToLocks> keysSaved = new List<KeyToLocks>();
    public static List<SavedSpellsAttached> spellsAttachedToHeroes = new List<SavedSpellsAttached>();
    public static List<SavedSpellsAttached> spellsFromSpellbook = new List<SavedSpellsAttached>();
    public static List<UniqueDialogueName> dialoguesFinished = new List<UniqueDialogueName>(); //delete dialogue names on load level

    public static List<string> journalEntries = new List<string>();
    public static List<UniqueDialogueName> currentUniqueDialogueNames = new List<UniqueDialogueName>();

    public static Dictionary<Vector3Int,bool> customBattlesInPlaceFinished = new Dictionary<Vector3Int, bool>();

    public static bool noEncounter = true;
    public static int savedTimeToEncounter = 0, expPoints = 0, moneyCollected = 0, gemsCollected =0, partyLevel = 0, partyFood = 0;
    public static List<HeroSavedCurrentData> heroesCurrentData = new List<HeroSavedCurrentData>();

    //Chest and doors
    static SaveOptionsData saveOptionsData = new SaveOptionsData();

    public static void ClearAllInstantSavedData()
    {
        equipmentHeroesSavedWithGUID.Clear();
        inventoryItemsSaved.Clear();
        itemsOnLevelSavedWithGUID.Clear();
        savedItemsState.Clear();
        levelsVisited.Clear();
        visitedBlocks.Clear();
        mainStatsAdded.Clear();
        skillStatSaves.Clear();
        heroesPortraits.Clear();
        heroesNames.Clear();
        spellbooksSaved.Clear();
        keysSaved.Clear();
        spellsAttachedToHeroes.Clear();
        spellsFromSpellbook.Clear();
        dialoguesFinished.Clear();
        journalEntries.Clear();
        currentUniqueDialogueNames.Clear();
        customBattlesInPlaceFinished.Clear();
        gameTimeInNormalTime[0] = 0;
        gameTimeInNormalTime[1] = 0;
        gameTimeInNormalTime[2] = 0;
        savedTimeToEncounter = 0; expPoints = 0; moneyCollected = 0; gemsCollected = 0; partyLevel = 0;
        identifiedItems.Clear();
        levelChange = false;
        timeProgress = 1;
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


    public static int DiceRollingWithSkill(bool sum, Spell s, GameObject caster, int _divider)
    {
        int diceRollNumber = s.diceRollsNumber;
        int diceSides = s.diceSides;
        if (caster.GetComponent<IHero>() != null)
        {
            IHero heroCaster = caster.GetComponent<IHero>();
            if (s.skillToCheckInCalculations != SkillsStat.None)
            {
                int diceResult = 0;
                if (sum)
                {
                    diceResult= DiceRollingSum(diceRollNumber, diceSides);
                }
                else
                {
                    diceResult= DiceRollingBiggestNumber(diceRollNumber, diceSides);
                }
                diceResult += heroCaster.GetSkillsStat(s.skillToCheckInCalculations, false) / _divider;
                return diceResult;
            }
        }

        if (caster.GetComponent<IEnemy>() != null)
        {
            IEnemy enemyCaster = caster.GetComponent<IEnemy>();
            if (s.skillToCheckInCalculations != SkillsStat.None)
            {
                int diceResult = 0;
                if (sum)
                {
                    diceResult = DiceRollingSum(diceRollNumber, diceSides);
                }
                else
                {
                    diceResult = DiceRollingBiggestNumber(diceRollNumber, diceSides);
                }
                diceResult += enemyCaster.GetSkillsStat(s.skillToCheckInCalculations, false) / _divider;
                return diceResult;
            }
        }

        if (sum)
        {
            return  DiceRollingSum(diceRollNumber, diceSides);

        }
        else
        {
            return  DiceRollingBiggestNumber(diceRollNumber, diceSides);

        }


    }




    public static int GetAdditionalSkillPoints(Dictionary<SkillsStat,int> skillsUsed, out List<SkillsStat> winSkills) 
        // Player can get up to 5 additional skill points during levelup for using specific two skills
    {
        if (skillsUsed.Count == 0) { winSkills = null; return 0; }
        int total = 0;
        List<int> usedNums = new List<int>();
        List<SkillsStat> usedskills = new List<SkillsStat>();   
        foreach(KeyValuePair<SkillsStat, int> sNum in skillsUsed)
        {
            total += sNum.Value;
            usedNums.Add(sNum.Value);

        }
        if (total == 0) { winSkills = null; return 0; }

        usedNums.Sort();

        foreach (int i in usedNums)
        {
            foreach (KeyValuePair<SkillsStat, int> sNum in skillsUsed)
            {
                if(i==sNum.Value)usedskills.Add(sNum.Key);
            }
        }
        Dictionary<SkillsStat, float> percentage = new Dictionary<SkillsStat, float>();

        foreach (KeyValuePair<SkillsStat, int> sNum in skillsUsed)
        {
            percentage.Add(sNum.Key, ((float)sNum.Value/(float)total) );
        }

        int resultPercent = (int)((percentage[usedskills[0]] * 100) / 20);
        if(skillsUsed.Count == 2) resultPercent += (int)((percentage[usedskills[1]] * 100) / 20);
        winSkills = new List<SkillsStat>();
        winSkills.Add(usedskills[0]);
        if (skillsUsed.Count == 2) winSkills.Add(usedskills[1]);
        return resultPercent; 
    } 

    public static void SetMouseCursor(Texture2D norm, Vector2 hotspot)
    {
        cursorNormal = norm;
        Cursor.SetCursor(cursorNormal, hotspot, cursorMode);
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
        moneyCollected = party.SellBuyMoneyCheck(0);
        gemsCollected = party.CheckGems(0);
        expPoints = party.addExperiencePoints(0);
        partyLevel = party.GetPartyLevel();
        savedTimeToEncounter = playerController.GetCountdownToEncounter();
        partyFood = party.CheckFoodSupply(0);

        if (!levelsVisited.Contains(SceneManager.GetActiveScene().name)) levelsVisited.Add(SceneManager.GetActiveScene().name);
        heroesCurrentData.Clear();
        foreach (IHero ih in party.GetIHeroes())
        {
            HeroSavedCurrentData heroCurrentData = new HeroSavedCurrentData();
            heroCurrentData.FillCurrentData(ih);
            heroesCurrentData.Add(heroCurrentData);
        }

        foreach(HeroSavedCurrentData h in heroesCurrentData)
            {
           // Debug.Log(" hero current data " + h.heroIndex + " " + h.currentHealth + " " + h.currentMana + " " + h.currentHunger);
        }


        if (party != null) party.SaveEquipment();
        inventoryItemsSaved.Clear();
        getInventoryItem();
        foreach(visitedBlock v in playerController.GetVisitedBlocksCooordinates())
        {
            if(!visitedBlocks.Contains(v)) visitedBlocks.Add(v);
        }
        spellbooksSaved.Clear();
        party.SaveHeroesSpells();
        spellsAttachedToHeroes.Clear();
        party.GetSpellsAttached();

        spellsFromSpellbook.Clear();
        spellbook.SaveContinousSpells();
        mainStatsAdded.Clear();
        mainStatsAdded = party.ConvertHeroesMainStatsToSave();
        skillStatSaves.Clear();
        skillStatSaves = party.ConvertHeroesSkillsToSave();

        currentLevelName = levelName;
        levelChange = true;
        inventory.SaveKeysToGameInstance();
        soundManagerInGame.StopCurrentMusic();
        SceneManager.LoadScene(levelName, LoadSceneMode.Single);

    }


    public static void SaveIdentifiedItems(HeroInventoryItem heroInventoryItem)
    {
        identifiedItems.TryAdd(heroInventoryItem.container, heroInventoryItem);
    }

    public static bool CheckIfItemIdentified(int index)
    {
        return identifiedItems.TryGetValue(index, out HeroInventoryItem heroInventoryItem);
    }

    public static void  ChangeTimeFlow(float amount)
    {
        gameTimeFrame = amount;
    }

    public static float GetTimeFlow()
    {
        return gameTimeFrame;
    }

    public static int RestTime()
    {
        return timeProgress + (60*8);
    }


    public static int GetUnformattedTime()
    {
        return timeProgress;
    }

    public static int[] GetNormalTime()
    {
        int[] timeNormal = new int[4];
        timeNormal[0] = timeProgress;
        timeNormal[1] = gameTimeInNormalTime[0] % 60;
        timeNormal[2] = gameTimeInNormalTime[1] % 1440;
        timeNormal[3] = gameTimeInNormalTime[2] % 43200;
        return timeNormal;
    }


    public static IEnumerator TimeStep()
    {
        while (playerController.playerState != PlayerState.Battle)
        {
            if (timeStamp < timeProgress)
            {
                timeProgress++;
                progress(timeProgress);
                yield return new WaitForSeconds(gameTimeFrame);
            }
            else
            {
                timeProgress++;
                progress(timeProgress);
                yield return new WaitForSeconds(0.01f);
            }
            gameTimeInNormalTime[0] = (timeProgress / 60) + savedGameTimeInNormalTime[0];
            gameTimeInNormalTime[1]= (timeProgress / 1440) + savedGameTimeInNormalTime[1];
            gameTimeInNormalTime[2]= (timeProgress / 43200) + savedGameTimeInNormalTime[2];
        }
        Debug.Log("time coroutine stops");
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


    public static void SaveItemState(string _guid, SavedState _state)
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
        inventory.SaveInvetoryItemsToGameInstance();
        saveData.inventoryItemsSaved = inventoryItemsSaved;
        saveData.visitedBlocks = visitedBlocks;

        mainStatsAdded = party.ConvertHeroesMainStatsToSave();
        saveData.mainStatsAdded = mainStatsAdded;
        skillStatSaves = party.ConvertHeroesSkillsToSave();
        saveData.skillStatSaves = skillStatSaves;
        saveData.heroesPortraits = heroesPortraits;
        saveData.heroesNames = heroesNames;
        saveData.spellbooksSaved = spellbooksSaved;
        saveData.keysSaved = keysSaved;
        saveData.spellsFromSpellbook = spellsFromSpellbook;
        saveData.heroesSpellsAttachedSaved = spellsAttachedToHeroes;

        savedGameTimeInNormalTime[0] = gameTimeInNormalTime[0];
        savedGameTimeInNormalTime[1] = gameTimeInNormalTime[1];
        savedGameTimeInNormalTime[2] = gameTimeInNormalTime[2];
        saveData.savedGameTimeInNormalTime = savedGameTimeInNormalTime;
        saveData.dialoguesFinished = dialoguesFinished;
        party.SaveDialoguesToInstance();
        saveData.partyDialogues = currentUniqueDialogueNames;
        saveData.journalEntries = journalEntries;
        saveData.encounterOn =  playerController.GetEncounterState();
        saveData.moneyCollected = party.SellBuyMoneyCheck(0);
        saveData.gemsCollected = party.CheckGems(0);
        saveData.expPoints = party.addExperiencePoints(0);
        saveData.partyFood = party.CheckFoodSupply(0);  
        saveData.savedTimeToEncounter = playerController.GetCountdownToEncounter();
        heroesCurrentData.Clear();
        foreach (IHero ih in party.GetIHeroes())
        {
            HeroSavedCurrentData heroCurrentData = new HeroSavedCurrentData();
            heroCurrentData.FillCurrentData(ih);
            heroesCurrentData.Add(heroCurrentData);
        }
        foreach (HeroInventoryItem item in identifiedItems.Values)
        {
            saveData.identifiedItems.Add(item);
        }
        saveData.heroesCurrentData = heroesCurrentData;

        List<Vector3Int> customBattlesFinished = new List<Vector3Int>();
        foreach (KeyValuePair<Vector3Int, bool> c in customBattlesInPlaceFinished)
        {
            if (c.Value == true) customBattlesFinished.Add(c.Key);
        }
        saveData.customBattlesInPlaceFinished = customBattlesFinished;


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

            savedGameTimeInNormalTime = saveData.savedGameTimeInNormalTime;
            levelsVisited = saveData.visitedLevels;
            nextLevelPosition = saveData.playerPosition;
            nextLevelRotation = saveData.playercardinalDirection;
            inventoryItemsSaved = saveData.inventoryItemsSaved;
            visitedBlocks = saveData.visitedBlocks ;
            levelChange = true;
            keysSaved = saveData.keysSaved;
            dialoguesFinished = saveData.dialoguesFinished;
            spellsAttachedToHeroes = saveData.heroesSpellsAttachedSaved;
            spellsFromSpellbook = saveData.spellsFromSpellbook;
            journalEntries = saveData.journalEntries;
            currentUniqueDialogueNames = saveData.partyDialogues;

            mainStatsAdded = saveData.mainStatsAdded;
            skillStatSaves = saveData.skillStatSaves;
            noEncounter = saveData.encounterOn;
            customBattlesInPlaceFinished.Clear();
            foreach(Vector3Int v in saveData.customBattlesInPlaceFinished)
            {
              customBattlesInPlaceFinished.Add(v, true);
            }
            moneyCollected = saveData.moneyCollected;
            gemsCollected = saveData.gemsCollected;
            expPoints = saveData.expPoints;
            savedTimeToEncounter = saveData.savedTimeToEncounter;
            partyFood = saveData.partyFood;
            foreach (HeroInventoryItem item in saveData.identifiedItems)
            {
                identifiedItems.Add(item.container, item);
            }

            foreach(HeroSavedCurrentData h in saveData.heroesCurrentData)
            {
                heroesCurrentData.Add(h);
            }

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
        saveNames.fileNames = fileNamesList;
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
            fileNamesList.Clear();
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
        //Debug.Log("main stat check " +newmainsave[0].mainStat +" "+ newmainsave[0].amount);
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

    public static void OptionsDataSaver(SaveOptionsData saveOptionsData)
    {
        if (saveOptionsData != null)
        {
           Debug.Log("saving options " + saveOptionsData.musicVolume);
            string path = Application.persistentDataPath + "/" + "OptionsSaved";
            Gley.AllPlatformsSave.API.Save(saveOptionsData, path, DataWasSaved, false);
        }
        LoadOptionsSaved();
    }

    public static void LoadOptionsSaved()
    {
        string path = Application.persistentDataPath + "/" + "OptionsSaved";
        Gley.AllPlatformsSave.API.Load<SaveOptionsData>(path, OptionsWasLoaded, false);


    }
    static void OptionsWasLoaded(SaveOptionsData saveData, SaveResult result, string message)
    {
        if (result == SaveResult.EmptyData || result == SaveResult.Error)
        {
            Debug.Log("No Data File Found -> Creating new data...");
            saveData = new SaveOptionsData();
        }

        if (result == SaveResult.Success)
        {
            //Debug.Log("loading options" + saveData.musicVolume);
            saveOptionsData = saveData;
        }
    }

    public static SaveOptionsData GetSaveOptionsData()
    {
        return saveOptionsData;
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
    public List<int> heroesCurrentHealth = new List<int>();
    public List<int> heroesCurrentMana = new List<int>();
    public List<int> heroesCurrentHunger = new List<int>();
    public List<HeroSpellbookSaved> spellbooksSaved = new List<HeroSpellbookSaved>();
    public List<KeyToLocks> keysSaved = new List<KeyToLocks>();
    public List<SavedSpellsAttached> heroesSpellsAttachedSaved = new List<SavedSpellsAttached>();
    public List<SavedSpellsAttached> spellsFromSpellbook = new List<SavedSpellsAttached>();
    public List<int> savedGameTimeInNormalTime = new List<int>();
    public int timeProgress;
    public List<UniqueDialogueName> dialoguesFinished = new List<UniqueDialogueName>();    
    public List<UniqueDialogueName> partyDialogues = new List<UniqueDialogueName>();
    public List<string> journalEntries = new List<string>();
    public bool encounterOn = false;
    public List<Vector3Int> customBattlesInPlaceFinished = new List<Vector3Int>();
    public int savedTimeToEncounter = 0, expPoints = 0, moneyCollected = 0, gemsCollected = 0, partyFood = 0;
    public bool isDungeon = true;
    public List<HeroSavedCurrentData> heroesCurrentData = new List<HeroSavedCurrentData>();
    public List<HeroInventoryItem> identifiedItems = new List<HeroInventoryItem>();
}

[System.Serializable]
public class SaveOptionsData
{
    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public float uiVolume = 1.0f;

    public SaveOptionsData GetThis() { return this; }
}


[System.Serializable]
public struct HeroSavedCurrentData
{
    public int heroIndex;
    public int currentHealth;
    public int currentMana;
    public int currentHunger;
    public List<GameplayStatus> gameplayStatus;


    public void FillCurrentData(IHero hero)
    {
        heroIndex = hero.GetHeroIndex();
        currentHealth = hero.GetHeroHealth();
        currentMana = hero.GetHeroMana();
        currentHunger = hero.GetHeroHunger();
        gameplayStatus = new List<GameplayStatus>();
        gameplayStatus = hero.GetHeroStatus();
    }
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
    public int levelOfIdenifySaved = 0;
    public bool stackable = false;



    public void FillHeroItemData(HeroInventoryItem heroInventoryItem)
    {
        
        heroIndex = heroInventoryItem.heroIndex;
        itemType = heroInventoryItem.itemType;
        container = heroInventoryItem.container;
        stackAmount = heroInventoryItem.stackAmount;
        positionReplaced = heroInventoryItem.positionReplaced;
        level = heroInventoryItem.level;
        levelOfIdenifySaved = heroInventoryItem.levelOfIdenifySaved;
    }
    

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


public class GameTime
{
    public int minute;
    public int hour;
    public int day;

    public List<int> ConvertTime(int minutes)
    {
        List<int> convertedMinutes = new List<int>();

        convertedMinutes.Add(0);
        convertedMinutes.Add(0);
        convertedMinutes.Add(0);
        convertedMinutes[2] = minutes / 1440;
        day = convertedMinutes[2];
        minutes -= convertedMinutes[2] * 1440;
        convertedMinutes[1] = minutes / 60;
        hour = convertedMinutes[1];
        minutes -= convertedMinutes[1] * 60;
        convertedMinutes[0] = minutes;
        minute = convertedMinutes[0];

        return convertedMinutes;
    }

}


public class GameMoney
{
    public int coin;
    public int sivercoin;
    public int goldcoin;

    public List<int> ConvertCoins(int coinamount)
    {
        List<int> convertedcoins = new List<int>();

        convertedcoins.Add(0);
        convertedcoins.Add(0);
        convertedcoins.Add(0);
        convertedcoins[2] = coinamount / 1000;
        coinamount -= convertedcoins[2] * 1000;
        convertedcoins[1] = coinamount / 100;
        coinamount -= convertedcoins[1] * 100;
        convertedcoins[0] = coinamount;

        return convertedcoins;
    }
}

public class KeyToLocks
{
    public KeyType keyType;
    public int amount;
}

[System.Serializable]
public class SavedSpellsAttached
{
    public int heroID;
    public List<Spell> spell = new List<Spell>();
    public List<int> timesToFinish = new List<int>(); // if -1 means infinite
}