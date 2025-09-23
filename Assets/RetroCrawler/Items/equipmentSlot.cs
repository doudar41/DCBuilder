
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class equipmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemType itemType;

    HeroInventoryItem ItemScriptable;
    [SerializeField] Image itemAvatar;
    [SerializeField] Sprite emptySlotSprite;

    public UnityEvent<HeroInventoryItem, ItemType> sendItemToParty;

    private void Start()
    {
        sendItemToParty.AddListener(GameInstance.party.GetItemFromEquipmentSlot);
    }

    public void SetEquipmentSlot(HeroInventoryItem item)
    {
        if(item != null)
        {
            ItemScriptable = item;
            itemAvatar.sprite = GameInstance.dataBase.GetItemFromBaseByIndex( ItemScriptable.container).InventorySprite;
            sendItemToParty.Invoke(item, itemType);
        }
        else
        {
            itemAvatar.sprite = emptySlotSprite;
            ItemScriptable = null;
            sendItemToParty.Invoke(null, itemType);
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsEmpty())
        {  
            HeroInventoryItem slotStruct = GameInstance.playerController.GetItemFromCursor();

            if (slotStruct != null)
            {
                if (GameInstance.dataBase.GetItemFromBaseByIndex(slotStruct.container).itemType == itemType)
                {
                    slotStruct.heroIndex = GameInstance.party.activeHero.GetHeroIndex();
                    ItemScriptable = slotStruct;
                    itemAvatar.sprite = GameInstance.dataBase.GetItemFromBaseByIndex(ItemScriptable.container).InventorySprite;
                    sendItemToParty.Invoke(ItemScriptable, itemType);
                    if(itemType == ItemType.WEAPON)
                    {
                        if (GameInstance.dataBase.GetItemFromBaseByIndex(ItemScriptable.container).twoHanded) 
                        {
                            //Shield diabled
                        }
                    }
                    //Two handed weapons and range weapons should 
                }
                else
                {
                    if (slotStruct.stackAmount == 1) 
                    {
                        slotStruct.stackAmount = 1;
                        GameInstance.playerController.SetPlayerCursorBusy(slotStruct); 
                    }
                }

                if (slotStruct.stackAmount > 1) 
                { 
                    slotStruct.stackAmount = slotStruct.stackAmount - 1;
                    GameInstance.playerController.SetPlayerCursorBusy(slotStruct);
                    GameInstance.inventory.FindEmptySlotAndPutItem(slotStruct, slotStruct.stackAmount - 1);
                }
            }

        }
        else
        {
            HeroInventoryItem slotStruct = GameInstance.playerController.GetItemFromCursor();
            //if (slotStruct.stackAmount > 1) return; // it's not possible to exchange multiple items to one, possible to take 1 rest return to inventory
            HeroInventoryItem itemTemp = slotStruct;
            print("item is full "+ slotStruct);
            if (itemTemp != null)
            {
                if (itemTemp.itemType == itemType)
                {
                    if(ItemScriptable == itemTemp)
                    {

                        GameInstance.playerController.SetPlayerCursorBusy(slotStruct);

                    }
                    else
                    {
                        GameInstance.playerController.SetPlayerCursorBusy(ItemScriptable);
                        sendItemToParty.Invoke(ItemScriptable, itemType);
                        itemTemp.stackAmount = 1;
                        ItemScriptable = itemTemp;
                        itemAvatar.sprite = GameInstance.dataBase.GetItemFromBaseByIndex(ItemScriptable.container).InventorySprite;

                    }

                    if (slotStruct.stackAmount > 1) GameInstance.inventory.FindEmptySlotAndPutItem(slotStruct, slotStruct.stackAmount - 1);
                }
                else
                {
                    if (slotStruct.stackAmount == 1)
                    {
                        GameInstance.playerController.SetPlayerCursorBusy(slotStruct);
                    }
                    if (slotStruct.stackAmount > 1)
                    {
                        GameInstance.playerController.SetPlayerCursorBusy(slotStruct);
                    }
                }
            }
            else
            {
                if (ItemScriptable == null) return;
                GameInstance.playerController.SetPlayerCursorBusy(ItemScriptable);
                ItemScriptable = null;
                itemAvatar.sprite = emptySlotSprite;
                sendItemToParty.Invoke(null, itemType);
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

