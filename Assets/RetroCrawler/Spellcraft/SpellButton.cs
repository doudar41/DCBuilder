using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class SpellButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image buttonImage;
    [SerializeField] Sprite unknownSprite;
    [SerializeField] TextMeshProUGUI spellName;
    [SerializeField] SpellContainer spellContainer;
    public UnityEvent<SpellContainer> spellReady;

    private void Awake()
    {
        DeactivateSpell();
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        if (buttonImage.sprite == unknownSprite) return;
        spellName.color = Color.blue;
        spellReady.Invoke(spellContainer);
    }

    public void ResetButton()
    {
        spellName.color = Color.black;
    }

    public SpellContainer GetSpellContainer()
    {
        return spellContainer;
    }

    public void SetSpellActive()
    {
        buttonImage.sprite = spellContainer.spellIcon;
        spellName.text = spellContainer.spellName;
        buttonImage.preserveAspect = true;
    }

    public void DeactivateSpell()
    {
        buttonImage.sprite = unknownSprite;
        spellName.text = "";
    }
}
