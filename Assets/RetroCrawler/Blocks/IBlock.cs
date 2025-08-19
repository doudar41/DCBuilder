using System.Collections.Generic;
using UnityEngine;

public interface IBlock
{

    public Vector3Int GetBlockCoordinate();

    public OnBlockPlacement GetPortalPoint();

    public GameObject[] GatWalls();

    public Vector3 GetLocation();

    public void ShowOnMap(bool active);

     
}


