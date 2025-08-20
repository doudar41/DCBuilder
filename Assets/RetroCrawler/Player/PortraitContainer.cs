using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="Portraits")]
public class PortraitContainer : ScriptableObject
{
    public List<Portrait> portraits = new List<Portrait>();

    public bool GetStatePortrait(GameplayStatus state, out Sprite sprite)
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
    public GameplayStatus state;
    public Sprite sprite;
}