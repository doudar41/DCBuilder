using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Partyrest : MonoBehaviour
{
    int timestamp = 0;
    bool timerStarted = false;
    [SerializeField] GameObject restCampAnimation;
    [SerializeField] int restHours = 8;
    [SerializeField] int survivalThreshold = 10;
    int maxSurvival = 4; //example value
    int currentHour = 0;
    [SerializeField] CameraOrder cameraOrder;

    private void Awake()
    {
        GameInstance.progress += OnProgressChanged;
    }


    public void StartRest()
    {
        if (timerStarted) return;
        if (GameInstance.playerController.GetEncounterState())
        {
            GameInstance.spellbook.battlelogEvent.Invoke(new List<string>() {"Please, proceed to inn to have a rest"},null);
            return;
        }
        timestamp = GameInstance.GetNormalTime()[1]%24;
        timerStarted = true;
        GameInstance.dayNightChange.ChangeTimeFlow(0.001f);
        restCampAnimation.SetActive(true);
        restCampAnimation.GetComponent<animateUIImage>().StartAnimation();
        cameraOrder.ShopWithoutBattlelog();
    }

    public void SetSurvivalThreshhold(int amount)
    {
        survivalThreshold = amount;
    }

    void OnProgressChanged(int count)
    {
        if (!timerStarted) return;
        int hour = GameInstance.GetNormalTime()[1] % 24;

        if(currentHour != hour)
        {
            currentHour = hour;
            CheckHeroSurvival();
            if(Random.Range(0, survivalThreshold) > maxSurvival)
            {
                timerStarted = false;
                GameInstance.dayNightChange.ChangeTimeFlow(0.5f);
                restCampAnimation.GetComponent<animateUIImage>().StopAnimation();
                restCampAnimation.SetActive(false);
                IBlock iblock = GameInstance.playerController.GetBlockInterface(GameInstance.playerController.gameObject.transform.position);
                GameInstance.battleManager.CustomBattleStart(null, iblock, iblock.GetBattleGroundEnvironment());
            cameraOrder.BattleLogWithGameplay();
            }
        }
        if (hour == (timestamp + 8)%24)
        {
            timerStarted = false;
            GameInstance.dayNightChange.ChangeTimeFlow(0.5f);
            GameInstance.party.AddSomeFood(0);
            restCampAnimation.GetComponent<animateUIImage>().StopAnimation();
            restCampAnimation.SetActive(false);
            cameraOrder.BattleLogWithGameplay();
        }
    }


    void CheckHeroSurvival() 
    { 

        foreach (Hero hero in GameInstance.party.GetPartyMembers())
        {
            int survival = hero.GetMainStatsForUI()[MainStat.Survival];
            if (maxSurvival< survival)
            {
                maxSurvival = survival;
            }
        }

    }

}
