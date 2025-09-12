using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemShopSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image itemPicture;
    [SerializeField] Sprite emptySprite;
    ItemScriptableContainer itemToSell;
    SpellContainer spellToSell;


    private void Awake()
    {
        itemPicture.sprite = emptySprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemToSell == null && spellToSell ==null) return;
        if(itemToSell != null)
        {
            GetItemFromSlot(itemToSell);
            itemToSell = null;
            itemPicture.sprite = emptySprite;
        }
        if (spellToSell != null)
        {
            if (!GameInstance.party.activeHero.GetActiveHeroSpellbook().Contains(spellToSell))
            {
                GameInstance.party.activeHero.GetActiveHeroSpellbook().Add(spellToSell);
                spellToSell = null;
                itemPicture.sprite = emptySprite;
            }
            else
            {
                print("hero already have this spell");
            }
        }
    }

    void GetItemFromSlot(ItemScriptableContainer item)
    {
        //get
        HeroInventoryItem heroInventoryItem = new HeroInventoryItem();
        heroInventoryItem.container = GameInstance.dataBase.GetItemIndexFromDataBase(item);
        heroInventoryItem.heroIndex = -1;
        heroInventoryItem.itemType = item.itemType;

        heroInventoryItem.stackAmount = 1;
        heroInventoryItem.positionReplaced = Vector3.zero;
        heroInventoryItem.level = "Level01";
        GameInstance.inventory.FindEmptySlotAndPutItem(heroInventoryItem, 1);
    }


    public void SetItemToSell(ItemScriptableContainer item)
    {
        itemToSell = item;
        itemPicture.sprite = itemToSell.InventorySprite;
    }
    public void SetSpellToSell(SpellContainer spell)
    {
        spellToSell = spell;
        itemPicture.sprite = spellToSell.spellIcon;
    }
}
