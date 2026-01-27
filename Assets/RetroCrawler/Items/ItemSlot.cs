using Ami.BroAudio;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{

    int stackAmount = 1;
    HeroInventoryItem inventoryItem;
    [SerializeField] Image itemAvatar;
    [SerializeField]
    Sprite emptySlotSprite;
    [SerializeField]
    TextMeshProUGUI amountText;
    [SerializeField] GameObject describePrefab;
    GameObject describeInstance;


    private void Awake()
    {
        GameInstance.getInventoryItem += SaveInventoryItemsToGameInstance;
    }

    private void OnDestroy()
    {
        GameInstance.getInventoryItem -= SaveInventoryItemsToGameInstance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //print(eventData.button + " "+ eventData.clickCount);
        int clickCount = eventData.clickCount;

        if (GameInstance.spellbook.IdentifyModeActive()) { return; }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (inventoryItem == null) 
            { print("no more clicking"); return; }
            if (inventoryItem.itemType == ItemType.CONSUMABLE)
            {
                if (stackAmount > 1)
                {
                    stackAmount--;
                    inventoryItem.stackAmount = stackAmount;
                    List<ResultMsg> results = GameInstance.party.activeHero.ApplySpellToHero(GameInstance.dataBase.GetItemFromBaseByIndex( inventoryItem.container).spellContainer);
                    amountText.text = stackAmount.ToString();
                    GameInstance.spellbook.battlelogEvent.Invoke(new List<string>() { },results);
                    BroAudio.Play(GameInstance.inventory.GetHeroItemScriptableByIndex(inventoryItem.container).inventorySound);
                }
                else
                {
                    List<ResultMsg> results = GameInstance.party.activeHero.ApplySpellToHero(GameInstance.dataBase.GetItemFromBaseByIndex(inventoryItem.container).spellContainer);
                    stackAmount = 0;
                    inventoryItem = null;
                    itemAvatar.sprite = emptySlotSprite;
                    amountText.text = stackAmount.ToString();
                    GameInstance.spellbook.battlelogEvent.Invoke(new List<string>() { }, results);

                }
                return;
            }
            if (inventoryItem.itemType == ItemType.QUEST)
            {
                ItemScriptableContainer item =  GameInstance.dataBase.GetItemFromBaseByIndex(inventoryItem.container);
                if (item.journalEntry!="")
               GameInstance.gameJournal.AddEntryToJournal(item.journalEntry);

                foreach(UniqueDialogueName un in item.dialogueKeys)
                {
                    if (!GameInstance.party.currentUniqueDialogueNames.Contains(un))
                    {
                        GameInstance.party.currentUniqueDialogueNames.Add(un);
                    }
                }
                RemoveItem();
                return;
            }
        }
        if (clickCount == 1)
        {
           if (IsEmpty())
            {
                HeroInventoryItem slotStruct = GameInstance.playerController.GetItemFromCursor();
                inventoryItem = slotStruct;

                if (inventoryItem != null)
                {
                    if (slotStruct.stackAmount > 1)
                    { 
                        stackAmount = slotStruct.stackAmount; 
                    }
                    else stackAmount = 1;
                    itemAvatar.sprite = GameInstance.dataBase.GetItemFromBaseByIndex(inventoryItem.container).InventorySprite;
                    amountText.text = stackAmount.ToString();
                    BroAudio.Play(GameInstance.inventory.GetHeroItemScriptableByIndex(inventoryItem.container).inventorySound);
                }
            }
            else
            {
                if (stackAmount >= 1 && GameInstance.playerController.IsCursorBusy())
                {
                    HeroInventoryItem slotStruct =  GameInstance.playerController.GetItemFromCursor();
                    if (GameInstance.dataBase.GetItemFromBaseByIndex(slotStruct.container) == GameInstance.dataBase.GetItemFromBaseByIndex(inventoryItem.container))
                    {
                        if (slotStruct.stackable)
                        {
                            stackAmount += slotStruct.stackAmount;
                        }
                        else
                        {
                            GameInstance.playerController.SetPlayerCursorBusy(slotStruct); return;
                        }

                    }
                    else
                    {
                        GameInstance.playerController.SetPlayerCursorBusy(inventoryItem);
                        if (slotStruct.stackAmount > 1)
                        {
                            stackAmount = slotStruct.stackAmount;
                        }
                        else stackAmount = 1;
                        inventoryItem = slotStruct;
                        itemAvatar.sprite = GameInstance.dataBase.GetItemFromBaseByIndex(inventoryItem.container).InventorySprite;
                        amountText.text = stackAmount.ToString();
                        //exchange items in a slot
                    }

                    amountText.text = stackAmount.ToString();
                    BroAudio.Play(GameInstance.inventory.GetHeroItemScriptableByIndex(inventoryItem.container).inventorySound);
                    return;
                }
                if (stackAmount >= 1 && !GameInstance.playerController.IsCursorBusy())
                {
                   // print("one item left");
                GameInstance.playerController.SetPlayerCursorBusy(inventoryItem);
                stackAmount = 0;
                inventoryItem = null;
                itemAvatar.sprite = emptySlotSprite;
                amountText.text = stackAmount.ToString();
                return;
                }
            }
        }
    }

    public bool AddItemInSlot(HeroInventoryItem itemTemp, int amount)
    {
        if (itemTemp != null)
        {
            if (itemTemp== inventoryItem)
            {
                stackAmount += amount;
                return true;
            }

            if (IsEmpty())
            {
                inventoryItem = itemTemp;
                stackAmount = amount;
                itemAvatar.sprite = GameInstance.dataBase.GetItemFromBaseByIndex(inventoryItem.container).InventorySprite;
                amountText.text = stackAmount.ToString();
                return true;
            }
        }
        return false;
    }

    public bool IsEmpty()
    {
        return inventoryItem==null;
    }

    bool SaveInventoryItemsToGameInstance()
    {
        
        if (inventoryItem != null)
        {
            inventoryItem.stackAmount = stackAmount;
            GameInstance.AddInventoryItem(inventoryItem);
            return true;
        }
        else
        {
            GameInstance.AddInventoryItem(inventoryItem);
            return false;
        }
    }

    public HeroInventoryItem GetItemFromSlot()
    {
        return inventoryItem;
    }


    public void RemoveItem()
    {
        stackAmount = 0;
        inventoryItem = null;
        itemAvatar.sprite = emptySlotSprite;
        amountText.text = stackAmount.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!GameInstance.spellbook.IdentifyModeActive()) { return; }
        if (inventoryItem == null) return;
/*        if (describeInstance == null) describeInstance = Instantiate(describePrefab, Get);
        else describeInstance.SetActive(true);

        describeInstance.gameObject.SetActive(true);*/
       // describeInstance.GetComponent<RectTransform>().localPosition = gameObject.GetComponent<RectTransform>().localPosition+(new Vector3(1,-1,0)*15);
        
        TextMeshProUGUI textDesc = GameInstance.inventory.GetDescriptionTab().GetComponentInChildren<TextMeshProUGUI>();

       // print(describeInstance.transform.position.x + " " + describeInstance.transform.position.y);


        if (textDesc != null)
        {
            ItemScriptableContainer itemToDesc = GameInstance.inventory.GetHeroItemScriptableByIndex(inventoryItem.container);

            if(itemToDesc.itemLevel >= GameInstance.party.activeHero.GetSkillsStat(SkillsStat.Identify)/3)
            {
                textDesc.color = Color.red;
                textDesc.text = "\n."+  " Can't identify   "+ "\n.";

            }
            else
            {
                textDesc.color = Color.green;
                string spellTexts = "";
                foreach (Spell s in itemToDesc.spellContainer.spells)
                {
                    spellTexts += ". " + s.SpellDescription;
                }
                textDesc.text = itemToDesc.itemDescription + spellTexts + ". " + "Price: " + ((int)(itemToDesc.price)).ToString();
                GameInstance.SaveIdentifiedItems(inventoryItem);
            }
            if (GameInstance.CheckIFItemIdentified(inventoryItem.container))
            {
                textDesc.color = Color.green;
                string spellTexts = "";
                foreach (Spell s in itemToDesc.spellContainer.spells)
                {
                    spellTexts += ". " + s.SpellDescription;
                }
                textDesc.text = itemToDesc.itemDescription + spellTexts + ". " + "Price: " + ((int)(itemToDesc.price)).ToString();

            }

        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!GameInstance.spellbook.IdentifyModeActive()) { return; }
        TextMeshProUGUI textDesc = GameInstance.inventory.GetDescriptionTab().GetComponentInChildren<TextMeshProUGUI>();
        if (textDesc != null)
        {
            textDesc.text = string.Empty;
        }
    }
}
