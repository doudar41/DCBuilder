using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellShopSlot : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] Image scrollPicture;
    [SerializeField] Sprite emptySprite;
    [SerializeField] GameObject descriptionPrefab;
    [SerializeField] List<Sprite> magicSchoolScrollSprites = new List<Sprite>();
    SpellContainer spellToSell;
    public float sellMultiplier = 1;
    GameObject desc;
    public UnityEvent refreshCoins;


    private void Awake()
    {
       //scrollPicture.sprite = emptySprite; 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(spellToSell != null)
        {
            int price = Mathf.RoundToInt(spellToSell.spellPrice * sellMultiplier);

            if (!GameInstance.party.activeHero.GetActiveHeroSpellbook().Contains(spellToSell))
            {

                if (GameInstance.party.CheckGemsAmountForSell(price) >= 0)
                {
                    GameInstance.party.activeHero.GetActiveHeroSpellbook().Add(spellToSell);
                    GameInstance.party.GemGoes(spellToSell.spellPrice);
                    spellToSell = null;
                    scrollPicture.sprite = emptySprite;
                    refreshCoins.Invoke();
                    if (desc != null) desc.SetActive(false);
                }
            }
            else
            {
                //already have spell
            }

        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spellToSell != null)
        {
            if (desc == null)
            {
                desc = Instantiate(descriptionPrefab, transform);
            }
            else desc.SetActive(true);

            TextMeshProUGUI textObject = desc.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            Image[] spellIcon = desc.gameObject.GetComponentsInChildren<Image>();
            string spellTexts = "";
            foreach (Spell s in spellToSell.spells)
            {
                spellTexts += "\n" + s.SpellDescription;
            }
            textObject.text = spellToSell.spellName + "\n" + spellTexts + "\n" + "Price: " + ((int)(spellToSell.spellPrice * sellMultiplier)).ToString();
            spellIcon[1].sprite = spellToSell.spellIcon;
            spellIcon[1].preserveAspect = true;
            if (GameInstance.party.CheckGemsAmountForSell((int)(spellToSell.spellPrice * sellMultiplier)) >= 0) textObject.color = Color.green;
            else textObject.color = Color.red;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spellToSell == null) return;
        if (desc == null) return;
        desc.SetActive(false);
    }

    public void SetSpellToSell(SpellContainer spell)
    {
        spellToSell = spell;
        if (spellToSell.spells[0].magicType == MagicType.Fire
                || spellToSell.spells[0].magicType == MagicType.Water
                || spellToSell.spells[0].magicType == MagicType.Ice
                || spellToSell.spells[0].magicType == MagicType.Air
                || spellToSell.spells[0].magicType == MagicType.Earth)
        {
            scrollPicture.sprite = magicSchoolScrollSprites[0];
            //print("setting spell to sell " + spell.spells[0].magicType + " - " + magicSchoolScrollSprites[0]);
        }
        if (spellToSell.spells[0].magicType == MagicType.Light)
        {
            scrollPicture.sprite = magicSchoolScrollSprites[1];
        }
        if (spellToSell.spells[0].magicType == MagicType.Dark)
        {
            scrollPicture.sprite = magicSchoolScrollSprites[2];
        }
    }

    public void ClearSlot()
    {
        scrollPicture.sprite = emptySprite;
    }

}
