using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSpriteSwitcher : MonoBehaviour
{

    [SerializeField] int playTimes = 0;
    [SerializeField] float delayMultiplier;
    [SerializeField] List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    int count = 0, countsnapshot = 0, countplays;
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
        sprites[Random.Range(0, sprites.Count)].color = Color.clear;
        sprites[Random.Range(0, sprites.Count)].color = Color.white;
    }

}
