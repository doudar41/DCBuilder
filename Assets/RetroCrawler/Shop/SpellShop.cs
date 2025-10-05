using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class SpellShop : MonoBehaviour
{
    [SerializeField] Image backGroundImage;
    [SerializeField] List<ItemShopSlot> spellSlots = new List<ItemShopSlot>();
    [SerializeField] List<MagicType> magicTypes = new List<MagicType>();
    [SerializeField] List<ItemType> itemsTypesToSell = new List<ItemType>();
    [SerializeField] List<TextMeshProUGUI> heroesCoinsText;
    [SerializeField] float sellMultiplier = 1;
    [SerializeField] Vector2Int itemsLevel = new Vector2Int(0,1);
    //[SerializeField] int shopIndex = 1;
    [SerializeField] Camera cam;
    [SerializeField] TextMeshProUGUI textOfShopState;

    ShopState shopState = ShopState.SellToPlayer;
    public UnityEvent closeShopPanel;
    private void OnEnable()
    {
        cam.depth = 1;
        GetPlayersCoins();
        textOfShopState.text = "Spells";
    }


    public void GetPlayersCoins()
    {
        var money = GameInstance.party.GetCoinsForUI();
        for (int i = 0; i < money.Count; i++)
        {
            heroesCoinsText[i].text = money[i].ToString();
        }
    }
    public void RefreshSoldSpells()
    {

        for (int i = 0; i < spellSlots.Count; i++)
        {
            spellSlots[i].SetSpellToSell(RandomSpellToSell(magicTypes[Random.Range(0, magicTypes.Count)]));
        }
    }

    public void NewItemsToSell()
    {
        ClearSlots();
        for (int i = 0; i < spellSlots.Count; i++)
        {
            spellSlots[i].SetItemToSell(RandomItemsToSell(itemsTypesToSell[Random.Range(0, itemsTypesToSell.Count)]));
            spellSlots[i].sellMultiplier = sellMultiplier;
            spellSlots[i].shopState = shopState;
        }
    }


    public void CameraOut()
    {
        cam.depth = -2;
    }

    public SpellContainer RandomSpellToSell(MagicType magicType)
    { 
        List<SpellContainer> spellOfType = new List<SpellContainer>();

        foreach (SpellContainer spell in GameInstance.dataBase.GetAllSpells())
        {
            if (spell.spells[0].magicType == magicType)
            {
                spellOfType.Add(spell);
            }
        }

        return spellOfType[Random.Range(0, spellOfType.Count)];
    }


    public ItemScriptableContainer RandomItemsToSell(ItemType itemType)
    {
        List<ItemScriptableContainer> itemsOfType = new List<ItemScriptableContainer>();

        foreach (ItemScriptableContainer item in GameInstance.dataBase.GetWholeItemDatabase())
        {
            if (item.itemType == itemType)
            {
                itemsOfType.Add(item);
            }
        }
        List<ItemScriptableContainer> randomItems = new List<ItemScriptableContainer>();


        return itemsOfType[Random.Range(0, itemsOfType.Count)];
    }


    public void CloseShop()
    {
        closeShopPanel.Invoke();
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        gameObject.SetActive(false);
    }
    public void ClearSlots()
    {
        for (int i = 0; i < spellSlots.Count; i++)
        {

            spellSlots[i].ClearSlot();
        }
    }
    public void SwitchToSellPotions()
    {
        shopState = ShopState.SellToPlayer;
        NewItemsToSell();
        textOfShopState.text = "Potions";
    }

    public void SwitchToSellSpell()
    {
        shopState = ShopState.SellToPlayer;
        RefreshSoldSpells();
        textOfShopState.text = "Spells";
    }

}
