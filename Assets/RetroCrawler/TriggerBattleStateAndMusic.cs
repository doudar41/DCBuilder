using Ami.BroAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBattleStateAndMusic : MonoBehaviour
{
    [SerializeField] SoundID previousExploreMusic, exploreMusicLocal;
    [SerializeField] bool noEncounter = false;
    [SerializeField] List<EnemySized> listOfEnemies = new List<EnemySized>();
    private void OnTriggerEnter(Collider other)
    {
        GameInstance.playerController.SetEncounter(noEncounter);
        if (!BroAudio.HasAnyPlayingInstances(exploreMusicLocal))
        {
            BroAudio.Stop(previousExploreMusic, 0.5f);
            BroAudio.Play(exploreMusicLocal, 0.5f);
            GameInstance.battleManager.SetExplorationMusicIndex(exploreMusicLocal);
        }
        if(!noEncounter)
        {
           GameInstance.battleManager.SetListOfEnemies(listOfEnemies);
        }

    }
    
}
