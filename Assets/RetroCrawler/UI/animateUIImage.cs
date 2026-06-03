
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class animateUIImage : MonoBehaviour
{
    [SerializeField] Image uiImage;
    [SerializeField] List<Sprite> sprites;
    [SerializeField] float frameRate = 0.1f;
    [SerializeField] bool playOnce = false, startOnEnable = false;
    [SerializeField] Sprite emptySpell;
    Dictionary<string, List<Sprite>> savedSpriteLists  = new Dictionary<string, List<Sprite>>();
    string currentName;
    bool stopAnimation = false;

    public UnityEvent onAnimationEnd;

    private void Start()
    {

    }

    private void OnEnable()
    {
        if (startOnEnable) StartAnimation();
    }

    public void StartAnimation()
    {
        if(!uiImage.gameObject.activeSelf) uiImage.gameObject.SetActive(true);
        StartCoroutine(AnimateImage());
    }
    public void StopAnimation()
    {
        StopCoroutine(AnimateImage());
    }
    IEnumerator AnimateImage()
    {
        foreach (var sprite in sprites)
        {
            uiImage.sprite = sprite;
            yield return new WaitForSeconds(frameRate);
        }
        if(emptySpell !=null)uiImage.sprite = emptySpell;
        if(!playOnce)
        {
            StartCoroutine(AnimateImage());
        }
        if (playOnce) onAnimationEnd.Invoke();
        yield return null;

    }

    public void FillSpriteList(string effectName, List<Sprite> _sprites)
    {
        currentName = effectName;   
        if (savedSpriteLists.ContainsKey(effectName)) return;
        List<Sprite> listSprites = new List<Sprite>();
        foreach(Sprite s in _sprites)
        {
            listSprites.Add(s);
        }
        savedSpriteLists.Add(effectName, listSprites);
    }

    public void StartFXAnimation(string effectName)
    {
        if(!savedSpriteLists.ContainsKey((string)effectName)) return;
        StartCoroutine(AnimateSavedFX(effectName));
        stopAnimation = false;
    }

    public void StopFXAnimation()
    {
        stopAnimation = true;
        StopCoroutine(AnimateSavedFX(currentName));
        if (emptySpell != null) uiImage.sprite = emptySpell;

    }

    IEnumerator AnimateSavedFX(string nameFX)
    {

        for (int i=0;i< savedSpriteLists[nameFX].Count;i++)
        {
            uiImage.sprite = savedSpriteLists[nameFX][i];
            yield return new WaitForSeconds(frameRate);
        }
        if (!playOnce && !stopAnimation)
        {
            StartFXAnimation(nameFX);
        }
        if (playOnce || stopAnimation) { if (emptySpell != null) uiImage.sprite = emptySpell; }
        yield return null;

    }

}
