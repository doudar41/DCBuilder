using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealBox : MonoBehaviour
{
    [SerializeField] FogOfWarTexture _fogOfWarTexture;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("Revealing area at " + transform.position);
            _fogOfWarTexture.RevealArea(transform.position, GetComponent<BoxCollider>().size);
            GetComponent<BoxCollider>().enabled = false; // disable the collider so it doesn't trigger again
        }
    }

}
