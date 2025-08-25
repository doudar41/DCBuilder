using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SaveFileToggleContainer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textContainer; 
    public string fileNameToggle = "";

    public void SetFileName(string fileName)
    {
        fileNameToggle = fileName;
        textContainer.text = fileNameToggle;
    }

}
