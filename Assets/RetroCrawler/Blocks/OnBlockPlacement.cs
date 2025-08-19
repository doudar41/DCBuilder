using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;


public class OnBlockPlacement : MonoBehaviour, IBlock, IInteractables
{

    public TextMeshPro coordinatesTextOn; //Shown only in Editor
    public GameObject[] walls = new GameObject[4]; //filled while placing on tilemap
    public Vector3Int position; //given when placing on a Tilemap
    public OnBlockPlacement blockParent; //temporary parent for pathfinding


    public List<InteractablesEnum> blockInteractables = new List<InteractablesEnum>();

    [SerializeField] OnBlockPlacement portalDestination;

    [SerializeField] GameObject mapGraphics;
    [SerializeField] GroundType groundType;

    private void Start()
    {
        coordinatesTextOn = GetComponentInChildren<TextMeshPro>();
        coordinatesTextOn.gameObject.SetActive(false);
    }

    public void CheckGridForGameObject(Tilemap tilemap, Vector3Int position)
    {
        var blocks = transform.parent.GetComponentsInChildren<Transform>();
        CheckWallsForNeighbors(tilemap, CardinalDirections.EAST, position, 1, 0, blocks);
        CheckWallsForNeighbors(tilemap, CardinalDirections.WEST, position, -1, 0, blocks);
        CheckWallsForNeighbors(tilemap, CardinalDirections.NORTH, position, 0, 1, blocks);
        CheckWallsForNeighbors(tilemap, CardinalDirections.SOUTH, position, 0, -1, blocks);
    }

    public void CheckWallsForNeighbors(Tilemap tilemap,
                                    CardinalDirections wallIndex,
                                    Vector3Int position,
                                    int shiftBlockX,
                                    int shiftBlockY,
                                    Transform[] blocks)
    {

        Vector3 BlockWorldCoordinate = tilemap.GetCellCenterWorld(new Vector3Int(position.x + shiftBlockX, position.y + shiftBlockY, position.z));
        foreach (Transform p in blocks)
        {
            if (p.position == new Vector3(BlockWorldCoordinate.x, p.position.y, BlockWorldCoordinate.z))
            {
                var neighbour = p.gameObject.GetComponent<OnBlockPlacement>();

                walls[(int)wallIndex].SetActive(false);
                if (neighbour != null)
                    neighbour.walls[(int)CardinalDir.GetOpposite(wallIndex)].SetActive(false);
            }
        }
    }
    OnBlockPlacement CheckForNeighbor(Tilemap tilemap,
                                CardinalDirections wallIndex,
                                Vector3Int position,
                                int shiftBlockX,
                                int shiftBlockY,
                                Transform[] blocks)
    {

        Vector3 BlockWorldCoordinate = tilemap.GetCellCenterWorld(new Vector3Int(position.x + shiftBlockX, position.y + shiftBlockY, position.z));

        foreach (Transform p in blocks)
        {
            if (p.position == new Vector3(BlockWorldCoordinate.x, p.position.y, BlockWorldCoordinate.z))
            {
                if(p.gameObject.GetComponent<OnBlockPlacement>() !=null) return p.gameObject.GetComponent<OnBlockPlacement>();
            }
        }
        return null;
    }

    public List<OnBlockPlacement> CheckForNeighbors(Tilemap tilemap)
    {
        List<OnBlockPlacement> neighborsAround = new List<OnBlockPlacement>();
        var blocks = transform.parent.GetComponentsInChildren<Transform>();
        neighborsAround.Add(CheckForNeighbor(tilemap, CardinalDirections.EAST, position, 1, 0, blocks));
        neighborsAround.Add(CheckForNeighbor(tilemap, CardinalDirections.WEST, position, -1, 0, blocks));
        neighborsAround.Add(CheckForNeighbor(tilemap, CardinalDirections.NORTH, position, 0, 1, blocks));
        neighborsAround.Add(CheckForNeighbor(tilemap, CardinalDirections.SOUTH, position, 0, -1, blocks));
        return neighborsAround;
    }

    

    public bool IfWallOpened(CardinalDirections dir)
    {
        
        bool access = true;
        switch (dir)
        {
            case CardinalDirections.EAST:
                if (walls[1].activeSelf) access = false;
                break;
            case CardinalDirections.SOUTH:
                if (walls[2].activeSelf) access = false;
                break;
            case CardinalDirections.WEST:
                if (walls[3].activeSelf) access = false;
                break;
            case CardinalDirections.NORTH:
                if (walls[0].activeSelf) access = false;
                break;
        }
        return access;
    }



    public Vector3Int GetPortalDestination()
    {
        return portalDestination.position;
    }

    public void CoordinatesToText()
    {
        coordinatesTextOn.text = position.ToString();
    }

    public Vector3Int GetBlockCoordinate()
    {
        return position;
    }

    public OnBlockPlacement GetPortalPoint()
    {
        return portalDestination;
    }

    public GameObject[] GatWalls()
    {
        return walls;
    }

    public List<InteractablesEnum> WhatIsIt()
    {
        return blockInteractables;
    }

    public int GetWeight(out int capacity)
    {
        capacity = 0;
        return 0;
    }

    public Vector3 GetLocation()
    {
        return this.transform.position;
    }

    public void ShowOnMap(bool active)
    {
        if(mapGraphics != null)
        mapGraphics.SetActive(active);
    }

    public GroundType GetGroundType()
    {
        return groundType;
    }
}



public enum GroundType
{

    Concrete,
    Sand,
    Dirt,
    Snow,
    Fire,
    Water,
    None

}



