using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PageTurnButton : MonoBehaviour
{
    [SerializeField]List<PageTurn> pageTurns = new List<PageTurn>();
    [SerializeField] bool turnRight = true;
    Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        foreach (PageTurn pageTurn in pageTurns)
        {
            //if (pageTurn.gameObject == null || pageTurn.gameObject.activeSelf) return;
            pageTurn.FlipPagesStart(turnRight);
        }
    }

}

