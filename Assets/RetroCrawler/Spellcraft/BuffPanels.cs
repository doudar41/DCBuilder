using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffPanels : MonoBehaviour
{
    [SerializeField] GameObject smallPanel, bigPanel;
    [SerializeField] List<BuffIcon> buffIcons = new List<BuffIcon>();

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
            if (buffIcons[i].spellContainer == null) continue;
            if (buffIcons[i].spellContainer.spells.Contains(spell))
            {
                buffIcons[i].ClearBuffIcon();
                SortBuffListAfterRemove(i);
            }
        }
    }
}
