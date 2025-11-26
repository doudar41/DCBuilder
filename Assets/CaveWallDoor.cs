using Ami.BroAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveWallDoor : MonoBehaviour, IDoor, IInteractables
{

    [SerializeField] Transform doorTransform;
    [SerializeField] ParticleSystem wallFall1, wallFall2;
    [SerializeField] Collider _collider;
    [SerializeField] Animator doorAnimator;
    [SerializeField] SoundID doorSound;
    bool _isOpen = false;

    public void CloseDoor()
    {
        wallFall1.Play(); wallFall2.Play();
        _collider.enabled = true;
        _isOpen = false;

        doorAnimator.CrossFade("CloseCaveDoor",0.3f);
        BroAudio.Play(doorSound, transform);
    }

    public int GetWeight(out int carringCapacity)
    {
        carringCapacity = 0;
        return 0;
    }

    public bool isOpen()
    {
        return _isOpen;
    }

    public void OpenDoor()
    {
        wallFall1.Play(); wallFall2.Play();
        _collider.enabled = false;
        _isOpen = true;

        doorAnimator.CrossFade("OpenCaveDoor", 0.3f);
        BroAudio.Play(doorSound, transform);
    }

    public void WeightDoor(int weightTarget, int weightAmount)
    {
        
    }

    public List<InteractablesEnum> WhatIsIt()
    {
        return new() { InteractablesEnum.DOOR };
    }
}
