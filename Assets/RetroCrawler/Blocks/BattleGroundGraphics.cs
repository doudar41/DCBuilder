using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGroundGraphics : MonoBehaviour
{
    [SerializeField] List<MeshRenderer> wallMeshes = new List<MeshRenderer>();
    [SerializeField] List<MeshRenderer> groundMeshes = new List<MeshRenderer>();
    [SerializeField] List<MeshRenderer> ceilingMeshes = new List<MeshRenderer>();
    [SerializeField] List<Material> wallMaterials = new List<Material>();
    [SerializeField] List<Material> groundMaterials = new List<Material>();
    [SerializeField] List<Material> ceilingMaterials = new List<Material>();


    public void SetBattleGround(BattleGroundEnvironment battleGroundEnvironment)
    {
        print("set walls to " + (int)battleGroundEnvironment);

        foreach (MeshRenderer mr in wallMeshes)
        {
            mr.material = wallMaterials[(int)battleGroundEnvironment];
        }
        foreach (MeshRenderer mr in groundMeshes)
        {
            mr.material = groundMaterials[(int)battleGroundEnvironment];
        }
        foreach (MeshRenderer mr in ceilingMeshes)
        {
            mr.material = ceilingMaterials[(int)battleGroundEnvironment];
        }


    }



}


public enum BattleGroundEnvironment
{
    WOOD,
    HOUSE,
    STONE
}