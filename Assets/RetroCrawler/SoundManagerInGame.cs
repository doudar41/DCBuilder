using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ami.BroAudio;


public class SoundManagerInGame : MonoBehaviour
{
    [SerializeField] SoundID battleMusic, exploreMusic;
    [SerializeField] List<ExploreMusicFromEnvironment> exploreMusicFromEnvironments = new List<ExploreMusicFromEnvironment>();

    Dictionary<BattleGroundEnvironment, SoundID> exploreMusicDict = new Dictionary<BattleGroundEnvironment, SoundID>();
    SoundID currentExploreMusic = default;
    BattleGroundEnvironment currentExploreEnvironment = BattleGroundEnvironment.NONE;
    [SerializeField] List<SoundID> footstepSounds = new List<SoundID>();
    [SerializeField] SoundID startingMusic = default;

#if BroAudio_InitManually
        public static void Init()
        {
            SoundManager.Init();
        }
#endif

    private void Awake()
    {
        GameInstance.soundManagerInGame = this;
        foreach (ExploreMusicFromEnvironment emfe in exploreMusicFromEnvironments)
        {
            exploreMusicDict.Add(emfe.battleGroundEnvironment, emfe.exploreMusicID);
        }
    }
    private void Start()
    {

        print("start playing "+startingMusic);
        BroAudio.Play(startingMusic);
        currentExploreMusic = startingMusic;
        /*        if (currentExploreMusic != startingMusic)
                {
                    if (BroAudio.HasAnyPlayingInstances(currentExploreMusic)) BroAudio.Stop(currentExploreMusic, 1f);
                    BroAudio.Play(startingMusic);
                    currentExploreMusic = startingMusic;
                }
                else
                {
                    currentExploreMusic = startingMusic;
                    if (!BroAudio.HasAnyPlayingInstances(currentExploreMusic)) BroAudio.Play(currentExploreMusic, 1f);
                }*/
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
        if (currentExploreMusic != default) BroAudio.Play(currentExploreMusic, 0.5f);
        else
        {
            if(GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.NONE) ChangeExploreMusicOnBattleGround(BattleGroundEnvironment.CITY);
            else ChangeExploreMusicOnBattleGround(GameInstance.playerController.GetBattleGroundEnvironment());
        }
    }


    public void PlayFootsteps(GroundType groundType)
    {
        if (GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.NONE) return;

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


    public void DuckingCurrentMusic(SoundID forwardTrack) 
    {         
        BroAudio.SetVolume(currentExploreMusic, 0.2f, 0.5f);
        BroAudio.Play(forwardTrack);
    }

    public void UnduckingCurrentMusic(SoundID forwardTrack)
    {
        BroAudio.SetVolume(currentExploreMusic, 1.0f, 0.5f);
        BroAudio.Stop(forwardTrack, 0.5f);
    }


}

[System.Serializable]
public struct ExploreMusicFromEnvironment
{
    public BattleGroundEnvironment battleGroundEnvironment;
    public SoundID exploreMusicID;
}