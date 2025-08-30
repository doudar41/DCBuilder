using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameMenu : MonoBehaviour
{
 public void LoadGameFromStart()
    {
        GameInstance.LoadGameFromStart();
    }

    public void EndGame()
    {
        Application.Quit();
    }

    public void LoadMainGameMenu()
    {
        GameInstance.LoadGameMainMenu();
    }
}
