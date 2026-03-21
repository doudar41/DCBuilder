using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Trap : MonoBehaviour
{

    System.Guid _guid;

    [SerializeField] string GUIDString;
    [SerializeField] TrapLauncher trapLauncher;
    bool _enabled = true;

    private void OnValidate()
    {
        if (GUIDString == "")
        {
            _guid = System.Guid.NewGuid();
            GUIDString = _guid.ToString();
        }
    }
    private void Awake()
    {
        GameInstance.initItems += Init;
    }
    private void OnDestroy()
    {
        GameInstance.initItems -= Init;
    }

    void Init()
    {
        if (GameInstance.savedItemsState.ContainsKey(GUIDString))
        {
            _enabled = false;
        }
    }


    public void TriggerTrap()
    {
        if (_enabled) 
        {
            trapLauncher.LaunchMissile();
        }

    }


    public void SwitchOffTrap()
    {
        GameInstance.savedItemsState.Add(GUIDString, SavedState.Opened);
        _enabled = false;
    }


}
