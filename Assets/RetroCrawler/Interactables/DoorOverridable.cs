using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent (typeof(BoxCollider))]
public class DoorOverridable : MonoBehaviour, IDoor, IInteractables
{
    [SerializeField] GameObject door;
    [SerializeField]
    bool isOpened = false;
    bool stop = false;
    [SerializeField]
    float blockHeight = 1;
    [SerializeField]
    AnimationCurve curveDoor;
    float clampYMin, clampYMax;

    private void Start()
    {
        clampYMin = door.transform.position.y;
        clampYMax = blockHeight + clampYMin;
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

    }
    
    public List<InteractablesEnum> WhatIsIt()
    {
        List<InteractablesEnum> interactablesEnums = new List<InteractablesEnum>();
        interactablesEnums.Add(InteractablesEnum.DOOR);
        return interactablesEnums;
    }

    IEnumerator OpenDoorSmoothly(float endPoint)
    {
        float starty = transform.position.y;
        float startPoint = Mathf.Clamp((starty / clampYMax), 0, 1);
        if(endPoint> startPoint)
        {
            print(endPoint);
            if (endPoint>0.98f) isOpened = true; else isOpened = false;
            float currentPoint = startPoint;
            while (currentPoint < endPoint)
            {
                door.transform.position = new Vector3(transform.position.x,
                Mathf.Clamp(clampYMin + blockHeight * curveDoor.Evaluate(currentPoint),clampYMin,clampYMax), transform.position.z);
                yield return new WaitForSeconds(0.001f);
                currentPoint += 0.01f;
            }
        }
        else
        {
            isOpened = false;
            float currentPoint = startPoint;
            while (currentPoint > endPoint)
            {
                door.transform.position = new Vector3(transform.position.x,
                Mathf.Clamp(clampYMin + (blockHeight * curveDoor.Evaluate(currentPoint)), clampYMin, clampYMax), transform.position.z);
                yield return new WaitForSeconds(0.001f);
                currentPoint -= 0.01f;
            }
        }
        yield return null;
    }

    public void WeightDoor(int weightTarget, int weightAmount)
    {
        float deltaWeight = Mathf.Clamp(((float)weightAmount / (float)weightTarget), 0, 1);
        StartCoroutine(OpenDoorSmoothly(deltaWeight));
    }

    public void CloseDoor()
    {

    }
}



