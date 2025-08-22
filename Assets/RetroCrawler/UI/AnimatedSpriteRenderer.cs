using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSpriteRenderer : MonoBehaviour
{
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] int playTimes = 0;
    [SerializeField] float delayMultiplier;
    [SerializeField] List<Sprite> sprites = new List<Sprite>();
    int count = 0, countsnapshot = 0, countplays, countSprites;
    // Start is called before the first frame update
    void Start()
    {
        countplays = playTimes* sprites.Count;
    }

    void Update()
    {
        //print(count + " " + countsnapshot);
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

}
