using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDoor : MonoBehaviour, IDoor, IInteractables
{
    [SerializeField] GameObject rightDoorPart, leftDoorPart;
    [SerializeField] BoxCollider col;
    [SerializeField]
    float blockLenght = 5;
    public AnimationCurve curveDoorR, curveDoorL;
    bool busy = false;
    float clampXMinR, clampXMaxR, clampXMaxL, clampXMinL;
    [SerializeField]
    bool isOpened = false;

    void Start()
    {
        clampXMinR = transform.position.x;
        clampXMaxR = blockLenght + clampXMinR;
        clampXMaxL = transform.position.x;
        clampXMinL = blockLenght - clampXMinR;
        if (isOpened)
        {
            OpenDoor();
        }

    }  
    
    
    
    public void CloseDoor()
    {
        StartCoroutine(OpenDoorSmoothly(1));
        
    }

    public bool isOpen()
    {
        return isOpened;
    }

    public void OpenDoor()
    {
        StartCoroutine(OpenDoorSmoothly(0));
    }

    public void WeightDoor(int weightTarget, int weightAmount)
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update

    IEnumerator OpenDoorSmoothly(int startPoint)
    {

        busy = true;
        float startxR = rightDoorPart.transform.position.x;
        float startxL = leftDoorPart.transform.position.x;
        float currentPoint = 0;
        while (currentPoint < 1)
        {
            if (startPoint <= 0)
            {
                rightDoorPart.transform.position = new Vector3(Mathf.Clamp(startxR + blockLenght * curveDoorR.Evaluate(currentPoint), clampXMinR, clampXMaxR),
                    transform.position.y, transform.position.z);
                leftDoorPart.transform.position = new Vector3(Mathf.Clamp(startxL - (blockLenght * curveDoorR.Evaluate(currentPoint)),  clampXMinL,clampXMaxL),
                transform.position.y, transform.position.z);
            }
            else
            {
                rightDoorPart.transform.position = new Vector3(Mathf.Clamp(startxR - blockLenght * curveDoorR.Evaluate(currentPoint), clampXMinR, clampXMaxR),
                    transform.position.y, transform.position.z);
                leftDoorPart.transform.position = new Vector3(Mathf.Clamp(startxL + blockLenght * curveDoorR.Evaluate(currentPoint), clampXMinL, clampXMaxL),
                transform.position.y, transform.position.z);
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
            col.enabled =false;
        }
        else
        {
            col.enabled = true;
            isOpened = false;
        }
    }

    public List<InteractablesEnum> WhatIsIt()
    {
        List<InteractablesEnum> interactablesEnums = new List<InteractablesEnum>();
        interactablesEnums.Add(InteractablesEnum.DOOR);
        return interactablesEnums;
    }

    public int GetWeight(out int carringCapacity)
    {
        carringCapacity = 0;
        return 0;
    }
}
