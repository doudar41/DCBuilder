using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrapMissile : MonoBehaviour
{

    [SerializeField] ItemScriptableContainer itemOnHit;
    [SerializeField] float missileSpeed = 2.5f;
    [SerializeField] SpellContainer spellContainer;
    [SerializeField] int trapComplexity = 5;
    private void Awake()
    {
        GameInstance.progress += GameUpdate;
        transform.parent = null;
    }

    private void OnDestroy()
    {
        GameInstance.progress -= GameUpdate;
    }



    void GameUpdate(int count) 
    {
        transform.position += Vector3.forward * missileSpeed;

    }


    private void OnTriggerEnter(Collider other)
    {
        print(other);
        if(other.tag == "Player")
        {
            this.gameObject.SetActive(false);
            GameInstance.progress -= GameUpdate;
            GameInstance.party.TrapDamage(spellContainer, trapComplexity, true);
            if(itemOnHit !=null) GameInstance.playerController.CreateItemInWorld(GameInstance.dataBase.HeroInventoryFromITemScriptable(itemOnHit));
            print("hit the player");
        }


    }
}
