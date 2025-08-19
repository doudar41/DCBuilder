using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.UI;

public class GameMenuToggle : MonoBehaviour
{
    [SerializeField] List<Sprite> gameMenuSprites = new List<Sprite>();
    [SerializeField] Image panelImage;
    [SerializeField] ToggleGroup group;


    public void SwitchToSprite()
    {
        if (group.GetFirstActiveToggle() == null) { panelImage.sprite = gameMenuSprites[5]; return;  }
        //print(group.GetFirstActiveToggle().transform.GetSiblingIndex());
        panelImage.sprite = gameMenuSprites[group.GetFirstActiveToggle().transform.GetSiblingIndex()];
    }


}
