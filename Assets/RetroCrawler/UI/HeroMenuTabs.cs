using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HeroMenuTabs : MonoBehaviour
{
    ToggleGroup group;
    [SerializeField]
    List<Toggle> toggles = new List<Toggle>();
    [SerializeField]
    List<GameObject> menus = new List<GameObject>();
    Dictionary<Toggle, GameObject> mapToggleMenu = new Dictionary<Toggle, GameObject>();
    Dictionary<Toggle, Sprite> mapSpriteToggleMenu = new Dictionary<Toggle, Sprite>();
    [SerializeField]
    GameObject inGameMenu;
    [SerializeField]
    Image backgroundImage;
    [SerializeField]
    List<Sprite> spritebackGround = new List<Sprite>();



    private void Start()
    {
        if(toggles.Count != menus.Count)
        {
            print("toogles and menus should be the same size"); return;
        }
        group = GetComponent<ToggleGroup>();
        for (int i = 0;i<toggles.Count;i++)
        {
            mapToggleMenu.Add(toggles[i], menus[i]);
            mapSpriteToggleMenu.Add(toggles[i], spritebackGround[i]);
            toggles[i].onValueChanged.AddListener(CheckToggle);
        }

    }

    void CheckToggle(bool check)
    {
        if (!inGameMenu.activeSelf) inGameMenu.SetActive(true);
        if (check) 
        {
            for(int i=0;i<menus.Count;i++)
            {
                menus[i].SetActive(false);
            }
            mapToggleMenu[group.GetFirstActiveToggle()].SetActive(true);
            backgroundImage.sprite = mapSpriteToggleMenu[group.GetFirstActiveToggle()];
        }

    }

}
