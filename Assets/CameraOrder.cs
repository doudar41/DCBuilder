using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrder : MonoBehaviour
{
    [SerializeField] Camera frontUICamera, shopInventoryCamera;


    public void ShopWithoutBattlelog() 
    { 
        frontUICamera.depth = -1;
        shopInventoryCamera.depth = 2;
    }

    public void ShopWithDialogue() 
    {
        frontUICamera.depth = 3;
        shopInventoryCamera.depth = 2;
    }

    public void BattleLogWithGameplay()
    {
        frontUICamera.depth = 2;
        shopInventoryCamera.depth = -1;
    }



}
