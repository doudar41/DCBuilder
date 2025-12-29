using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Inventory : MonoBehaviour
{
    [SerializeField]    List<equipmentSlot> equipmentSlotsList = new List<equipmentSlot>();
    [SerializeField]    GameObject slotsParent;
    [SerializeField]    GameObject slotPrefab;
    [SerializeField]    TextMeshProUGUI weightCapacity;
    [SerializeField]    GameObject inventorySwitcher;
    [SerializeField] List<TextMeshProUGUI> keyAmountsText = new List<TextMeshProUGUI>();
    Dictionary<KeyType, TextMeshProUGUI> keyTexts = new Dictionary<KeyType, TextMeshProUGUI>();

    public UnityEvent<int> sendWeight;
    public UnityEvent enableInventory;

    bool isInventoryOpened = false;

    List<KeyToLocks> playersKeys = new List<KeyToLocks>();


    private void Awake()
    {
        GameInstance.inventory = this;
        keyTexts.Add(KeyType.IronKey, keyAmountsText[0]);
        keyTexts.Add(KeyType.RedKey, keyAmountsText[1]); 
        keyTexts.Add(KeyType.GoldKey, keyAmountsText[2]);
    }
 
    public void EnableInventory(bool switchInventory)
    {
        if (switchInventory) enableInventory.Invoke();
        isInventoryOpened = switchInventory;
        inventorySwitcher.SetActive(switchInventory);
    }

    public bool IsOpen()
    {
        return isInventoryOpened;
    }

    void Start()
    {

        enableInventory.AddListener(GameInstance.party.heroEquipmentToInventory);
        inventorySwitcher.SetActive(false);
    }

    public void GetEquipmentFromHero(Dictionary<ItemType,HeroInventoryItem> equipmentList)
    {
        if (true)
        {
            foreach (equipmentSlot e in equipmentSlotsList)
            {
                if (equipmentList.TryGetValue(e.itemType, out HeroInventoryItem outItem))
                {
                    e.SetEquipmentSlot(outItem);
                }
                else
                {
                    e.SetEquipmentSlot(null);
                }
            }
        }

        GameInstance.party.UpdatePartyWeight();
        GameInstance.party.RefreshUI.Invoke();
    }


    public void CheckWeaponForTwoHands()
    {

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
        weight = (float)heroWeight / (float)GameInstance.party.activeHero.GetDependedStat(DependedStat.CarryingCapacity);

        return weight;
    }



    public void FindEmptySlotAndPutItem(HeroInventoryItem itemScriptableTemp, int stackamount)
    {
        ItemSlot[] slots = slotsParent.GetComponentsInChildren<ItemSlot>();
        foreach(ItemSlot i in slots)
        {
            if (i.IsEmpty())
            {
                if (i.AddItemInSlot(itemScriptableTemp, stackamount))
                {
                    break;
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
    RedKey,
    IronKey,
    GoldKey
}