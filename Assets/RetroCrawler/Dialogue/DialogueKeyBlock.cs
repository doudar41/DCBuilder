using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueKeyBlock : MonoBehaviour, IInteractables, DialogueKey
{
    [SerializeField] UniqueDialogueName uniqueDialogueName;
    public int GetWeight(out int carringCapacity)
    {
        throw new System.NotImplementedException();
    }

    public List<InteractablesEnum> WhatIsIt()
    {
        List<InteractablesEnum> interactablesEnums = new List<InteractablesEnum>();
        interactablesEnums.Add(InteractablesEnum.DIALOGUEKEY);
        return interactablesEnums;
    }

    public UniqueDialogueName GetUniqueDialogueName()
    {
        GetComponent<BoxCollider>().enabled = false;
        return uniqueDialogueName;
    }
}


public interface DialogueKey
{
    public UniqueDialogueName GetUniqueDialogueName();
}