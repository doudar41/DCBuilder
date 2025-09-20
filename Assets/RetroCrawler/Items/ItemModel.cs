using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Splines;


[RequireComponent(typeof(SplineAnimate))]

public class ItemModel : MonoBehaviour, IItem, IInteractables, IPointerClickHandler
{

    System.Guid _guid;
    [SerializeField] string GUIDString = "";
    string currentLevel;
    [SerializeField] string itemName;
    [SerializeField] int stackAmount = 1;

    [SerializeField]
    ItemScriptableContainer itemScriptableLocal;

    HeroInventoryItem heroInventoryLocalItem;

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
        //GameInstance.itemsFound.Add(GUIDString);
        GameInstance.initItems += LevelEnterInit;
    }
    private void OnDestroy()
    {
        OnDestoryedByCursor.RemoveAllListeners();
        GameInstance.initItems -= LevelEnterInit;
    }

    public void ChangeGUID()
    {
        _guid = System.Guid.NewGuid();
        GUIDString = _guid.ToString();
    }

    void LevelEnterInit()
    {
        if (itemScriptableLocal == null) return;

        if (!GameInstance.levelsVisited.Contains(GameInstance.GetLevelName()))
        {
            heroInventoryLocalItem = CreateNewHeroInventoryItem();
            GameInstance.AddItemFromLevel(GUIDString, heroInventoryLocalItem);
        }
        else
        {
            return ;
        }
        currentLevel = GameInstance.GetLevelName();
        GameObject item = Instantiate(itemScriptableLocal.prefab, transform);
        IItemHolder itemHolder = itemScriptableLocal.prefab.GetComponent<IItemHolder>();
        SphereCollider[] b = gameObject.GetComponents<SphereCollider>();
        if (b.Length <1)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 1f;
/*            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            col.material = frictionMaterial;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePositionX;
            rb.constraints = RigidbodyConstraints.FreezePositionZ;*/
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

    public HeroInventoryItem CreateNewHeroInventoryItem()
    {
        HeroInventoryItem createdItem = new HeroInventoryItem();
        createdItem.positionReplaced = transform.position;
        createdItem.heroIndex =-1;
        createdItem.stackAmount = stackAmount;
        createdItem.itemType = itemScriptableLocal.itemType;
        createdItem.container = GameInstance.dataBase.GetItemIndexFromDataBase(itemScriptableLocal);
        createdItem.level = GameInstance.GetLevelName();

        return createdItem;
    }


    public void RemoveFromTheWorld()
    {

        GameInstance.RemoveItemFromLevel(GUIDString, heroInventoryLocalItem);
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

        return heroInventoryLocalItem;
    }

    public Texture2D GetCursorTexture()
    {
        return itemScriptableLocal.texture2DMouse;
    }

    public void PlaceCreatedItem(Vector3 pos)
    {
        currentLevel = GameInstance.GetLevelName();
        GameObject item = Instantiate(itemScriptableLocal.prefab, transform);
        IItemHolder itemHolder = itemScriptableLocal.prefab.GetComponent<IItemHolder>();
        SphereCollider[] b = gameObject.GetComponents<SphereCollider>();
        if (b.Length < 1)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 1f;
/*            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.drag = 1;
            col.material = frictionMaterial;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePositionX;
            rb.constraints = RigidbodyConstraints.FreezePositionZ;*/
            //rb.useGravity = false;
        }
        transform.position = pos;
        heroInventoryLocalItem = CreateNewHeroInventoryItem();
        GameInstance.AddItemFromLevel(GUIDString, heroInventoryLocalItem);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        GameInstance.playerController.GetInterfaceFromItem(gameObject);
    }
}




public interface IItem
{
    public void RemoveFromTheWorld();

    public HeroInventoryItem WhatItem();
    public void PlaceCreatedItem(Vector3 pos);
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

[System.Serializable]
public enum ItemType
{
    WEAPON,
    AMMUNITION,
    TORSO_ARMOR,
    HELM,
    GLOVES,
    AMULET,
    BOOT,
    BELT,
    SHIELD,
    RING,
    RING2,
    RING3,
    RING4,
    RING5,
    RING6,
    CONSUMABLE,
    QUEST,
    LOOT,
    Upgrades,
    Key

}

public enum WeaponType
{
    None,
    Blades,
    Polyarm,
    Blunt,
    Range
}