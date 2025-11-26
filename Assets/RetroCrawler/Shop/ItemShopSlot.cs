using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemShopSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image itemPicture;
    [SerializeField] Sprite emptySprite;
    [SerializeField] GameObject descriptionPrefab;
    ItemScriptableContainer itemToSell;
    SpellContainer spellToSell;
    public float sellMultiplier = 1;
    GameObject desc;
    public ShopState shopState = ShopState.SellToPlayer;
    public int inventorySlotForSell = -1;


    public UnityEvent refreshCoins;
    private void Awake()
    {
        itemPicture.sprite = emptySprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemToSell == null && spellToSell == null) return;

        switch (shopState)
        {
            case ShopState.BuyFromPlayer:
                if (itemToSell != null)
                {
                    SellItemFromSlot(itemToSell);
                    GameInstance.party.MoneyGoes(-itemToSell.price);
                    refreshCoins.Invoke();
                    itemToSell = null;
                    itemPicture.sprite = emptySprite;
                    GameInstance.inventory.RemoveItemFromInventory(inventorySlotForSell);
                    desc.SetActive(false);
                }

                break;
            case ShopState.SellToPlayer:
                if (itemToSell != null)
                {
                    print("seller price " + GameInstance.party.SellBuyMoneyCheck((int)(itemToSell.price * sellMultiplier)));
                    if (GameInstance.party.SellBuyMoneyCheck((int)(itemToSell.price * sellMultiplier)) >= 0)
                    {
                        GetItemFromSlot(itemToSell);
                        GameInstance.party.MoneyGoes(itemToSell.price);
                        refreshCoins.Invoke();
                        itemToSell = null;
                        itemPicture.sprite = emptySprite;
                        desc.SetActive(false);
                    }
                }
                if (spellToSell != null)
                {
                    if (GameInstance.party.SellBuyMoneyCheck((int)(spellToSell.spellPrice * sellMultiplier)) >= 0)
                    {

                        if (!GameInstance.party.activeHero.GetActiveHeroSpellbook().Contains(spellToSell))
                        {

                            GameInstance.party.activeHero.GetActiveHeroSpellbook().Add(spellToSell);
                            GameInstance.party.MoneyGoes(spellToSell.spellPrice);
                            spellToSell = null;
                            itemPicture.sprite = emptySprite;

                        }
                    }
                    else
                    {
                        print("hero already have this spell");
                    }
                    desc.SetActive(false);
                }
                break;
            case ShopState.Idenify:
                break;
            case ShopState.Spell:
                if (spellToSell != null)
                {
                    if (GameInstance.party.SellBuyMoneyCheck((int)(spellToSell.spellPrice * sellMultiplier)) >= 0)
                    {

                        if (!GameInstance.party.activeHero.GetActiveHeroSpellbook().Contains(spellToSell))
                        {

                            GameInstance.party.activeHero.GetActiveHeroSpellbook().Add(spellToSell);
                            GameInstance.party.MoneyGoes(spellToSell.spellPrice);
                            spellToSell = null;
                            itemPicture.sprite = emptySprite;

                        }
                    }
                    else
                    {
                        print("hero already have this spell");
                    }
                    desc.SetActive(false);
                }
                 break;
            case ShopState.Heal:
                break;
            case ShopState.Ressurect:
                break;
        }

        refreshCoins.Invoke();
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

    void SellItemFromSlot(ItemScriptableContainer item)
    {
        //get
        HeroInventoryItem heroInventoryItem = new HeroInventoryItem();
        heroInventoryItem.container = GameInstance.dataBase.GetItemIndexFromDataBase(item);
        heroInventoryItem.heroIndex = -1;
        heroInventoryItem.itemType = item.itemType;

        heroInventoryItem.stackAmount = 1;
        heroInventoryItem.positionReplaced = Vector3.zero;
        heroInventoryItem.level = "Level01";

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemToSell == null && spellToSell == null) return;
        if (itemToSell != null)
        {
            if (desc == null)
            {
                desc = Instantiate(descriptionPrefab, transform);
                //desc.transform.SetParent(null);
            }
            else desc.SetActive(true);

            TextMeshProUGUI textObject = desc.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            if (textObject != null)
            {
                string spellTexts = "";
                foreach (Spell s in itemToSell.spellContainer.spells)
                {
                    spellTexts += "\n" + s.SpellDescription;
                }
                switch (shopState)
                {
                    case ShopState.BuyFromPlayer:
                        textObject.text = itemToSell.itemDescription + spellTexts + "\n" + "Price: " + ((int)(itemToSell.price * sellMultiplier)).ToString();
                        textObject.color = Color.green;
                        break;
                    case ShopState.SellToPlayer:
                        textObject.text = itemToSell.itemDescription + spellTexts + "\n" + "Price: " + ((int)(itemToSell.price * sellMultiplier)).ToString();
                        if (GameInstance.party.SellBuyMoneyCheck((int)(itemToSell.price * sellMultiplier)) >= 0) textObject.color = Color.green;
                        else textObject.color = Color.red;
                        break;
                    case ShopState.Idenify:
                        break;
                    case ShopState.Spell:
                        break;
                    case ShopState.Heal:
                        break;
                    case ShopState.Ressurect:
                        break;
                }

            }
        }
        if (spellToSell != null)
        {
            if (desc == null)
            {
                desc = Instantiate(descriptionPrefab, transform);
            }
            else desc.SetActive(true);

            TextMeshProUGUI textObject = desc.gameObject.GetComponentInChildren<TextMeshProUGUI>();

            string spellTexts = "";
            foreach (Spell s in spellToSell.spells)
            {
                spellTexts += "\n" + s.SpellDescription;
            }
            textObject.text = spellToSell.spellName + "\n" + spellTexts + "\n" + "Price: " + ((int)(spellToSell.spellPrice * sellMultiplier)).ToString();
            if (GameInstance.party.SellBuyMoneyCheck((int)(spellToSell.spellPrice * sellMultiplier)) >= 0) textObject.color = Color.green;
            else textObject.color = Color.red;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (itemToSell == null && spellToSell == null) return;
        if (desc == null) return;
        desc.SetActive(false);
        //Hide UI;
    }



    public void LevelUpShop()
    {

    }




    public void ClearSlot()
    {

        itemToSell = null;
        itemPicture.sprite = emptySprite;
    }


}
