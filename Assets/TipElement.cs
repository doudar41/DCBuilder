
using UnityEngine;
using UnityEngine.EventSystems;

public class TipElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] string tipText;
    [SerializeField] string tipTitle;
    [SerializeField] Sprite tipSprite;
    bool hovered = false;
    void Start()
    {
        GameInstance.playerController.rightClickEvent +=OnRightClick;
    }
    private void OnDestroy()
    {
          GameInstance.playerController.rightClickEvent -=OnRightClick;      
    }
    private void OnRightClick(Vector3 mousePos)
    {
        if(hovered)GameInstance.spawnTipWindow.FillTextField(tipText, tipTitle, tipSprite);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }
}
