using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MarkRecallMenu : MonoBehaviour
{
    [SerializeField] CameraOrder cameraOrder;
    [SerializeField] GameObject markRecallMenuPanel;
    [SerializeField] animateUIImage animateUIImage;
    public void OpenMarkRecall()
    {
        GameInstance.spellbook.CloseSpellbook();
        cameraOrder.ShopWithoutBattlelog();
        markRecallMenuPanel.SetActive(true);
        GameInstance.playerController.MenuOpened(true);
        animateUIImage.StartAnimation();
    }

    public void SetMarkPlace()
    {
        if(GameInstance.playerController.GetPlayerState() == PlayerState.Battle)
        {
            GameInstance.spellbook.BattleLogMessage(new List<string>() { "You cannot mark a location while in battle!" }, null);
            markRecallMenuPanel.SetActive(false);
            cameraOrder.BattleLogWithGameplay();
            GameInstance.playerController.MenuOpened(false);
            return;
        }
        Vector3Int position =GameInstance.playerController.GetCurrentPosition();
        CardinalDirections direction = GameInstance.playerController.GetCurrentDirection();
        string levelName = SceneManager.GetActiveScene().name;

        GameInstance.SetMarkLocation(position, direction, levelName);
        markRecallMenuPanel.SetActive(false);
        cameraOrder.BattleLogWithGameplay();
        GameInstance.playerController.MenuOpened(false);
        GameInstance.spellbook.BattleLogMessage(new List<string>() { "Mark is added " + position + " on level " + levelName  }, null);
    }

    public void RecallMark()
    {
        GameInstance.playerController.TeleportToMarkDestination (GetMarkLocation());

        markRecallMenuPanel.SetActive(false);
        cameraOrder.BattleLogWithGameplay();
        GameInstance.playerController.MenuOpened(false);

    }

    public MarkSavedLocation GetMarkLocation()
    {
        if (GameInstance.playerController.GetPlayerState() == PlayerState.Battle)
        {
            markRecallMenuPanel.SetActive(false);
            GameInstance.spellbook.BattleLogMessage(new List<string>() { "You cannot recall to a marked location while in battle!" }, null);
            markRecallMenuPanel.SetActive(false);
            cameraOrder.BattleLogWithGameplay();
            GameInstance.playerController.MenuOpened(false);

            return null;
        }
        markRecallMenuPanel.SetActive(false);
        cameraOrder.BattleLogWithGameplay();
        GameInstance.playerController.MenuOpened(false);

        return GameInstance.GetMarkLocation();
    }




}
