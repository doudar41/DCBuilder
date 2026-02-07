using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animancer;
using Ami.BroAudio;

public class DamagePlayerVFX : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Sprite emptySprite;
    [SerializeField] animateUIImage anim;
    [SerializeField] List<SpellAnimationList> clips;
    Dictionary<string, SpellAnimationList> listClips = new Dictionary<string, SpellAnimationList>();

    private void Awake()
    {
        foreach(SpellAnimationList cl in clips)
        {
            listClips.Add(cl.stateName, cl);
            anim.FillSpriteList(cl.stateName, cl.clip);
        }
    }
    private void Start()
    {

    }

    void ChangeAnimation(string animationStateName)
    {
        anim.StartFXAnimation(animationStateName);
        GameInstance.soundManagerInGame.ProtectedPlay(listClips[animationStateName].soundID);

    }
    public void PlaySpellEffect(SpellContainer spell)
    {
        ChangeAnimation(spell.animationTriggerName);
    }


}

[System.Serializable]
public class SpellAnimationList
{
    public string stateName = "";
    public List<Sprite> clip;
    public SoundID soundID;
}