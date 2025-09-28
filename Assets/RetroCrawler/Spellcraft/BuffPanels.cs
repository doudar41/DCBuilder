using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffPanels : MonoBehaviour
{
    [SerializeField] GameObject smallPanel, bigPanel;
    [SerializeField] List<BuffIcon> buffIcons = new List<BuffIcon>();
    [SerializeField] List<SpelEffectIcon> spelleffects = new List<SpelEffectIcon>();

    Sprite FindSpellEffectIcon(Spell spell)
    {
        foreach(SpelEffectIcon effect in spelleffects)
        {
            if(effect.spellEffect == spell.spellEffect)
            {
                if(spell.magicType != MagicType.None)
                {
                    if(effect.magicType == spell.magicType)
                    {
                        return effect.sprite;
                    }
                }

                if(spell.changedMainStat != MainStat.None)
                {
                    if(effect.changedMainStat == spell.changedMainStat)
                    {
                        return effect.sprite;
                    }
                }
                if (spell.changedDependedStat != DependedStat.None)
                {
                    if (effect.changedDependedStat == spell.changedDependedStat)
                    {
                        return effect.sprite;
                    }
                }

                if(spell.skillStatAdded != SkillsStat.None)
                {
                    if(effect.skillStatAdded == spell.skillStatAdded)
                    {
                        return effect.sprite;
                    }
                }
            }


        }
        return null;
    }

    public void AddBuffToList(SpellContainer spellAttached)
    {
        foreach(BuffIcon b in buffIcons)
        {
            if(b.spellContainer == null)
            {
                b.SetSpriteToImages(spellAttached); return;
            }
        }
    }



    public void AddBuffToList(Spell spell)
    {
        foreach (BuffIcon b in buffIcons)
        {
            if (b.spellContainer == null)
            {
                b.SetSpriteToImages(FindSpellEffectIcon(spell), spell);
                return;
            }
        }
    }

    void SortBuffListAfterRemove(int startIndex)
    {
        for(int i = startIndex; i < buffIcons.Count-1; i++)
        {
            if (buffIcons[i + 1].spellContainer != null)
            {
                buffIcons[i].SetSpriteToImages(buffIcons[i + 1].spellContainer);
            }
            else
            {
                buffIcons[i].ClearBuffIcon();
            }
        }
    }

    public void RemoveBuffFromList(Spell spell)
    {
        for(int i = 0; i < buffIcons.Count; i++)
        {
            if (buffIcons[i].spellContainer != null)
            {
                if (buffIcons[i].spellContainer.spells.Contains(spell))
                {
                    buffIcons[i].ClearBuffIcon();
                    SortBuffListAfterRemove(i);
                }
            }
            if (buffIcons[i].spell != null)
            {
                if(buffIcons[i].spell == spell)
                {
                    buffIcons[i].ClearBuffIcon();
                    SortBuffListAfterRemove(i);
                }

            }
        }
    }
}


[System.Serializable]
public struct SpelEffectIcon
{
    public SpellEffects spellEffect;
    public MagicType magicType;
    public MainStat changedMainStat;
    public DependedStat changedDependedStat;
    public SkillsStat skillStatAdded;
    public Sprite sprite;
}