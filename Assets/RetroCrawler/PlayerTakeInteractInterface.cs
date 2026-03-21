using System.Collections;


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
                        IItem iitem = other.GetComponent<IItem>();
                        HeroInventoryItem item = other.GetComponent<IItem>().WhatItem();
                        GameInstance.inventory.AddToInventoryItems(item, item.stackAmount);
                        _collider.enabled = false;
                        iitem.RemoveFromTheWorld();

                        break;

                    case InteractablesEnum.SWITCH:
                        Debug.Log("Player triggered a switch item.");
                        if(other.gameObject.GetComponent<ISwitch>() !=null)
                        other.gameObject.GetComponent<ISwitch>().ToggleSwitch();
                        break;

                    default:
                        Debug.Log("Player triggered a non-pickable item.");
                        break;
                }
            }
        }

        if(other.gameObject.TryGetComponent<IChestLocked>(out IChestLocked chest))
        {
            Debug.Log("Chest found " + chest.IsOpen());

            chest.OpenChest();
            // Implement logic for enemy encounter
        }
    }

    IEnumerator SwitchOffCollider()
    {
        yield return new WaitForSeconds(0.1f);
        _collider.enabled = false;
    }
}
