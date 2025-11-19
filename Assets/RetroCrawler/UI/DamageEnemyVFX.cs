using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DamageEnemyVFX : MonoBehaviour
{

    [SerializeField] SpriteRenderer image;
    [SerializeField] Sprite emptySprite;
    [SerializeField] Animator animator;

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


    

}

