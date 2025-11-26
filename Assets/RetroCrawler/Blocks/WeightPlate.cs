using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightPlate : MonoBehaviour
{
    GameObject player;
    [SerializeField] GameObject interactionTarget;
    [SerializeField] OnBlockPlacement block;
    [SerializeField] int weightToOpen;

    public void CheckBlockForWeight(int amount)
    {
        IDoor door =  interactionTarget.GetComponent<IDoor>();
        if (door == null) return;
        print("weight on plate " + amount);
        if(player != null)
        {
            if (door.isOpen()) return;
        }
        if (amount >= weightToOpen)
        {
            if(!door.isOpen()) door.OpenDoor();
        }
        else
        {
             door.CloseDoor();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<PlayerController>() != null)
        {
            if (block.CheckWeightInBlock() < weightToOpen)
            {
                CheckBlockForWeight(block.CheckWeightInBlock() + GameInstance.party.GetPartyWeight());
            }
            player = collision.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        CheckBlockForWeight(block.CheckWeightInBlock());
        player = null;
    }
}
