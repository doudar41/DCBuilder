using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpellPage : MonoBehaviour
{
    [SerializeField] List<SpellButton> spellButtons = new List<SpellButton>();
    
    Dictionary<SpellContainer, bool> spellMap = new Dictionary<SpellContainer, bool>();
    List<SpellContainer> allPageSpells = new List<SpellContainer>();

    SpellContainer currentSpellContainer;

    public UnityEvent<SpellContainer> castSpell;

    [SerializeField] GameObject spellPage;


    private void Awake()
    {
        //InitializeSpellPage();
    }

    public void InitializeSpellPage()
    {
        spellPage.SetActive(true);
        foreach (SpellButton b in spellButtons)
        {
            spellMap.Add( b.GetSpellContainer(),false);
            allPageSpells.Add(b.GetSpellContainer());
        }
        spellPage.SetActive(false);
    }

    public void SetPageAvailableSpells(List<SpellContainer> spells)
    {
        foreach (SpellButton b in spellButtons)
        {

            b.DeactivateSpell();

        }
        foreach (SpellContainer sc in allPageSpells)
        {
            if (spells.Contains(sc))
            {
                spellMap[sc] = true;
            }
            else
            {
                spellMap[sc] = false;
            }
        }

        OpenSpellPage(spellPage.activeSelf);

    }

    public void OpenSpellPage(bool active)
    {
        if (active)
        {
            //print(" spell button count "+spellMap.Count);
            spellPage.SetActive(true);
            foreach (SpellButton b in spellButtons)
            {
                b.ResetButton(); b.DeactivateSpell();
                if (spellMap[b.GetSpellContainer()])
                {
                    //print(b);
                    b.SetSpellActive();
                }
                else
                {
                    b.DeactivateSpell();
                }
            }
        }
        else
        {
            spellPage.SetActive(false);
        }
    }

    public void SpellRelease(SpellContainer SpellContainerFromButton)
    {
        if(currentSpellContainer == SpellContainerFromButton)
        {
            GameInstance.spellbook.CastSpell(SpellContainerFromButton);
            //print("cast spell " + SpellContainerFromButton);
        }
        else
        {
            currentSpellContainer = SpellContainerFromButton;
        }
    }

    public void ResetAllSpellHighlight(SpellContainer SpellContainerFromButton)
    {
        foreach(SpellButton sb in spellButtons)
        {
            if (sb.GetSpellContainer() != SpellContainerFromButton)   sb.ResetButton();
        }
    }

}
