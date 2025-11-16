using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BattleLogLine : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] List<TextMeshProUGUI> textPlaces = new List<TextMeshProUGUI>();
    [SerializeField] Transform textsParent;
    [SerializeField] TMP_FontAsset font;
    [SerializeField] GameObject textbox;
 
    private void Start()
    {
        /*        foreach(TextMeshProUGUI t in textsParent.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    textPlaces.Add(t);
                }*/
        BackToBottom();
    }


    public void LogTexts(List<string> texts, List<ResultMsg> results)
    {
        if (GameInstance.playerController.playerState != PlayerState.Battle) return;
        GameObject g  = Instantiate(textbox,textsParent);
        TextMeshProUGUI textLog = g.GetComponent<TextMeshProUGUI>();
        g.transform.SetAsFirstSibling();
        textLog.text = textLog.text + " " + System.DateTime.Now.Hour+"." + System.DateTime.Now.Minute + "." + System.DateTime.Now.Second; 

        foreach (string s in texts)
        {
            textLog.text = textLog.text + " " + s;
        }
        if (results == null) return;
/*        for(int i = 0; i < digits.Count; i+=3)
        {
            if (i + 1 >= digits.Count ) break;
            if (int.Parse(digits[i]) >= int.Parse(digits[i+1]))
            {
                if (i + 2 >= digits.Count || i + 3 >= digits.Count) break;
                textLog.text = textLog.text + " success ";
                textLog.text = textLog.text + " damage " + digits[i+2];
            }
            else textLog.text = textLog.text + " miss ";
        }*/
        foreach(ResultMsg r in results)
        {
            switch (r.msgType)
            {
                case "i":
                    textLog.text = textLog.text + " " + r.msgInt;
                    break;
                    case "f":   
                    textLog.text = textLog.text + " " + r.msgFloat;
                    break;
                    case "s":
                    textLog.text = textLog.text + " " + r.msgString;
                    break;
            }
        }



    }

    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(gameObject.GetComponent<RectTransform>().anchoredPosition.x, -232);

    }

    public void BackToBottom()
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(gameObject.GetComponent<RectTransform>().anchoredPosition.x, -590);
    }
}
