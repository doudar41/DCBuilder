using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEnemyVFX : MonoBehaviour
{
    [SerializeField] List<SpellAnimationList> spellAnimationLists = new List<SpellAnimationList>();
    [SerializeField] SpriteRenderer image;
    [SerializeField] Sprite emptySprite;


    public void PlaySpellEffect(SpellContainer spell)
    {
        foreach (SpellAnimationList anims in spellAnimationLists)
        {
            if (anims.spellEffect == spell.spells[0].spellEffect)
            {
                if (anims.magicType == spell.spells[0].magicType)
                {
                    PlayAnimation(anims.animationList, 1);

                }
            }
        }
    }

    public void PlayAnimation(List<Sprite> sprites, int times)
    {
        StartCoroutine(Play(sprites, times));
    }
    
    IEnumerator Play(List<Sprite> sprites, int times)
    {
        for (int i = 0; i < times; i++)
        {

        foreach(Sprite s in sprites)
            {
                
                image.sprite = s;
                yield return new WaitForSeconds(0.05f);
            }
        }
        image.sprite = emptySprite;
        yield return null;
    }
}

