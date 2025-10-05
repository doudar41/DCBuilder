
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class DialogueButtonUI : MonoBehaviour
{
    public int buttonIndex;
    public string buttonText;
    [SerializeField] TextMeshProUGUI buttonTextUI;
    UniqueDialogueName uniqueDialogueName;
    [SerializeField] Button button;


    public UnityEvent<UniqueDialogueName> getDialogueName;

    private void Start()
    {
        button.onClick.AddListener(ClickButton);
    }

    public void SetDialogueName(UniqueDialogueName _uniqueDialogueName)
    {
        uniqueDialogueName = _uniqueDialogueName;
    }

    void ClickButton()
    {
        getDialogueName.Invoke(uniqueDialogueName);
    }
}
