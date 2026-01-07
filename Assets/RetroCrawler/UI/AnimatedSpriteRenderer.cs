using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSpriteRenderer : MonoBehaviour
{
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] int playTimes = 0;
    [SerializeField] float delayMultiplier;
    [SerializeField] List<Sprite> sprites = new List<Sprite>();
    [SerializeField] bool dayNightCycle = false;
    int count = 0, countsnapshot = 0, countplays, countSprites;
    bool isNight = true;
    // Start is called before the first frame update

    private void Awake()
    {
        if (dayNightCycle)
        {
            GameInstance.progress += DayNightChange;
        }
    }
    void Start()
    {
        countplays = playTimes* sprites.Count;
    }

    void Update()
    {
        if (isNight == false) { renderer.sprite = sprites[sprites.Count-1];  return; }
        if (count - countsnapshot >= (delayMultiplier/Time.deltaTime)*10)
        {
            
            countsnapshot = count;
            if(playTimes == 0) 
            { 
                PlayOnce(); 
            }
            if (playTimes > 0 && countplays > 0)
            {
                PlayOnce();
                countplays--;
            }
        }
        count++;
        if (count >= int.MaxValue - 100) count = 0;
    }


    public void PlayOnce()
    {
        renderer.sprite = sprites[countSprites%sprites.Count];
        countSprites++;
        if (countSprites >= int.MaxValue - 100) countSprites = 0;
    }

    void DayNightChange(int count)
    {

        //print(GameInstance.GetNormalTime()[1].ToString() + ":" + GameInstance.GetNormalTime()[2].ToString()+":"+GameInstance.GetNormalTime()[3].ToString());
        if (GameInstance.GetNormalTime()[1]%24 >= 6 && GameInstance.GetNormalTime()[1]%24 < 19)
        {
            isNight = false;
        }
        else
        {
            isNight = true;
        }
    }




}
