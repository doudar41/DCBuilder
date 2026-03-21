using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;



[RequireComponent(typeof(BoxCollider))]
public class SingleSwitch : MonoBehaviour, IInteractables, IPointerClickHandler
{
    System.Guid _guid;

    [SerializeField] string GUIDString;
    [SerializeField] GameObject doorTarget;
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] Sprite openSprite, closeSprite;
    [SerializeField] int lockIndex;
    [SerializeField] bool complexLock = false;
    [SerializeField] List<Sprite> openSprites, closeSprites;
    [SerializeField]Billboard3D billboard3D;
    bool isOn = false;


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
        billboard3D = GetComponent<Billboard3D>();
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
        if (complexLock) return;
        if (GameInstance.savedItemsState.ContainsKey(GUIDString))
        {

            if (doorTarget.GetComponent<IDoor>() == null) return;
            IDoor idoor = doorTarget.GetComponent<IDoor>();
            if (GameInstance.savedItemsState[GUIDString] == SavedState.Opened)
            {
                idoor.OpenDoor();
                idoor.OpenDoor(lockIndex, this.gameObject);
               if(openSprites.Count ==0) _renderer.sprite = openSprite;
                else billboard3D.ReplaceSprite(openSprites);

            }
            if (GameInstance.savedItemsState[GUIDString] == SavedState.Closed)
            {
                idoor.CloseDoor();
                idoor.OpenDoor(lockIndex, this.gameObject);
                if (openSprites.Count == 0) _renderer.sprite = closeSprite;
                else billboard3D.ReplaceSprite(closeSprites);
            }
        }
    }

    public void ToggleSwitch()
    {
        if (doorTarget.GetComponent<IDoor>() == null) return;
        IDoor idoor = doorTarget.GetComponent<IDoor>();
        if (!complexLock)
        {
            if (!idoor.isOpen())
            {
                idoor.OpenDoor();
                if (openSprites.Count == 0) _renderer.sprite = openSprite;
                else billboard3D.ReplaceSprite(openSprites);
                GameInstance.SaveItemState(GUIDString, SavedState.Opened, null);
            }
            else
            {
                idoor.CloseDoor();
                if (closeSprites.Count == 0) _renderer.sprite = closeSprite;
                else billboard3D.ReplaceSprite(closeSprites);
                GameInstance.SaveItemState(GUIDString, SavedState.Closed, null);
            }
        }
        else
        {
            if (!isOn)
            {
                idoor.OpenDoor(lockIndex, this.gameObject );
                if (openSprites.Count == 0) _renderer.sprite = openSprite;
                else billboard3D.ReplaceSprite(openSprites);
                isOn = true;
            }
            else
            {
                idoor.CloseDoor(lockIndex, this.gameObject);
                if (closeSprites.Count == 0) _renderer.sprite = closeSprite;
                else billboard3D.ReplaceSprite(closeSprites);
                isOn = false;
            }
        }

    }

    public void ResetSwitch()
    {
        print("autoreset");
        StartCoroutine(DelaySwitch());
    }


    IEnumerator DelaySwitch() {         
        yield return new WaitForSeconds(0.1f);
        if (closeSprites.Count == 0) _renderer.sprite = closeSprite;
        else billboard3D.ReplaceSprite(closeSprites);
        isOn = false;
    }
    /*    private void Update()
        {
            if(complexLock && !isOn)
            {
                _renderer.sprite = closeSprite;
            }
        }*/

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
        ToggleSwitch();
    }

    public string GetGUID()
    {
       return GUIDString;
    }
}


public interface ISwitch
{
    public void ToggleSwitch();
    public void HighlightSwitch();

}
