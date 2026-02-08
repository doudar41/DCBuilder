using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using TMPro;


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

    SphereCollider col;

    public Vector3 itemPosition;

    public bool stackable = false;

    public UnityEvent<int, GameObject> OnDestoryedByCursor;
    public UnityEvent AnimComplete;

    [SerializeField] PhysicMaterial frictionMaterial;
    [SerializeField] TextMeshPro itemNameInEditor;
    [SerializeField] SpriteRenderer itemIconInEditor;

    public void OnValidate()
    {
        if (GUIDString == "")
        {
            _guid = System.Guid.NewGuid();
            GUIDString = _guid.ToString();
        }

        if (itemScriptableLocal != null) 
        { 
            itemNameInEditor.text = itemScriptableLocal.itemName;
            itemIconInEditor.sprite = itemScriptableLocal.InventorySprite;
            if(!itemScriptableLocal.stackable) stackAmount = 1;
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
        GameInstance.checkWeight += CheckWeight;
        if (itemScriptableLocal != null)
        {
            itemNameInEditor.gameObject.SetActive(false);
            itemIconInEditor.gameObject.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        OnDestoryedByCursor.RemoveAllListeners();
        GameInstance.initItems -= LevelEnterInit;
        GameInstance.checkWeight -= CheckWeight;
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
            return;
        }
        currentLevel = GameInstance.GetLevelName();
        GameObject item = Instantiate(itemScriptableLocal.prefab, transform);
        IItemHolder itemHolder = itemScriptableLocal.prefab.GetComponent<IItemHolder>();
        SphereCollider[] b = gameObject.GetComponents<SphereCollider>();
        if (b.Length < 1)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 1f;

        }
        else
        {
            col = b[0];
        }

        //Check if block has a weightPlate

    }

    private void CheckWeight()
    {
        if (GameInstance.playerController.GetIIteractableInterfaces(transform.position) != null)
        {
            foreach (InteractablesEnum inter in GameInstance.playerController.GetIIteractableInterfaces(transform.position))
            {
                if (inter == InteractablesEnum.WEIGHTPLATE)
                {
                    print("weight plate");
                    if (GameInstance.playerController.GetBlockInterface(transform.position) != null)
                    {
                        GameInstance.playerController.GetBlockInterface(transform.position).AddWeightToBlock(itemScriptableLocal.weight);
                    }
                }
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        //CheckWeight();

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
        foreach (InteractablesEnum inter in GameInstance.playerController.GetIIteractableInterfaces(transform.position))
        {
            if (inter == InteractablesEnum.WEIGHTPLATE)
            {
                if (GameInstance.playerController.GetBlockInterface(transform.position) != null)
                {
                    GameInstance.playerController.GetBlockInterface(transform.position).AddWeightToBlock(-itemScriptableLocal.weight);
                }
            }
        }

        GameInstance.RemoveItemFromLevel(GUIDString, heroInventoryLocalItem);
        Destroy(gameObject);
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

        }
        transform.position = pos;
        heroInventoryLocalItem = CreateNewHeroInventoryItem();

        GameInstance.AddItemFromLevel(GUIDString, heroInventoryLocalItem);
        if (GameInstance.playerController.GetIIteractableInterfaces(transform.position) != null)
        {
            foreach (InteractablesEnum inter in GameInstance.playerController.GetIIteractableInterfaces(transform.position))
            {
                if (inter == InteractablesEnum.WEIGHTPLATE)
                {
                    print("weight plate");
                    if (GameInstance.playerController.GetBlockInterface(transform.position) != null)
                    {
                        GameInstance.playerController.GetBlockInterface(transform.position).AddWeightToBlock(itemScriptableLocal.weight);
                    }
                }
            }
        }
        if (itemScriptableLocal != null)
        {
            itemNameInEditor.gameObject.SetActive(false);
            itemIconInEditor.gameObject.SetActive(false);
        }
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
    Key,
    LEARNINGSCROLL

}

public enum WeaponType
{
    None,
    Blades,
    Polyarm,
    Blunt,
    Range
}