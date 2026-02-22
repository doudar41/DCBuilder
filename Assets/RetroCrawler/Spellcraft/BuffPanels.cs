using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffPanels : MonoBehaviour
{
    [SerializeField] GameObject smallPanel, bigPanel;
    [SerializeField] List<BuffIcon> buffIcons = new List<BuffIcon>();
    Dictionary<BuffIcon,int> buffIconsWithTime = new Dictionary<BuffIcon, int>();

    private void Start()
    {
        GameInstance.playerController.timeForward += TimeForward;
        GameInstance.battleManager.battlePassTime += TimeForward;
    }

    private void OnDestroy()
    {
        GameInstance.playerController.timeForward -= TimeForward;
        GameInstance.battleManager.battlePassTime -= TimeForward;
    }

    public void AddBuffToList(SpellContainer spellAttached , int timeSpellLasts)
    {

        foreach (BuffIcon b in buffIcons)
        {

            if (!b.SetSpriteToImages(spellAttached, timeSpellLasts)) {  continue; }
            else
            {
                if (!buffIconsWithTime.ContainsKey(b)) buffIconsWithTime.Add(b, timeSpellLasts);
                else buffIconsWithTime[b] = timeSpellLasts;
            }

        }
    }

    void TimeForward(int count)
    {
        foreach (BuffIcon b in buffIcons)
        {
            if (buffIconsWithTime.ContainsKey(b))
            {
                buffIconsWithTime[b] -= count;
                if (buffIconsWithTime[b] <= 0)
                {
                    b.ClearBuffIcon();
                    buffIconsWithTime.Remove(b);
                }
            }
            
        }

    }


    /*
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
    */
    /*    public void RemoveBuffFromList(Spell spell)
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
        }*/
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