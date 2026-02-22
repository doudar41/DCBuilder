using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffIcons : MonoBehaviour
{
    [SerializeField] List<DebuffIcon> debuffIcons = new List<DebuffIcon>();
    Dictionary<GameplayStates, DebuffIcon> debuffIconsDict= new Dictionary<GameplayStates, DebuffIcon>();

    private void Awake()
    {
            
        foreach (DebuffIcon d in debuffIcons)
        {
            d.gameObject.SetActive(true);
            if (!debuffIconsDict.ContainsKey(d.GetDebuffType())) debuffIconsDict.Add(d.GetDebuffType(), d);
            d.gameObject.SetActive(false);
        }
    }

    public void AddDebuffIcon(GameplayStates debuffType, bool onOff)
    {
        if (debuffIconsDict.ContainsKey(debuffType)) { debuffIconsDict[debuffType].gameObject.SetActive(onOff); debuffIconsDict[debuffType].OnOffState(onOff); }
    }

    public void ClearAllDebuffs()
    {
        foreach (GameplayStates gameplayStates in Enum.GetValues(typeof(GameplayStates)))
        {
            if (debuffIconsDict.ContainsKey(gameplayStates))
            {
                AddDebuffIcon(gameplayStates, false);
            }

        }
    }

}
