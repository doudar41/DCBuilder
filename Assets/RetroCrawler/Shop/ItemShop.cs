using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Ami.BroAudio;

public class ItemShop : MonoBehaviour
{
    [SerializeField] Image backGroundImage;
    [SerializeField] List<ItemShopSlot> itemsSlots = new List<ItemShopSlot>();
    [SerializeField] List<ItemType> itemsTypesToSell = new List<ItemType>();
    [SerializeField] CameraOrder cam;
    [SerializeField] float sellMultiplier = 1;
    [SerializeField] Vector2Int itemsLevel = new Vector2Int(0,1);
    [SerializeField] List<TextMeshProUGUI> heroesCoinsText;
    [SerializeField] TextMeshProUGUI textOfShopState;
    [SerializeField] GameObject[] arrowsItems;
    [SerializeField] GameObject shopInsides;
    Dictionary<int, HeroInventoryItem> itemsToSell = new Dictionary<int, HeroInventoryItem>();
    List<int> itemsToSellKeys = new List<int>();
    int sellItemIndexStart = 0;
    ShopState shopState = ShopState.SellToPlayer;
    [SerializeField]  SoundID closeShopSound, voicePhrase, openShopPhrase, ambience = default, moneyGoesSound;
    int coinsSpent = 0;
    bool isNight = false;
    List<ItemScriptableContainer> itemsForSale = new List<ItemScriptableContainer>();

    public UnityEvent closeShopPanel;

    private void OnEnable()
    {

    }

    private void Awake()
    {

    }

    private void Start()
    {
        arrowsItems[0].SetActive(false);
        arrowsItems[1].SetActive(false);

        for (int i = 0; i < itemsSlots.Count; i++)
        {
            itemsSlots[i].ItemSold.AddListener(ItemSold);
            itemsSlots[i].itemBought.AddListener(ItemBought);
        }
    }

    public void OpenShop()
    {
        backGroundImage.enabled = true;
        shopInsides.SetActive(true);
        //cam.depth = 1;
        var money = GameInstance.party.GetCoinsForUI();
        for (int i = 0; i < money.Count; i++)
        {
            heroesCoinsText[i].text = money[i].ToString();
        }
        sellItemIndexStart = 0;
        ReadItemsToSEll();
        textOfShopState.text = "Buy";
    }

    public void PlayerCoins(int coins)
    {
        coinsSpent = coins;
        BroAudio.Play(ambience);
    }

    public void NewItemsToSell()
    {
        ClearSlots();
        itemsForSale.Clear();
        for (int i = 0; i < itemsSlots.Count; i++)
        {
            ItemScriptableContainer itemToSell = RandomItemsToSell(itemsTypesToSell[Random.Range(0, itemsTypesToSell.Count)]);
            if(itemToSell != null)
            {
                itemsForSale.Add(itemToSell);
            }
        }
        ReadItemsToSEll();
        arrowsItems[0].SetActive(false);
        arrowsItems[1].SetActive(false);
    }

    public void ReadItemsToSEll()
    {
        ClearSlots();
        if (itemsForSale.Count == 0)
        {
            return;
        }

        //itemsForSale.Clear();

        for (int i = 0; i < itemsSlots.Count; i++)
        {
            if (i < itemsForSale.Count)
            {
                print("reading items to sell " + itemsForSale[i].itemName);
                itemsSlots[i].SetItemToSell(itemsForSale[i], i);
                itemsSlots[i].sellMultiplier = sellMultiplier;
                itemsSlots[i].shopState = shopState;
            }

        }
    }


    public void ItemSold(ItemScriptableContainer item, int index)
    {

        itemsForSale.Remove(item);
        print("item sold" + itemsForSale.Count);
        ReadItemsToSEll();
    }

    public void ItemBought(ItemScriptableContainer item)
    {
        if(itemsToSell.Count<itemsSlots.Count)
        itemsForSale.Add(item);
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
        sellItemIndexStart = 0;
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
            if (i<itemsToSellKeys.Count)
            {
                 print("index " +i+ " modifier "+ sellItemIndexStart + " all items' keys " +itemsToSellKeys.Count); 

                itemsSlots[i].SetItemToSell(GameInstance.dataBase.GetItemFromBaseByIndex(itemsToSell[itemsToSellKeys[i+sellItemIndexStart]].container),i);
                itemsSlots[i].shopState = shopState;
                itemsSlots[i].inventorySlotForSell = itemsToSellKeys[i + sellItemIndexStart];
            }

        }
        if(itemsToSell.Count> itemsSlots.Count)
        {
            arrowsItems[0].SetActive(true);
            arrowsItems[1].SetActive(true);
        }

    }


    public void GetNextItemToSellPage(int minusPlusOne)
    {
        sellItemIndexStart = Mathf.Clamp(sellItemIndexStart + (9 * minusPlusOne), 0, itemsToSell.Count);
        GetItemsFromInventoryToBuy();
    }


    public void CameraOut()
    {
        cam.BattleLogWithGameplay();
    }

    public ItemScriptableContainer RandomItemsToSell(ItemType itemType)
    {
        List<ItemScriptableContainer> itemsOfType = new List<ItemScriptableContainer>();

        foreach (ItemScriptableContainer item in GameInstance.dataBase.GetWholeItemDatabase())
        {
            if (item.itemLevel >= itemsLevel.x && item.itemLevel <= itemsLevel.y)
            {
                if (item.itemType == itemType)
                {
                    itemsOfType.Add(item);
                }
            }

        }
        if(itemsOfType.Count == 0) { print("no items"); return null; }
        //print("item for random choose "+itemsOfType.Count);
        return itemsOfType[Random.Range(0, itemsOfType.Count)];
    }

    public void CloseShop()
    {
        closeShopPanel.Invoke();
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        gameObject.SetActive(false);
        BroAudio.Play(closeShopSound);
        if (GameInstance.party.SellBuyMoneyCheck(coinsSpent) != 0)
        {
            BroAudio.Play(voicePhrase).SetVelocity(Random.Range(3,6));
        }
        else
        {
            BroAudio.Play(voicePhrase).SetVelocity(Random.Range(0, 3));
        }
        GetComponent<animateUIImage>().StopAnimation();
        BroAudio.Stop(openShopPhrase);
        BroAudio.Stop(ambience,0.3f);
        backGroundImage.enabled = false;
        shopInsides.SetActive(false);
    }

    void GetPlayersCoins()
    {

        var money = GameInstance.party.GetCoinsForUI();
        for (int i=0;i< money.Count;i++)
        {
            heroesCoinsText[i].text = money[i].ToString();
        }
        print("ask for money");

        if(!BroAudio.HasAnyPlayingInstances(moneyGoesSound)) BroAudio.Play(moneyGoesSound);
    }

    public void SwitchToSellToPlayer()
    {
        shopState = ShopState.SellToPlayer;
        ReadItemsToSEll();
        textOfShopState.text = "Buy";
        arrowsItems[0].SetActive(false);
        arrowsItems[1].SetActive(false);
    }

    public void SwitchToBuyFromPlayer()
    {
        shopState = ShopState.BuyFromPlayer;
        GetItemsFromInventoryToBuy();
        textOfShopState.text = "Sell";
    }


}


public enum ShopState
{
    BuyFromPlayer,
    SellToPlayer,
    Idenify,
    Spell,
    Heal,
    Ressurect
}