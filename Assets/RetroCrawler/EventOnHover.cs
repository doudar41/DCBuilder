
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class EventOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent<string> onHover;
    [SerializeField] string hoverText;

    private void Start()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHover.Invoke(hoverText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHover.Invoke("");
    }
}
