using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SingleSwitch : MonoBehaviour, IInteractables, IPointerClickHandler
{
    System.Guid _guid;

    [SerializeField] string GUIDString;
    [SerializeField] GameObject doorTarget;
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] Sprite openSprite, closeSprite;

    private void OnValidate()
    {
        if (GUIDString == "")
        {
            _guid = System.Guid.NewGuid();
            GUIDString = _guid.ToString();
        }
    }

    private void Start()
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

    public void ToggleSwitch()
    {
        if (doorTarget.GetComponent<IDoor>() == null) return;
        IDoor idoor = doorTarget.GetComponent<IDoor>();
        if (!idoor.isOpen())
        {
            idoor.OpenDoor();
            renderer.sprite = openSprite;
            GameInstance.SaveItemState(GUIDString, SavedState.Opened);
        }
        else
        {
            idoor.CloseDoor();
            renderer.sprite = closeSprite;
            GameInstance.SaveItemState(GUIDString, SavedState.Closed);
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
        ToggleSwitch();
    }
}


public interface ISwitch
{
    public void ToggleSwitch();
    public void HighlightSwitch();

}
