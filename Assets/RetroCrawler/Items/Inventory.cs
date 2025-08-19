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

    public void GetEquipmentFromHero(Dictionary<ItemType,ItemScriptableContainer> equipmentList)
    {
        //print("roll through equipment "+ equipmentList.Count);
        if (true)
        {
            foreach (equipmentSlot e in equipmentSlotsList)
            {

                if (equipmentList.TryGetValue(e.itemType, out ItemScriptableContainer outItem))
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

    public void FindEmptySlotAndPutItem(ItemScriptableContainer itemScriptableTemp, int stackamount)
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

}


public struct ItemSlotStruct
{
    public ItemScriptableContainer item;
    public int stackAmount;
    //special effect
}