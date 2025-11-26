using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This will play status animations continuously to show enemy status effect

public class InitStatus : MonoBehaviour
{
    [SerializeField]Animator animator;

    private void Start()
    {

        animator.StartPlayback();
        animator.gameObject.SetActive(true);

        animator.speed = 0.5f;
    }

    public void PlayStatusAnimation(string statusName)
    {
        animator.CrossFade(statusName,0.1f);
    }
}
