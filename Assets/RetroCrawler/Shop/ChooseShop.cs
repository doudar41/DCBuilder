using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseShop : MonoBehaviour
{
    [SerializeField] List<GameObject> shopToChoose = new List<GameObject>();


    private void Start()
    {
        foreach(GameObject g in shopToChoose)
        {

            //g.SetActive(false);
        }
    }
    public void ChooseShopOfType(int index)
    {
        foreach(GameObject g in shopToChoose)
        {
            g.SetActive(false);

        }
        shopToChoose[index].SetActive(true);
        if(shopToChoose[index].GetComponent<ItemShop>() != null) shopToChoose[index].GetComponent<ItemShop>().NewItems();
        if (shopToChoose[index].GetComponent<SpellShop>() != null) shopToChoose[index].GetComponent<SpellShop>().NewItems();

    }



}
