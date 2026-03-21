using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class ThrownItem : MonoBehaviour
{
    HeroInventoryItem item;
    int stackAmount = 1;
    SpriteRenderer spriteRenderer;
    [SerializeField] GameObject itemModelPrefab;
    SplineAnimate splineAnimate;
    bool once = true;
    private void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();
    }


    private void Update()
    {
        if(splineAnimate.NormalizedTime >= 1f)
        {
            if(GameInstance.playerController == null) return;
            if (GameInstance.playerController.GetBlockFromVector3(transform.position) != null)
            {
                GameInstance.playerController.GetBlockFromVector3(transform.position).GetComponent<OnBlockPlacement>().HitByThrownItem(item, stackAmount, itemModelPrefab);
                Destroy(gameObject.transform.parent.gameObject);
            }
            else
                {
                if (once) { GameInstance.inventory.AddToInventoryItems(item, stackAmount); once = false; }
                Destroy(gameObject.transform.parent.gameObject, 2f);
                }

        }
    }
    private void OnTriggerEnter(Collider other)
    {

        //print(other.gameObject.layer + " - "+other.gameObject.transform.parent.parent.name);
        if(other.gameObject.layer == 11)
        {
            GetComponent<SplineAnimate>().Pause();
            if(other.gameObject.transform.parent.parent.gameObject != null)
            {
                other.gameObject.transform.parent.parent.gameObject.GetComponent<OnBlockPlacement>().HitByThrownItem(item, stackAmount, itemModelPrefab);
                Destroy(gameObject.transform.parent.gameObject);
            }
            else
            {
                if (once)
                {
                    GameInstance.inventory.AddToInventoryItems(item, stackAmount);
                    once = false;
                }

                Destroy(gameObject.transform.parent.gameObject, 2f);
            }

        }
        
    }


    public void SetItemAndIcon(HeroInventoryItem newItem, Sprite icon, int amount)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        item = newItem;
        spriteRenderer.sprite = icon;
        stackAmount = amount;
    }



}
