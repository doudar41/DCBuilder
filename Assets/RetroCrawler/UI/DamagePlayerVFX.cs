using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamagePlayerVFX : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Sprite emptySprite;
    [SerializeField] Animator animator;

    private void Start()
    {
        if (animator == null) return;
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

[System.Serializable]
public class SpellAnimationList
{
    public bool onEnemy = false;
    public SpellEffects spellEffect;
    public MagicType magicType = MagicType.None;
    public Animator aspriteAnim;
    public List<Sprite> animationList = new List<Sprite>();
}