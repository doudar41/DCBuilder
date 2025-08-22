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

    static bool loadingLevel;

    static Vector3 nextLevelPlayerPosition;
    static CardinalDirections nextLevelDirection;

    // Heroes data
    public static Dictionary<int, Dictionary<MainStat, int>> mainHeroesStatsSaved = new Dictionary<int, Dictionary<MainStat, int>>();
    public static Dictionary<int, Dictionary<SkillsStat, int>> skillBonusHeroesStatsSaved = new Dictionary<int, Dictionary<SkillsStat, int>>();
    public static Dictionary<int, Dictionary<ItemType, ItemScriptableContainer>> equipmentHeroesSaved = new Dictionary<int, Dictionary<ItemType, ItemScriptableContainer>>();
    

    //SpellAttachedSaved 
    
    public static Vector3Int playerPositionSaved, nextLevelPosition;
    public static CardinalDirections playerRotationSaved, nextLevelRotation;


    

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
        SceneManager.LoadScene("Spellbook", LoadSceneMode.Single);
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


    
}
