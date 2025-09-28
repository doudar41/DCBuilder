
using UnityEngine;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour
{
    [SerializeField] Image frontImage, backImage;
    public SpellContainer spellContainer;
    public Spell spell;
    [SerializeField] Sprite emptySprite;
    public void SetSpriteToImages(SpellContainer spellactive)
    {
        spellContainer = spellactive;
        frontImage.sprite = spellactive.spellIcon;
        backImage.sprite = spellactive.spellIcon;
    }

    public void SetSpriteToImages(Sprite spellSprite, Spell _spell)
    {
        spell = _spell;
        frontImage.sprite = spellSprite;
        backImage.sprite = spellSprite;
    }
    public SpellContainer GetSpellContainer()
    {
        return spellContainer;
    }

    public void ClearBuffIcon()
    {
        spellContainer = null;
        frontImage.sprite = emptySprite;
        backImage.sprite = emptySprite;
    }
}
