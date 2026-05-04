using Ami.BroAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChestLocked : MonoBehaviour, IPointerClickHandler, IChestLocked
{
    System.Guid _guid;
    [SerializeField] string GUIDString = "";
    [SerializeField] bool stackamountOverride = false;
    [SerializeField] List<ItemScriptableContainer> thingsInsideChest = new List<ItemScriptableContainer>();
    [SerializeField] List<int> stackAmounts = new List<int>();
    List<HeroInventoryItem> inventoryInsideChest = new List<HeroInventoryItem>();
    [SerializeField] bool _isOpen = true;
    [SerializeField] List<Sprite> openAnimation = new List<Sprite>();
    [SerializeField] List<Sprite> openMimic= new List<Sprite>();
    [SerializeField] SpriteRenderer chestPicture;
    [SerializeField] KeyType keyType;
    [SerializeField] SoundID openSound;
    [SerializeField] List<EnemySized> enemyList = new List<EnemySized>();
    [SerializeField] bool isMimic = false;
    [SerializeField] Collider chestCollider;
    [SerializeField] GameObject mimicPlace;
    [SerializeField] Sprite emptySprite;
    [SerializeField] Sprite mapSpriteOpened;
    [SerializeField] SpriteRenderer mapSpriteRenderer;

    [SerializeField] List<Sprite> closedSprites, openSprites;
    Billboard3D billboard;
    bool battle = false;
    public void OnValidate()
    {
        if (GUIDString == "")
        {
            _guid = System.Guid.NewGuid();
            GUIDString = _guid.ToString();
        }
        if (!stackamountOverride)
        {
            stackAmounts.Clear();
            for (int i = 0; i < thingsInsideChest.Count; i++)
            {

                stackAmounts.Add(1);
            }
        }
        billboard = GetComponent<Billboard3D>();
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
        if (thingsInsideChest.Count != stackAmounts.Count)
        {
            return;
        }

        for (int i = 0; i < thingsInsideChest.Count; i++)
        {
            HeroInventoryItem newItem = new HeroInventoryItem();
            newItem.container = GameInstance.dataBase.GetItemIndexFromDataBase(thingsInsideChest[i]);
            newItem.heroIndex = -1;
            newItem.itemType = thingsInsideChest[i].itemType;
            newItem.stackAmount = stackAmounts[i];
            newItem.positionReplaced = Vector3.zero;
            newItem.level = SceneManager.GetActiveScene().name ;
            newItem.levelOfIdenifySaved = 0;
            inventoryInsideChest.Add(newItem);

        }
        if (GameInstance.savedItemsState.ContainsKey(GUIDString))
        {
            //print("Check chest");
            if (inventoryInsideChest.Count == 0) return;
            if (GameInstance.savedItemsState[GUIDString] == SavedState.Opened)
            {
               // print("Chest was opened");
                if(!isMimic)
                {
                    StartCoroutine(AnimateOpen());
                    inventoryInsideChest.Clear();
                }
                else
                {
                    StartCoroutine(AnimateOpenMimic());
                    inventoryInsideChest.Clear();
                }

            }
        }
    }


    void MimicBattle()
    {
        battle = true;
        chestPicture.enabled = false;
        chestCollider.enabled = false;
        GameInstance.playerController.StartCustomBattle(chestPicture.gameObject.transform);
        GameInstance.battleManager.CustomBattleInPlace(enemyList[0], mimicPlace.transform, this.gameObject);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if(Vector3.Distance(GameInstance.playerController.gameObject.transform.position, transform.position) > 7) return;
        if (battle) return;
        if (inventoryInsideChest.Count == 0) return;
        if(isMimic)
        {
            MimicBattle();
            return;
        }
        if (Vector3.Distance(GameInstance.playerController.gameObject.transform.position, transform.position) > 7) return;
        OpenChest();
    }

    public void OpenChest()
    {
        if (_isOpen)
        {
            print("Chest is open ");
            foreach (HeroInventoryItem item in inventoryInsideChest)
            {
                GameInstance.inventory.AddToInventoryItems(item, item.stackAmount);
                GameInstance.spellbook.BattleLogMessage(new List<string>() { "item added " + GameInstance.dataBase.GetItemFromBaseByIndex(item.container).itemName }, null);
            }
            StartCoroutine(AnimateOpen());
            inventoryInsideChest.Clear();
            GameInstance.SaveItemState(GUIDString, SavedState.Opened);
            BroAudio.Play(openSound, transform);
        }
        else
        {

            HeroInventoryItem key = GameInstance.playerController.GetItemFromCursor();
            if (key == null) return;


            if (GameInstance.dataBase.GetItemFromBaseByIndex(key.container).keyType == keyType)
            {
                foreach (HeroInventoryItem item in inventoryInsideChest)
                {
                    GameInstance.inventory.AddToInventoryItems(item, item.stackAmount);
                    GameInstance.spellbook.BattleLogMessage(new List<string>() { "item added " + GameInstance.dataBase.GetItemFromBaseByIndex(item.container).itemName }, null);
                }
                StartCoroutine(AnimateOpen());
                inventoryInsideChest.Clear();
                GameInstance.SaveItemState(GUIDString, SavedState.Opened);
                BroAudio.Play(openSound, transform);
            }
            
            else
            {
                GameInstance.spellbook.BattleLogMessage(new List<string>() { " you need " + keyType + " key" }, null);
                GameInstance.playerController.SetPlayerCursorBusy(key);
            }

            BroAudio.Play(openSound, transform);
        }
        if(_isOpen)         
        {
            if(!isMimic)mapSpriteRenderer.sprite = mapSpriteOpened;
        }
    }


    public void OpenMimic()
    {
        battle = false;
        chestPicture.enabled = true;
        foreach (HeroInventoryItem item in inventoryInsideChest)
            {
                GameInstance.inventory.AddToInventoryItems(item, item.stackAmount);
                GameInstance.spellbook.BattleLogMessage(new List<string>() { "item added " + GameInstance.dataBase.GetItemFromBaseByIndex(item.container).itemName }, null);
            }
            StartCoroutine(AnimateOpenMimic());
            inventoryInsideChest.Clear();
            GameInstance.SaveItemState(GUIDString, SavedState.Opened);
        mapSpriteRenderer.sprite = openMimic[openMimic.Count-1];

    }


    IEnumerator AnimateOpen()
    {
        billboard.ReplaceSprite(openSprites);
        billboard.AnimationPlaying(true);
        foreach (Sprite s in openAnimation)
        {
            chestPicture.sprite = s;
            yield return new WaitForSeconds(0.2f);
        }
        billboard.AnimationPlaying(false);
        yield return null;
    }   
    IEnumerator AnimateOpenMimic()
    {
        foreach(Sprite s in openMimic)
        {
            chestPicture.sprite = s;
            yield return new WaitForSeconds(0.2f);
        }

        yield return null;
    }

    public bool IsOpen()
    {
        return _isOpen;
    }

    public KeyType GetKeyType()
    {
        return keyType;
    }
}


public interface IChestLocked
{
    public KeyType GetKeyType();
    public bool IsOpen();
    public void OpenChest();
}