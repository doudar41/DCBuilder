using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleLock : MonoBehaviour, IInteractables, IPointerClickHandler
{
    System.Guid _guid;

    [SerializeField] string GUIDString;
    [SerializeField] GameObject doorTarget;
    [SerializeField] SpriteRenderer renderer;
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
                renderer.sprite = openSprite;

            }
            if (GameInstance.savedItemsState[GUIDString] == SavedState.Closed)
            {
                idoor.CloseDoor();
                renderer.sprite = closeSprite;
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
            renderer.sprite = openSprite;
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
        //print(Vector3.Distance(GameInstance.playerController.gameObject.transform.position, transform.position));
        if (Vector3.Distance(GameInstance.playerController.gameObject.transform.position, transform.position) > 5) return;
        if (doorTarget.GetComponent<IDoor>() == null) return;
        IDoor idoor = doorTarget.GetComponent<IDoor>();
        if (!idoor.isOpen())
        {
        if (GameInstance.inventory.UseKey(_keyType)) OpenLock(_keyType);
        }

    }


    IEnumerator AnimateOpen()
    {
        foreach (Sprite s in openAnimation)
        {
            renderer.sprite = s;
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }
}


