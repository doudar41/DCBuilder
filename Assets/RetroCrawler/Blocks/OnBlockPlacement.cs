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
    [SerializeField] Vector3Int nextLevelPosition;
    [SerializeField] CardinalDirections nextLevelDirection;
    [SerializeField] string nextLevelName;
    [SerializeField] int shopIndex;
    int weightInBlock;
    [SerializeField] WeightPlate weightPlate;
    [SerializeField] List<UniqueDialogueName> dialogues = new List<UniqueDialogueName>();
    [SerializeField] List<EnemySized> enemyList = new List<EnemySized>();
    [SerializeField] BattleGroundEnvironment battleGroundEnvironment;
    [SerializeField] List<ItemScriptableContainer> afterBattleLoot = new List<ItemScriptableContainer>();
    [SerializeField] List<KeyToLocks> afterBattleKeys = new List<KeyToLocks>();
    [SerializeField] List<UniqueDialogueName> afterBattleDialogue = new List<UniqueDialogueName>();
    [SerializeField] int goldAmount = 0;
    [SerializeField] GameObject characterSprite;


    private void Awake()
    {
        GameInstance.initItems += BlockInit;
    }

    private void OnDestroy()
    {
        GameInstance.initItems -= BlockInit;
    }

    private void Start()
    {
        coordinatesTextOn = GetComponentInChildren<TextMeshPro>();
        coordinatesTextOn.gameObject.SetActive(false);
    }

    void BlockInit()
    {
        foreach(UniqueDialogueName un in GameInstance.dialoguesFinished)
        {
            DeleteDialogueOption(un);
        }
        if (dialogues.Count == 0)
        {
            if (blockInteractables.Contains(InteractablesEnum.DIALOGUE)) blockInteractables.Remove(InteractablesEnum.DIALOGUE);
        }


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

    public void GetNextLevelInfo(out Vector3Int position, out CardinalDirections dir, out string levelName)
    {
        position = nextLevelPosition;
        dir = nextLevelDirection;
        levelName = nextLevelName;
    }

    public OnBlockPlacement GetOnBlock()
    {
        return this;
    }

    public int GetShopIndex()
    {
        return shopIndex;
    }

    public void AddWeightToBlock(int amount)
    {

        weightInBlock = Mathf.Clamp(weightInBlock + amount, 0,int.MaxValue);
        if (weightPlate !=null)weightPlate.CheckBlockForWeight(weightInBlock);
    }

    public int CheckWeightInBlock()
    {
        return weightInBlock;
    }


    public List<UniqueDialogueName> RunDialogue()
    {

        return dialogues;
    }

    public void DeleteDialogueOption(UniqueDialogueName uniqueDialogueName)
    {
        if(dialogues.Contains(uniqueDialogueName)) dialogues.Remove(uniqueDialogueName);
        if (afterBattleDialogue.Contains(uniqueDialogueName)) 
        { 
            afterBattleDialogue.Remove(uniqueDialogueName);
            blockInteractables.Remove(InteractablesEnum.CUSTOMBATTLE);
        }
    }
    public void DeleteDialogue()
    {

        if(blockInteractables.Contains(InteractablesEnum.DIALOGUE)) blockInteractables.Remove(InteractablesEnum.DIALOGUE);
        if (characterSprite != null) characterSprite.SetActive(false);
    }


    public void SetCustomBattle()
    {
        GameInstance.battleManager.CustomBattleStart(enemyList, gameObject.GetComponent<IBlock>(), battleGroundEnvironment) ;
    }

    public void FinishTheBattle()
    {
        blockInteractables.Remove(InteractablesEnum.CUSTOMBATTLE);
        foreach(ItemScriptableContainer item in afterBattleLoot)
        {
            GameInstance.inventory.FindEmptySlotAndPutItem(GameInstance.dataBase.HeroInventoryFromITemScriptable(item), 1);
        }
        foreach(KeyToLocks keys in afterBattleKeys)
        {
            GameInstance.inventory.SaveKeyToList(keys.keyType);
        }
        foreach(UniqueDialogueName un in afterBattleDialogue)
        {
           if (!GameInstance.party.currentUniqueDialogueNames.Contains(un)) GameInstance.party.currentUniqueDialogueNames.Add(un);
        }
        GameInstance.party.MoneyGoes(-goldAmount);
        //
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


public interface IBlock
{

    public Vector3Int GetBlockCoordinate();

    public OnBlockPlacement GetPortalPoint();

    public GameObject[] GatWalls();

    public Vector3 GetLocation();

    public void ShowOnMap(bool active);


    public OnBlockPlacement GetOnBlock();
    public void AddWeightToBlock(int amount);
    public int CheckWeightInBlock();
    public List<UniqueDialogueName> RunDialogue();
    public void DeleteDialogueOption(UniqueDialogueName uniqueDialogueName);
    public void DeleteDialogue();

    public void SetCustomBattle();
    public void FinishTheBattle();
}