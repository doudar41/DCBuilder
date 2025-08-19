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

    ItemScriptableContainer ItemScriptable;
    [SerializeField] Image itemAvatar;
    [SerializeField]
    Sprite emptySlotSprite;
    [SerializeField]
    TextMeshProUGUI amountText;

    private void Awake()
    {
        //itemAvatar = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsEmpty())
        {
            ItemSlotStruct slotStruct = GameInstance.playerController.GetItemFromCursor();
            ItemScriptable = slotStruct.item;

            if (ItemScriptable != null)
            {
                if (slotStruct.stackAmount > 1)
                { 
                    stackAmount = slotStruct.stackAmount; 
                }
                else stackAmount = 1;
                itemAvatar.sprite = ItemScriptable.InventorySprite;
                amountText.text = stackAmount.ToString();
            }
        }
        else
        {
            if (stackAmount >= 1 && GameInstance.playerController.IsCursorBusy())
            {
                ItemSlotStruct slotStruct =  GameInstance.playerController.GetItemFromCursor();
                if (slotStruct.item == ItemScriptable) 
                {
                    stackAmount += slotStruct.stackAmount;
                }
                else
                {
                    GameInstance.playerController.SetPlayerCursorBusy(ItemScriptable, stackAmount);
                    if (slotStruct.stackAmount > 1)
                    {
                        stackAmount = slotStruct.stackAmount;
                    }
                    else stackAmount = 1;
                    ItemScriptable = slotStruct.item;
                    itemAvatar.sprite = ItemScriptable.InventorySprite;
                    amountText.text = stackAmount.ToString();
                    //exchange items in a slot
                }
                amountText.text = stackAmount.ToString();
                return;
            }
            if (stackAmount >= 1 && !GameInstance.playerController.IsCursorBusy())
            {
               // print("one item left");
            GameInstance.playerController.SetPlayerCursorBusy(ItemScriptable, stackAmount);
            stackAmount = 0;
            ItemScriptable = null;
            itemAvatar.sprite = emptySlotSprite;
            amountText.text = stackAmount.ToString();
                //GameInstance.inventory.RemoveItemFromInventory(slotIndex);
            }
        }
    }


    public bool AddItemInSlot(ItemScriptableContainer itemTemp, int amount)
    {
        if (itemTemp != null)
        {
            if (itemTemp== ItemScriptable)
            {
                stackAmount += amount;
                return true;
            }

            if (IsEmpty())
            {
                ItemScriptable = itemTemp;
                stackAmount = amount;
                itemAvatar.sprite = ItemScriptable.InventorySprite;
                amountText.text = stackAmount.ToString();
                return true;
            }
        }
        return false;
    }

    public bool IsEmpty()
    {
        return ItemScriptable==null;
    }
}
