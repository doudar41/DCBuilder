using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ami.BroAudio;

public class ChooseShop : MonoBehaviour
{
    [SerializeField] List<GameObject> shopToChoose = new List<GameObject>();
    [SerializeField] List<GameObject> buttonPanels = new List<GameObject>();
    [SerializeField] Camera cameraUI;
    [SerializeField] SoundID openShopSound = default;

    public UnityEvent switchOnPanel;



    public void ChooseShopOfType(int index)
    {

        foreach (GameObject g in shopToChoose)
        {
            g.SetActive(false);

        }
        cameraUI.depth = 2;
        //switchOnPanel.Invoke();
        shopToChoose[index].SetActive(true);
        buttonPanels[index].SetActive(true);

        if (shopToChoose[index].GetComponent<ItemShop>() != null) 
        { 
            BroAudio.Play(openShopSound);
            shopToChoose[index].GetComponent<ItemShop>().NewItemsToSell(); 
        }

        if (shopToChoose[index].GetComponent<SpellShop>() != null) 
        {
            BroAudio.Play(openShopSound);
            shopToChoose[index].GetComponent<SpellShop>().RefreshSoldSpells(); 
        }

    }



}
