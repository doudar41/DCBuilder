using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Splines;


[RequireComponent(typeof(SplineAnimate))]

public class ItemModel : MonoBehaviour, IItem, IInteractables
{

    System.Guid _guid;
    [SerializeField] string GUIDString = "";
    string currentLevel;
    [SerializeField] string itemName;
    [SerializeField] int stackAmount = 1;

    [SerializeField]
    ItemScriptableContainer itemScriptableLocal;

    SplineAnimate anim;
    SphereCollider col;

    public Vector3 itemPosition;

    public bool stackable = true;

    public UnityEvent<int, GameObject> OnDestoryedByCursor;
    public UnityEvent AnimComplete;

    [SerializeField] PhysicMaterial frictionMaterial;

    public void OnValidate()
    {
        if (GUIDString == "")
        {
            _guid = System.Guid.NewGuid();
            GUIDString = _guid.ToString();
        }
    }

    private void OnEnable()
    {
        if (GUIDString == "")
        {
            ChangeGUID();
        }
    }

    private void Awake()
    {
        GameInstance.itemsFound.Add(GUIDString);
        GameInstance.initItems += Init;
    }
    private void OnDestroy()
    {
        OnDestoryedByCursor.RemoveAllListeners();
        GameInstance.initItems -= Init;
    }

    public void ChangeGUID()
    {
        _guid = System.Guid.NewGuid();
        GUIDString = _guid.ToString();
    }

    void Init()
    {
        if (itemScriptableLocal == null) return;
        if (GameInstance.levelChange || GameInstance.loadingLevel)
        {
            if (GameInstance.savedItemsState.ContainsKey(GUIDString))
            {
                if (GameInstance.savedItemsState[GUIDString] == SavedState.Replaced)
                {
                    if (GameInstance.CheckItemLevelInReplaced(GUIDString)) 
                    { 
                        transform.position = GameInstance.savedItemsReplaced[GUIDString].positionReplaced; 
                    }
                    else
                    {
                        return;
                    }
                }
                if (GameInstance.savedItemsState[GUIDString] == SavedState.Equipment || GameInstance.savedItemsState[GUIDString] == SavedState.Inventory)
                {
                    return;
                }
                if (GameInstance.savedItemsState[GUIDString] == SavedState.Cursor) return;
            }
        }

        currentLevel = GameInstance.GetLevelName();
        GameObject item = Instantiate(itemScriptableLocal.prefab, transform);
        IItemHolder itemHolder = itemScriptableLocal.prefab.GetComponent<IItemHolder>();
        SphereCollider[] b = gameObject.GetComponents<SphereCollider>();
        if (b.Length <1)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 1f;
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            col.material = frictionMaterial;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePositionX;
            rb.constraints = RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            col = b[0];
        }
    }



    private void OnCollisionEnter(Collision other)
    {
        SplineAnimate anim = GetComponent<SplineAnimate>();
        if (anim == null) return;
        anim.Pause();
        AnimComplete.Invoke();
        AnimComplete.RemoveAllListeners();
    }

    public void RemoveFromTheWorld()
    {
        HeroInventoryItem takenItem = new HeroInventoryItem();
        takenItem.positionReplaced = Vector3.zero;
        takenItem.savedState = SavedState.Cursor; //inventoryEquipment ?   SavedState.Equipment: SavedState.Inventory;
        takenItem._GUID = GUIDString;
        takenItem.heroIndex = GameInstance.party.activeHero.GetHeroIndex() ;
        takenItem.stackAmount = stackAmount;
        takenItem.itemType = itemScriptableLocal.itemType;
        takenItem.container = itemScriptableLocal;
        takenItem.level = GameInstance.GetLevelName();
        
        GameInstance.SaveItemState(GUIDString, SavedState.Cursor, takenItem);
        OnDestoryedByCursor.Invoke(itemScriptableLocal.weight*stackAmount, this.gameObject);
        DestroyImmediate(gameObject);
    }

    



    public List<InteractablesEnum> WhatIsIt()
    {
        List<InteractablesEnum> interactablesEnums = new List<InteractablesEnum>();
        interactablesEnums.Add(InteractablesEnum.PICKABLE);
        return interactablesEnums;
    }

    public HeroInventoryItem WhatItem()
    {
        HeroInventoryItem takenItem = new HeroInventoryItem();
        takenItem.positionReplaced = Vector3.zero;
        takenItem.savedState = SavedState.Cursor;
        takenItem._GUID = GUIDString;
        takenItem.heroIndex = GameInstance.party.activeHero.GetHeroIndex();
        takenItem.stackAmount = stackAmount;
        takenItem.itemType = itemScriptableLocal.itemType;
        takenItem.container = itemScriptableLocal;
        takenItem.level = GameInstance.GetLevelName();

        return takenItem;
    }

    public Texture2D GetCursorTexture()
    {
        return itemScriptableLocal.texture2DMouse;
    }

    public void InitializeItem(Vector3 pos)
    {
        currentLevel = GameInstance.GetLevelName();
        GameObject item = Instantiate(itemScriptableLocal.prefab, transform);
        IItemHolder itemHolder = itemScriptableLocal.prefab.GetComponent<IItemHolder>();
        SphereCollider[] b = gameObject.GetComponents<SphereCollider>();
        if (b.Length < 1)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 1f;
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.drag = 1;
            col.material = frictionMaterial;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePositionX;
            rb.constraints = RigidbodyConstraints.FreezePositionZ;
            //rb.useGravity = false;
        }
        transform.position = pos;
    }

    public void SetPrefab(ItemScriptableContainer itemScriptable)
    {
        itemScriptableLocal = itemScriptable;
    }

    public void SetTransformPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void RemoveFromParent()
    {
        gameObject.transform.parent = null;
    }



    public int itemsAmount()
    {
        return stackAmount;
    }

    public void SetItemsAmount(int amount)
    {
        stackAmount = amount;
    }

    public int GetWeight(out int capacity)
    {
        capacity = 0;
        return itemScriptableLocal.weight*stackAmount;
    }

    public string GetGUID()
    {
        return GUIDString;
    }

    public void SetGUID(string _GUID)
    {
        GUIDString = _GUID;

    }
}




public interface IItem
{
    public void RemoveFromTheWorld();

    public HeroInventoryItem WhatItem();
    public void InitializeItem(Vector3 pos);
    public void SetPrefab(ItemScriptableContainer itemScriptable);
    public void SetTransformPosition(Vector3 pos);
    public void RemoveFromParent();

    public int itemsAmount();
    public void SetItemsAmount(int amount);
    public void ChangeGUID();
    public string GetGUID();
    public void SetGUID(string _GUID);
}


public interface IItemHolder
{
    public MeshFilter GetMeshFilter();
    public MeshRenderer GetMeshRenderer();
    public Vector3 GetMeshSizeBounds();

}




public enum WeaponType
{
    None,
    Blades,
    Polyarm,
    Blunt,
    Range
}