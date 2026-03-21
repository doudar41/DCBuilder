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
    [SerializeField] SoundRoom soundroom;
    RoomSpaces currentRoomspace = RoomSpaces.None;

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

        foreach (ExploreMusicFromEnvironment emfe in exploreMusicFromEnvironments)
        {
           BroAudio.Stop( emfe.exploreMusicID); 
        }
        // print("start playing "+startingMusic);
        BroAudio.Play(startingMusic);
        currentExploreMusic = startingMusic;

    }

    private void OnDestroy()
    {
        print("destroying sound manager, stopping music");
        BroAudio.Stop(battleMusic);
        BroAudio.Stop(currentExploreMusic);
    }

    public void ProtectedPlay(SoundID soundID)
    {
        if (soundID != default) BroAudio.Play(soundID);
    }

    public void StopCurrentMusic()
    {
        if (currentExploreMusic !=default) BroAudio.Stop(currentExploreMusic,1f);
    }

    public void ChangeExploreMusicOnBattleGround(BattleGroundEnvironment battleGroundEnvironment)
    {
        if (battleGroundEnvironment == currentExploreEnvironment) return;
        currentExploreEnvironment = battleGroundEnvironment;
        SetExplorationMusicIndex(exploreMusicDict[battleGroundEnvironment]);
    }

    public void DuckExploreMusicSwitchToAmbience(RoomSpaces roomSpace)
    {
        if (roomSpace == RoomSpaces.None) return;
        currentRoomspace = roomSpace;
        BroAudio.SetVolume(currentExploreMusic, 0.2f, 0.5f);
        soundroom.SwitchSoundRoom(roomSpace, true);
    }
    public void UnDuckExploreMusicSwitchToAmbience()
    {
        if (currentRoomspace == RoomSpaces.None) return;
        BroAudio.SetVolume(currentExploreMusic, 1f, 0.5f);
        soundroom.SwitchSoundRoom(currentRoomspace, false);
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
        if (GameInstance.playerController.GetBattleGroundEnvironment() == BattleGroundEnvironment.NONE)
        {
            BroAudio.Play(footstepSounds[0]); return; }
        print("sound of ground "+ GameInstance.playerController.GetBattleGroundEnvironment());
        ChangeExploreMusicOnBattleGround(GameInstance.playerController.GetBattleGroundEnvironment());
        switch (groundType)
        {
            case GroundType.Concrete:
                BroAudio.Play(footstepSounds[0]);
                break;
            case GroundType.Sand:
                break;
            case GroundType.Dirt:
                BroAudio.Play(footstepSounds[0]);
                break;
            case GroundType.Snow:
                break;
            case GroundType.Fire:
                break;
            case GroundType.Water:
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