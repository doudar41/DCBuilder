using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SpellChoiceIcon : MonoBehaviour, IPointerClickHandler
{

    public SpellContainer spell;
    [SerializeField] Image spellIcon;
    [SerializeField] Image borderImage;

    public UnityEvent<SpellContainer> SendSpell;

    private void Start()
    {
        spellIcon.sprite = spell.spellIcon;
    }

    public void SetIconActive(bool active)
    {
        if (active)
        {
            borderImage.color = Color.white;
        }
        else
        {
            borderImage.color = Color.clear;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SendSpell.Invoke(spell);
    }
}
