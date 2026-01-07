using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ami.BroAudio;


public class SoundManager : MonoBehaviour
{
    [SerializeField] SoundID battleMusic, exploreMusic;
    [SerializeField] List<ExploreMusicFromEnvironment> exploreMusicFromEnvironments = new List<ExploreMusicFromEnvironment>();
    Dictionary<BattleGroundEnvironment, SoundID> exploreMusicDict = new Dictionary<BattleGroundEnvironment, SoundID>();
    SoundID currentExploreMusic;
    BattleGroundEnvironment currentExploreEnvironment = BattleGroundEnvironment.NONE;
    [SerializeField] List<SoundID> footstepSounds = new List<SoundID>();


#if BroAudio_InitManually
        public static void Init()
        {
            SoundManager.Init();
        }
#endif

    private void Awake()
    {
        GameInstance.soundManager = this;
        foreach (ExploreMusicFromEnvironment emfe in exploreMusicFromEnvironments)
        {
            exploreMusicDict.Add(emfe.battleGroundEnvironment, emfe.exploreMusicID);
        }
    }
    private void Start()
    {
        /*        if (!BroAudio.HasAnyPlayingInstances(exploreMusic)) BroAudio.Play(exploreMusic).SetVolume(0.4f);
                currentExploreMusic = exploreMusic;*/
        //ChangeMusicOnStep(GameInstance.playerController.GetBattleGroundEnvironment());


    }

    private void OnDestroy()
    {
        BroAudio.Stop(battleMusic);
        BroAudio.Stop(currentExploreMusic);
    }

    public void ChangeExploreMusicOnBattleGround(BattleGroundEnvironment battleGroundEnvironment)
    {
        if (battleGroundEnvironment == currentExploreEnvironment) return;
        currentExploreEnvironment = battleGroundEnvironment;
        SetExplorationMusicIndex(exploreMusicDict[battleGroundEnvironment]);
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
        ChangeExploreMusicOnBattleGround(GameInstance.playerController.GetBattleGroundEnvironment());
        switch (groundType)
        {
            case GroundType.Concrete:
                BroAudio.Play(footstepSounds[0]);
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

[System.Serializable]
public struct ExploreMusicFromEnvironment
{
    public BattleGroundEnvironment battleGroundEnvironment;
    public SoundID exploreMusicID;
}