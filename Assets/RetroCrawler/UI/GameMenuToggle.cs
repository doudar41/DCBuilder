using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.UI;

public class GameMenuToggle : MonoBehaviour
{
    [SerializeField] List<Sprite> gameMenuSprites = new List<Sprite>();
    [SerializeField] Image panelImage;
    [SerializeField] ToggleGroup group;
    [SerializeField] CameraOrder cameraOrder;

    public void SwitchToSprite()
    {
        if(GameInstance.playerController.shopIsOpened) 
        {  
            return; 
        }
        if (group.GetFirstActiveToggle() == null) 
        { 
            panelImage.sprite = gameMenuSprites[5];
            cameraOrder.BattleLogWithGameplay();
            GameInstance.playerController.MenuOpened(false);
            return;  
        }

        panelImage.sprite = gameMenuSprites[group.GetFirstActiveToggle().transform.GetSiblingIndex()];
        cameraOrder.ShopWithoutBattlelog();
        GameInstance.playerController.MenuOpened(true);
    }

    public void SwitchToSprite(int index)
    {
        if (GameInstance.playerController.shopIsOpened)
        {
            return;
        }
        if (index<0)
        {
            group.SetAllTogglesOff();
            panelImage.sprite = gameMenuSprites[5];
            cameraOrder.BattleLogWithGameplay();
            GameInstance.playerController.MenuOpened(false);

            return;
        }

        panelImage.sprite = gameMenuSprites[index];
        cameraOrder.ShopWithoutBattlelog();
        GameInstance.playerController.MenuOpened(true);
    }


    public void MoveCamUpFront()
    {
        cameraOrder.ShopWithoutBattlelog();
    }

    public void Reset()
    {
        group.SetAllTogglesOff();
        SwitchToSprite();
    }

}
