using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SingleSwitch : MonoBehaviour, IInteractables, IPointerClickHandler
{
    [SerializeField] GameObject doorTarget;
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] Sprite openSprite, closeSprite;

    public void ToggleSwitch()
    {
        IDoor idoor = doorTarget.GetComponent<IDoor>();
        if (!idoor.isOpen())
        {
            idoor.OpenDoor();
            renderer.sprite = openSprite;

        }
        else
        {
            idoor.CloseDoor();
            renderer.sprite = closeSprite;
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
