using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageTurn : MonoBehaviour
{
    [SerializeField] List<GameObject> Pages = new List<GameObject>();
    [SerializeField] List<Sprite> flipSprites = new List<Sprite>();
    [SerializeField] Image flipPageImage;
    [SerializeField] float animDelay =0.2f;

    int currentPagesIndex = 0;

    public void TurnPageRight()
    {
        currentPagesIndex = Mathf.Clamp(currentPagesIndex + 1, 0, Pages.Count - 1);
        Pages[currentPagesIndex].transform.SetSiblingIndex(Pages.Count-1);

    }
    public void TurnPageLeft()
    {
        currentPagesIndex = Mathf.Clamp(currentPagesIndex - 1, 0, Pages.Count - 1);
        Pages[currentPagesIndex].transform.SetSiblingIndex(Pages.Count - 1);
    }

    public void FlipPagesStart(bool side)
    {
        StartCoroutine(FlipPages(side));
    }

    IEnumerator FlipPages(bool side)
    {

        if (side)
        {
            for (int i = 0; i < flipSprites.Count; i++)
            {
                flipPageImage.sprite = flipSprites[i];
                yield return new WaitForSeconds(animDelay * Time.deltaTime);
            }
            TurnPageRight();
        }
        else
        {
            for (int i = flipSprites.Count-1; i >-1; i--)
            {
                flipPageImage.sprite = flipSprites[i];
                yield return new WaitForSeconds(animDelay * Time.deltaTime);
            }
            TurnPageLeft();
        }

        yield return null;
    }
}
