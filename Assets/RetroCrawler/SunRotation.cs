using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{


    private void Update()
    {
        transform.Rotate(Vector3.right);
        transform.Rotate(Vector3.forward);
    }
}
