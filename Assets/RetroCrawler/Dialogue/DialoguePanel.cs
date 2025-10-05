using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : MonoBehaviour
{
    [SerializeField] GameObject dialoguePanelUI;
    [SerializeField] Image portrait;
    [SerializeField] Sprite noPortrait;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject buttonPanel;
    [SerializeField] List<DialogueButtonUI> dialogueButtons = new List<DialogueButtonUI>();
    [SerializeField] Button nextButton;
    [SerializeField] Button letMeGoQuitTalk;
    [SerializeField] Camera cameraUI;
    List<TextMeshProUGUI> buttonsTextFields = new List<TextMeshProUGUI>();
    UniqueDialogueName currentdialogue;
    bool dialogueFinished = false;
    IBlock blockWithUniqueNames;

    List<UniqueDialogueName> dialogueTreeForRemove = new List<UniqueDialogueName>();
    int dialoguePhraseIndex = 0;

    public UnityEvent<UniqueDialogueName> deleteDialogueOption;

    private void Awake()
    {
        gameObject.SetActive(true);
        GameInstance.dialoguePanel = this;
        foreach (DialogueButtonUI b in dialogueButtons)
        {
            buttonsTextFields.Add(b.GetComponentInChildren<TextMeshProUGUI>());
        }

        gameObject.SetActive(false);
        letMeGoQuitTalk.onClick.AddListener(CloseDialogue);
        
    }

    void CloseDialogue()
    {
        if (!dialogueFinished) 
        { 
            NextDialoguePhrase(); return; 
        }

        buttonPanel.SetActive(false);
        var dialogue = GameInstance.dataBase.GetDialogue(currentdialogue);

        if (dialogue != null)
        {
            if (dialogue.deleteDialogue)
            {
                blockWithUniqueNames.DeleteDialogue();
            }

            foreach(UniqueDialogueName un in dialogue.namesToDeleteFromBlock)
            {
                if(blockWithUniqueNames != null)
                {
                    blockWithUniqueNames.DeleteDialogue();
                }
            }

            foreach (UniqueDialogueName un in dialogue.namesToDeleteFromParty)
            {
               if(GameInstance.party.currentUniqueDialogueNames.Contains(un)) GameInstance.party.currentUniqueDialogueNames.Remove(un);
            }
        }



        currentdialogue = UniqueDialogueName.None;
        dialogueText.text = "";
        portrait.sprite = noPortrait;
        GameInstance.playerController.dialogueIsOpened = false;
        if(!GameInstance.playerController.shopIsOpened) cameraUI.depth = -1;
        dialoguePanelUI.SetActive(false);
        dialogueFinished = false;
        dialogueTreeForRemove.Clear();
    }

    public void SetIBlock(IBlock iblock)
    {
        blockWithUniqueNames = iblock;
    }

    public void ActivateFirstDialogue()
    {
        UniqueDialogueName nameDialogue = blockWithUniqueNames.RunDialogue();
        GameInstance.dialoguePanel.ActivateDialogue(nameDialogue);
    }

    public void ActivateDialogue(UniqueDialogueName uniqueDialogueName)
    {

        if (GameInstance.dataBase.GetDialogue(uniqueDialogueName) == null)
            { print("dialogue is not in a base"); CloseDialogue(); return; }

        dialoguePanelUI.SetActive(true);

        buttonPanel.SetActive(true);
        cameraUI.depth = 2;
        var dialogue = GameInstance.dataBase.GetDialogue(uniqueDialogueName);
        currentdialogue = uniqueDialogueName;

        foreach(DialogueButtonUI b in  dialogueButtons)
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
        }


        if(dialogue.dialogue_phrases.Count != 0)
        {
            dialoguePhraseIndex = 0;
            dialogueText.text = dialogue.dialogue_phrases[dialoguePhraseIndex].dialogueTexts;
            portrait.sprite = dialogue.dialogue_phrases[dialoguePhraseIndex].portraits; 
        }

        if (dialoguePhraseIndex == dialogue.dialogue_phrases.Count - 1) 
        { 
            letMeGoQuitTalk.GetComponentInChildren<TextMeshProUGUI>().text = dialogue.lastButtonText;
            dialogueFinished = true;
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
        dialoguePhraseIndex = Mathf.Clamp(dialoguePhraseIndex + 1, 0, GameInstance.dataBase.GetDialogue(currentdialogue).dialogue_phrases.Count - 1);
        dialogueText.text = GameInstance.dataBase.GetDialogue(currentdialogue).dialogue_phrases[dialoguePhraseIndex].dialogueTexts;
        var dialogue = GameInstance.dataBase.GetDialogue(currentdialogue);
        if (dialoguePhraseIndex == GameInstance.dataBase.GetDialogue(currentdialogue).dialogue_phrases.Count - 1)
        {
            //GameInstance.dataBase.GetDialogue(currentdialogue).dialogueDone = true;
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
        }
    }


    public void DeleteDialogueOptionsInBlock(UniqueDialogueName uniqueDialogueName, bool deleteRepeating)
    {
        if (deleteRepeating)
        {
            print("delete " + uniqueDialogueName + blockWithUniqueNames);
            blockWithUniqueNames.DeleteDialogueOption(uniqueDialogueName);
        }


    }
}
