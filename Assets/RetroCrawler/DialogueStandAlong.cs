
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class DialogueStandAlong : MonoBehaviour, IInteractables, IDialogue
{
    
    [SerializeField]  List<UniqueDialogueName> dialogues = new List<UniqueDialogueName>();
    [SerializeField] List<InteractablesEnum> dialogueEnum = new List<InteractablesEnum>();
    [SerializeField] Collider col;
    [SerializeField] UniqueDialogueName dialogueToOpen;
    [SerializeField] GameObject objectToOpen;

    public UnityEvent OnDialogueFinished;
    public void DeleteDialogue()
    {
        if (dialogues.Count == 0) { if (dialogueEnum.Contains(InteractablesEnum.DIALOGUE)) dialogueEnum.Remove(InteractablesEnum.DIALOGUE); }
        OnDialogueFinished.Invoke();
    }

    public void DeleteDialogueOption(UniqueDialogueName uniqueDialogueName)
    {
        if (dialogues.Contains(uniqueDialogueName)) dialogues.Remove(uniqueDialogueName);
        DeleteDialogue();
        if (uniqueDialogueName == dialogueToOpen)
        {
            var door = objectToOpen.GetComponent<IDoor>();

            if (door != null)
            {
                door.OpenDoor();
            }
        }
    }

    public string GetGUID()
    {
        return "";
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
    public int GetDialogueOptionCount()
    {
        return dialogues.Count;
    }

}
