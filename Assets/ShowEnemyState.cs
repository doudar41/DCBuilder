using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowEnemyState : MonoBehaviour
{
    [SerializeField]  List<GameObject> stateIcons = new List<GameObject>();
    [SerializeField] List<GameplayStates> gameplayStatuses = new List<GameplayStates>();

    Dictionary<GameplayStates, GameObject> stateIconDictionary = new Dictionary<GameplayStates, GameObject>();
    List<int> turnsLeft = new List<int>();


    void Start()
    {
        for (int i = 0; i < stateIcons.Count; i++)
        {
            stateIconDictionary.Add(gameplayStatuses[i], stateIcons[i]);
        }

        GameInstance.playerController.timeForward += TimeForward;
        GameInstance.battleManager.battlePassTime += TimeForward;
        for (int i = 0; i < stateIcons.Count; i++)
        {
            turnsLeft.Add(0);
        }

    }
    private void OnDestroy()
    {
        GameInstance.playerController.timeForward -= TimeForward;
        GameInstance.battleManager.battlePassTime -= TimeForward;
    }

    public void ShowStateIcon(GameplayStates state, bool onOff, int numberOfTurns)
    {
        if (stateIconDictionary.ContainsKey(state))
        {
            stateIconDictionary[state].SetActive(onOff);
            if (onOff)
            {
               int index =  stateIcons.IndexOf(stateIconDictionary[state]);
               if( turnsLeft.Contains(index))
                {
                    turnsLeft[index] = numberOfTurns;
                }
            }
        }
    }

    void TimeForward(int count)
    {

        for (int i = 0; i < turnsLeft.Count; i++)
        {
            print(turnsLeft[i]+ " - " + gameplayStatuses[i]);
            turnsLeft[i]--;
            if (turnsLeft[i] <= 0)
            {
                stateIcons[i].SetActive(false);
                turnsLeft[i] = 0;

            }
        }
    }


}
