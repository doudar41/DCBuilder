using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TempleItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{    
    
    [SerializeField] ItemScriptableContainer itemToSell;
    public UnityEvent<string> onHover;
    public UnityEvent refreshMoney;
    int price = 0;
    Image storeImage;
    private void Awake()
    {
        storeImage = GetComponent<Image>();
    }

    private void Start()
    {

        if (itemToSell == null) return;
        storeImage.sprite = itemToSell.InventorySprite;
        price = itemToSell.price;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHover.Invoke(itemToSell.itemName+"-"+itemToSell.price);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHover.Invoke("");
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameInstance.party.SellBuyMoneyCheck(price) >= 0)
        {
            GameInstance.inventory.AddToInventoryItems( GameInstance.dataBase.HeroInventoryFromITemScriptable(itemToSell), 1);
            //GameInstance.inventory.FindEmptySlotAndPutItem(GameInstance.dataBase.HeroInventoryFromITemScriptable(itemToSell),1, false);
            if (!GameInstance.CheckIfItemIdentified(GameInstance.dataBase.HeroInventoryFromITemScriptable(itemToSell).container))
            {
                GameInstance.SaveIdentifiedItems(GameInstance.dataBase.HeroInventoryFromITemScriptable(itemToSell));
            }
            GameInstance.party.MoneyGoes(price);
        }
    }

}
