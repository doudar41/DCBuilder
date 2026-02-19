using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="Portraits")]
[System.Serializable]
public class PortraitContainer : ScriptableObject
{
    public List<Portrait> portraits = new List<Portrait>();
    Dictionary<GameplayStates, Sprite> portraitDict = new Dictionary<GameplayStates, Sprite>();
    Dictionary<GameplayStates, List<Sprite>> animationDict = new Dictionary<GameplayStates, List<Sprite>>();

    private void OnEnable()
    {
        PortritInit();
    }
    public void PortritInit()
    {
        portraitDict.Clear();
        animationDict.Clear();
        foreach(Portrait p in portraits)
        {
            portraitDict.Add(p.state, p.sprite);
            animationDict.Add(p.state, p.animationState);
        }
    }


    public bool GetStatePortrait(GameplayStates state, out Sprite sprite)
    {
        if(portraitDict.TryGetValue(state, out sprite)) { return true; }
        foreach (Portrait p in portraits)
        {
            if (p.state == state) { sprite = p.sprite; return true;  }
        }
        sprite = null;
        return false;
    }

    public bool IsAnimatedState(GameplayStates state, out List<Sprite> animation)
    {
        if(animationDict.TryGetValue(state, out animation)) { return true; }
        foreach (Portrait p in portraits)
        {
            if (p.state == state) { animation = p.animationState; return true; }
        }
        animation = null;
        return false;
    }

}

[System.Serializable]
public class  Portrait
{
    public GameplayStates state;
    public Sprite sprite;
    public List<Sprite> animationState = new List<Sprite>();
}