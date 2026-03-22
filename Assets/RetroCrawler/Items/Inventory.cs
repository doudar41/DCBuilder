using Ami.BroAudio;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    [SerializeField]    List<equipmentSlot> equipmentSlotsList = new List<equipmentSlot>();
    [SerializeField]    GameObject slotsParent;
    [SerializeField]    GameObject slotPrefab;
    [SerializeField]    TextMeshProUGUI weightCapacity;
    [SerializeField]    GameObject inventorySwitcher, paperDoll, descriptionTab;
    [SerializeField]    List<TextMeshProUGUI> keyAmountsText = new List<TextMeshProUGUI>();
    [SerializeField]    SoundID openInventory = default;
    Dictionary<KeyType, TextMeshProUGUI> keyTexts = new Dictionary<KeyType, TextMeshProUGUI>();
    Dictionary<int, ItemScriptableContainer> itemDatabase = new Dictionary<int, ItemScriptableContainer>();
    List<HeroInventoryItem> inventoryItems = new List<HeroInventoryItem>();
    SortingItemType sortingType = SortingItemType.NONE;
    Dictionary<SortingItemType, List<ItemType>> sortingGroups = new Dictionary<SortingItemType, List<ItemType>>()
    { { SortingItemType.WEAPON, new List<ItemType>(){ ItemType.WEAPON } },
    { SortingItemType.ARMOUR, new List<ItemType>(){ ItemType.TORSO_ARMOR, ItemType.HELM, ItemType.GLOVES, ItemType.AMULET, ItemType.BOOT, ItemType.BELT,
     ItemType.SHIELD, ItemType.RING} },
    { SortingItemType.CONSUMABLE, new List<ItemType>(){ ItemType.CONSUMABLE, ItemType.LEARNINGSCROLL} },
    { SortingItemType.KEY, new List<ItemType>(){ ItemType.Key } },
    { SortingItemType.QUEST, new List<ItemType>(){ ItemType.QUEST, ItemType.LOOT } }};

    public UnityEvent<int> sendWeight;
    public UnityEvent enableInventory;

    bool isInventoryOpened = false;

    List<KeyToLocks> playersKeys = new List<KeyToLocks>();


    private void Awake()
    {
        GameInstance.inventory = this;
        keyTexts.Add(KeyType.IronKey, keyAmountsText[0]);
        keyTexts.Add(KeyType.BronzeKey, keyAmountsText[1]); 
        keyTexts.Add(KeyType.GoldKey, keyAmountsText[2]);
        GameInstance.GetInventoryItemDelegate += SaveInventoryItemsToGameInstance;
    }
    private void OnDestroy()
    {
        GameInstance.GetInventoryItemDelegate -= SaveInventoryItemsToGameInstance;
    }


    bool SaveInventoryItemsToGameInstance()
    {
        foreach(HeroInventoryItem hi in  inventoryItems)
        {
         GameInstance.AddInventoryItem(hi);

        }

        return true;
    }

    public void EnableInventory(bool switchInventory)
    {
        if (GameInstance.playerController.shopIsOpened)
        {
            paperDoll.SetActive(false);
            inventorySwitcher.SetActive(false);
            descriptionTab.SetActive(false);
            return;
        }
        if (switchInventory) enableInventory.Invoke();
        isInventoryOpened = switchInventory;
        inventorySwitcher.SetActive(switchInventory);
        ShowSortedInventory();
        descriptionTab.SetActive(switchInventory);
        paperDoll.SetActive(switchInventory);
        if (switchInventory)
        {
            if (openInventory != default) { BroAudio.Play(openInventory).SetVelocity(0); }

        }
        else
        {
            if (openInventory != default)
            {
                BroAudio.Play(openInventory).SetVelocity(1);
            }
            if (GameInstance.playerController.playerState == PlayerState.Battle)
            {
                GameInstance.battleManager.ResetActiveHero();
            }        
        
        }
    }

    public Transform GetDescriptionTab()
    {
        return descriptionTab.transform;
    }


    public bool IsOpen()
    {
        return isInventoryOpened;
    }

    void Start()
    {

        enableInventory.AddListener(GameInstance.party.heroEquipmentToInventory);
        inventorySwitcher.SetActive(false);
        descriptionTab.SetActive(false);
    }

    public void GetEquipmentFromHero(Dictionary<ItemType,HeroInventoryItem> equipmentList)
    {
        equipmentSlot shieldSlot = null;

        foreach (equipmentSlot e in equipmentSlotsList)
        {
            if (e.itemType == ItemType.SHIELD)
            {
                shieldSlot = e;
            }
            if (equipmentList.TryGetValue(e.itemType, out HeroInventoryItem outItem))
            {
                e.SetEquipmentSlot(outItem);
            }
            else
            {                    
                e.SetEquipmentSlot(null);
            }
        }

        shieldSlot.CheckWeaponSlot();
        GameInstance.party.RefreshUI.Invoke();
    }


    public equipmentSlot FindEquipmentSlotOfType(ItemType type)
    {
        foreach(equipmentSlot e in equipmentSlotsList)
        {
            if(e.itemType == type) return e;
        }

        return null;
    }


    public void BuildItemDatabase()
    {
        foreach(ItemScriptableContainer item in GameInstance.dataBase.GetWholeItemDatabase())
        {
            HeroInventoryItem heroInventoryItem = GameInstance.dataBase.HeroInventoryFromITemScriptable(item);
            itemDatabase.Add(heroInventoryItem.container, item);
        }
    }

    public ItemScriptableContainer GetHeroItemScriptableByIndex(int index)
    {
        return itemDatabase[index];
    }


    public float GetCurrentHeroWeight()
    {
        float weight = 0;

       Dictionary<ItemType, HeroInventoryItem> heroEq = GameInstance.party.activeHero.GetHeroEquipment();
        //print("hero equipment count "+heroEq.Count);
        int heroWeight = 0;
        foreach (KeyValuePair<ItemType, HeroInventoryItem> equi in heroEq)
        {
            if(equi.Value != null)
            {
                heroWeight += GameInstance.dataBase.GetItemFromBaseByIndex(equi.Value.container).weight;
            }

        }
        weight = (float)heroWeight / (float)GameInstance.party.activeHero.GetMaxDependedStat(DependedStat.CarryingCapacity);

        return weight;
    }

    public void AddToInventoryItems(HeroInventoryItem itemScriptableTemp, int stackamount)
    {
        if (inventoryItems.Contains(itemScriptableTemp)) { inventoryItems[inventoryItems.IndexOf(itemScriptableTemp)].stackAmount += 1; return; }
        else itemScriptableTemp.stackAmount = stackamount;
        inventoryItems.Add(itemScriptableTemp);
    }


    public void RemoveFromInventory(HeroInventoryItem itemToRemove, int amount)
    {
        if(itemToRemove.stackAmount > amount)
        {
            itemToRemove.stackAmount -= amount;
        }
        else
        {
            inventoryItems.Remove(itemToRemove);
        }

    }
    public void ShowSortedInventory()
    {
        ItemSlot[] slots = slotsParent.GetComponentsInChildren<ItemSlot>();
        foreach (ItemSlot i in slots)
        {
            i.RemoveItemFromSlotOnly();
        }
        foreach (HeroInventoryItem inventoryItem in inventoryItems) 
        { 
            if(sortingType == SortingItemType.NONE)
            {
                FindEmptySlotAndPutItem(inventoryItem, inventoryItem.stackAmount, true);
            }
            else
            {
                if (sortingGroups[sortingType].Contains(inventoryItem.itemType))
                {

                    FindEmptySlotAndPutItem(inventoryItem, inventoryItem.stackAmount, true);
                }

            }

        }
    }

    public void SetSortingType(int index)
    {
        sortingType = (SortingItemType)index;
        //print((SortingItemType)index);
        ShowSortedInventory();
    }

    public void FindEmptySlotAndPutItem(HeroInventoryItem itemScriptableTemp, int stackamount, bool noSound = true)
    {
        bool itemAdded = false;
        int countSlots = 0;
        ItemSlot[] slots = slotsParent.GetComponentsInChildren<ItemSlot>();


        foreach (ItemSlot i in slots)
        {
            if (i.IsEmpty())
            {
                if (i.AddItemInSlot(itemScriptableTemp, stackamount))
                {
                    if (!noSound) { //print("pllay item sound");

                        GameInstance.soundManagerInGame.ProtectedPlay(itemDatabase[itemScriptableTemp.container].inventorySound); 
                    }
                    itemAdded = true;

                    if( slots.Count() - countSlots == 5)
                    {
                        for(int j = 0; j < 5; j++)
                        {
                            GameObject _slot = Instantiate(slotPrefab, slotsParent.transform);
                            _slot.GetComponent<ItemSlot>().Init();
                        }
                    }
                    break;
                }
            }
            countSlots++;
        }
        if(itemAdded == false)
        {
            GameObject _slot = Instantiate(slotPrefab, slotsParent.transform);
            _slot.GetComponent<ItemSlot>().Init();
            if (_slot.GetComponent<ItemSlot>().AddItemInSlot(itemScriptableTemp, stackamount))
            {
                if (!noSound)
                { //print("pllay item sound");

                    GameInstance.soundManagerInGame.ProtectedPlay(itemDatabase[itemScriptableTemp.container].inventorySound);
                }
            }
        }

    }

    public Dictionary<int, HeroInventoryItem> GetItemsFromInventory()
    {
        Dictionary<int, HeroInventoryItem> items = new Dictionary<int, HeroInventoryItem>();
        ItemSlot[] slots = slotsParent.GetComponentsInChildren<ItemSlot>();
        for(int i=0;i< slots.Length;i++)
        {
            if (slots[i].GetItemFromSlot() != null) { items.Add(i, slots[i].GetItemFromSlot()); }
        }
        print("items count in inventory " + items.Count);
        return items;
    }


    public void SaveInvetoryItemsToGameInstance() {         
        
        GameInstance.inventoryItemsSaved.Clear();
        ItemSlot[] slots = slotsParent.GetComponentsInChildren<ItemSlot>();
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetItemFromSlot() != null)
            {
                GameInstance.inventoryItemsSaved.Add(slots[i].GetItemFromSlot());
            }
        }
    }
    


    public void RemoveItemFromInventory(int slotIndex)
    {
        ItemSlot[] slots = slotsParent.GetComponentsInChildren<ItemSlot>();
        slots[slotIndex].RemoveItem();
    }

    public void SaveKeyToList(KeyType keyType)
    {
        KeyToLocks keyToLocks = new KeyToLocks();
        keyToLocks.keyType = keyType;
        keyToLocks.amount = 1;
        playersKeys.Add(keyToLocks);
        if(keyTexts.ContainsKey(keyType)) keyTexts[keyType].text = keyToLocks.amount.ToString();
    }

    public bool UseKey(KeyType keyType)
    {
        foreach (KeyToLocks key in playersKeys)
        {
            if (key.keyType == keyType)
            {
                if (key.amount > 0)
                {
                    key.amount--;
                    keyTexts[keyType].text = key.amount.ToString();
                    return true;
                }
            }
        }
        return false;
    }


    public void SaveKeysToGameInstance()
    {
        GameInstance.keysSaved = playersKeys;
    }

    public void LoadKeys()
    {
        playersKeys = GameInstance.keysSaved;
        foreach(KeyToLocks key in playersKeys)
        {
          keyTexts[key.keyType].text = key.amount.ToString();          
        }
    }
}


public struct ItemSlotStruct
{
    public ItemScriptableContainer item;
    public int stackAmount;
    public string _GUID;
    //special effect
}

public enum KeyType
{
    BronzeKey,
    IronKey,
    GoldKey
}