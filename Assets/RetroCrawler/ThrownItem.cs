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

    private void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();
    }


    private void Update()
    {
        if(splineAnimate.NormalizedTime >= 1f)
        {
            GameInstance.playerController.GetBlockFromVector3(transform.position).GetComponent<OnBlockPlacement>().HitByThrownItem(item, stackAmount, itemModelPrefab);
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {

        //print(other.gameObject.layer + " - "+other.gameObject.transform.parent.parent.name);
        if(other.gameObject.layer == 11)
        {
            GetComponent<SplineAnimate>().Pause();
            other.gameObject.transform.parent.parent.gameObject.GetComponent<OnBlockPlacement>().HitByThrownItem(item, stackAmount, itemModelPrefab);
            Destroy(gameObject.transform.parent.gameObject);

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
