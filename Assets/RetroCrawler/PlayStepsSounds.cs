using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayStepsSounds : MonoBehaviour
{
    [SerializeField] List<AudioClip> stepSound;
    Queue<int> playedSounds = new Queue<int>();


    public void PlayStepSound()
    {
        if (stepSound.Count == 0) return;
        AudioSource audio = GetComponent<AudioSource>();
        if (audio == null)
        {
            audio = gameObject.AddComponent<AudioSource>();
        }
        if(playedSounds.Count == 0) {
            stepSound.Shuffle();
            for (int i = 0; i < stepSound.Count; i++)
            {
                playedSounds.Enqueue(i);
            }
        }

        audio.PlayOneShot(stepSound[playedSounds.Dequeue()]);
    }
}
