using UnityEngine;
using UnityEngine.Tilemaps;
#if (UNITY_EDITOR) 
using UnityEditor.Tilemaps;

// This is game object brush and it should have a attached gameobject with OnBlockPlacement script and 4 walls 

[CreateAssetMenu(fileName = "BlockPrefab", menuName = "Brushes/Block Prefab")]
[CustomGridBrush(false, true, false, "Block Prefab")]
public class BlockBrush :   GameObjectBrush
{
    public Vector3 overrideScale; //Add offset to scale
    public Vector3 overridePosition; //Add offset to position


public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
{
    Grid grid = gridLayout.GetComponent<Grid>();

    for (int i = 0; i < cells.Length; i++) //If brush is fills few cells, not this case but I left it in case I have an idea 
    {
        Debug.Log(brushTarget.gameObject.GetComponentsInChildren<OnBlockPlacement>().Length);
        OnBlockPlacement[] blocks = brushTarget.gameObject.GetComponentsInChildren<OnBlockPlacement>();
        foreach (OnBlockPlacement b in blocks)
        {
            if (grid.WorldToCell( b.gameObject.transform.position) == position)
            {
                return;
            }
        }
        GameObject block = Instantiate(cells[i].gameObject, grid.GetCellCenterWorld(position), Quaternion.identity);
        OnBlockPlacement blockWalls = block.GetComponent<OnBlockPlacement>(); 
        block.transform.SetParent(brushTarget.transform); // Sets tilemap as a parent
        block.transform.localScale += new Vector3(overrideScale.x, overrideScale.z, overrideScale.y); 
        //This is used to adjust size of prefab to cell size depend on your prefab size
        block.transform.position += overridePosition; // Mostly for adjusting height

        if (blockWalls != null)
        {
            foreach (GameObject g in blockWalls.walls)
            {
                g.SetActive(true); // Set walls active to launch this functionality otherwise it will be not accessible
            }
            //Debug.Log("tilemap name" + brushTarget.name);
            var tilemapOfBlocks = brushTarget.GetComponent<Tilemap>();
            blockWalls.position = position;
            blockWalls.CoordinatesToText(); // In editor blocks shows coordinates
            blockWalls.CheckGridForGameObject(tilemapOfBlocks, position); // OnBlockPlacement script checks for neighbor tiles.
                                                                          // If it finds other block on neighbor tile it deactivates 
                                                                          // walls between them. If dev needs wall between tiles it need to be 
                                                                          // made manually. 

        }
    }
}

public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
{
    Grid grid = gridLayout.transform.parent.GetComponent<Grid>();
    Vector3 positionInWorld = grid.GetCellCenterWorld(position.position);
    var blocks = brushTarget.GetComponentsInChildren<Transform>();

    foreach (Transform p in blocks)
    {
        if (p.position == new Vector3(positionInWorld.x, p.position.y, positionInWorld.z))
        {
            DestroyImmediate(p.gameObject);
            return;
        }
    }
}
}
#endif

