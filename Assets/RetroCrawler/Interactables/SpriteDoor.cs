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
    //bool busy = false;
    [SerializeField]float clampXMinR, clampXMaxR, clampXMaxL, clampXMinL;
    [SerializeField]
    bool isOpened = false;

    void Start()
    {
/*        clampXMinR = transform.localPosition.x;
        clampXMaxR = blockLenght + clampXMinR;
        clampXMaxL = transform.localPosition.x;
        clampXMinL = blockLenght - clampXMinR;*/
        if (isOpened)
        {
            OpenDoor();
        }

    }  
    
    
    
    public void CloseDoor()
    {
        StopAllCoroutines();
        StartCoroutine(OpenDoorSmoothly(1));
        
    }

    public bool isOpen()
    {
        return isOpened;
    }

    public void OpenDoor()
    {
        StopAllCoroutines();
        StartCoroutine(OpenDoorSmoothly(0));
    }

    public void WeightDoor(int weightTarget, int weightAmount)
    {

    }

    // Start is called before the first frame update

    IEnumerator OpenDoorSmoothly(int startPoint)
    {

        //busy = true;
        float startxR = rightDoorPart.transform.localPosition.x;
        float startxL = leftDoorPart.transform.localPosition.x;
        float currentPoint = 0;
        while (currentPoint < 1)
        {
            if (startPoint <= 0)
            {
                rightDoorPart.transform.localPosition = new Vector3(Mathf.Clamp(startxR +  curveDoorR.Evaluate(currentPoint), clampXMinR, clampXMaxR),
                    0, 0);
                leftDoorPart.transform.localPosition = new Vector3(Mathf.Clamp(startxL - curveDoorR.Evaluate(currentPoint),  clampXMinL,clampXMaxL),
                0, 0);
            }
            else
            {
                rightDoorPart.transform.localPosition = new Vector3(Mathf.Clamp(startxR -  curveDoorR.Evaluate(currentPoint), clampXMinR, clampXMaxR),
                    0, 0);
                leftDoorPart.transform.localPosition = new Vector3(Mathf.Clamp(startxL +  curveDoorR.Evaluate(currentPoint), clampXMinL, clampXMaxL),
                0, 0);
                //print("start corouting" + currentPoint);
            }

            yield return new WaitForSeconds(0.01f);

            currentPoint += 0.01f;
        }
        //print("cancel");
        //busy = false;
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
