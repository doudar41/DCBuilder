
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class equipmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{


    public ItemType itemType;

    ItemScriptableContainer ItemScriptable;
    [SerializeField]
    Image itemAvatar;
    [SerializeField]
    Sprite emptySlotSprite;

    public UnityEvent<ItemType,ItemScriptableContainer> sendItemToParty;


    private void Start()
    {
        sendItemToParty.AddListener(GameInstance.party.GetItemFromEquipmentSlot);
    }

    public void SetEquipmentSlot(ItemScriptableContainer item)
    {
        if(item != null)
        {
            ItemScriptable = item;
            itemAvatar.sprite = ItemScriptable.InventorySprite;
            //sendItemToParty.Invoke(itemType,item);
        }
        else
        {
            itemAvatar.sprite = emptySlotSprite;
            ItemScriptable = null;
            //sendItemToParty.Invoke(itemType,null);
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {

        if (IsEmpty())
        {  
            ItemSlotStruct slotStruct = GameInstance.playerController.GetItemFromCursor();

            if (slotStruct.item != null)
            {
                if (slotStruct.item.itemType == itemType)
                {
                    ItemScriptable = slotStruct.item;
                    itemAvatar.sprite = ItemScriptable.InventorySprite;
                    sendItemToParty.Invoke(itemType, ItemScriptable);
                    if(itemType == ItemType.WEAPON)
                    {
                        if (ItemScriptable.twoHanded) 
                        {
                            //Shield diabled
                        }
                    }
                    //Two handed weapons and range weapons should 
                }
                else
                {

                    if (slotStruct.stackAmount == 1) GameInstance.playerController.SetPlayerCursorBusy(slotStruct.item, 1);
                }

                if (slotStruct.stackAmount > 1) GameInstance.playerController.SetPlayerCursorBusy(slotStruct.item, slotStruct.stackAmount - 1);
            }

        }
        else
        {
            ItemSlotStruct slotStruct = GameInstance.playerController.GetItemFromCursor();
            //if (slotStruct.stackAmount > 1) return; // it's not possible to exchange multiple items to one, possible to take 1 rest return to inventory
            ItemScriptableContainer itemTemp = slotStruct.item;
            if (itemTemp != null)
            {
                if (itemTemp.itemType == itemType)
                {
                    GameInstance.playerController.SetPlayerCursorBusy(ItemScriptable, 1);
                    ItemScriptable = itemTemp;
                    itemAvatar.sprite = ItemScriptable.InventorySprite;
                    sendItemToParty.Invoke(itemType, ItemScriptable);
                    if (slotStruct.stackAmount > 1) GameInstance.inventory.FindEmptySlotAndPutItem(slotStruct.item, slotStruct.stackAmount - 1);
                }
                else
                {
                    if (slotStruct.stackAmount == 1) GameInstance.playerController.SetPlayerCursorBusy(itemTemp, 1);
                    if (slotStruct.stackAmount > 1) GameInstance.playerController.SetPlayerCursorBusy(itemTemp, slotStruct.stackAmount);
                }
            }
            else
            {
                if (ItemScriptable == null) return;
                GameInstance.playerController.SetPlayerCursorBusy(ItemScriptable, 1);
                ItemScriptable = null;
                itemAvatar.sprite = emptySlotSprite;
                sendItemToParty.Invoke(itemType, null);
                
            }
        }

        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsEmpty())
        {
            //show describtion ItemScriptable
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }


    public bool IsEmpty()
    {
        return ItemScriptable == null;
    }
}

