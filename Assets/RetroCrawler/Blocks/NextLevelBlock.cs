using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelBlock : MonoBehaviour, IInteractables
{
    public int GetWeight(out int carringCapacity)
    {
        carringCapacity = 0;
        return 0;
    }

    public List<InteractablesEnum> WhatIsIt()
    {
        List<InteractablesEnum> interactablesEnums = new List<InteractablesEnum>();
        interactablesEnums.Add(InteractablesEnum.LEVEL_EXIT);
        return interactablesEnums;
    }


}
