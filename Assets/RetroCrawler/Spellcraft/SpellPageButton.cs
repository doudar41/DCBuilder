using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpellPageButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] int pageIndex = 0;
    
    [SerializeField] float startPosition, offsetPosition;
    [SerializeField] RectTransform rectTransform;

    private void Start()
    {
        //startPosition =  rectTransform.anchoredPosition.x;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        GameInstance.spellbook.SetSpellPage(pageIndex);
    }

    public void OnUncheckPage()
    {
        //print("unchecked page");
        rectTransform.anchoredPosition = new Vector3(startPosition, rectTransform.anchoredPosition.y);
    }

    public void CheckedPage()
    {
        ///print("checked page");
        rectTransform.anchoredPosition = new Vector3(offsetPosition, rectTransform.anchoredPosition.y);
    }
}
