
using UnityEngine;

public interface IItem 
{
    public void RemoveFromTheWorld();

    public ItemScriptableContainer WhatItem();
    public void InitializeItem();
    public void SetPrefab(ItemScriptableContainer itemScriptable);
    public void SetTransformPosition(Vector3 pos);
    public void RemoveFromParent();

    public int itemsAmount();
    public void SetItemsAmount(int amount);
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