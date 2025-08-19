using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolder : MonoBehaviour, IItemHolder
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;


    public MeshFilter GetMeshFilter()
    {
        return meshFilter;
    }

    public MeshRenderer GetMeshRenderer()
    {
        return meshRenderer;
    }

    public Vector3 GetMeshSizeBounds()
    {
        return meshFilter.sharedMesh.bounds.size;
    }

}
