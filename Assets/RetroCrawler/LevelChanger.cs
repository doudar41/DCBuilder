using Ami.BroAudio.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChanger : MonoBehaviour
{

    [SerializeField] List<LevelGraphic> levelGraphicsList = new List<LevelGraphic>();
    [SerializeField] CameraOrder cameraOrder;
    [SerializeField] MarkRecallMenu markRecallMenu;
    Dictionary<string, GameObject> levelGraficDictionary = new Dictionary<string, GameObject>();
    string levelNameSaved = string.Empty;

    private void Awake()
    {
        foreach(LevelGraphic lg in levelGraphicsList)
        {
            levelGraficDictionary.Add(lg.name, lg.graphicObject);
        }
    }

    public bool CheckLevelName(string levelName)
    {
        return levelGraficDictionary.ContainsKey(levelName);
    }

    public void OpenLevelEntranceGraphics(string levelName)
    {
        markRecallMenu.CancelMarkRecall();
        cameraOrder.BattleLogWithGameplay();
        GameInstance.playerController.shopIsOpened = true;
        GameObject levelGraphic;
        levelNameSaved = levelName;
        if (levelGraficDictionary.ContainsKey(levelName)) { levelGraphic = levelGraficDictionary[levelName]; }
        else return;

        if(levelName == "CaveLevel01")
        {
            GameInstance.soundManagerInGame.DuckExploreMusicSwitchToAmbience(RoomSpaces.CaveEntrance);
        }
        levelGraphic.SetActive(true);
        levelGraphic.GetComponentInChildren<animateUIImage>().StartAnimation();
    }


    public void EnterLevel()
    {
        GameInstance.LoadNextLevel(levelNameSaved);
    }

    public void CancelEnteringLevel()
    {
        GameObject levelGraphic = levelGraficDictionary[levelNameSaved];
        levelGraphic.SetActive(false);
        levelGraphic.GetComponentInChildren<animateUIImage>().StopAnimation();
        levelNameSaved = string.Empty;
        GameInstance.playerController.InputEnable(true);
        GameInstance.soundManagerInGame.UnDuckExploreMusicSwitchToAmbience();
        GameInstance.playerController.shopIsOpened = false;
    }

}

[System.Serializable]
public struct LevelGraphic
{
    public string name;
    public GameObject graphicObject;
}