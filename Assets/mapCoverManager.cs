
using UnityEngine;
using UnityEngine.Tilemaps;
public class mapCoverManager : MonoBehaviour
{
    [SerializeField] Tilemap mapTileMap;
    [SerializeField] TileBase[] tileBases;
    [SerializeField] TileBase blackTile;
    [SerializeField] TileBase whiteTile;
    private void Awake()
    {
        GameInstance.mapTileMap = this;
    }
    public void SetMapCover(bool isCovered)
    {
        if (isCovered)
        {
            mapTileMap.color = new Color(0, 0, 0, 1);
        }
        else
        {
            mapTileMap.color = new Color(0, 0, 0, 0);
        }
    }

    public bool SetTileDiscovered(Vector3Int tilePosition)
    {
        Vector3Int[] neighbors = new Vector3Int[]
        {            
            new Vector3Int(tilePosition.x, tilePosition.y + 1, tilePosition.z),
            new Vector3Int(tilePosition.x + 1, tilePosition.y, tilePosition.z),
            new Vector3Int(tilePosition.x, tilePosition.y - 1, tilePosition.z),
            new Vector3Int(tilePosition.x - 1, tilePosition.y, tilePosition.z),


            new Vector3Int(tilePosition.x + 1, tilePosition.y + 1, tilePosition.z),           
            new Vector3Int(tilePosition.x + 1, tilePosition.y - 1, tilePosition.z),
            new Vector3Int(tilePosition.x - 1, tilePosition.y - 1, tilePosition.z),
            new Vector3Int(tilePosition.x - 1, tilePosition.y + 1, tilePosition.z)

        };


        if (mapTileMap.GetTile(tilePosition) == null )
        {
            //print("Tile Discovered is null: " + tilePosition);
        }
        //print("Tile Discovered at: " + tilePosition);
        TileBase tilebase = mapTileMap.GetTile(tilePosition);
       mapTileMap.SetTile(tilePosition, null);

        for (int i = 0; i < neighbors.Length; i++)
        {
            if (mapTileMap.GetTile(neighbors[i]) != null )
            {

                mapTileMap.SetTile(neighbors[i], null);
            }
            else
            {
                mapTileMap.SetTile(neighbors[i], whiteTile);
            }
        }
        return true;
    }

    public void OpenArea(Vector3Int leftTopCorner, Vector3Int rightBottomCorner)
    {
        for (int x = leftTopCorner.x; x <= rightBottomCorner.x; x++)
        {
            for (int y = rightBottomCorner.y; y <= leftTopCorner.y; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                print("Opening tile at: " + tilePosition);
                mapTileMap.SetTile(tilePosition, null);
                
            }
        }
    }


}
