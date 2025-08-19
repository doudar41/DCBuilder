using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Transform player;
    [SerializeField]float y, clampLow, clampHigh, snapValue;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameInstance.playerController.playerState != PlayerState.Battle)
        {
            transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);
            cam.orthographicSize = y; 
        }
    }

    public void ChangeHeight(bool higher)
    {
        if (higher) y = Mathf.Clamp(y + snapValue, clampLow, clampHigh);
        if (!higher) y = Mathf.Clamp(y - snapValue, clampLow, clampHigh);
    }
}
