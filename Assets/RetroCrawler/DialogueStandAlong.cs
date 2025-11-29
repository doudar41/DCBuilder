using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueStandAlong : MonoBehaviour, IInteractables, IDialogue
{

    [SerializeField]  List<UniqueDialogueName> dialogues = new List<UniqueDialogueName>();
    [SerializeField] List<InteractablesEnum> dialogueEnum = new List<InteractablesEnum>();
    [SerializeField] Collider col;

    public void DeleteDialogue()
    {
        if (dialogueEnum.Contains(InteractablesEnum.DIALOGUE)) dialogueEnum.Remove(InteractablesEnum.DIALOGUE);
        //col.enabled = false;

    }

    public void DeleteDialogueOption(UniqueDialogueName uniqueDialogueName)
    {
        if (dialogues.Contains(uniqueDialogueName)) dialogues.Remove(uniqueDialogueName);

    }

    public int GetWeight(out int carringCapacity)
    {
        carringCapacity = 0;
        return 0;
    }

    public List<UniqueDialogueName> RunDialogue()
    {
        return dialogues;
    }

    public List<InteractablesEnum> WhatIsIt()
    {

        return dialogueEnum;
    }


}
