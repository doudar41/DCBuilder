using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.Splines;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public Tilemap moveTilemap;

    Vector3Int startposition;
    Vector3Int currentposition;
    OnBlockPlacement currentWallBlock;
    GroundType currentGroundType;
    CardinalDirections currentforwardDirection;
    Dictionary<Vector3Int, OnBlockPlacement> wallsAccess = new Dictionary<Vector3Int, OnBlockPlacement>();
    public UnityEvent noWay, stepSound, turnAround, portalTransfer;
    public UnityEvent<CardinalDirections> cardinalDirectionToUI;

    List<Vector3Int> visitedBlocks = new List<Vector3Int>();

    [SerializeField] Light torchlight;

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
    [Range(0.5f, 3f)]
    float walkSpeed = 0.05f;

    [SerializeField]
    [Range(0.5f, 3f)]
    float rotationSpeed = 0.05f;

    [SerializeField]
    [Range(0.001f, 1.0f)]
    float couroutingDelayinSec = 0.01f;

    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    bool cursorBusy = false;
    int stackAmountCursor;
    ItemScriptableContainer cursorItemScriptable;
    [SerializeField]
    GameObject itemModelPrefab;
    [SerializeField]
    Transform dropItemPosition, throwItemPosition;
    int hoverPortraitIndex = -1;
    bool cursorHoveringUI = false;


    int countdownToEncounter = 22;
    public Vector2Int rangeOfEnCounter = new Vector2Int(15,25);
    public UnityEvent<int> EnCounter;


    Vector3 beforeBattleTransformPos;
    public Quaternion beforeBattleTransformRot;

    float intensivity = 0.1f;
    bool lightBusy =false;

    public bool waterWalk = false, lavaWalk = false;

    private void OnEnable()
    {
        GameInstance.playerController = this;
    }

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        NewGamePlayerStruct();
        _input = new DungeonInputs();
        _input.Enable();

        _input.CrawlerStandart.Move.performed += MovementUpdate;
        _input.CrawlerStandart.Turn.performed += TurnAround;
        _input.CrawlerStandart.Inventory.started += OpenCloseInventory;
        _input.CrawlerStandart.Attack.started += ReceiveAttackInput;
        _input.CrawlerStandart.LastSpell.started += ReceiveLastSpellInput;
        _input.CrawlerStandart.Cancel.started += ReleaseSpellWithoutCasting;
        leftMouse.action.started += MouseRaycast;
        countdownToEncounter = Random.Range(rangeOfEnCounter.x, rangeOfEnCounter.y);
        currentWallBlock.ShowOnMap(true);
        GameInstance.progress += TimeEvents;


    }


    private void OnDestroy()
    {
        _input.CrawlerStandart.Move.performed -= MovementUpdate;
        _input.CrawlerStandart.Turn.performed -= TurnAround;
        _input.CrawlerStandart.Inventory.started -= OpenCloseInventory;
        _input.CrawlerStandart.Attack.started -= ReceiveAttackInput;
        _input.CrawlerStandart.LastSpell.started -= ReceiveLastSpellInput;
        _input.CrawlerStandart.Cancel.started -= ReleaseSpellWithoutCasting;
        leftMouse.action.started -= MouseRaycast;
        GameInstance.progress -= TimeEvents;
    }


    public void LightARoom(float amount)
    {
        intensivity = amount;
        torchlight.intensity = intensivity;
    }


    void ReceiveAttackInput(InputAction.CallbackContext context)
    {
        if (GameInstance.battleManager.BattleEffect) return;
        if (playerState == PlayerState.Battle && !GameInstance.spellbook.SpellCharged) 
        { 
             GameInstance.battleManager.ReceiveAttackInput();
            GameInstance.battleManager.BattleEffect = true;
        }
    }

    public void ReceiveLastSpellInput(InputAction.CallbackContext context)
    {
        GameInstance.battleManager.ReceiveLastSpellInput();
    }

    public void ReleaseSpellWithoutCasting(InputAction.CallbackContext context)
    {
        GameInstance.spellbook.ReleaseSpellWithoutCasting();
    }

    public bool IsCursorBusy()
    {
        return cursorBusy;
    }


    public void OpenInventoryWithUIButton()
    {
        if (GameInstance.inventory.gameObject.activeSelf)
        {
            GameInstance.inventory.gameObject.SetActive(false);
            ExitHover();
        }
        else
        {
            GameInstance.inventory.gameObject.SetActive(true);
        }
    }
    void OpenCloseInventory(InputAction.CallbackContext context)
    {

        if (GameInstance.inventory.IsOpen())
        {
            GameInstance.inventory.EnableInventory(false);
            ExitHover();
        }
        else
        {
            GameInstance.inventory.EnableInventory(true);
        }
    }

    void ReadPlayerStructFromSaveFile()
    {

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
            if (!wallsAccess.ContainsKey(w.position))
                wallsAccess.Add(w.position, w);
        }
        currentWallBlock = wallsAccess[currentposition];
        RotateToCardinalLocation();

    }

    void OpenBlocksForMap(List<Vector3Int> blocksVisited)
    {
        foreach (Vector3Int b in blocksVisited)
        {
            if (wallsAccess.TryGetValue(b, out OnBlockPlacement block)) block.ShowOnMap(true);
        }
    }


    void MovementUpdate(InputAction.CallbackContext context)
    {

        if (playerState == PlayerState.Battle) { /*print("battle state");*/ return; }
        //GameInstance.savedInt++;
        MovementUpdateFuther(context.ReadValue<Vector2>());

    }

    void TurnAround(InputAction.CallbackContext context)
    {
        if (playerState == PlayerState.Battle) { /*print("battle state");*/ return; }
        TurnAroundFloat(context.ReadValue<float>());
    }

    void TurnAroundFloat(float moveInput)
    {
        if (playerState == PlayerState.Battle) { /*print("battle state");*/ return; }
        if (moveInput > 0)
        {
            TurnRight();
        }
        if (moveInput < 0)
        {
            TurnLeft();
        }
    }

    public void MoveForwardGrahic()
    {
        MovementUpdateFuther(Vector2.up);
    }
    public void MoveBackwardGrahic()
    {
        MovementUpdateFuther(Vector2.down);
    }

    public void MoveStrafeLeft()
    {
        MovementUpdateFuther(Vector2.left);
    }

    public void MoveStrafeRight()
    {
        MovementUpdateFuther(Vector2.right);
    }

    void MovementUpdateFuther(Vector2 moveInput)
    {
        if (playerState != PlayerState.Battle) EnCounter.Invoke(countdownToEncounter);
        countdownToEncounter--;

        if (countdownToEncounter == 0)
        {
            beforeBattleTransformPos = gameObject.transform.position;
            beforeBattleTransformRot = gameObject.transform.rotation;
            //Look for free block near 
            playerState = PlayerState.Battle;
            busyWalking = false;
            GameInstance.battleManager.BattleStart();
            countdownToEncounter = Random.Range(rangeOfEnCounter.x, rangeOfEnCounter.y);

        }

        if (playerState == PlayerState.Battle) { /*print("battle state");*/ return; }
        if (!lightBusy) StartCoroutine(LightFlickering());

        if (moveInput.y > 0)
        {
            MoveForward();
        }

        if (moveInput.y < 0)
        {
            MoveBackward();
        }

        if (moveInput.x > 0)
        {
            StrafeRight();
        }

        if (moveInput.x < 0)
        {
            StrafeLeft();
        }
    }

    public void ReturnToPreBattlePosition()
    {
        playerState = PlayerState.Explore;
        //print("go back " + beforeBattleTransformPos);
        EnCounter.Invoke(countdownToEncounter);
        gameObject.transform.position = new Vector3(currentWallBlock.GetLocation().x, beforeBattleTransformPos.y, currentWallBlock.GetLocation().z); ;

        float y = beforeBattleTransformRot.eulerAngles.y;
        if (beforeBattleTransformRot.eulerAngles.y % 90 != 0) y= CardinalDir.GetRotationYForCardinal(currentforwardDirection);


        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, y , 0));
    }

    public void MoveForward()
    {
        if (!currentWallBlock.IfWallOpened(currentforwardDirection)) { noWay.Invoke(); return; }
        if (!RaycastOnMovement(Vector3.forward)) return;
        //print(transform.rotation.eulerAngles);
        if (busyWalking) return;
        Vector3 v = CardinalDir.GetNewPoint(currentforwardDirection, currentposition, moveTilemap);
        if (!CheckBlockInterfaces(v)) return;
        stepSound.Invoke();
        StartCoroutine(SmoothWalk(currentforwardDirection, v));
    }

    public void MoveBackward()
    {
        if (!currentWallBlock.IfWallOpened(CardinalDir.GetOpposite(currentforwardDirection))) return;
        if (!RaycastOnMovement(Vector3.back)) return;
        if (busyWalking) return;
        var v = CardinalDir.GetNewPoint(CardinalDir.GetOpposite(currentforwardDirection), currentposition, moveTilemap);
        if (!CheckBlockInterfaces(v)) return;
        stepSound.Invoke();
        StartCoroutine(SmoothWalk(CardinalDir.GetOpposite(currentforwardDirection), v));
    }

    public void StrafeRight()
    {
        if (!currentWallBlock.IfWallOpened(CardinalDir.GetRightDir(currentforwardDirection))) return;
        if (!RaycastOnMovement(Vector3.right)) return;
        if (busyWalking) return;
        var v = CardinalDir.GetNewPoint(CardinalDir.GetRightDir(currentforwardDirection), currentposition, moveTilemap);
        if (!CheckBlockInterfaces(v)) return;
        stepSound.Invoke();
        StartCoroutine(SmoothWalk(CardinalDir.GetRightDir(currentforwardDirection), v));

    }

    public void StrafeLeft()
    {
        if (!currentWallBlock.IfWallOpened(CardinalDir.GetOpposite(CardinalDir.GetRightDir(currentforwardDirection)))) return;
        if (!RaycastOnMovement(Vector3.left)) return;
        if (busyWalking) return;
        var v = CardinalDir.GetNewPoint(CardinalDir.GetOpposite(CardinalDir.GetRightDir(currentforwardDirection)), currentposition, moveTilemap);
        if (!CheckBlockInterfaces(v)) return;
        stepSound.Invoke();
        StartCoroutine(SmoothWalk(CardinalDir.GetOpposite(CardinalDir.GetRightDir(currentforwardDirection)), v));
    }

    public void TurnRight()
    {
        if (playerState == PlayerState.Battle) { /*print("battle state");*/ return; }
        if (busyWalking) return;
        turnAround.Invoke();
        StartCoroutine(SmoothRotation(90));
    }
    public void TurnLeft()
    {
        if (playerState == PlayerState.Battle) { /*print("battle state");*/ return; }
        if (busyWalking) return;
        turnAround.Invoke();
        StartCoroutine(SmoothRotation(-90));
    }


    public Vector2 GetcurrentPosition()
    {
        return new Vector2(currentposition.x, currentposition.y);
    }

    public CardinalDirections GetCurrentDirection()
    {
        return currentforwardDirection;
    }

    IEnumerator SmoothWalk(CardinalDirections dir, Vector3 targetDestination)
    {
        float currentpoint = 0;
        float startX = transform.position.x, startZ = transform.position.z;

        busyWalking = true;
        while (currentpoint < 1 && playerState == PlayerState.Explore)
        {
            switch (dir)
            {
                case CardinalDirections.NORTH:
                    transform.position = new Vector3(transform.position.x, transform.position.y, startZ + blockSize * walkCurve.Evaluate(currentpoint));

                    break;
                case CardinalDirections.EAST:
                    transform.position = new Vector3(startX + blockSize * walkCurve.Evaluate(currentpoint), transform.position.y, transform.position.z);
                    break;
                case CardinalDirections.SOUTH:
                    transform.position = new Vector3(transform.position.x, transform.position.y, startZ - blockSize * walkCurve.Evaluate(currentpoint));
                    break;
                case CardinalDirections.WEST:
                    transform.position = new Vector3(startX - blockSize * walkCurve.Evaluate(currentpoint), transform.position.y, transform.position.z);

                    break;
            }
            yield return new WaitForSeconds(couroutingDelayinSec * Time.deltaTime);
            
            currentpoint += walkSpeed * Time.deltaTime;

        }

        transform.position = new Vector3(targetDestination.x, transform.position.y, targetDestination.z);
        currentposition = moveTilemap.WorldToCell(transform.position);
        currentWallBlock = wallsAccess[currentposition];
        currentGroundType = currentWallBlock.GetGroundType();
        busyWalking = false;
        if (_input.CrawlerStandart.Move.ReadValue<Vector2>() != Vector2.zero)
        {

            MovementUpdateFuther(_input.CrawlerStandart.Move.ReadValue<Vector2>());
        }
        if (_input.CrawlerStandart.Turn.ReadValue<float>() != 0)
        {
            TurnAroundFloat(_input.CrawlerStandart.Turn.ReadValue<float>());
        }

        foreach(OnBlockPlacement block in currentWallBlock.CheckForNeighbors(moveTilemap))
        {
            if (block == null) continue;
            block.ShowOnMap(true);
            if (!visitedBlocks.Contains(block.GetBlockCoordinate())) visitedBlocks.Add( block.GetBlockCoordinate());
        }
        if (!visitedBlocks.Contains(currentWallBlock.GetBlockCoordinate())) visitedBlocks.Add(currentWallBlock.GetBlockCoordinate()); 
        currentWallBlock.ShowOnMap(true);
    }


    IEnumerator SmoothRotation(float angle)
    {
        float currentpoint = 0;
        float startY = transform.rotation.eulerAngles.y;
        busyWalking = true;
        if (Mathf.Abs(angle) == 90)
        {
            if (angle > 0) currentforwardDirection = CardinalDir.SetDirectionRight(currentforwardDirection);
            else currentforwardDirection = CardinalDir.GetOpposite(CardinalDir.SetDirectionRight(currentforwardDirection));
        }
        cardinalDirectionToUI.Invoke(currentforwardDirection);
        while (currentpoint < 1 && playerState == PlayerState.Explore)
        {
            transform.rotation = Quaternion.Euler(0, startY + angle * rotationCurve.Evaluate(currentpoint), 0);
            yield return new WaitForSeconds(couroutingDelayinSec * Time.deltaTime);
            currentpoint += rotationSpeed * Time.deltaTime;
        }



        transform.rotation = Quaternion.Euler(0, startY + angle, 0);


        busyWalking = false;
        if (_input.CrawlerStandart.Turn.ReadValue<float>() != 0)
        {
            TurnAroundFloat(_input.CrawlerStandart.Turn.ReadValue<float>());
        }
        if (_input.CrawlerStandart.Move.ReadValue<Vector2>() != Vector2.zero)
        {
            MovementUpdateFuther(_input.CrawlerStandart.Move.ReadValue<Vector2>());
        }
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

    void RotateSmoothToCardinal(CardinalDirections cardinalTarget)
    {
        RotateToCardinalLocation();
        float y = CardinalDir.GetRotationYForCardinal(cardinalTarget) - transform.rotation.eulerAngles.y;
        StartCoroutine(SmoothRotation(y));
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
            //print("cursor busy "+ cursorBusy+ " cursorHoveringUI "+ cursorHoveringUI);
            ThrowToTheWorld(throwItemPosition, currentMouse.position.ReadValue().y);

            // print("mouse throw "+currentMouse.position.ReadValue().y);

            cursorBusy = false;
            cursorItemScriptable = null;
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        }

        if (Physics.Raycast(ray, out hit))
        {

            if (hit.distance > blockSize) return;
            IInteractables inter = hit.collider.GetComponent<IInteractables>();

            if (inter != null)
            {
               // print(hit.distance + " " + hit.collider + " " + inter);
                List<InteractablesEnum> list = inter.WhatIsIt();
                foreach (InteractablesEnum i in list)
                {
                    switch (i)
                    {
                        case InteractablesEnum.ENEMY:  //get collader off on bigger enemy container?
                            //print("enemy");
/*                            IEnemy iEnemy = hit.collider.GetComponent<IEnemy>();
                            print(iEnemy.GetEnemyName());
                            if (GameInstance.party.spellReady)
                            {
                                GameInstance.party.ManaCost();
                                GameInstance.battleManager.ChooseTargetForSpell(iEnemy.GetEnemyObject(), GameInstance.party.GetPreparedSpellOfHero());
                                Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);

                            }

                            *//*
                           if(playerState == PlayerState.BATTLE)
                           {
                               IEnemy iEnemy = hit.collider.GetComponent<IEnemy>();
                               iEnemy.Damage(20);
                               return;
                           }*/
                            break;
                        case InteractablesEnum.DOOR:

                            //IDoor door = hit.collider.GetComponent<IDoor>(); // Highlight?

                            break;
                        case InteractablesEnum.SWITCH:
                            //ISwitch switchThing = hit.collider.GetComponent<ISwitch>();
                            //switchThing.ToggleSwitch();

                            break;
                        case InteractablesEnum.PICKABLE:
                            IItem iItem = hit.collider.GetComponent<IItem>();
                            cursorItemScriptable = iItem.WhatItem();
                            SetPlayerCursorBusy(cursorItemScriptable, iItem.itemsAmount());
                            iItem.RemoveFromTheWorld();
                            //print(" cursor busy on picking " + cursorBusy + " item " + cursorItemScriptable.itemName);
                            break;
                    }
                }
            }
        }
    }
    public void ThrowToTheWorld(Transform spawnPoint, float screenPosition)
    {
        GameObject item = Instantiate(itemModelPrefab, spawnPoint);

        IItem iItem = item.GetComponent<IItem>();
        iItem.SetPrefab(cursorItemScriptable);
        iItem.InitializeItem();
        iItem.SetItemsAmount(stackAmountCursor);

        //iItem.SetTransformPosition(Vector3.zero);
        iItem.RemoveFromParent();


        if (screenPosition > 600) //throw
        {
            //print("throw");
/*            SplineAnimate anim = item.GetComponent<SplineAnimate>();//item.AddComponent<SplineAnimate>();
            //anim.Loop = SplineAnimate.LoopMode.Once;

            anim.Container = throwSplines[0];
            anim.Duration = 0.8f;
            anim.Play();
            playerState = PlayerState.BATTLE;
            anim.Completed += Completeanim;
            item.GetComponent<ItemModel>().AnimComplete.AddListener(Completeanim);*/
        }
    }
    void Completeanim()
    {
        playerState = PlayerState.Explore;
    }

    public ItemSlotStruct GetItemFromCursor()
    {
        ItemScriptableContainer tempItem = cursorItemScriptable;
        cursorBusy = false;
        cursorItemScriptable = null;
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        ItemSlotStruct structNew = new ItemSlotStruct();
        structNew.item = tempItem;
        structNew.stackAmount = stackAmountCursor;
        stackAmountCursor = 0;
        return structNew;
    }

    public void SetPlayerCursorBusy(ItemScriptableContainer tempItem, int stackAmount)
    {
        cursorItemScriptable = tempItem;
        stackAmountCursor = stackAmount;
        //print("stack " + stackAmount);
        Cursor.SetCursor(cursorItemScriptable.texture2DMouse, hotSpot, cursorMode);
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

                        case InteractablesEnum.DOOR:
                            IDoor door = hit.collider.GetComponent<IDoor>();
                            print("Door is " + door.isOpen());
                            return door.isOpen();

                        case InteractablesEnum.SWITCH:
                            break;
                        case InteractablesEnum.LEVEL_EXIT:
                            GameInstance.playerController = null;
                            //GameInstance.inventory = null;
                            _input.Disable();
                            SceneManager.LoadScene("Level02", LoadSceneMode.Single);
                            break;
                        case InteractablesEnum.TRAP:
                            break;
                        case InteractablesEnum.STORY:
                            break;
                        case InteractablesEnum.WALL:
                            return false;
                    }
                }
            }

        }
        return true;
    }


    bool CheckBlockInterfaces(Vector3 v)
    {
        IInteractables iblock = wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IInteractables>();
        List<InteractablesEnum> interactableList = iblock.WhatIsIt();

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

                    //Autosave, read location and rotation of destination from IBlock save it to gameinstance 
                    // check for level exit interface, save tranfer point on another level to save file  load target level
                    break;
                case InteractablesEnum.PORTAL:
                    OnBlockPlacement portalDest =  wallsAccess[moveTilemap.WorldToCell(v)].GetComponent<IBlock>().GetPortalPoint();
                    transform.position = portalDest.gameObject.transform.position;
                    currentposition = portalDest.GetBlockCoordinate(); return false;

                case InteractablesEnum.LADDER:
                    // move player to another level of a tilemap
                    break;
                case InteractablesEnum.TRAP:
                    // get trap interface 
                    break;
                case InteractablesEnum.STORY:
                    // get story interface getting a text of a message, delete story from interactablesEnum list
                    break;
                case InteractablesEnum.WALL:
                    return false;

            }
        }
        return true;
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

    public void ExitHover()
    {
        cursorHoveringUI = false;
        hoverPortraitIndex = -1;
        //print("hover off  " + cursorHoveringUI + " cursor busy " + cursorBusy);
    }

    IEnumerator LightFlickering()
    {
        lightBusy = true;
        //Vector3 pos = torchlight.gameObject.transform.position;
        for (int i = 0; i < 10; i++)
        {
            torchlight.intensity = intensivity + Random.Range(0.1f, 0.2f);
            torchlight.colorTemperature = Random.Range(0.1f, 0.5f);
            torchlight.range = Random.Range(9, 10);
            torchlight.gameObject.transform.position = transform.position + new Vector3(Random.Range(0.1f, 0.6f), Random.Range(0.1f, 0.6f), Random.Range(0.1f, 0.6f));
            yield return new WaitForSeconds(0.2f);
        }
        torchlight.gameObject.transform.position =  transform.position;
        lightBusy = false;
    }

    void TimeEvents(int count)
    {
        if (currentGroundType == GroundType.Water)
        {
            if (!waterWalk)
            {
                foreach(Hero h in GameInstance.party.GetPartyMembers())
                {
                    h.healthDecrease((int)(h.GetDependedStat(DependedStat.maxHealth)*0.3f));
                }
            }
        }

        if (currentGroundType == GroundType.Fire)
        {
            if (!lavaWalk)
            {
                foreach(Hero h in GameInstance.party.GetPartyMembers())
                {
                    h.healthDecrease((int)(h.GetDependedStat(DependedStat.maxHealth)*0.3f));
                }
            }
        }
    }


}


public interface IPlayerInterface
{
    public void SetPlayerState(PlayerState state);

}




public enum PlayerState
{
    Explore,
    Battle
}