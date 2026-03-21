using Ami.BroAudio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBattleLogMessage : MonoBehaviour
{

    [SerializeField] string textToBattleLog;
    [SerializeField] SoundID specialMessageSound = default;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            GameInstance.spellbook.ResultsToBattleLog(new List<string>() { textToBattleLog }, null);
            GameInstance.soundManagerInGame.ProtectedPlay(specialMessageSound);
        }
    }
}
