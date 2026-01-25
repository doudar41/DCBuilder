

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class equipmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemType itemType;

    HeroInventoryItem itemScriptable = new HeroInventoryItem();
    [SerializeField] Image itemAvatar;
    [SerializeField] Sprite emptySlotSprite;
    [SerializeField] equipmentSlot shieldSlot, weaponSlot;

    public UnityEvent<HeroInventoryItem, ItemType> sendItemToParty;

    private void Start()
    {
        sendItemToParty.AddListener(GameInstance.party.GetItemFromEquipmentSlot);
    }

    public void SetEquipmentSlot(HeroInventoryItem item)
    {
        if(item != null)
        {

                
            itemScriptable = item;
            itemAvatar.sprite = GameInstance.dataBase.GetItemFromBaseByIndex( itemScriptable.container).InventorySprite;
            sendItemToParty.Invoke(item, itemType);
        }
        else
        {
            itemAvatar.sprite = emptySlotSprite;
            itemScriptable = null;
            sendItemToParty.Invoke(null, itemType);
        }
    }

    public void CheckWeaponSlot()
    {
        if (itemScriptable != null) return;
        if (weaponSlot == null) return;
        if(weaponSlot.itemScriptable == null) return;
        if (GameInstance.dataBase.GetItemFromBaseByIndex(weaponSlot.itemScriptable.container).twoHanded) 
        {
            PlacePlaceholderOfItem(weaponSlot.itemScriptable);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HeroInventoryItem itemFromCursor = GameInstance.playerController.GetItemFromCursor();

        if (itemFromCursor == null)
        {
            
            if (!IsEmpty())
            {
                if (GameInstance.dataBase.GetItemFromBaseByIndex(itemScriptable.container).twoHanded) shieldSlot.ClearSlot();
                GiveBackItemToCursor(itemScriptable, true);
                return;
            }
            else 
            {
                return;
            }
        }

        //print("clicked equipment slot" + itemFromCursor.itemType);

        if (itemFromCursor.itemType != itemType) { GiveBackItemToCursor(itemFromCursor, false); return; }
        //print("correct item type for slot");

        if (CheckForTwohandedWeapon(itemFromCursor, IsEmpty())) {  return; }

        if (IsEmpty()) 
        {
            
            FillSlotWithItem(itemFromCursor); return;
            // check if cursor has item of the same type as equipment slot
        }
        if(!IsEmpty())
        {


            GiveBackItemToCursor(itemScriptable, true);
            FillSlotWithItem(itemFromCursor);

        }
    }


    bool CheckForTwohandedWeapon(HeroInventoryItem itemFromCursor, bool emptySlot)
    {
        if (itemFromCursor.itemType == ItemType.WEAPON)
        {
            if (GameInstance.dataBase.GetItemFromBaseByIndex(itemFromCursor.container).twoHanded)
            {

                if (emptySlot && shieldSlot.IsEmpty())
                {
                    FillSlotWithItem(itemFromCursor);
                    shieldSlot.PlacePlaceholderOfItem(itemFromCursor);
                }

                if (emptySlot && !shieldSlot.IsEmpty())
                {
                    GiveBackItemToCursor(itemFromCursor, false);
                }


                if (!emptySlot && shieldSlot.IsEmpty())
                {
                    GiveBackItemToCursor(itemScriptable, false);
                    FillSlotWithItem(itemFromCursor);
                    shieldSlot.PlacePlaceholderOfItem(itemFromCursor);
                }

                if (!emptySlot && !shieldSlot.IsEmpty())
                {

                    GiveBackItemToCursor(itemFromCursor, false);
                }

                return true;
            }
        }


        if (itemFromCursor.itemType == ItemType.SHIELD)
        {
            if(weaponSlot.IsEmpty()) return false;
            if (GameInstance.dataBase.GetItemFromBaseByIndex(weaponSlot.itemScriptable.container).twoHanded)
            {
                GiveBackItemToCursor(itemFromCursor,false); return true;
            }

        }

        return false;
    }

    void FillSlotWithItem(HeroInventoryItem itemToFill)
    {
        //if(itemScriptable !=null) GiveBackItemToCursor(itemScriptable);
        itemScriptable = itemToFill;
        itemAvatar.sprite = GameInstance.dataBase.GetItemFromBaseByIndex(itemScriptable.container).InventorySprite;
        sendItemToParty.Invoke(itemScriptable, itemType);
    }

    void GiveBackItemToCursor(HeroInventoryItem itemToGiveBack, bool nullifyItem)
    {

        GameInstance.playerController.SetPlayerCursorBusy(itemToGiveBack);
        if (nullifyItem)
        {
            if (GameInstance.dataBase.GetItemFromBaseByIndex(itemScriptable.container).twoHanded)
            {
                shieldSlot.ClearSlot();
            }

            itemScriptable = null;
            itemAvatar.sprite = emptySlotSprite;
            sendItemToParty.Invoke(null, itemType);
        }
    }
    
    public void PlacePlaceholderOfItem(HeroInventoryItem itemToPlace)
    {
        itemAvatar.sprite = GameInstance.dataBase.GetItemFromBaseByIndex(itemToPlace.container).InventorySprite;
    }


    public void ClearSlot()
    {
        //GameInstance.playerController.SetPlayerCursorBusy(itemScriptable);
        itemScriptable = null;
        itemAvatar.sprite = emptySlotSprite;
        //sendItemToParty.Invoke(null, itemType);
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
        return itemScriptable == null;
    }



}

