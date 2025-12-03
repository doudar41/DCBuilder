
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class SaveFileToggleContainer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textContainer;
    [SerializeField] Image image01, image02;
    public string fileNameToggle = "";

    public void SetFileName(string fileName)
    {
        fileNameToggle = fileName;
        textContainer.text = fileNameToggle;
    }

    public void Visibility(bool visible)
    {
        if (visible) { image01.enabled = true; image02.enabled = true; textContainer.enabled = true; }
        else { image01.enabled = false; image02.enabled = false; textContainer.enabled = false; }   
    }

}
