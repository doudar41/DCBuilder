using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraSmoothFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothSpeedDefault = 1f;
    [SerializeField] Vector3 offset = Vector3.zero;
    float lastpositionX, lastpositionZ;
    [SerializeField]float smoothSpeed;
    private void Awake()
    {
        smoothSpeed = 50;
    }

    private void Start()
    {
        transform.position = target.position + offset;
    }

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

    public void SetCameraSpeedDefault(float speed)
    {
        smoothSpeedDefault = speed;
    }

    public void ResetCameraSpeed()
    {
        smoothSpeed = smoothSpeedDefault;
    }
}
