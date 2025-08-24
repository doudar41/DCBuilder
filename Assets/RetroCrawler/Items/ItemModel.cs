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
    [SerializeField] string itemName;
    [SerializeField] int stackAmount = 1;

    [SerializeField]
    ItemScriptableContainer itemScriptableLocal;

    SplineAnimate anim;
    SphereCollider col;
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
            _guid = System.Guid.NewGuid();
            GUIDString = _guid.ToString();
        }
    }

    private void Awake()

    {


    }

    private void Start()
    {
        Init();


        //print ("guid "+ _guid);
       // Complete += EmptyMethod;
    }


    
    void Init()
    {
        if (GameInstance.savedItemsState.ContainsKey(GUIDString)) 
        {
           if( GameInstance.savedItemsState[GUIDString] == SavedState.Taken) return; 
        }

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
        GameInstance.SaveItemState(GUIDString, SavedState.Taken);
        OnDestoryedByCursor.Invoke(itemScriptableLocal.weight*stackAmount, this.gameObject);
        DestroyImmediate(gameObject);
    }

    public List<InteractablesEnum> WhatIsIt()
    {
        List<InteractablesEnum> interactablesEnums = new List<InteractablesEnum>();
        interactablesEnums.Add(InteractablesEnum.PICKABLE);
        return interactablesEnums;
    }

    public ItemScriptableContainer WhatItem()
    {
        return itemScriptableLocal;
    }

    public Texture2D GetCursorTexture()
    {
        return itemScriptableLocal.texture2DMouse;
    }

    public void InitializeItem()
    {
        IItemHolder itemHolder = itemScriptableLocal.prefab.GetComponent<IItemHolder>();
        SphereCollider[] b = gameObject.GetComponents<SphereCollider>();
        if (b.Length < 1)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 1f;
            Rigidbody r = gameObject.AddComponent<Rigidbody>();
            r.drag = 1;
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

    private void OnDestroy()
    {
        OnDestoryedByCursor.RemoveAllListeners();
        //Complete -= EmptyMethod;
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
}
