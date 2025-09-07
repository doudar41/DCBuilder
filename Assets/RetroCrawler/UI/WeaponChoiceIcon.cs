
using UnityEngine;using UnityEngine.UI;using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WeaponChoiceIcon : MonoBehaviour, IPointerClickHandler
{
    public int weapon = -1;

    [SerializeField] Image spellIcon;
    [SerializeField] Image borderImage;
    [SerializeField] ItemScriptableContainer weaponScriptable;

    public UnityEvent<int> SendWeaponIndex;

    private void Start()
    {
        spellIcon.sprite = weaponScriptable.InventorySprite;
        weapon =  GameInstance.dataBase.GetItemIndexFromDataBase(weaponScriptable);
    }

    internal void SetIconActive(bool active)
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
        SendWeaponIndex.Invoke(weapon);
    }
}
