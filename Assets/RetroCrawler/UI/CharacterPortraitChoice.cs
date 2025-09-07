
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPortraitChoice : MonoBehaviour,IPointerClickHandler
{

    [SerializeField] int heroIndex=0;
    public UnityEvent<int> GetHeroIndex;
    [SerializeField] Image borderTopGrey, borderButtomGrey, borderTopGold, borderButtomGold;

    [SerializeField] Image image;

    private void Awake()
    {
        GetHeroIndex.AddListener(ActivateHero);    
    }

    public void SetSprite( Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GetHeroIndex.Invoke(heroIndex);
    }

    public void ActivateHero(int index)
    {
//print("refresh "+ index);
        if (index == heroIndex)
        {
            borderTopGold.color = Color.white; borderButtomGold.color = Color.white;
        }
        else
        {
            borderTopGold.color = Color.clear; borderButtomGold.color = Color.clear;
        }
    }

}
