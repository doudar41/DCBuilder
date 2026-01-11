
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
    public float sellMultiplier = 1;
    GameObject desc;
    public ShopState shopState = ShopState.SellToPlayer;
    public int inventorySlotForSell = -1;
    public UnityEvent refreshCoins;

    public UnityEvent<ItemScriptableContainer, int> ItemSold;
    public UnityEvent<ItemScriptableContainer>  itemBought;

    private void Awake()
    {
        //itemPicture.sprite = emptySprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemToSell == null) return;

        switch (shopState)
        {
            case ShopState.BuyFromPlayer:
                if (itemToSell != null)
                {
                    SellItemFromSlot(itemToSell);
                    GameInstance.party.MoneyGoes(-itemToSell.price);
                    refreshCoins.Invoke();
                    itemBought.Invoke(itemToSell);
                    itemToSell = null;
                    itemPicture.sprite = emptySprite;
                    GameInstance.inventory.RemoveItemFromInventory(inventorySlotForSell);
                    if(desc !=null) desc.SetActive(false);
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
                        ItemSold.Invoke(itemToSell, inventorySlotForSell);

                    }
                }

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


    public ItemScriptableContainer GetItemInSlot()
    {
        return itemToSell;
    }

    public void SetItemToSell(ItemScriptableContainer item, int index)
    {
        itemToSell = item;
        itemPicture.sprite = itemToSell.InventorySprite;
        inventorySlotForSell = index;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemToSell == null) return;

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

    public void OnPointerExit(PointerEventData eventData)
    {
        if (itemToSell == null) return;
        if (desc == null) return;
        desc.SetActive(false);
        //Hide UI;
    }



    public void ClearSlot()
    {

        itemToSell = null;
        itemPicture.sprite = emptySprite;
        if(desc !=null)desc.SetActive(false);
    }


}
