using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnTipWindow : MonoBehaviour
{
    [SerializeField] GameObject tipWindow;
    RectTransform tipWindowRect;
    [SerializeField] GameObject textFieldMain;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Image tipImage;
    [SerializeField] Sprite defaultSprite;


    private void Awake()
    {
        GameInstance.spawnTipWindow = this;
        if (tipWindow != null) 
        { 
            tipWindow.SetActive(false); 
            tipWindowRect = tipWindow.GetComponent<RectTransform>();
            print("SpawnTipWindow Awake, tipWindowRect: " + tipWindowRect.anchoredPosition);
        }
        print(Screen.width);
        textFieldMain.GetComponent<TextMeshProUGUI>().text = "";
        titleText.text = "";
    }



    public void FillTextField(HeroInventoryItem item)
    {

        if (item.container != -1)
            {
            textFieldMain.GetComponent<TextMeshProUGUI>().text = GameInstance.dataBase.GetItemFromBaseByIndex(item.container).itemDescription;
            tipImage.sprite = GameInstance.dataBase.GetItemFromBaseByIndex(item.container).worldSprite;
            titleText.text = GameInstance.dataBase.GetItemFromBaseByIndex(item.container).itemName;
            }
            else tipImage.sprite = defaultSprite;
    }

    public void FillTextField(EnemyBase enemy)
    {

    }

    public void FillTextField(SpellContainer spellContainer)
    {
    }

    public void FillTextField(string text, string _titleText = "", Sprite _sprite = null)
    {
        this.titleText.text = _titleText;
        textFieldMain.GetComponent<TextMeshProUGUI>().text = text;
        if(_sprite != null)tipImage.sprite = _sprite;
        else tipImage.sprite = defaultSprite;

    }

    public void OpenTipWindow(Vector2 mousePos)
    {
        if (tipWindow != null)
        {
            if (textFieldMain.GetComponent<TextMeshProUGUI>().text == "") return;
            tipWindow.SetActive(true); 
        }
        else return;


       if (mousePos.x + tipWindowRect.rect.width > Screen.width)
        {
            tipWindowRect.pivot = new Vector2(1, tipWindowRect.pivot.y);
        }
        else
        {
            tipWindowRect.pivot = new Vector2(0, tipWindowRect.pivot.y);
        }
        if (mousePos.y + tipWindowRect.rect.height > Screen.height)
        {
            tipWindowRect.pivot = new Vector2(tipWindowRect.pivot.x,1) ;
        }
        else
        {
            tipWindowRect.pivot = new Vector2(tipWindowRect.pivot.x, 0);
        }

        tipWindowRect.position = new Vector3(mousePos.x, mousePos.y, 0);



    }

    public void CloseTipWindow()
    {
        textFieldMain.GetComponent<TextMeshProUGUI>().text = "";
        if (tipWindow !=null)tipWindow.SetActive(false); 
        print("CloseTipWindow");
        titleText.text = "";

    }
}
