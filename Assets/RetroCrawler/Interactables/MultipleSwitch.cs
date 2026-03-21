using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleSwitch : MonoBehaviour, IDoor
{
    System.Guid _guid;
    [SerializeField] string GUIDString;

    [SerializeField] GameObject doorToOpen;
    [SerializeField] int switchesToActivate;
    [SerializeField] List<GameObject> switchToActivateOrder = new List<GameObject>();

    [SerializeField] bool inOrder = false, autoSwitchReset = false;
    IDoor doorScript;
    bool isOpened = false;
    List<int> lockOrder = new List<int>();
    List<GameObject> switchList = new List<GameObject>();

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
        doorScript = doorToOpen.GetComponent<IDoor>();
        if (GameInstance.savedItemsState.ContainsKey(GUIDString))
        {
            if(GameInstance.savedItemsState[GUIDString] == SavedState.Opened)
            {
                doorScript.OpenDoor();
                isOpened = true;
            }
            if (GameInstance.savedItemsState[GUIDString] == SavedState.Closed)
            {
                doorScript.CloseDoor();
                isOpened = false;
            }

        }
    }


    public void CloseDoor()
    {

    }

    public bool isOpen()
    {
        return isOpened;
    }

    public void OpenDoor()
    {
        
    }

    public void WeightDoor(int weightTarget, int weightAmount)
    {
       
    }

    public void OpenDoor(int index, GameObject _switch)
    {
        if(isOpened) return;
        if (!inOrder)
        {

            if (!lockOrder.Contains(index)) lockOrder.Add(index);
            if (lockOrder.Count >= switchesToActivate)
            {
                doorScript.OpenDoor();
                isOpened = true;
                GameInstance.spellbook.BattleLogMessage(new List<string>() { "Door opened" }, null);
                GameInstance.SaveItemState(GUIDString, SavedState.Opened, null);
                return;
            }
            isOpened = false;
            if(doorScript.isOpen()) doorScript.CloseDoor();
        }
        else
        {
            if (!switchList.Contains(_switch)) { switchList.Add(_switch); }
            if (switchToActivateOrder.Count != switchList.Count)
            {

                return;
            }
            for (int i = 0; i < switchList.Count; i++)
            {

                if (switchToActivateOrder[i] != switchList[i])
                {
                    print(switchList.Count + " " + (switchToActivateOrder[i] == switchList[i]));
                    GameInstance.spellbook.BattleLogMessage(new List<string>() { "Something is not right" }, null);
                    if (doorScript.isOpen()) doorScript.CloseDoor();
                    isOpened = false;
                    if (autoSwitchReset)
                    {
   
                        foreach (GameObject switchObj in switchList)
                        {
                            switchObj.GetComponent<SingleSwitch>().ResetSwitch();
                        }
                        switchList.Clear();
                        GameInstance.spellbook.BattleLogMessage(new List<string>() { "Let's start again" }, null);
                    }
                    return;
                }
            }
            doorScript.OpenDoor();
            isOpened = true;
            GameInstance.spellbook.BattleLogMessage(new List<string>() { "Door opened" }, null);
            GameInstance.SaveItemState(GUIDString, SavedState.Opened, null);

        }
    }

    public void CloseDoor(int index, GameObject _switch)
    {
        if (isOpened) return;
        if (!inOrder)
        {
            if (lockOrder.Contains(index)) { lockOrder.Remove(index);}
            if (lockOrder.Count >= switchesToActivate)
            {
                doorScript.OpenDoor();
                isOpened = true;
                GameInstance.spellbook.BattleLogMessage(new List<string>() { "Door opened" }, null);
                GameInstance.SaveItemState(GUIDString, SavedState.Opened, null);
                return;
            }
            isOpened = false;
            if (doorScript.isOpen()) doorScript.CloseDoor();
           
        }
        else
        {
            if (switchList.Contains(_switch)) { switchList.Remove(_switch); }
            if (switchToActivateOrder.Count != switchList.Count)
            {

                return;
            }
            for (int i = 0; i < switchList.Count; i++)
            {
                if (switchList[i] != switchToActivateOrder[i])
                {
                    print(switchList.Count + " " + (switchToActivateOrder[i] == switchList[i]));
                    isOpened = false;
                    if (doorScript.isOpen()) doorScript.CloseDoor();
                    if (autoSwitchReset)
                    {

                        lockOrder.Clear();
                        foreach (GameObject switchObj in switchList)
                        {
                            switchObj.GetComponent<SingleSwitch>().ResetSwitch();
                        }
                        switchList.Clear();
                        GameInstance.spellbook.BattleLogMessage(new List<string>() { "Let's start again" }, null);
                    }
                    return;
                }
            }
            doorScript.OpenDoor();
            isOpened = true;
            GameInstance.spellbook.BattleLogMessage(new List<string>() { "Door opened" }, null);
            GameInstance.SaveItemState(GUIDString, SavedState.Opened, null);
        }
    }
}
