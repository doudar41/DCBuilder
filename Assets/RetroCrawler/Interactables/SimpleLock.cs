using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleLock : MonoBehaviour, IInteractables, IPointerClickHandler
{
    System.Guid _guid;

    [SerializeField] string GUIDString;
    [SerializeField] GameObject doorTarget;
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] Sprite openSprite, closeSprite;
    [SerializeField] KeyType _keyType;
    [SerializeField] List<Sprite> openAnimation = new List<Sprite>();
    private void OnValidate()
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
    private void Start()
    {

    }

    void Init()
    {
        if (GameInstance.savedItemsState.ContainsKey(GUIDString))
        {
            if (doorTarget.GetComponent<IDoor>() == null) return;
            IDoor idoor = doorTarget.GetComponent<IDoor>();
            if (GameInstance.savedItemsState[GUIDString] == SavedState.Opened)
            {
                idoor.OpenDoor();
                _renderer.sprite = openSprite;

            }
            if (GameInstance.savedItemsState[GUIDString] == SavedState.Closed)
            {
                idoor.CloseDoor();
                _renderer.sprite = closeSprite;
            }
        }
    }

    public void OpenLock(KeyType keyType)
    {
        if (doorTarget.GetComponent<IDoor>() == null) return;
        IDoor idoor = doorTarget.GetComponent<IDoor>();


        if (keyType == _keyType)
        {
            idoor.OpenDoor();
            _renderer.sprite = openSprite;
            GameInstance.SaveItemState(GUIDString, SavedState.Opened, null);
            StartCoroutine( AnimateOpen());
        }


    }

    public List<InteractablesEnum> WhatIsIt()
    {
        List<InteractablesEnum> interactablesEnums = new List<InteractablesEnum>();
        interactablesEnums.Add(InteractablesEnum.SWITCH);
        return interactablesEnums;
    }

    public int GetWeight(out int capacity)
    {
        capacity = 0;
        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print("click click");
        if (Vector3.Distance(GameInstance.playerController.gameObject.transform.position, transform.position) > 5) return;
        if (doorTarget.GetComponent<IDoor>() == null) return;
        IDoor idoor = doorTarget.GetComponent<IDoor>();
        if (!idoor.isOpen())
        {
            //if (GameInstance.inventory.UseKey(_keyType)) OpenLock(_keyType);
           HeroInventoryItem key =  GameInstance.playerController.GetItemFromCursor();
            if (key == null) return;

            if(key.itemType == ItemType.Key)
            {
                if(GameInstance.dataBase.GetItemFromBaseByIndex(key.container).keyType == _keyType)
                {
                    idoor.OpenDoor();
                    StartCoroutine(AnimateOpen());
                }
            }
            else
            {
                GameInstance.spellbook.BattleLogMessage(new List<string>() { " you need " + _keyType + " key"}, null);
                GameInstance.playerController.SetPlayerCursorBusy(key);
            }
            
            
        }

    }


    IEnumerator AnimateOpen()
    {   

        for ( int i = 0;i<openAnimation.Count*1;i++)
        {
            _renderer.sprite = openAnimation[i% openAnimation.Count];
            yield return new WaitForSeconds(0.1f);

        }
 

        yield return null;
    }

    public string GetGUID()
    {
        return GUIDString;
    }
}


