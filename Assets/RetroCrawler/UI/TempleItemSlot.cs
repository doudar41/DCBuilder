using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TempleItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] ItemScriptableContainer itemToSell;
    [SerializeField] Image storeImage;
    [SerializeField] int price;
    [SerializeField] GameObject descriptionPrefab;
    GameObject desc;

    private void Start()
    {
        if (itemToSell == null) return;
        storeImage.sprite = itemToSell.InventorySprite;
        price = itemToSell.price;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameInstance.party.SellBuyMoneyCheck(price) >= 0)
        {
            GameInstance.inventory.FindEmptySlotAndPutItem(GameInstance.dataBase.HeroInventoryFromITemScriptable(itemToSell),1, false);
            GameInstance.party.MoneyGoes(price);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemToSell == null) return;
        if (itemToSell != null)
        {
            if (desc == null)
            {
                desc = Instantiate(descriptionPrefab, transform);
                //desc.transform.SetParent(null);
            }
            else
            {
                desc.SetActive(true);
            }


            TextMeshProUGUI textObject = desc.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            textObject.text = itemToSell.itemDescription + "\n" + "Price: " + ((int)(price)).ToString();
            textObject.color = Color.green;
            if (GameInstance.party.SellBuyMoneyCheck(price) < 0)
            {
                textObject.color = Color.red;
            }


        }
    }
    public void OnPointerExit(PointerEventData eventData)
        {
        if (itemToSell == null) return;
        if (desc == null) return;
        desc.SetActive(false);
    }
}
