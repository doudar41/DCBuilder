using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemShop : MonoBehaviour
{
    [SerializeField] Image backGroundImage;
    [SerializeField] List<ItemShopSlot> itemsSlots = new List<ItemShopSlot>();
    [SerializeField] List<ItemType> itemsTypesToSell = new List<ItemType>();
    [SerializeField] Camera cam;

    private void OnEnable()
    {
        //NewItems();
        cam.depth = 1;
    }

    private void Start()
    {
        //NewItems();
    }

    public void NewItems()
    {
        for (int i = 0; i < itemsSlots.Count; i++)
        {

            itemsSlots[i].SetItemToSell(RandomItemsToSell(itemsTypesToSell[Random.Range(0, itemsTypesToSell.Count)]));

        }
    }

    public void CameraOut()
    {
        cam.depth = -2;
    }

    public void GetItemFromSlot(ItemScriptableContainer item)
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
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        gameObject.SetActive(false);

    }


}
