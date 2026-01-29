
using Ami.BroAudio;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterCreationPanel : MonoBehaviour
{
    [SerializeField] MainStatTextUI mainStatsUITexts;
    [SerializeField] DependedStatTextUI dependedStatsUIText;
    [SerializeField] SkillsStatTextUI skillsStatsUIText;
    [SerializeField] int pointsGiven = 10;
    [SerializeField] TextMeshProUGUI pointsTextField;
    [SerializeField] List<PortraitContainer> portraitSprites = new List<PortraitContainer>();
    [SerializeField] List<CharacterPortraitChoice> portraitChoices = new List<CharacterPortraitChoice>();
    [SerializeField] TMP_InputField nameField;
    [SerializeField] TextMeshProUGUI tipsField;

    [SerializeField] List<SpellChoiceIcon> spellsToChoose = new List<SpellChoiceIcon>();
    [SerializeField] List<WeaponChoiceIcon> weaponToChoose = new List<WeaponChoiceIcon>();
    [SerializeField] List<SkillChoiceIcon> skillsToChoose = new List<SkillChoiceIcon>();
    [SerializeField] SoundID mainTheme;
    int heroIndex = 0, mainStatIndex = 0, minStat, maxStat;

    Dictionary<int, Dictionary<MainStat, int>> mainS = new Dictionary<int, Dictionary<MainStat, int>>();
    Dictionary<int, int> pointsLeft = new Dictionary<int, int>() { { 0, 10 }, { 1, 10 }, { 2, 10 }, { 3, 10 } };
    Dictionary<int, int> portraitIndex = new Dictionary<int, int>() { { 0, -1 }, { 1, -1 }, { 2, -1 }, { 3, -1 } };

    Dictionary<int, List<SpellContainer>> spellsChosen = new Dictionary<int, List<SpellContainer>>();
    Dictionary<int, int> weaponChosen = new Dictionary<int, int>();
    Dictionary<int, List<SkillsStat>> skillsChosen = new Dictionary<int, List<SkillsStat>>();
    Dictionary<int, string> heroesNames = new Dictionary<int, string>() { {0, "" }, { 1, "" }, { 2, "" }, { 3, "" }};


    private void Start()
    {
        mainStatsUITexts.KeyPairFill();
        dependedStatsUIText.KeyPairFill();
        skillsStatsUIText.KeyPairFill();
        for(int i = 0; i < 4; i++)
        {
            Dictionary<MainStat, int> newMainStat = new Dictionary<MainStat, int>();
            foreach(KeyValuePair<MainStat, int> st in HeroStatsDefault.GetFullMinStats())
            {
                newMainStat.Add(st.Key,st.Value);
            }

            pointsLeft[i] = pointsGiven;
            if(!mainS.ContainsKey(i)) mainS.Add(i, newMainStat);
        }
        pointsTextField.text = pointsLeft[heroIndex].ToString();
        minStat = HeroStatsDefault.GetFullMinStats()[(MainStat)1];
        maxStat = pointsGiven + HeroStatsDefault.GetFullMinStats()[(MainStat)1];
    }

    private void RefreshStats()
    {

        foreach (KeyValuePair<MainStat, int> k in mainS[heroIndex])
        {
            //print(k.Key + " " + k.Value + " hero " + heroIndex);
            mainStatsUITexts.GetValue(k.Key).text = k.Value.ToString();
        }

        foreach (WeaponChoiceIcon sc in weaponToChoose)
        {
            sc.SetIconActive(false);
        }

        foreach (WeaponChoiceIcon sc in weaponToChoose)
        {
            if (weaponChosen.ContainsKey(heroIndex)) 
            { 
                if (weaponChosen[heroIndex] == sc.weapon) sc.SetIconActive(true); 
            }
        }


        foreach (SpellChoiceIcon sc in spellsToChoose)
        {
                sc.SetIconActive(false);
        }


        if (spellsChosen.ContainsKey(heroIndex))
        {
            foreach (SpellContainer st in spellsChosen[heroIndex])
            {
                foreach (SpellChoiceIcon sc in spellsToChoose)
                {
                    if (st == sc.spell) sc.SetIconActive(true);
                }
            }
        }


        foreach (SkillChoiceIcon sc in skillsToChoose)
        {
            sc.SetTextActive(false);
        }

        if (skillsChosen.ContainsKey(heroIndex))
        {
            foreach (SkillsStat st in skillsChosen[heroIndex])
            {
                foreach (SkillChoiceIcon sc in skillsToChoose)
                {
                    if (st == sc.skillsStat) sc.SetTextActive(true);
                }
            }
        }

        nameField.text = heroesNames[heroIndex];

    }

    public void SetHeroIndex(int index)
    {
        heroIndex = index;
        foreach(CharacterPortraitChoice cpc in portraitChoices)
        {
            cpc.ActivateHero(heroIndex);
        }
        
        RefreshStats();
        pointsTextField.text = pointsLeft[heroIndex].ToString();
    }

    public void ChangeName(string nameText)
    {
        heroesNames[heroIndex] = nameText;
    }


    public void ChangeMainStats(bool plusMinus)
    {
        if (plusMinus)
        {
            if (mainS[heroIndex][(MainStat)mainStatIndex] >= maxStat)
            {
                return;
            }

            if(pointsLeft[heroIndex] <=0)
            {
                return;
            }
            pointsLeft[heroIndex] = Mathf.Clamp(pointsLeft[heroIndex] - 1, 0, pointsGiven);

            mainS[heroIndex][(MainStat)mainStatIndex] = Mathf.Clamp(mainS[heroIndex][(MainStat)mainStatIndex] + 1, minStat, maxStat);

            pointsTextField.text = pointsLeft[heroIndex].ToString();
        }
        else
        {
            if (mainS[heroIndex][(MainStat)mainStatIndex] <= minStat)
            {

                return;
            }

            if (pointsLeft[heroIndex] >= pointsGiven)
            {
                return;
            }
            pointsLeft[heroIndex] = Mathf.Clamp(pointsLeft[heroIndex] + 1, 0, pointsGiven);

            mainS[heroIndex][(MainStat)mainStatIndex] = Mathf.Clamp(mainS[heroIndex][(MainStat)mainStatIndex] - 1, minStat, maxStat);

            pointsTextField.text = pointsLeft[heroIndex].ToString();
        }
        RefreshStats();
    }

    public void StatIndex(int index)
    {
        mainStatIndex = index;
    }


    public void CircleThroughPortrait(bool plusMinus)
    {
        if (plusMinus)
        {
            portraitIndex[heroIndex] = (portraitIndex[heroIndex] + 1) % portraitSprites.Count;
        }
        else
        {
            if ((portraitIndex[heroIndex] - 1) < 0) 
            { 
                portraitIndex[heroIndex] = portraitSprites.Count - 1; return; 
            }

            portraitIndex[heroIndex] = (portraitIndex[heroIndex] - 1) % portraitSprites.Count;
        }
        portraitSprites[portraitIndex[heroIndex]].GetStatePortrait(GameplayStatus.None, out Sprite sprite);
        portraitChoices[heroIndex].SetSprite(sprite);
    }


    public void ChooseSpell(SpellContainer spell)
    {
        if (!spellsChosen.ContainsKey(heroIndex))
        {
            spellsChosen.Add(heroIndex, new List<SpellContainer>());
        }
 
        if (spellsChosen[heroIndex].Contains(spell))
        {
            spellsChosen[heroIndex].Remove(spell);
            foreach (SpellChoiceIcon sc in spellsToChoose)
            {
                if(spell == sc.spell) sc.SetIconActive(false);
            }
        }
        else
        {
            if (spellsChosen[heroIndex].Count < 2)
            {
                spellsChosen[heroIndex].Add(spell);
                foreach (SpellChoiceIcon sc in spellsToChoose)
                {
                    if (spell == sc.spell) sc.SetIconActive(true);
                }
            }
        }
    }


    public void ChooseWeapon(int weapon)
    {
        if (!weaponChosen.ContainsKey(heroIndex))
        {
            weaponChosen.Add(heroIndex, -1);
        }


        if (weaponChosen[heroIndex] == weapon)
        {
            weaponChosen[heroIndex] = -1;
            foreach (WeaponChoiceIcon sc in weaponToChoose)
            {
                if (weapon == sc.weapon) sc.SetIconActive(false);
            }
        }
        else
        {
            if (weaponChosen[heroIndex] == -1)
            {
                weaponChosen[heroIndex] = weapon;
                foreach (WeaponChoiceIcon sc in weaponToChoose)
                {
                    if (weapon == sc.weapon) sc.SetIconActive(true);
                }
            }
        }

    }




    public void ChooseTwoSkills(SkillsStat skillStat)
    {
        if (!skillsChosen.ContainsKey(heroIndex))
        {
            skillsChosen.Add(heroIndex, new List<SkillsStat>());
        }

        if (skillsChosen[heroIndex].Contains(skillStat))
        {
            skillsChosen[heroIndex].Remove(skillStat);
            foreach (SkillChoiceIcon sc in skillsToChoose)
            {
                if (skillStat == sc.skillsStat) sc.SetTextActive(false);
            }
        }
        else
        {
            if (skillsChosen[heroIndex].Count < 2)
            {
                skillsChosen[heroIndex].Add(skillStat);
                foreach (SkillChoiceIcon sc in skillsToChoose)
                {
                    if (skillStat == sc.skillsStat) sc.SetTextActive(true);
                }
            }
        }
    }


    public void StartGame()
    {
        //Check if all points spent
        int pointssum = 0;
        foreach (KeyValuePair<int, int> i in pointsLeft)
        {
            pointssum += i.Value;
        }
        if (pointssum > 0)
        {
            tipsField.text = "Some points left to spend";
            return;
        }

        pointssum = 0;
        foreach(KeyValuePair<int,List<SpellContainer>> spells in spellsChosen)
        {
            pointssum += spells.Value.Count;
        }
        if(pointssum < 8)
        {
            tipsField.text = "Choose a couple more spells";
            return;
        }

        pointssum = 0;
        foreach (KeyValuePair<int, List<SkillsStat>> skills in skillsChosen)
        {
            pointssum += skills.Value.Count;
        }
        if (pointssum < 8)
        {
            tipsField.text = "Choose a couple more skills";
            return;
        }

        bool weaponReady = true;
        foreach (KeyValuePair<int, int> weapon in weaponChosen)
        {
            print(" weapon check "+ weapon.Value);
            if (weapon.Value == -1) weaponReady = false;
        }
        if (!weaponReady)
        {
            tipsField.text = "Don't forget your weapons";
            return;
        }


        pointssum = 0;
        foreach (KeyValuePair<int, string> name in heroesNames)
        {
            if(name.Value == "")
            {
                pointssum++;
            }
        }
        if (pointssum >0)
        {
            tipsField.text = "Please give names to all of your heroes";
            return;
        }
        BroAudio.Stop(mainTheme, 0.5f);

        SendStatsToGameInstance();
        GameInstance.LoadGameFromStart();

    }


    void SendStatsToGameInstance()
    {
        GameInstance.mainStatsAdded = GameInstance.ConvertMainStatsToSave(mainS);


        foreach (KeyValuePair<int, List<SkillsStat>> keysValues in skillsChosen)
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
                heroInventoryItem.container = weaponChosen[i];
                heroInventoryItem.stackAmount = 1;
                heroInventoryItem.positionReplaced = Vector3.zero;
                heroInventoryItem.level = "Level01";
            GameInstance.AddInventoryItem(heroInventoryItem);
        }

        GameInstance.heroesNames = new List<string>() { heroesNames[0], heroesNames[1], heroesNames[2], heroesNames[3] };

        foreach(KeyValuePair<int,List<SpellContainer>> sbs in spellsChosen)
        {
            HeroSpellbookSaved heroSpellbookSaved = new HeroSpellbookSaved();
            heroSpellbookSaved.heroIndex = sbs.Key;
            heroSpellbookSaved.spells = sbs.Value;

            GameInstance.spellbooksSaved.Add(heroSpellbookSaved);
        }

    }

    public void QuickGameStart()
    {

        mainS.Clear();
        skillsChosen.Clear();
        
        for (int i = 0; i < 4; i++)
        {
            mainS.Add (i,new Dictionary<MainStat, int>() {  { MainStat.Strength, 10}, 
                                                            { MainStat.Agility, 10 }, 
                                                            { MainStat.Mind, 10 }, 
                                                            { MainStat.Endurance, 10 }, 
                                                            { MainStat.Willpower, 10 },
                                                            { MainStat.Survival, 10}});
            skillsChosen.Add(i, new List<SkillsStat>() { SkillsStat.BladedWeapons, SkillsStat.ElementalMagic});
            ChooseSpell(GameInstance.dataBase.GetSpellByIndex(3));
            ChooseSpell(GameInstance.dataBase.GetSpellByIndex(1));
            weaponChosen.Add(i, i);
        }
        GameInstance.heroesPortraits = new List<int>() { 1, 2, 3, 4 };
        GameInstance.heroesNames = new List<string>() { "Jim", "John", "Jeremy", "Jason" };
        SendStatsToGameInstance();
        GameInstance.LoadGameFromStart();
    }



}
