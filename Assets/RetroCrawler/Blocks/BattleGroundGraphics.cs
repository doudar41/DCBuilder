using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGroundGraphics : MonoBehaviour
{
    [SerializeField] List<SpriteRenderer> wallMeshes = new List<SpriteRenderer>();
    [SerializeField] List<SpriteRenderer> groundMeshes = new List<SpriteRenderer>();
    [SerializeField] List<MeshRenderer> ceilingMeshes = new List<MeshRenderer>();
    [SerializeField] List<Sprite> wallMaterials = new List<Sprite>();
    [SerializeField] List<Sprite> groundMaterials = new List<Sprite>();
    [SerializeField] List<Material> ceilingMaterials = new List<Material>();


    public void SetBattleGround(BattleGroundEnvironment battleGroundEnvironment)
    {
        print("set walls to " + (int)battleGroundEnvironment);

        foreach (SpriteRenderer mr in wallMeshes)
        {
            mr.sprite = wallMaterials[(int)battleGroundEnvironment];
        }
        foreach (SpriteRenderer mr in groundMeshes)
        {
            mr.sprite = groundMaterials[(int)battleGroundEnvironment];
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
    STONE,
    CAVE,
    CITY,
    TEMPLE,
    TOMB,
    CASTLE,
    ICECAVE,
    NONE
}