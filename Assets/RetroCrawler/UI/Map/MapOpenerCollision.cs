
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MapOpenerCollision : MonoBehaviour
{
    [SerializeField] Vector3Int leftTopCorner, rightBottomCorner;
    BoxCollider boxCollider;


    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("Player entered MapOpenerCollision");
            GameInstance.mapTileMap.OpenArea(leftTopCorner, rightBottomCorner);
            boxCollider.enabled = false;
        }
    }


}
