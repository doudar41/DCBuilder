using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(BoxCollider))]
public class Door : MonoBehaviour, IDoor, IInteractables
{
    [SerializeField]
    bool isOpened = false;
    bool busy = false;
    [SerializeField]
    float blockHeight = 1;
    [SerializeField]
    AnimationCurve curveDoor;

    float clampYMin, clampYMax;

    private void Start()
    {
        clampYMin = transform.position.y;
        clampYMax = blockHeight + clampYMin;
    }


    public void CloseDoor()
    {
        if (!isOpened) return;
        if (busy) return;
        StartCoroutine(OpenDoorSmoothly(1));
               
    }

    public int GetWeight(out int capacity)
    {
        capacity = 0;
        return 0;
    }

    public bool isOpen()
    {
        return isOpened;
    }

    public void OpenDoor()
    {
        if (isOpened) return;
        if (busy) return;
        StartCoroutine(OpenDoorSmoothly(0));

    }
    

    public List<InteractablesEnum> WhatIsIt()
    {
        List<InteractablesEnum> interactablesEnums = new List<InteractablesEnum>();
        interactablesEnums.Add(InteractablesEnum.DOOR);
        return interactablesEnums;
    }

    IEnumerator OpenDoorSmoothly(int startPoint)
    {

        busy = true;
        float starty = transform.position.y;
        float currentPoint = 0;
        while (currentPoint < 1)
        {
            if (startPoint <= 0)
            {
                transform.position = new Vector3(transform.position.x, 
                    Mathf.Clamp(starty + blockHeight* curveDoor.Evaluate( currentPoint),clampYMin,clampYMax), transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, 
                    Mathf.Clamp(starty - blockHeight * curveDoor.Evaluate(currentPoint), clampYMin, clampYMax), transform.position.z);
                //print("start corouting" + currentPoint);
            }

            yield return new WaitForSeconds(0.01f);
            
            currentPoint += 0.01f;
        }
        //print("cancel");
        busy = false;
        if (startPoint <= 0)
        {
            isOpened = true;
        }
        else
        {
            isOpened = false;
        }
    }

    public void WeightDoor(int weightTarget, int weightAmount)
    {
        
    }
}



public interface IDoor
{
    public bool isOpen();
    public void OpenDoor();
    public void CloseDoor();
    public void WeightDoor(int weightTarget, int weightAmount);
}