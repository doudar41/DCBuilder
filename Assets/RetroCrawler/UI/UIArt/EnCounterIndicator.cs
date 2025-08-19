using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnCounterIndicator : MonoBehaviour
{

    [SerializeField] List<Sprite> indicatorSprites = new List<Sprite>();
    [SerializeField] Image imageIndicator;
    int state = 0;

    public void Indicator(int amount)
    {
        //print(GameInstance.playerController.rangeOfEnCounter.y - amount +  "encounter");
        if(GameInstance.playerController.rangeOfEnCounter.y - amount < GameInstance.playerController.rangeOfEnCounter.y/2)
        {
            if (state != 0) 
            { 
                state = 0;
                StartCoroutine(CounterAnimation(state));
            }
        }
        if (GameInstance.playerController.rangeOfEnCounter.y - amount >= GameInstance.playerController.rangeOfEnCounter.y / 2
            && GameInstance.playerController.rangeOfEnCounter.y - amount < GameInstance.playerController.rangeOfEnCounter.y - 3)
        {
            if (state != 1)
            {
                state = 1;
                StartCoroutine(CounterAnimation(state));
            }
        }
        if (GameInstance.playerController.rangeOfEnCounter.y - amount >= GameInstance.playerController.rangeOfEnCounter.y - 3)
        {
            if (state != 2)
            {
                state = 2;
                StartCoroutine(CounterAnimation(state));
            }
        }

    }

    IEnumerator CounterAnimation(int state)
    {
        switch (state)
        {
            case 0:
                for(int i = 3; i < 7; i++)
                {
                    yield return new WaitForSeconds(0.1f);
                    imageIndicator.sprite = indicatorSprites[i];
                }
                yield return new WaitForFixedUpdate();
                imageIndicator.sprite = indicatorSprites[0];
                break;
            case 1:
                for (int i = 3; i < 7; i++)
                {
                    yield return new WaitForSeconds(0.1f);
                    imageIndicator.sprite = indicatorSprites[i];
                }
                yield return new WaitForEndOfFrame();
                imageIndicator.sprite = indicatorSprites[1];
                break;
            case 2:
                for (int i = 3; i < 7; i++)
                {
                    yield return new WaitForSeconds(0.1f);
                    imageIndicator.sprite = indicatorSprites[i];
                }
                yield return new WaitForEndOfFrame();
                imageIndicator.sprite = indicatorSprites[2];
                break;
        }

        yield return null;
    }

}
