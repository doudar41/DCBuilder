using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IInteractables {

    public List<InteractablesEnum> WhatIsIt();
    public int GetWeight(out int carringCapacity);


}
public enum InteractablesEnum
{
    ENEMY,
    DOOR,
    SWITCH,
    LEVEL_EXIT,
    PORTAL,
    LADDER,
    TRAP,
    STORY,
    PICKABLE,
    WEIGHTPLATE,
    PLAYER,
    HERO
}
