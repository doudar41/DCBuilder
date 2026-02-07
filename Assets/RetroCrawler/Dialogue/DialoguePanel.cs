using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Ami.BroAudio;

public class DialoguePanel : MonoBehaviour
{
    [SerializeField] GameObject dialoguePanelUI;
    [SerializeField] Image portrait;
    [SerializeField] Sprite noPortrait;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject buttonPanel;
    [SerializeField] List<DialogueButtonUI> dialogueButtons = new List<DialogueButtonUI>();
    [SerializeField] Button letMeGoQuitTalk;
    [SerializeField] CameraOrder cameraUI;
    [SerializeField] SoundID openCloseDialogue = default;
    List<TextMeshProUGUI> buttonsTextFields = new List<TextMeshProUGUI>();
    UniqueDialogueName currentdialogue;
    bool dialogueFinished = false;
    IDialogue iDialogueWithUniqueNames;

    List<UniqueDialogueName> dialogueTreeForRemove = new List<UniqueDialogueName>();
    int dialoguePhraseIndex = 0;

    public UnityEvent<UniqueDialogueName> deleteDialogueOption;
    public UnityEvent<List<string>, List<ResultMsg>> textToLog;

    private void Awake()
    {
        gameObject.SetActive(true);
        GameInstance.dialoguePanel = this;
        foreach (DialogueButtonUI b in dialogueButtons)
        {
            buttonsTextFields.Add(b.GetComponentInChildren<TextMeshProUGUI>());
        }
        letMeGoQuitTalk.onClick.AddListener(CloseDialogue);
    }

    void CloseDialogue()
    {
        if (!dialogueFinished) 
        { 
            NextDialoguePhrase(); return; 
        }
        print("finishing dialogue ");
        BroAudio.Play(openCloseDialogue).SetVelocity(1);
        buttonPanel.SetActive(false);
        var dialogue = GameInstance.dataBase.GetDialogue(currentdialogue);

        if (dialogue != null)
        {
            if (dialogue.deleteDialogue)
            {

                iDialogueWithUniqueNames.DeleteDialogue();
            }

            foreach(UniqueDialogueName un in dialogue.namesToDeleteFromBlock)
            {
                if (iDialogueWithUniqueNames != null)
                {
                    iDialogueWithUniqueNames.DeleteDialogueOption(un);
                }
                if (!GameInstance.dialoguesFinished.Contains(un)) GameInstance.dialoguesFinished.Add(un);
            }

            foreach (UniqueDialogueName un in dialogue.namesToDeleteFromParty)
            {
               if(GameInstance.party.currentUniqueDialogueNames.Contains(un)) GameInstance.party.currentUniqueDialogueNames.Remove(un);
               if (!GameInstance.dialoguesFinished.Contains(un)) GameInstance.dialoguesFinished.Add(un);
            }

            foreach (UniqueDialogueName un in dialogue.namesAddToParty)
            {
                if (!GameInstance.party.currentUniqueDialogueNames.Contains(un)) GameInstance.party.currentUniqueDialogueNames.Add(un);
            }

            foreach (ItemScriptableContainer item in dialogue.itemsAddToParty)
            {
                GameInstance.inventory.FindEmptySlotAndPutItem( GameInstance.dataBase.HeroInventoryFromITemScriptable(item), 1, false);
            }

            GameInstance.party.MoneyGoes(-dialogue.goldAmount);
            if(dialogue.journalEnter !="") GameInstance.gameJournal.AddEntryToJournal(dialogue.journalEnter);
        }
        currentdialogue = UniqueDialogueName.None;
        dialogueText.text = "";
        portrait.sprite = noPortrait;
        GameInstance.playerController.dialogueIsOpened = false;
        if(!GameInstance.playerController.shopIsOpened) cameraUI.BattleLogWithGameplay();
        else cameraUI.ShopWithoutBattlelog();
        dialoguePanelUI.SetActive(false);
        dialogueFinished = false;
        dialogueTreeForRemove.Clear();
        dialoguePhraseIndex = 0;
    }

    public void SetIDialogue(IDialogue idialogue)
    {
        iDialogueWithUniqueNames = idialogue;
    }

    public void ActivateFirstDialogue()
    {
        BroAudio.Play(openCloseDialogue).SetVelocity(0);
        dialogueFinished =false;
        letMeGoQuitTalk.interactable = false;
        letMeGoQuitTalk.GetComponentInChildren<TextMeshProUGUI>().text = "Next";

        List<UniqueDialogueName> nameDialogue = iDialogueWithUniqueNames.RunDialogue();
        UniqueDialogueName un = GameInstance.dataBase.CheckPriority(nameDialogue);
        if(un == UniqueDialogueName.None && nameDialogue.Count>0) 
        {
            GameInstance.playerController.dialogueIsOpened = false;
            return;
        }
        if (GameInstance.party.currentUniqueDialogueNames.Contains(un) && un != UniqueDialogueName.None) GameInstance.dialoguePanel.ActivateDialogue(un);
    }

    public void ActivateDialogue(UniqueDialogueName uniqueDialogueName)
    {

        if (GameInstance.dataBase.GetDialogue(uniqueDialogueName) == null)
            { print("dialogue is not in a base " + uniqueDialogueName); CloseDialogue(); return; }

        dialoguePanelUI.SetActive(true);
        buttonPanel.SetActive(true);
        cameraUI.ShopWithDialogue();
        var dialogue = GameInstance.dataBase.GetDialogue(uniqueDialogueName);
        currentdialogue = uniqueDialogueName;
        dialogueFinished = false;
        foreach (DialogueButtonUI b in  dialogueButtons)
        {
            b.gameObject.SetActive(false);
        }

        if (dialogue.dialogue_phrases.Count > 1) { letMeGoQuitTalk.interactable = true; }
        else
        {
            if (dialogue.dialogueButtons.Count != 0)
            {
                for (int i = 0; i < dialogue.dialogueButtons.Count; i++)
                {
                    dialogueButtons[i].gameObject.SetActive(true);
                    buttonsTextFields[i].text = dialogue.dialogueButtons[i].buttonText;
                    dialogueButtons[i].SetDialogueName(dialogue.dialogueButtons[i].uniqueName);
                    dialogueButtons[i].getDialogueName.AddListener(NextDialogue);
                }
            }

            GameInstance.party.addExperiencePoints(GameInstance.dataBase.GetDialogue(currentdialogue).experiencePoints);
        }


        if(dialogue.dialogue_phrases.Count != 0)
        {
            dialoguePhraseIndex = 0;
            dialogueText.text = dialogue.dialogue_phrases[dialoguePhraseIndex].dialogueTexts;
            textToLog.Invoke(new () { dialogueText.text }, null);
            portrait.sprite = dialogue.dialogue_phrases[dialoguePhraseIndex].portraits; 
        }

        if (dialoguePhraseIndex == dialogue.dialogue_phrases.Count - 1) 
        { 
            letMeGoQuitTalk.GetComponentInChildren<TextMeshProUGUI>().text = dialogue.lastButtonText;
            if (dialogue.dialogueButtons.Count == 0) { letMeGoQuitTalk.interactable = true; dialogueFinished = true; }
        }
        else
        {
             letMeGoQuitTalk.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
            dialogueFinished = false;
        }

    }


    void NextDialogue(UniqueDialogueName uniqueDialogueName)
    {
        ActivateDialogue(uniqueDialogueName);
    }

    public void NextDialoguePhrase()
    {
        if (GameInstance.dataBase.GetDialogue(currentdialogue) == null) return;

        dialoguePhraseIndex = Mathf.Clamp(dialoguePhraseIndex + 1, 0, GameInstance.dataBase.GetDialogue(currentdialogue).dialogue_phrases.Count - 1);
        dialogueText.text = GameInstance.dataBase.GetDialogue(currentdialogue).dialogue_phrases[dialoguePhraseIndex].dialogueTexts;
        textToLog.Invoke(new() { dialogueText.text }, null);
        portrait.sprite = GameInstance.dataBase.GetDialogue(currentdialogue).dialogue_phrases[dialoguePhraseIndex].portraits;

        var dialogue = GameInstance.dataBase.GetDialogue(currentdialogue);
        if (dialoguePhraseIndex == GameInstance.dataBase.GetDialogue(currentdialogue).dialogue_phrases.Count - 1)
        {
            if(GameInstance.dataBase.GetDialogue(currentdialogue).dialogueButtons.Count == 0)
            {
                letMeGoQuitTalk.GetComponentInChildren<TextMeshProUGUI>().text = dialogue.lastButtonText;
                dialogueFinished = true;
            }
            else
            {
                for (int i = 0; i < dialogue.dialogueButtons.Count; i++)
                {
                    dialogueButtons[i].gameObject.SetActive(true);
                    buttonsTextFields[i].text = dialogue.dialogueButtons[i].buttonText;
                    dialogueButtons[i].SetDialogueName(dialogue.dialogueButtons[i].uniqueName);
                    dialogueButtons[i].getDialogueName.AddListener(NextDialogue);
                }
            }
            GameInstance.party.addExperiencePoints(GameInstance.dataBase.GetDialogue(currentdialogue).experiencePoints);
        }
    }


    public void DeleteDialogueOptionsInBlock(UniqueDialogueName uniqueDialogueName, bool deleteRepeating)
    {
        if (deleteRepeating)
        {
            iDialogueWithUniqueNames.DeleteDialogueOption(uniqueDialogueName);
        }


    }
}
