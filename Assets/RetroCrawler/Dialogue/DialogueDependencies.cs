
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName ="Dialogue")]
public class DialogueDependencies: ScriptableObject
{
    public UniqueDialogueName uniqueName;
    public List<DialogueTextContainer> dialogue_phrases = new List<DialogueTextContainer>();
    public string journalEnter = "";
    public int priorityIndex = 0;
    public List<DialogueButton> dialogueButtons = new List<DialogueButton>();
    public bool oneTimeDialogue = true;
    public List<UniqueDialogueName> namesToDeleteFromBlock = new List<UniqueDialogueName>();
    public List<UniqueDialogueName> namesToDeleteFromParty = new List<UniqueDialogueName>();
    public bool deleteDialogue = false;
    public string lastButtonText = "Quit";
}

[System.Serializable]
public class DialogueTextContainer
{
    public Sprite portraits;
    public string dialogueTexts;
}

[System.Serializable]
public class DialogueButton
{
    public UniqueDialogueName uniqueName;
    public string buttonText;
}

public enum UniqueDialogueName //Party would have current list of dialogue names if block have one of them then launch dialogue
{
    None,
    Enquiry_on_Hitherin,
    FirstTavernDialogue,
    FirstDukeEncounter,
    BartenderCaveTalks,
    AfterCaveDukeDialogue,
    CastleGuardsProhibitEntrance,
    CastleGuardsAllowToEnter
}