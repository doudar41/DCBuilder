using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ami.BroAudio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] SoundID battleMusic, exploreMusic;
    SoundID currentExploreMusic;

    private void Awake()
    {
        GameInstance.soundManager = this;
    }
    private void Start()
    {
        if (!BroAudio.HasAnyPlayingInstances(exploreMusic)) BroAudio.Play(exploreMusic).SetVolume(0.4f);
        currentExploreMusic = exploreMusic;
    }

    public void StartMusic()
    {

    }

    public void LaunchBattleMusic(BattleGroundEnvironment battleGroundEnvironment)
    {

        BroAudio.Play(battleMusic);
        BroAudio.Stop(currentExploreMusic, 0.3f);
    }

    public void ChangeExplorationMusic(SoundID _id)
    {

    }

    public void SetExplorationMusicIndex(SoundID _id)
    {
        if (!BroAudio.HasAnyPlayingInstances(_id))
        {
            BroAudio.Stop(currentExploreMusic, 0.5f);
            BroAudio.Play(_id, 0.5f);
            currentExploreMusic = _id;
        }
    }

    public void BackToCurrentExploreMusic()
    {
        BroAudio.Stop(battleMusic, 0.5f);
        BroAudio.Play(currentExploreMusic, 0.5f);
    }


    public void PlayFootsteps(GroundType groundType)
    {
        switch (groundType)
        {
            case GroundType.Concrete:
                break;
            case GroundType.Sand:
                break;
            case GroundType.Dirt:
                break;
            case GroundType.Snow:
                break;
            case GroundType.Fire:
                break;
            case GroundType.Water:
                break;
            case GroundType.None:
                break;
        }
    }


    public void PlayInterfaceSound(SoundID _id)
    {
        if (!BroAudio.HasAnyPlayingInstances(_id))
        {
            BroAudio.Play(_id);
        }
    }
}
