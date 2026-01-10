
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class animateUIImage : MonoBehaviour
{
    [SerializeField] Image uiImage;
    [SerializeField] List<Sprite> sprites;
    [SerializeField] float frameRate = 0.1f;
    [SerializeField] bool playOnce = false;


    public void StartAnimation()
    {
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
        if(!playOnce)
        {
            StartCoroutine(AnimateImage());
        }
        yield return null;

    }
}
