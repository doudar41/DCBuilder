using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraSmoothFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothSpeed = 1f;
    [SerializeField] Vector3 offset = Vector3.zero;
    float lastpositionX, lastpositionZ;

    private void FixedUpdate()
    {
        if (target != null)
        {
            if(lastpositionX != target.position.x || lastpositionZ != target.position.z)
            {

                transform.position = new Vector3(lastpositionX, transform.position.y, lastpositionZ) + offset;
                lastpositionX = target.position.x;
                lastpositionZ = target.position.z;

            }
            Vector3 desiredPosition = new Vector3(target.position.x, transform.position.y, target.position.z) + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            
            transform.position = smoothedPosition;


        }
    }

  

    public void SetCameraSpeed(float speed)
    {
        smoothSpeed = speed;
    }
}
