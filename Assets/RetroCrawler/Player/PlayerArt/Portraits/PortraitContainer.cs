using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="Portraits")]
[System.Serializable]
public class PortraitContainer : ScriptableObject
{
    public List<Portrait> portraits = new List<Portrait>();

    public bool GetStatePortrait(GameplayStates state, out Sprite sprite)
    {
        foreach(Portrait p in portraits)
        {
            if (p.state == state) { sprite = p.sprite; return true;  }
        }
        sprite = null;
        return false;
    }
}

[System.Serializable]
public class  Portrait
{
    public GameplayStates state;
    public Sprite sprite;
}