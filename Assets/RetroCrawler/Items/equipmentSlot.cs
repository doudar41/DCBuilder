
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class equipmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{


    public ItemType itemType;

    HeroInventoryItem ItemScriptable;
    string _GUID;
    [SerializeField]
    Image itemAvatar;
    [SerializeField]
    Sprite emptySlotSprite;

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
            itemAvatar.sprite = ItemScriptable.container.InventorySprite;
            _GUID = item._GUID;
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
                
                print(" guid from cursor " + slotStruct._GUID);
                if (slotStruct.container.itemType == itemType)
                {
                    slotStruct.heroIndex = GameInstance.party.activeHero.GetHeroIndex();
                    ItemScriptable = slotStruct;
                    itemAvatar.sprite = ItemScriptable.container.InventorySprite;
                    _GUID =  slotStruct._GUID;
                    sendItemToParty.Invoke(ItemScriptable, itemType);
                    if(itemType == ItemType.WEAPON)
                    {
                        if (ItemScriptable.container.twoHanded) 
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
                    slotStruct.stackAmount = 1;
                    GameInstance.playerController.SetPlayerCursorBusy(slotStruct);
                    ItemScriptable = itemTemp;
                    itemAvatar.sprite = ItemScriptable.container.InventorySprite;
                    sendItemToParty.Invoke(ItemScriptable, itemType);
                    if (slotStruct.stackAmount > 1) GameInstance.inventory.FindEmptySlotAndPutItem(slotStruct, slotStruct.stackAmount - 1);
                }
                else
                {
                    if (slotStruct.stackAmount == 1) 
                    {

                        GameInstance.playerController.SetPlayerCursorBusy(slotStruct); 
                    }
                    if (slotStruct.stackAmount > 1) 
                    { GameInstance.playerController.SetPlayerCursorBusy(slotStruct); 
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

    public void SetGUID(string _guid)
    {
        _GUID = _guid;
    }


}

