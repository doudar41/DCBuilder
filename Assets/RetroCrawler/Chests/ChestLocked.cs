using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChestLocked : MonoBehaviour, IPointerClickHandler, IChestLocked
{
    System.Guid _guid;
    [SerializeField] string GUIDString = "";

    [SerializeField] List<ItemScriptableContainer> thingsInsideChest = new List<ItemScriptableContainer>();
    [SerializeField] List<int> stackAmounts = new List<int>();
    List<HeroInventoryItem> inventoryInsideChest = new List<HeroInventoryItem>();
    [SerializeField] bool isOpen = true;
    [SerializeField] List<Sprite> openAnimation = new List<Sprite>();
    [SerializeField] SpriteRenderer chestPicture;
    [SerializeField] KeyType keyType;

    public void OnValidate()
    {
        if (GUIDString == "")
        {
            _guid = System.Guid.NewGuid();
            GUIDString = _guid.ToString();
        }
    }
    private void Awake()
    {
        GameInstance.initItems += Init;
    }
    private void OnDestroy()
    {
        GameInstance.initItems -= Init;
    }


    void Init()
    {
        if (GameInstance.savedItemsState.ContainsKey(GUIDString))
        {
            if (inventoryInsideChest.Count == 0) return;
            if (GameInstance.savedItemsState[GUIDString] == SavedState.Opened)
            {
                StartCoroutine(AnimateOpen());
                inventoryInsideChest.Clear();
            }
        }
    }


    private void Start()
    {
        if(thingsInsideChest.Count!= stackAmounts.Count)
        {
            print("Things and amounts list counts should be equal");
            return;
        }

        for (int i=0;i<thingsInsideChest.Count;i++) 
        {
            HeroInventoryItem newItem = new HeroInventoryItem();
            newItem.container = GameInstance.dataBase.GetItemIndexFromDataBase(thingsInsideChest[i]);
            newItem.heroIndex = -1;
            newItem.itemType = thingsInsideChest[i].itemType;
            newItem.stackAmount = stackAmounts[i];
            newItem.positionReplaced = Vector3.zero;
            newItem.level = "Level01";
            newItem.levelOfIdenifySaved = 0;
            inventoryInsideChest.Add(newItem);
           
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryInsideChest.Count == 0) return;
        if (Vector3.Distance(GameInstance.playerController.gameObject.transform.position, transform.position) > 7) return;
        OpenChest();
    }

    public void OpenChest()
    {
        if (isOpen)
        {
            foreach (HeroInventoryItem item in inventoryInsideChest)
            {
                GameInstance.inventory.FindEmptySlotAndPutItem(item, item.stackAmount);
                GameInstance.spellbook.BattleLogMessage(new List<string>() { "item added " + GameInstance.dataBase.GetItemFromBaseByIndex(item.container).itemName }, null);
            }
            StartCoroutine(AnimateOpen());
            inventoryInsideChest.Clear();
            GameInstance.SaveItemState(GUIDString, SavedState.Opened, null);
        }
        else
        {
            if (GameInstance.inventory.UseKey(keyType))
            {
                foreach (HeroInventoryItem item in inventoryInsideChest)
                {
                    GameInstance.inventory.FindEmptySlotAndPutItem(item, item.stackAmount);
                    GameInstance.spellbook.BattleLogMessage(new List<string>() { "item added " + GameInstance.dataBase.GetItemFromBaseByIndex(item.container).itemName }, null);
                }
                StartCoroutine(AnimateOpen());
                inventoryInsideChest.Clear();
                GameInstance.SaveItemState(GUIDString, SavedState.Opened, null);
            }
        }
    }

    IEnumerator AnimateOpen()
    {
        foreach(Sprite s in openAnimation)
        {
            chestPicture.sprite = s;
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }
    public bool IsLocked()
    {
        return !isOpen;
    }

    public KeyType GetKeyType()
    {
        return keyType;
    }
}


public interface IChestLocked
{
    public KeyType GetKeyType();
    public bool IsLocked();
    public void OpenChest();
}