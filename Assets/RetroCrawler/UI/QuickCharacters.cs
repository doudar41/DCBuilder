
using System.Collections.Generic;
using TMPro;

using UnityEngine;

public class QuickCharacters : MonoBehaviour
{
    [SerializeField] List<string> heroNamesSerialized = new List<string>();
    [SerializeField] List<MainStatsSave> mainStats01 , mainStats02, mainStats03, mainStats04;
    [SerializeField] List<SkillStatSave> skills01, skills02, skills03, skills04;
    [SerializeField] List<ItemScriptableContainer> weaponsChosen;
    [SerializeField] List<SpellContainer> spellsChosen01, spellsChosen02, spellsChosen03, spellsChosen04;

    [SerializeField] List<UniqueDialogueName> startingUniqueDialogueNames = new List<UniqueDialogueName>();

    Dictionary<int, Dictionary<MainStat, int>> mainS = new Dictionary<int, Dictionary<MainStat, int>>();
    Dictionary<int, int> portraitIndex = new Dictionary<int, int>() { { 0, -1 }, { 1, -1 }, { 2, -1 }, { 3, -1 } };

    Dictionary<int, List<SpellContainer>> spellsChosenImport = new Dictionary<int, List<SpellContainer>>();
    Dictionary<int, int> weaponChosenImport = new Dictionary<int, int>();
    Dictionary<int, List<SkillsStat>> skillsChosenImport = new Dictionary<int, List<SkillsStat>>();
    Dictionary<int, string> heroesNames = new Dictionary<int, string>() { {0, "" }, { 1, "" }, { 2, "" }, { 3, "" }};



    void SendStatsToGameInstance()
    {
        GameInstance.mainStatsAdded = GameInstance.ConvertMainStatsToSave(mainS);


        foreach (KeyValuePair<int, List<SkillsStat>> keysValues in skillsChosenImport)
        {
            foreach (SkillsStat skill in keysValues.Value)
            {
                //tempSkills.Add(skill, 5);
                SkillStatSave newskill = new SkillStatSave();
                newskill.heroIndex = keysValues.Key;
                newskill.skill = skill;
                newskill.amount = 5;
                GameInstance.skillStatSaves.Add(newskill);
            }
        }


        GameInstance.heroesPortraits = new List<int>() { portraitIndex[0], portraitIndex[1], portraitIndex[2], portraitIndex[3] };

        for (int i = 0; i < 4; i++)
        {
            HeroInventoryItem heroInventoryItem = new HeroInventoryItem();
                heroInventoryItem.heroIndex = i;
                heroInventoryItem.itemType = ItemType.WEAPON;
                heroInventoryItem.container = weaponChosenImport[i];
                heroInventoryItem.stackAmount = 1;
                heroInventoryItem.positionReplaced = Vector3.zero;
                heroInventoryItem.level = "Level01";
            GameInstance.AddInventoryItem(heroInventoryItem);
        }

        GameInstance.heroesNames = new List<string>() { heroesNames[0], heroesNames[1], heroesNames[2], heroesNames[3] };

        List<List<SpellContainer>> spellsChosen = new List<List<SpellContainer>>() { spellsChosen01, spellsChosen02, spellsChosen03, spellsChosen04 };

        for ( int i=0;i< spellsChosen.Count;i++)
        {
            HeroSpellbookSaved heroSpellbookSaved = new HeroSpellbookSaved();
            for (int j=0;j< spellsChosen[i].Count;j++)
            {

                heroSpellbookSaved.heroIndex = i;
                heroSpellbookSaved.spells = spellsChosen[i];
            }
            GameInstance.spellbooksSaved.Add(heroSpellbookSaved);
        }

    }

    public void QuickGameStart()
    {

        mainS.Clear();
        skillsChosenImport.Clear();
        List<List<MainStatsSave>> mainStats = new List<List<MainStatsSave>>() { mainStats01, mainStats02, mainStats03, mainStats04 };
        List<List<SkillStatSave>> skills = new List<List<SkillStatSave>>() { skills01, skills02, skills03, skills04 };
        for (int i = 0; i < 4; i++)
        {
            
            mainS.Add (i,new Dictionary<MainStat, int>() {  { MainStat.Strength, mainStats[i][0].amount}, 
                                                            { MainStat.Agility, mainStats[i][1].amount }, 
                                                            { MainStat.Mind, mainStats[i][2].amount }, 
                                                            { MainStat.Endurance, mainStats[i][3].amount }, 
                                                            { MainStat.Willpower, mainStats[i][4].amount },
                                                            { MainStat.Survival, mainStats[i][5].amount}});


            skillsChosenImport.Add(i, new List<SkillsStat>() { skills[i][0].skill, skills[i][1].skill });

            weaponChosenImport.Add(i, GameInstance.dataBase.GetItemIndexFromDataBase( weaponsChosen[i]));
        }
        GameInstance.heroesPortraits = new List<int>() { 1, 2, 3, 4 };
        GameInstance.heroesNames = new List<string>() { heroNamesSerialized[0], heroNamesSerialized[1], heroNamesSerialized[2], heroNamesSerialized[3] };
        GameInstance.currentUniqueDialogueNames = startingUniqueDialogueNames;

        SendStatsToGameInstance();
        GameInstance.LoadGameFromStart();
    }



}
