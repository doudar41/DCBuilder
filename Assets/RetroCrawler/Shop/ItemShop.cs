using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ItemShop : MonoBehaviour
{
    [SerializeField] Image backGroundImage;
    [SerializeField] List<ItemShopSlot> itemsSlots = new List<ItemShopSlot>();
    [SerializeField] List<ItemType> itemsTypesToSell = new List<ItemType>();
    [SerializeField] Camera cam;
    [SerializeField] float sellMultiplier = 1;
    [SerializeField] List<TextMeshProUGUI> heroesCoinsText;
    [SerializeField] TextMeshProUGUI textOfShopState;
    Dictionary<int, HeroInventoryItem> itemsToSell = new Dictionary<int, HeroInventoryItem>();
    List<int> itemsToSellKeys = new List<int>();
    int sellItemIndexStart = 0;
    ShopState shopState = ShopState.Sell;


    public UnityEvent closeShopPanel;

    private void OnEnable()
    {
        //NewItems();
        cam.depth = 1;
        GetPlayersCoins();
        sellItemIndexStart = 0;
    }

    private void Start()
    {
        //NewItems();
    }

    public void NewItemsToSell()
    {
        ClearSlots();
        for (int i = 0; i < itemsSlots.Count; i++)
        {
            itemsSlots[i].SetItemToSell(RandomItemsToSell(itemsTypesToSell[Random.Range(0, itemsTypesToSell.Count)]));
            itemsSlots[i].sellMultiplier = sellMultiplier;
            itemsSlots[i].shopState = shopState;
        }
    }


    public void ClearSlots()
    {
        for (int i = 0; i < itemsSlots.Count; i++)
        {

            itemsSlots[i].ClearSlot() ;
        }
    }


    public void GetItemsFromInventoryToBuy()
    {
        ClearSlots();
        itemsToSell.Clear();
        itemsToSellKeys.Clear();
        if (GameInstance.inventory.GetItemsFromInventory().Count == 0) return;
        foreach (KeyValuePair<int, HeroInventoryItem> h in GameInstance.inventory.GetItemsFromInventory())
        {
            if (h.Value == null) continue;
            foreach(ItemType it in itemsTypesToSell)
            {
                print("item "+ GameInstance.dataBase.GetItemFromBaseByIndex(h.Value.container).itemName);
                if (h.Value.itemType == it)
                {
                    itemsToSell.Add(h.Key,h.Value);
                    itemsToSellKeys.Add(h.Key);
                }
            }
        }

       
        for (int i = 0; i < itemsSlots.Count; i++)
        {
            if(itemsToSellKeys.Contains(i + sellItemIndexStart))
            {
                itemsSlots[i].SetItemToSell(GameInstance.dataBase.GetItemFromBaseByIndex(itemsToSell[itemsToSellKeys[i+sellItemIndexStart]].container));
                itemsSlots[i].shopState = shopState;
                itemsSlots[i].inventorySlotForSell = itemsToSellKeys[i + sellItemIndexStart];
            }

        }
    }


    public void GetNextItemToSellPage(int minusPlusOne)
    {
        sellItemIndexStart = Mathf.Clamp(sellItemIndexStart + (9 * minusPlusOne), 0, itemsToSell.Count);
        GetItemsFromInventoryToBuy();
    }


    public void CameraOut()
    {
        cam.depth = -2;
    }

    public ItemScriptableContainer RandomItemsToSell(ItemType itemType)
    {
        List<ItemScriptableContainer> itemsOfType = new List<ItemScriptableContainer>();

        foreach (ItemScriptableContainer item in GameInstance.dataBase.GetWholeItemDatabase())
        {
            if (item.itemType == itemType)
            {
                itemsOfType.Add(item);
            }
        }
        List<ItemScriptableContainer> randomItems = new List<ItemScriptableContainer>();


        return itemsOfType[Random.Range(0, itemsOfType.Count)];
    }

    public void CloseShop()
    {
        closeShopPanel.Invoke();
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        gameObject.SetActive(false);

    }

    public void GetPlayersCoins()
    {
        var money = GameInstance.party.GetCoinsForUI();
        for (int i=0;i< money.Count;i++)
        {
            heroesCoinsText[i].text = money[i].ToString();
        }
    }

    public void SwitchToSell()
    {
        shopState = ShopState.Sell;
        NewItemsToSell();
        textOfShopState.text = "Buy";
    }

    public void SwitchToBuy()
    {
        shopState = ShopState.Buy;
        GetItemsFromInventoryToBuy();
        textOfShopState.text = "Sell";
    }


}


public enum ShopState
{
    Buy,
    Sell,
    Idenify,
    Spell,
    Heal,
    Ressurect
}