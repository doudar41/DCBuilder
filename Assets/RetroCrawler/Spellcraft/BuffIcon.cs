
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour
{
    [SerializeField] Image frontImage, backImage;

    [SerializeField] SpellEffects spellEffect;
    [SerializeField] MagicType magicType;
    [SerializeField] Sprite emptySprite;
    List<SpellEffects> spellEffects = new List<SpellEffects>();



    public SpellEffects GetSpellEffect(out MagicType _magicType)
    {
        _magicType = magicType;
        return spellEffect;
    }

    public bool SetSpriteToImages(SpellContainer spellactive, int timeSpellLasts)
    {
        spellEffects.Clear();
        foreach (Spell spell in spellactive.spells)
        {
            spellEffects.Add(spell.spellEffect);
        }

        if (spellEffects.Contains(spellEffect))
        {
            if(magicType != MagicType.None)
            {
                if ( magicType == spellactive.spells[spellEffects.IndexOf(spellEffect)].magicType)
                {
                frontImage.sprite = spellactive.spellIcon;
                backImage.sprite = spellactive.spellIcon;
                    return true;
                }
            }
            else
            {
                frontImage.sprite = spellactive.spellIcon;
                backImage.sprite = spellactive.spellIcon;
                return true;
            }

        }
        return false;
    }


    public void ClearBuffIcon()
    {

        frontImage.sprite = emptySprite;
        backImage.sprite = emptySprite;
    }
}
