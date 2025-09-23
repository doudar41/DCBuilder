using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeroStatsDefault
{
    static  Dictionary<MainStat, int> defaultMainStats = new Dictionary<MainStat, int>() 
    { 
        { MainStat.Strength, 4 },
        { MainStat.Agility, 4 },
        { MainStat.Endurance, 4 },
        { MainStat.Mind, 4 },
        { MainStat.Willpower, 4 },
        { MainStat.Survival, 4 },
    };


    static Dictionary<DependedStat, int> defaultDependedStats = new Dictionary<DependedStat, int>()
    {
        { DependedStat.heroLevel, 1},
        { DependedStat.maxHealth, 100}, 
        { DependedStat.maxMana, 100},

        { DependedStat.meleeDamage, 0},
        { DependedStat.rangeDamage, 0},
        { DependedStat.defence, 1},
        { DependedStat.initiative, 1},
        { DependedStat.accuracy, 1},
        { DependedStat.evasion, 1},
        { DependedStat.FireResistance, 0},
        { DependedStat.AirResistance, 0},
        { DependedStat.EarthResistance, 0},
        { DependedStat.WaterResistance, 0},
        { DependedStat.DarkResistance, 0},

        { DependedStat.CarryingCapacity, 15},
        { DependedStat.Hunger, 28800}
    };


    public static int GetDefaultDependedStat(DependedStat dependedStat)
    {
        if (defaultDependedStats.TryGetValue(dependedStat, out int amount)) return amount;
        else return -1;
    }

    public static Dictionary<DependedStat, int> GetFullDependedStats()
    {
        return defaultDependedStats;
    }


    public static Dictionary<MainStat, int> GetFullMinStats()
    {
        return defaultMainStats;
    }
}
