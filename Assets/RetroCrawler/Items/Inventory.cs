using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Inventory : MonoBehaviour
{
    PlayerController playerController;
    Dictionary<int, ItemSlotStruct> itemsInInventory = new Dictionary<int, ItemSlotStruct>();
    Dictionary<int, ItemSlotStruct> itemsEquipped = new Dictionary<int, ItemSlotStruct>();

    [SerializeField]
    List<equipmentSlot> equipmentSlotsList = new List<equipmentSlot>();
    [SerializeField]
    GameObject slotsParent;
    [SerializeField]
    TextMeshProUGUI weightCapacity;
    [SerializeField] GameObject inventorySwitcher;
    public UnityEvent<int> sendWeight;
    public UnityEvent enableInventory;

    bool isInventoryOpened = false;
    int enablecount = 0;

    private void Awake()
    {
        GameInstance.inventory = this;
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
        playerController = GameInstance.playerController;
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
        GameInstance.party.RefreshUI.Invoke();
        UpdatePartyWeight();
    }


    public void CheckWeaponForTwoHands()
    {

    }

    public void UpdatePartyWeight()
    {
        //int weightCarried = GameInstance.party.GetWeight(out int capacity);
       // weightCapacity.text = capacity.ToString() + "/" + weightCarried.ToString();
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


    public void RemoveItemFromInventory(int slotIndex)
    {
        ItemSlot[] slots = slotsParent.GetComponentsInChildren<ItemSlot>();
        slots[slotIndex].RemoveItem();
    }

}


public struct ItemSlotStruct
{
    public ItemScriptableContainer item;
    public int stackAmount;
    public string _GUID;
    //special effect
}