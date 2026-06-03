using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapCamera : MonoBehaviour, IDragHandler
{
    [SerializeField] Camera cam;
    [SerializeField] float y, clampLow, clampHigh, snapValue;
    [SerializeField] bool followPlayer = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        cam.orthographicSize = y;
        if (followPlayer) CenteredOnPlayer();
    }

    public void CenteredOnPlayer()
    {
        if (GameInstance.playerController == null) return;
        if (GameInstance.playerController.playerState != PlayerState.Battle)
        {
            cam.transform.position = new Vector3(Camera.main.transform.position.x, cam.transform.position.y, Camera.main.transform.position.z);

        }
    }

    public void ChangeHeight(bool higher)
    {
        if (higher) y = Mathf.Clamp(y + snapValue, clampLow, clampHigh);
        if (!higher) y = Mathf.Clamp(y - snapValue, clampLow, clampHigh);
    }

    public void OnDrag(PointerEventData eventData)
    {

        Vector3 delta = new Vector3(eventData.delta.x, 0, eventData.delta.y);
        cam.transform.position -= delta * (y / 1000f);
    }
}
