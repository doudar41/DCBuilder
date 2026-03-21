
using Ami.BroAudio;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSounds : MonoBehaviour, IPointerEnterHandler
{
    Button button;
    [SerializeField] SoundID clickButton, hoverButton;


    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ClickButton);

    }

    public void ClickButton()
    {
        if(clickButton !=default) BroAudio.Play(clickButton);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (clickButton != default) BroAudio.Play(hoverButton);
    }
}
