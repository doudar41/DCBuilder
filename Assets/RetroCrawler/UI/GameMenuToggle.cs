using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.UI;

public class GameMenuToggle : MonoBehaviour
{
    [SerializeField] List<Sprite> gameMenuSprites = new List<Sprite>();
    [SerializeField] Image panelImage;
    [SerializeField] ToggleGroup group;
    [SerializeField] Camera cam;

    public void SwitchToSprite()
    {
        if (group.GetFirstActiveToggle() == null) { panelImage.sprite = gameMenuSprites[5]; cam.depth = -2; return;  }
        //print(group.GetFirstActiveToggle().transform.GetSiblingIndex());
        panelImage.sprite = gameMenuSprites[group.GetFirstActiveToggle().transform.GetSiblingIndex()]; cam.depth = 1;
    }


    public void MoveCamUpFront()
    {
        cam.depth = 1;
    }
}
