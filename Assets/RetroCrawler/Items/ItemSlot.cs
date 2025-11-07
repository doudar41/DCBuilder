using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;


public class ItemSlot : MonoBehaviour, IPointerClickHandler
{

    int stackAmount = 1;
    HeroInventoryItem inventoryItem;
    [SerializeField] Image itemAvatar;
    [SerializeField]
    Sprite emptySlotSprite;
    [SerializeField]
    TextMeshProUGUI amountText;
    
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
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (inventoryItem == null) { print("no more clicking"); return; }
            if (inventoryItem.itemType == ItemType.CONSUMABLE)
            {
                if (stackAmount > 1)
                {
                    stackAmount--;
                    GameInstance.party.activeHero.ApplySpellToHero(GameInstance.dataBase.GetItemFromBaseByIndex( inventoryItem.container).spellContainer, null);
                    amountText.text = stackAmount.ToString();
                }
                else
                {
                    GameInstance.party.activeHero.ApplySpellToHero(GameInstance.dataBase.GetItemFromBaseByIndex(inventoryItem.container).spellContainer, null);
                    stackAmount = 0;
                    inventoryItem = null;
                    itemAvatar.sprite = emptySlotSprite;
                    amountText.text = stackAmount.ToString();
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
                }
            }
            else
            {
                if (stackAmount >= 1 && GameInstance.playerController.IsCursorBusy())
                {
                    HeroInventoryItem slotStruct =  GameInstance.playerController.GetItemFromCursor();
                    if (GameInstance.dataBase.GetItemFromBaseByIndex(slotStruct.container) == GameInstance.dataBase.GetItemFromBaseByIndex(inventoryItem.container)) 
                    {
                        stackAmount += slotStruct.stackAmount;
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
}
