using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class isHoveringUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IIUInterfaces
{
    public UnityEvent<HoverUIElementEnum, GameObject> EnterHoverUIElement;
    public UnityEvent ExitHoverUIElement;
    [SerializeField]
    HoverUIElementEnum elementType;
    [SerializeField]
    int index = 0;

    public int GetIndex()
    {
        return index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //print("hover works");
        EnterHoverUIElement.Invoke(elementType, gameObject);
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ExitHoverUIElement.Invoke();
    }
}

public enum HoverUIElementEnum
{
    PORTRAIT,
    INVENTORY,
    JOURNAL,
    COMPASS,
    SPELLBOOK,
    MAP
}
