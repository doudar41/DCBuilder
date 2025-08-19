
using UnityEngine;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour
{
    [SerializeField] Image frontImage, backImage;
    public SpellContainer spellContainer;
    [SerializeField] Sprite emptySprite;
    public void SetSpriteToImages(SpellContainer spellactive)
    {
        spellContainer = spellactive;
        frontImage.sprite = spellactive.spellIcon;
        backImage.sprite = spellactive.spellIcon;
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
