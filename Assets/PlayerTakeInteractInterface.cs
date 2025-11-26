using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerTakeInteractInterface : MonoBehaviour
{
    [SerializeField] Collider _collider;


    private void Start()
    {
        if(_collider == null)
        {
            _collider = GetComponent<Collider>();
        }
    }
    public void SwitchOnCollider()
    {
        _collider.enabled = true;
        StartCoroutine(SwitchOffCollider());
    }

/*    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<IInteractables>(out IInteractables interactable))
        {
            foreach(InteractablesEnum itemType in interactable.WhatIsIt())
            {

                switch (itemType)
                {
                    case InteractablesEnum.PICKABLE:
                        Debug.Log("Player collided with a pickable item.");
                        // Implement logic for picking up the item
                        break;
                    default:
                        Debug.Log("Player collided with a non-pickable item.");
                        break;

                }

            }
        }
    }*/


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<IInteractables>(out IInteractables interactable))
        {
            foreach (InteractablesEnum itemType in interactable.WhatIsIt())
            {
                switch (itemType)
                {
                    case InteractablesEnum.PICKABLE:
                        Debug.Log("Player triggered a pickable item.");
                        // Implement logic for picking up the item
                        break;

                    case InteractablesEnum.SWITCH:
                        Debug.Log("Player triggered a switch item.");
                        break;

                    default:
                        Debug.Log("Player triggered a non-pickable item.");
                        break;
                }
            }
        }
    }

    IEnumerator SwitchOffCollider()
    {
        yield return new WaitForSeconds(0.1f);
        _collider.enabled = false;
    }
}
