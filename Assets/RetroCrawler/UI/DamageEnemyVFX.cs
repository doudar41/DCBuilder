using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DamageEnemyVFX : MonoBehaviour
{
    [SerializeField] List<SpellAnimationList> spellAnimationLists = new List<SpellAnimationList>();
    
    [SerializeField] SpriteRenderer image;
    [SerializeField] Sprite emptySprite;
    [SerializeField] Animator animator;
    string currentAnimationName = "";

    private void Start()
    {
        animator.StartPlayback();
        animator.gameObject.SetActive(true);
        animator.speed = 0.5f;
    }


    void ChangeAnimation(string animationStateName)
    {
        animator.CrossFade(animationStateName, 0.1f);
    }

    public void PlaySpellEffect(SpellContainer spell)
    {
        ChangeAnimation(spell.animationTriggerName);
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

