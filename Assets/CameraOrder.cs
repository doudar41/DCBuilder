using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrder : MonoBehaviour
{
    [SerializeField] Camera frontUICamera, shopInventoryCamera;

    // dialogues should be in front of everything
    // dialogue window never goes with Inventory or Stats windows
    // battle log goes with gameplay but behind shops, inventory and stats windows

    public void ShopWithoutBattlelog()
    {
        if (!shopInventoryCamera.isActiveAndEnabled) shopInventoryCamera.gameObject.SetActive(true);
        frontUICamera.depth = -1;
        shopInventoryCamera.depth = 2;
    }

    public void ShopWithDialogue() 
    {
        if(!shopInventoryCamera.isActiveAndEnabled) shopInventoryCamera.gameObject.SetActive(true);
        frontUICamera.depth = 3;
        shopInventoryCamera.depth = 2;
    }

    public void BattleLogWithGameplay()
    {
        frontUICamera.depth = 2;
        shopInventoryCamera.gameObject.SetActive(false);
        shopInventoryCamera.depth = -1;
    }



}
