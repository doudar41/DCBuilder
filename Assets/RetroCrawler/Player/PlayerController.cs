using OldCode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.Tilemaps;


public class PlayerController : MonoBehaviour
{
    [SerializeField] bool noEncounter = false;
    [SerializeField] Tilemap moveTilemap;
    [SerializeField] TorchFlicker torchlight;
    [SerializeField] GameMenuToggle gameMenuToggle;

    Vector3Int startposition;
    Vector3Int currentposition;
    OnBlockPlacement currentWallBlock;
    GroundType currentGroundType;
    CardinalDirections currentforwardDirection;
    Dictionary<Vector3Int, OnBlockPlacement> wallsAccess = new Dictionary<Vector3Int, OnBlockPlacement>();

    [Header("New Movement Variables")]
    [SerializeField] GameObject nextPositionMarker;
    [SerializeField] bool noSmoothMovement = true;
    bool isMarkerMoves = false;
    Vector3Int nextPosition;

    List<Vector3Int> recordedPath = new List<Vector3Int>();
    [SerializeField] GameObject cameraFollow;
    [SerializeField] float movementRefreshRate = 0.16f;
    [SerializeField] float rotationMultiplyer = 0.2278787f;
    bool isPlayerRotating = false;
    int keypresseddirection = 0;
    bool stayPressedOnce = false;
    int inputSum = 0;

    public UnityEvent noWay, turnAround, portalTransfer;
    public UnityEvent<GroundType> stepSound;
    public UnityEvent<CardinalDirections> cardinalDirectionToUI;

    List<visitedBlock> visitedBlocks = new List<visitedBlock>();

    public PlayerState playerState = PlayerState.Explore;

    Mouse currentMouse;

    [SerializeField]
    InputActionReference leftMouse;
    DungeonInputs _input;
    [SerializeField]
    AnimationCurve walkCurve, rotationCurve;
    bool busyWalking = false;
    [SerializeField]
    float blockSize = 1;
    [SerializeField]


    [Range(0.001f, 1.0f)]
    float couroutingDelayinSec = 0.437f;

    [SerializeField] Texture2D cursorTexture;
    [SerializeField] CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] Vector2 hotSpot = Vector2.zero;

    bool cursorBusy = false;
    int stackAmountCursor;
    HeroInventoryItem cursorItemScriptable;
    [SerializeField]
    GameObject itemModelPrefab;
    [SerializeField]
    Transform dropItemPosition, throwItemPosition;
    int hoverPortraitIndex = -1;
    bool cursorHoveringUI = false;
    //string currentCursorGUID = "";


    int countdownToEncounter = 22;
    public Vector2Int rangeOfEnCounter = new Vector2Int(15, 25);
    public UnityEvent<int> EnCounter;


    Vector3 beforeBattleTransformPos;
    public Quaternion beforeBattleTransformRot;

    float intensivity = 0.1f;
    bool lightBusy = false;

    public bool waterWalk = false, lavaWalk = false;

    //Shops
    [SerializeField] ChooseShop chooseShop;
    [SerializeField] LevelChanger levelChanger;
    public bool shopIsOpened = false;
    public bool dialogueIsOpened = false;
    IDialogue textblock;
    public bool attackAllowed = false;

    bool menuOpened = false;

    [SerializeField] PlayerTakeInteractInterface takeInteractInterface;
    Dictionary<Vector3Int, BattleGroundEnvironment> groundValues = new Dictionary<Vector3Int, BattleGroundEnvironment>();

    [SerializeField] GameObject splinePrefab;

    int timeEventCounter = 0;
    public delegate void TimeEventDelegate(int timeCounter);
    public event TimeEventDelegate timeForward;

    IBlock weightPlateIBllock = null;

    Queue<MovementType> movementQueue = new Queue<MovementType>();
    Vector3 futureDestination = Vector3.zero;
    private void Awake()
    {
        GameInstance.playerController = this;

        cameraFollow.GetComponent<CameraSmoothFollow>().enabled = false;
    }



    public UnityAction InitComplete;
    private void RegisteringKeys()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);

        _input = new DungeonInputs();
        _input.Enable();

        _input.CrawlerStandart.Inventory.started += OpenCloseInventory;
        _input.CrawlerStandart.LastSpell.started += ReceiveLastSpellInput;
        _input.CrawlerStandart.Cancel.started += ReleaseSpellWithoutCasting;
        _input.CrawlerStandart.TakeInteract.started += TakeInteract;
        leftMouse.action.started += MouseRaycast;

        if (!noEncounter) countdownToEncounter = Random.Range(rangeOfEnCounter.x, rangeOfEnCounter.y);

    }

    private void OnDestroy()
    {

        _input.CrawlerStandart.Inventory.started -= OpenCloseInventory;
        _input.CrawlerStandart.LastSpell.started -= ReceiveLastSpellInput;
        _input.CrawlerStandart.Cancel.started -= ReleaseSpellWithoutCasting;
        _input.CrawlerStandart.TakeInteract.started -= TakeInteract;
        leftMouse.action.started -= MouseRaycast;
        _input.Disable();
    }


    void Update()
    {
        if(playerState == PlayerState.Battle) return;


        Vector2 moveVector2 = new Vector2(Mathf.RoundToInt(_input.CrawlerStandart.Move.ReadValue<Vector2>().x), Mathf.RoundToInt(_input.CrawlerStandart.Move.ReadValue<Vector2>().y));

        int _inputSum = (int)moveVector2.x+(int)moveVector2.y;
        if (inputSum != _inputSum)
        {
            inputSum = _inputSum;
            stayPressedOnce = false;
        }
        if (Mathf.Abs(moveVector2.x) != Mathf.Abs(moveVector2.y))
        {
            if(_input.CrawlerStandart.Move.IsPressed())
            {
                int _keypresseddirection = -1;
                if (Mathf.Abs(moveVector2.x) == 1) _keypresseddirection = 1;
                if (Mathf.Abs(moveVector2.y) == 1) _keypresseddirection = 0;
                if (keypresseddirection != _keypresseddirection)
                {
                    keypresseddirection = _keypresseddirection;
/*                    recordedPath.Clear();
                    StopCoroutine(MoveAlongPath(recordedPath, Vector2.zero));*/
                }
            }
            else
            {
                keypresseddirection = -1;
                stayPressedOnce = false;
            }
        }
        else
        {
            if (_input.CrawlerStandart.Move.IsPressed() && !stayPressedOnce)
            {
                print("change course");
                stayPressedOnce = true;
                if (keypresseddirection == 0) keypresseddirection = 1;
                else if (keypresseddirection == 1) keypresseddirection = 0;
            }
            if (!_input.CrawlerStandart.Move.IsPressed())
            {
                keypresseddirection = -1;
                stayPressedOnce = false;
            }

        }

        if (keypresseddirection == 0)
        {
            if (moveVector2.y == 1 && !isMarkerMoves) { StartCoroutine(MoveNextMarker(0, moveVector2)); return; }
            if (moveVector2.y == -1 && !isMarkerMoves) { StartCoroutine(MoveNextMarker(2, moveVector2)); return; }
        }
        if(keypresseddirection == 1)
        {
            if (moveVector2.x == 1 && !isMarkerMoves) { StartCoroutine(MoveNextMarker(1, moveVector2)); return; }
            if (moveVector2.x == -1 && !isMarkerMoves) { StartCoroutine(MoveNextMarker(3, moveVector2)); return; }
        }

        if (Mathf.Abs(moveVector2.x) ==0 && Mathf.Abs(moveVector2.y)==0)
        {
            keypresseddirection = -1;
            stayPressedOnce = false;
        }


            float turnVector = Mathf.RoundToInt(_input.CrawlerStandart.Turn.ReadValue<Vector2>().x);
        if (Mathf.Abs(turnVector) > 0)
        {
            if (!isPlayerRotating) StartCoroutine(CameraRotation(turnVector));
        }
    }
   


    IEnumerator MoveNextMarker(int _dir, Vector2 moveVector2)
    {
        //print("get input for marker move");
        isMarkerMoves = true;

        int dirMod = _dir;
        Vector2Int moveVectorInt = new Vector2Int((int)moveVector2.x, (int)moveVector2.y);

        //print("checking marker move to "+ dirMod +" - current direction int "+ (int)currentforwardDirection);
        if (keypresseddirection == 0 && (dirMod == 1 || dirMod == 3))
        {
            yield break;
        }
        if (keypresseddirection == 1 && (dirMod == 0 || dirMod == 2))
        {
            yield break;
        }
        
        if (!currentWallBlock.IfWallOpened((CardinalDirections)((dirMod + (int)currentforwardDirection) % 4))) { noWay.Invoke(); isMarkerMoves = false; yield break; }
        //print("walls are opened");
        //print(" checking direction " + (CardinalDirections)((dirMod + (int)currentforwardDirection) % 4));
        if (!RaycastOnMovement(CardinalDir.GetForwardVectorFromDirection((CardinalDirections)((dirMod + (int)currentforwardDirection) % 4)))) { isMarkerMoves = false; yield break; }
        //print("raycast is clear");
        if (shopIsOpened) { isMarkerMoves = false; yield break; }

        if (dialogueIsOpened) { isMarkerMoves = false; yield break; }
        //print("no menu is opened");
        Vector3 v = CardinalDir.GetNewPoint((CardinalDirections)((dirMod + (int)currentforwardDirection) % 4), currentposition, moveTilemap);
        if (!CheckBlockInterfaces(v)) { isMarkerMoves = false; yield break; }
        //print("no block interfaces");
        if (cameraFollow.GetComponent<CameraSmoothFollow>().enabled == false) cameraFollow.GetComponent<CameraSmoothFollow>().enabled = true;

        CheckForEncounter();
        TimeEvents(timeEventCounter);
        timeForward(timeEventCounter);

        if (CheckIfMarkerCanMoveToBlock((dirMod + (int)currentforwardDirection) % 4, nextPosition, out Vector3Int _newPosition))
        {
            nextPosition = _newPosition;
            Vector3 _npos = moveTilemap.GetCellCenterWorld(nextPosition);
            nextPositionMarker.transform.position = new Vector3(_npos.x, transform.position.y, _npos.z);
            transform.position = new Vector3(_npos.x, transform.position.y, _npos.z);
            currentposition = nextPosition;
            currentWallBlock = wallsAccess[currentposition];
        }
        yield return new WaitForSeconds(movementRefreshRate);


/*        foreach (OnBlockPlacement block in currentWallBlock.CheckForNeighbors(moveTilemap))
        {
            if (block == null) continue;
            block.ShowOnMap(true);
            visitedBlock newblock = new visitedBlock();
            newblock.coordinates = block.GetBlockCoordinate();
            newblock.level = GameInstance.GetLevelName();
            if (!visitedBlocks.Contains(newblock)) visitedBlocks.Add(newblock);

        }*/
        visitedBlock newblock2;
        newblock2.coordinates = currentWallBlock.GetBlockCoordinate();
        newblock2.level = GameInstance.GetLevelName();
        //print("current block coordinate " + currentWallBlock.GetBlockCoordinate());
        currentWallBlock.ShowOnMap(true);

        isMarkerMoves = false;
        yield return null;
    }

    IEnumerator CameraRotation(float turnVector)
    {

        currentforwardDirection = (CardinalDirections)(((int)currentforwardDirection + (turnVector == 1 ? 1 : turnVector == -1 ? -1 : 0) + 4) % 4);

        isPlayerRotating = true;
        recordedPath.Clear();
        //StopCoroutine(MoveAlongPath(recordedPath, Vector2.zero));
        float rotationDuration = 0;
        float startAngle = cameraFollow.transform.rotation.eulerAngles.y;
        while (rotationDuration < rotationMultiplyer)
        {
            if (turnVector > 0) cameraFollow.transform.rotation = Quaternion.Euler(0, Mathf.LerpAngle(startAngle, CardinalDir.GetRotationYForCardinal(currentforwardDirection), walkCurve.Evaluate(rotationDuration / rotationMultiplyer)), 0);
            else cameraFollow.transform.rotation = Quaternion.Euler(0, Mathf.LerpAngle(startAngle, CardinalDir.GetRotationYForCardinal(currentforwardDirection), walkCurve.Evaluate(rotationDuration / rotationMultiplyer)), 0);
            yield return new WaitForSeconds((rotationMultiplyer) / 10);
            rotationDuration += (rotationMultiplyer) / 10;
        }
        cameraFollow.transform.rotation = Quaternion.Euler(new Vector3(0, CardinalDir.GetRotationYForCardinal(currentforwardDirection), 0 ));
        isPlayerRotating = false;

        cardinalDirectionToUI.Invoke(currentforwardDirection);
    }


    public void InitWallAccess()
    {
        var walls = moveTilemap.GetComponentsInChildren<OnBlockPlacement>();
        currentMouse = Mouse.current;

        foreach (OnBlockPlacement w in walls)
        {
            if (!wallsAccess.ContainsKey(w.blockPosition)) { wallsAccess.Add(w.blockPosition, w); }
        }

        if (walls.Length != wallsAccess.Count)
        {
            //print("map blocks positions are broken");
            wallsAccess.Clear();
            foreach (OnBlockPlacement w in walls)
            {
                w.InitPosition(moveTilemap);
                if (!wallsAccess.ContainsKey(w.blockPosition))
                {
                    wallsAccess.Add(w.blockPosition, w);
                    groundValues.Add(w.blockPosition, w.GetBattleGroundEnvironment());
                }
            }


        }
        else
        {
            // print("blocks quantity checked");
        }

    }

    public OnBlockPlacement GetBlockByCoordinatesOnStart(Vector3Int coords)
    {

        if (wallsAccess.TryGetValue(coords, out OnBlockPlacement block)) { return block; }
        return null;
    }

    public void CheckIfLevelLoaded()
    {
        //print("wallaccess count "+wallsAccess.Count);
        if (GameInstance.levelChange)
        {
            transform.position = new Vector3(moveTilemap.GetCellCenterWorld(GameInstance.nextLevelPosition).x, 3, moveTilemap.GetCellCenterWorld(GameInstance.nextLevelPosition).z);
            transform.rotation = Quaternion.Euler(0, CardinalDir.GetRotationYForCardinal(GameInstance.nextLevelRotation), 0);
            currentforwardDirection = GameInstance.nextLevelRotation;
            cardinalDirectionToUI.Invoke(currentforwardDirection);
            currentposition = wallsAccess[moveTilemap.WorldToCell(transform.position)].GetBlockCoordinate();
            GameInstance.levelChange = false;
            currentWallBlock = wallsAccess[moveTilemap.WorldToCell(transform.position)];

        }
        else
        {
            NewGamePlayerStruct();
        }
        currentWallBlock.ShowOnMap(true);
        RegisteringKeys();
        print("Start position in cell "+ moveTilemap.WorldToCell(transform.position));
        nextPositionMarker.transform.position = transform.position;
        nextPosition = moveTilemap.WorldToCell(transform.position);
        cameraFollow.transform.position = new Vector3(transform.position.x, cameraFollow.transform.position.y, transform.position.z);

    }

    public List<visitedBlock> GetVisitedBlocksCooordinates()
    {
        return visitedBlocks;
    }

    public void ChangeTimeFlow(float multiplier)
    {
        GetComponentInChildren<DayNightChange>().ChangeTimeFlow(multiplier);
    }



    public void LightARoom(float amount)
    {
        if (amount > 0)
            torchlight.isOn = true;
        else torchlight.isOn = false;
    }


    public bool IsTorchIsOn()
    {
        return torchlight.isOn;
    }

/*
    void ReceiveAttackInput(InputAction.CallbackContext context)
    {
        if (playerState != PlayerState.Battle) return;
        if (!attackAllowed) return;
        //if (cursorHoveringUI) return; //??
        //if (GameInstance.battleManager.battleInputDelay) { context.action.Reset(); return; }

        if (!GameInstance.spellbook.SpellWaiting())
        {
            GameInstance.battleManager.ReceiveAttackInput();
        }
        attackAllowed = false;
    }
*/
    public void ReceiveAttackInput()
    {
        print("pointer on enemy");
        if (playerState != PlayerState.Battle) return;
        if (!attackAllowed) return;
        //if (cursorHoveringUI) return; //??
        //if (GameInstance.battleManager.battleInputDelay) { context.action.Reset(); return; }

        if (!GameInstance.spellbook.SpellWaiting())
        {
            print("battlemanager receive attack input");
            GameInstance.battleManager.ReceiveAttackInput();
        }

        attackAllowed = false;
    }


    public void ReceiveLastSpellInput(InputAction.CallbackContext context)
    {
        if (GameInstance.party.activeHero.GetThisHero().GetDefaultSpell() != null)
        {
            GameInstance.spellbook.CastSpell(GameInstance.party.activeHero.GetThisHero().GetDefaultSpell());
        }
    }

    public void ReceiveLastSpellInputFromUI()
    {
        if (GameInstance.party.activeHero.GetThisHero().GetDefaultSpell() != null)
        {
            GameInstance.spellbook.CastSpell(GameInstance.party.activeHero.GetThisHero().GetDefaultSpell());
        }
    }

    void ReleaseSpellWithoutCasting(InputAction.CallbackContext context)
    {
        if (cursorHoveringUI) return;
        GameInstance.spellbook.ReleaseSpellWithoutCasting();
        attackAllowed = true;
    }

    public void ReleaseSpellWithoutCasting()
    {
        if (cursorHoveringUI) return;
        GameInstance.spellbook.ReleaseSpellWithoutCasting();
        attackAllowed = true;

    }


    public bool IsCursorBusy()
    {
        return cursorBusy;
    }

    public void OpenCloseInventoryWithUIButton(bool onOff)
    {
        if (shopIsOpened) return;
        //if (cursorHoveringUI) return;
        if (!onOff)
        {
            gameMenuToggle.SwitchToSprite(-1);
            GameInstance.inventory.EnableInventory(false);
            ExitHover();

        }
        else
        {

            /*            GameInstance.inventory.EnableInventory(true);
                        gameMenuToggle.SwitchToSprite(2);*/
        }
    }


    void OpenCloseInventory(InputAction.CallbackContext context)
    {
        if (shopIsOpened) return;
        //if (cursorHoveringUI) return;
        if (GameInstance.inventory.IsOpen())
        {
            gameMenuToggle.SwitchToSprite(-1);
            GameInstance.inventory.EnableInventory(false);
            ExitHover();

        }
        else
        {

            GameInstance.inventory.EnableInventory(true);
            gameMenuToggle.SwitchToSprite(2);
        }
    }

    void NewGamePlayerStruct()
    {
        startposition = moveTilemap.WorldToCell(transform.position);
        var v = moveTilemap.GetCellCenterWorld(startposition);
        transform.position = new Vector3(v.x, transform.position.y, v.z);
        currentposition = startposition;

        var walls = moveTilemap.GetComponentsInChildren<OnBlockPlacement>();
        currentMouse = Mouse.current;

        foreach (OnBlockPlacement w in walls)
        {
            if (!wallsAccess.ContainsKey(w.blockPosition))
                wallsAccess.Add(w.blockPosition, w);
        }
        currentWallBlock = wallsAccess[currentposition];
        RotateToCardinalLocation();
        //print("Initializing controller and launching music");
        // GameInstance.soundManager.ChangeExploreMusicOnBattleGround(GameInstance.playerController.GetBattleGroundEnvironment());
        cameraFollow.transform.position = new Vector3(transform.position.x, cameraFollow.transform.position.y, transform.position.z);
        cameraFollow.GetComponent<CameraSmoothFollow>().enabled = true;
    }

    void OpenBlocksForMap(List<Vector3Int> blocksVisited)
    {
        foreach (Vector3Int b in blocksVisited)
        {
            if (wallsAccess.TryGetValue(b, out OnBlockPlacement block)) block.ShowOnMap(true);
        }
    }


    public void MenuOpened(bool onOff)
    {
        menuOpened = onOff;
    }




    public void SetEncounter(bool onOff)
    {
        noEncounter = onOff;
    }

    public bool GetEncounterState()
    {
        return noEncounter;
    }

    public BattleGroundEnvironment GetBattleGroundEnvironment()
    { 
        if (groundValues.TryGetValue(currentposition, out BattleGroundEnvironment env))
            return env;
        else return BattleGroundEnvironment.NONE;
    }


    public int GetCountdownToEncounter()
    {
        return countdownToEncounter;
    }

    public void SetCountdownToEncounter(int value)
    {
        countdownToEncounter = value;
    }

    private void CheckForEncounter()
    {
        if (!noEncounter)
        {
            if (playerState != PlayerState.Battle) EnCounter.Invoke(countdownToEncounter);
            countdownToEncounter--;

            if (countdownToEncounter <= 0)
            {
                beforeBattleTransformPos = gameObject.transform.position;
                beforeBattleTransformRot = gameObject.transform.rotation;
                //Look for free block near 
                playerState = PlayerState.Battle;
                busyWalking = false;
                GameInstance.battleManager.CustomBattleStart(null, currentWallBlock, currentWallBlock.GetBattleGroundEnvironment());
                countdownToEncounter = Random.Range(rangeOfEnCounter.x, rangeOfEnCounter.y);
                EnCounter.Invoke(countdownToEncounter);

            }
        }
    }

    public void StartCustomBattle()
    {
        beforeBattleTransformPos = gameObject.transform.position;
        beforeBattleTransformRot = gameObject.transform.rotation;

        //Look for free block near 
        playerState = PlayerState.Battle;
        busyWalking = false;
        //GameInstance.battleManager.CustomBattleStart();
        countdownToEncounter = Random.Range(rangeOfEnCounter.x, rangeOfEnCounter.y);
    }


    public void ResetCameraFollowState(bool reset)
    {
        if (reset)
        {

            cameraFollow.gameObject.transform.position = new Vector3(transform.position.x, cameraFollow.transform.position.y, transform.position.z);
            cameraFollow.gameObject.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            cameraFollow.gameObject.GetComponent<CameraSmoothFollow>().enabled = true;
        }
        else
        {
            cameraFollow.gameObject.GetComponent<CameraSmoothFollow>().enabled = false;
            cameraFollow.gameObject.transform.position = new Vector3(transform.position.x, cameraFollow.transform.position.y, transform.position.z);
            cameraFollow.gameObject.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        }
    }

    public void StartCustomBattle(Transform inPlace)
    {
        playerState = PlayerState.Battle;
        busyWalking = false;
    }

    public void BlockMovement(bool onOff)
    {
        shopIsOpened = onOff;
    }


    public void ReturnToPreBattlePosition()
    {
        playerState = PlayerState.Explore;
        EnCounter.Invoke(countdownToEncounter);
        gameObject.transform.position = new Vector3(currentWallBlock.GetLocation().x, beforeBattleTransformPos.y, currentWallBlock.GetLocation().z); ;

        float y = beforeBattleTransformRot.eulerAngles.y;
        if (beforeBattleTransformRot.eulerAngles.y % 90 != 0) y= CardinalDir.GetRotationYForCardinal(currentforwardDirection);


        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, y , 0));
        cameraFollow.transform.position = new Vector3(transform.position.x, cameraFollow.transform.position.y, transform.position.z);
        cameraFollow.gameObject.GetComponent<CameraSmoothFollow>().enabled = true;
    }

    public Vector3Int GetCurrentPosition()
    {
        return currentposition;
    }

    public CardinalDirections GetCurrentDirection()
    {
        return currentforwardDirection;
    }

    void RotateToCardinalLocation()
    {
        currentforwardDirection = CardinalDirections.NORTH;
        if (transform.rotation.eulerAngles.y != 0)
        {
            float YAngle = transform.rotation.eulerAngles.y % 360;


            if (YAngle > 0 && YAngle <= 45)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                cardinalDirectionToUI.Invoke(currentforwardDirection);
                return;
            }
            if (YAngle > 45 && YAngle <= 135)
            {
                transform.rotation = Quaternion.Euler(0, 90, 0);
                currentforwardDirection = CardinalDirections.EAST;
                cardinalDirectionToUI.Invoke(currentforwardDirection);
                return;
            }
            if (YAngle > 135 && YAngle <= 225)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
                currentforwardDirection = CardinalDirections.SOUTH;
                cardinalDirectionToUI.Invoke(currentforwardDirection);
                return;
            }
            if (YAngle > 225 && YAngle <= 325)
            {
                transform.rotation = Quaternion.Euler(0, 270, 0);
                currentforwardDirection = CardinalDirections.WEST;
                cardinalDirectionToUI.Invoke(currentforwardDirection);
                return;
            }
        }
    }

    public OnBlockPlacement GetBlockFromVector3(Vector3 position)
    {
        if (wallsAccess.TryGetValue(moveTilemap.WorldToCell(position), out OnBlockPlacement block)) { return block; }
        return null;
    }

    void MouseRaycast(InputAction.CallbackContext obj)
    {

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(currentMouse.position.ReadValue().x, currentMouse.position.ReadValue().y, 0));
        if (cursorHoveringUI)
        {
            
            return;
        }
        if (cursorBusy)
        {
            if(playerState == PlayerState.Battle) return;
            if (cursorItemScriptable.itemType == ItemType.Key) return;
            if(cursorHoveringUI && hoverPortraitIndex >0)
            {
               equipmentSlot _slot = GameInstance.inventory.FindEquipmentSlotOfType(cursorItemScriptable.itemType);
               if (_slot!=null) _slot.SlotChecking();
                return;
            }

            cursorItemScriptable.positionReplaced = dropItemPosition.position;
            //GameInstance.SaveItemState(currentCursorGUID, SavedState.Replaced, cursorItemScriptable);
            ThrowToTheWorld(dropItemPosition, currentMouse.position.ReadValue().y);
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
            /*            cursorBusy = false;
                        cursorItemScriptable = null;*/
            StartCoroutine(WaitToSetCursorFree());
            return;
        }

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.distance > blockSize) return;
            IInteractables inter = hit.collider.GetComponent<IInteractables>();

            if (inter != null)
            {
               //print(hit.distance + " " + hit.collider + " " + inter);
                List<InteractablesEnum> list = inter.WhatIsIt();
                foreach (InteractablesEnum i in list)
                {
                    switch (i)
                    {
                        case InteractablesEnum.PICKABLE:
                            //GetInterfaceFromItem(hit.collider.gameObject);
                            break;

                    }

                }
            }
        }
    }
    IEnumerator WaitToSetCursorFree()
    {
        yield return new WaitForSeconds(0.1f);
        cursorBusy = false;
        cursorItemScriptable = null;
    }

    public  void  GetInterfaceFromItem(GameObject hitObject)
    {
        if (cursorBusy || cursorHoveringUI) return;

        if (Vector3.Distance(transform.position, hitObject.transform.position) > blockSize) return;
        //print("click on item " + hitObject.name);
        if(hitObject.GetComponent<ItemModel>() != null)
        {
            GameInstance.spellbook.battlelogEvent.Invoke(
                                                    new List<string>() { "item taken  " },
                                                    new List<ResultMsg>() { new ResultMsg() { msgType = "s", msgString = GameInstance.dataBase.GetItemFromBaseByIndex(hitObject.GetComponent<ItemModel>().WhatItem().container).itemName } }
                                                    );
        }


        IItem iItem = hitObject.GetComponent<IItem>();
        cursorItemScriptable = iItem.WhatItem();

        SetPlayerCursorBusy(cursorItemScriptable);
        iItem.RemoveFromTheWorld();

    }

    public void CreateItemInWorld(HeroInventoryItem _item )
    {
        GameObject item = Instantiate(itemModelPrefab, dropItemPosition);

        IItem iItem = item.GetComponent<IItem>();
        iItem.ChangeGUID();
        iItem.SetPrefab(GameInstance.dataBase.GetItemFromBaseByIndex(_item.container));
        iItem.SetItemsAmount(stackAmountCursor);
        iItem.PlaceCreatedItem(dropItemPosition.position);
        iItem.RemoveFromParent();
    }


    public void ThrowToTheWorld(Transform spawnPoint, float screenPosition)
    {
        if (screenPosition <= 500)
        {
            GameObject item = Instantiate(itemModelPrefab, spawnPoint);

            IItem iItem = item.GetComponent<IItem>();
            iItem.ChangeGUID();
            iItem.SetPrefab(GameInstance.dataBase.GetItemFromBaseByIndex(cursorItemScriptable.container));
            iItem.SetItemsAmount(stackAmountCursor);
            iItem.PlaceCreatedItem(spawnPoint.position);
            iItem.RemoveFromParent();
        }



        if (screenPosition > 500) 
        {
            //print("throw");
            GameObject splineAhead = Instantiate(splinePrefab, throwItemPosition.position, throwItemPosition.rotation);
            splineAhead.transform.GetComponentInChildren<ThrownItem>().SetItemAndIcon(cursorItemScriptable, GameInstance.dataBase.GetItemFromBaseByIndex(cursorItemScriptable.container).worldSprite, stackAmountCursor);
            splineAhead.transform.parent = null;
        }
    }
    

    public HeroInventoryItem GetItemFromCursor()
    {
        HeroInventoryItem tempItem = new HeroInventoryItem();
        tempItem =  cursorItemScriptable;
        cursorBusy = false;
        cursorItemScriptable = null;
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        return tempItem;
    }

    public void SetPlayerCursorBusy(HeroInventoryItem heroInventoryItem)
    {
       // print("set cursor busy " + heroInventoryItem.itemType);
/*        if (GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).itemType == ItemType.Key)
        {
            GameInstance.inventory.SaveKeyToList(GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).keyType);
            return;
        }*/

        heroInventoryItem.heroIndex = -1;
        cursorItemScriptable = heroInventoryItem;
        stackAmountCursor = heroInventoryItem.stackAmount;
        //currentCursorGUID = heroInventoryItem._GUID;
        //GameInstance.SaveItemState(heroInventoryItem._GUID, SavedState.Cursor, heroInventoryItem);

        Cursor.SetCursor(GameInstance.dataBase.GetItemFromBaseByIndex(heroInventoryItem.container).texture2DMouse, hotSpot, cursorMode);
        cursorBusy = true;
    }

    bool RaycastOnMovement(Vector3 dir)
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        //LayerMask layerMask = LayerMask.GetMask("Blocks");
        if (Physics.Raycast(transform.position, transform.TransformDirection(dir), out hit, 5f))

        {
            IInteractables inter = hit.collider.GetComponent<IInteractables>();
            if (inter != null)
            {
                List<InteractablesEnum> list = inter.WhatIsIt();
                foreach (InteractablesEnum i in list)
                {
                    switch (i)
                    {
                        case InteractablesEnum.ENEMY:
                            // Custom Enemy Spawn
                            break;
                        case InteractablesEnum.DOOR:
                            IDoor door = hit.collider.GetComponent<IDoor>();
                            print("Door is " + door.isOpen());
                            return door.isOpen();

                        case InteractablesEnum.SWITCH:
                            break;
                        case InteractablesEnum.LEVEL_EXIT:

                            break;
                        case InteractablesEnum.TRAP:
                            break;
                        case InteractablesEnum.STORY:
                            break;

                        case InteractablesEnum.WALL:
                            return false;
                        case InteractablesEnum.DIALOGUEKEY:
                            UniqueDialogueName un =  hit.collider.GetComponent<DialogueKey>().GetUniqueDialogueName();
                            if(!GameInstance.party.currentUniqueDialogueNames.Contains(un)) GameInstance.party.currentUniqueDialogueNames.Add(un);
                            GameObject.DestroyImmediate(hit.collider.gameObject);
                            return false;
                        case InteractablesEnum.DIALOGUE:

                            textblock = hit.collider.GetComponent<IDialogue>();
                            GameInstance.dialoguePanel.SetIDialogue(textblock);
                            dialogueIsOpened = true;
                            GameInstance.dialoguePanel.ActivateFirstDialogue();
                            return false;

                    }
                }
            }

        }
        return true;
    }


    public List<InteractablesEnum> GetIIteractableInterfaces(Vector3 v)
    {
        IInteractables iInteractables;
        if (wallsAccess.ContainsKey(moveTilemap.WorldToCell(v)))
        { iInteractables = wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IInteractables>(); }
        else return null;
        List<InteractablesEnum> interactableList = iInteractables.WhatIsIt();
        return interactableList;
    }
    public IBlock GetBlockInterface(Vector3 v)
    {
        IBlock iblock;
        if (wallsAccess.ContainsKey(moveTilemap.WorldToCell(v)))
        {
            iblock = wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IBlock>();
            return iblock; }
        else return null;

    }


    bool CheckBlockInterfaces(Vector3 v)
    {
        IInteractables iInteractable;
        if (wallsAccess.ContainsKey(moveTilemap.WorldToCell(v)))
        { iInteractable = wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IInteractables>(); }
        else return false;
        List<InteractablesEnum> interactableList = iInteractable.WhatIsIt();

        foreach (InteractablesEnum i in interactableList)
        {
            switch (i)
            {

                case InteractablesEnum.DOOR:
                    //IDoor door = wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<Collider>().GetComponent<IDoor>();
                    //print("Door is " + door.isOpen());door.isOpen()

                    break;
                // check for door interface, action accordinly 

                case InteractablesEnum.LEVEL_EXIT:

                    OnBlockPlacement leveldestination = wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<OnBlockPlacement>();
                    InputEnable(false);
                    leveldestination.GetNextLevelInfo(out Vector3Int pos, out CardinalDirections dir, out string levelName);
                    GameInstance.nextLevelPosition = pos;
                    GameInstance.nextLevelRotation = dir;
                    if (levelChanger == null) { GameInstance.LoadNextLevel(levelName); return false; }
                    if (levelChanger.CheckLevelName(levelName)) levelChanger.OpenLevelEntranceGraphics(levelName);
                    else GameInstance.LoadNextLevel(levelName);
                    //Autosave, read location and rotation of destination from IBlock save it to gameinstance 
                    // check for level exit interface, save tranfer point on another level to save file  load target level
                    return false;
                case InteractablesEnum.PORTAL:

                    OnBlockPlacement portalDest =  wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IBlock>().GetPortalPoint();
                    transform.position = new Vector3( portalDest.gameObject.transform.position.x, transform.position.y, portalDest.gameObject.transform.position.z);
                    currentposition = portalDest.GetBlockCoordinate(); 
                    
                    return false;

                case InteractablesEnum.LADDER:
                    // move player to another level of a tilemap
                    break;
                case InteractablesEnum.TRAP:
                    wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IBlock>().LaunchTrap(); 
                    
                    break;

                case InteractablesEnum.WALL:
                    return false;
                case InteractablesEnum.STORE:
                    //print("open a store");
                    shopIsOpened = true;
                    chooseShop.ChooseShopOfType( wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<OnBlockPlacement>().GetShopIndex());

                    if (interactableList.Contains(InteractablesEnum.DIALOGUE)) break;
                    else return false;
                case InteractablesEnum.DIALOGUE:
                    textblock = wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IDialogue>();
                    if (textblock.GetDialogueOptionCount() == 0)
                    {
                        return false;
                    }
                    GameInstance.dialoguePanel.SetIDialogue(textblock);
                    dialogueIsOpened = true;
                    GameInstance.dialoguePanel.ActivateFirstDialogue();
                    return false;
                case InteractablesEnum.CUSTOMBATTLE:
                    print("start custom battle");
                    wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IBlock>().SetCustomBattle();
                    playerState = PlayerState.Battle;
                    return false;

                case InteractablesEnum.CUSTOMBATTLEINPLACE:
                    if(CardinalDir.GetNewPoint(currentforwardDirection, currentposition, moveTilemap) != v) return false;
                    StartCustomBattle(transform);
                    List<GameObject> enemy = wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<OnBlockPlacement>().GetEnemyListForCustomBattle();
                    GameInstance.battleManager.CustomBattleInPlace(enemy, wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IBlock>());

                    return false;
                case InteractablesEnum.WEIGHTPLATE:
                    weightPlateIBllock = wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IBlock>();
                    weightPlateIBllock.AddWeightToBlock(GameInstance.party.GetPartyWeight());
                    
                    break;
            }
        }
        if (weightPlateIBllock == null) return true;

        if (weightPlateIBllock != wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IBlock>())
        {
            weightPlateIBllock.AddWeightToBlock(-GameInstance.party.GetPartyWeight());
            weightPlateIBllock = null;
        }
        return true;
    }


    public void TeleportToMarkDestination(MarkSavedLocation mark)
    {
        if (GameInstance.markSavedLocations == null) return;
        if (mark.levelName != SceneManager.GetActiveScene().name)
        {
            GameInstance.nextLevelPosition = mark.position;
            GameInstance.nextLevelRotation = mark.direction;
            GameInstance.LoadNextLevel(mark.levelName);  
        }
        else
        {
            transform.position = new Vector3(wallsAccess[mark.position].GetLocation().x, transform.position.y, wallsAccess[mark.position].GetLocation().z);
            currentposition = mark.position;
            currentforwardDirection = mark.direction;
            RotateToCardinalLocation();
        }

    }


    public void InputEnable(bool onOff)
    {
       if(!onOff) _input.Disable();
       if(onOff) _input.Enable();
    }

    public void SetPlayerState(PlayerState state)
    {
        playerState = state;
    }

    public PlayerState GetPlayerState()
    {
        return playerState;
    }

    // Switch off and on mouse world functionality to UI
    public void EnterHover(HoverUIElementEnum elementType, GameObject g)
    {
        cursorHoveringUI = true;
        //print("hover on  " + cursorHoveringUI + " cursor busy " + cursorBusy);
        switch (elementType)
        {
            case HoverUIElementEnum.PORTRAIT:

                hoverPortraitIndex = g.GetComponent<IIUInterfaces>().GetIndex();
                //print(g + " index "+ hoverPortraitIndex);
                break;
        }
    }

    public void NoMouseInGameInteraction(bool switchUI)
    {
        cursorHoveringUI = switchUI;
    }



    public void ExitHover()
    {
        cursorHoveringUI = false;
        hoverPortraitIndex = -1;
        //print("hover off  " + cursorHoveringUI + " cursor busy " + cursorBusy);
    }



    void TimeEvents(int count)
    {
        if (currentGroundType == GroundType.Water)
        {
            if (!waterWalk)
            {
                foreach(Hero h in GameInstance.party.GetPartyMembers())
                {
                    h.HealthDecrease((int)(h.GetMaxDependedStat(DependedStat.maxHealth)*0.1f));
                }
            }
        }

        if (currentGroundType == GroundType.Fire)
        {
            if (!lavaWalk)
            {
                foreach(Hero h in GameInstance.party.GetPartyMembers())
                {
                    h.HealthDecrease((int)(h.GetMaxDependedStat(DependedStat.maxHealth)*0.3f));
                }
            }
        }
        timeEventCounter++;
    }

    void TakeInteract(InputAction.CallbackContext context)
    {
        if (cursorHoveringUI) return;
        if (shopIsOpened) { return; }
        CheckBlockInterfaces(CardinalDir.GetNewPoint(currentforwardDirection, currentposition, moveTilemap));
        RaycastOnMovement(Vector3.forward);
        takeInteractInterface.SwitchOnCollider();

    }

    public Vector3 GetVector3PosFromBlock(Vector3Int _pos)
    {
        if (wallsAccess.ContainsKey(_pos))
        {
            return wallsAccess[_pos].gameObject.transform.position;
        }

        return _pos;
    }

    public bool CheckIfMarkerCanMoveToBlock(int dir, Vector3Int _currentposition, out Vector3Int newPosition)
    {
        newPosition = _currentposition;

        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.right,
            Vector3.back,
            Vector3.left
        };

        Vector3 forward = nextPositionMarker.transform.TransformDirection(directions[dir]) * 5;
        Debug.DrawRay(nextPositionMarker.transform.position, forward, Color.red, 10, false);
 
        newPosition = _currentposition + new Vector3Int((int)directions[dir].x, (int)directions[dir].z, 0);
        return true;
    }

    public bool CheckForBlockAvailable(Vector3Int pos)
    {
        if (wallsAccess.ContainsKey(pos))
        {
            return true;
        }
        else return true;
    }

    public Tilemap GetMovementTilemap() 
    {         
        return moveTilemap;
    }


    public float GetPlayerSpeed()
    {
        return movementRefreshRate;
    }

    public float GetPlayerRotationSpeed()
    {
        return rotationMultiplyer;
    }

    public void SetPlayerSpeed(float speed)
    {
        movementRefreshRate = speed;
        //cameraFollow.GetComponent<CameraSmoothFollow>().SetCameraSpeed(speed);
    }

    public void SetPlayerRotationSpeed(float speed)
    {
        rotationMultiplyer = speed;
    }

}


public interface IPlayerInterface
{
    public void SetPlayerState(PlayerState state);

}




public enum PlayerState
{
    Explore,
    Battle,
    MenuOpened
}

public enum MovementType
{
    Forward,
    Backward,
    StrafeLeft,
    StrafeRight,
    TurnLeft,
    TurnRight
}