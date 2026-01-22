using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingShop : MonoBehaviour
{

    [SerializeField] GameObject buttons, menus, infoBlock;
    [SerializeField] List<TextMeshProUGUI> skillsTexts = new List<TextMeshProUGUI>();
    List<int> heroSkillNum = new List<int>();
    [SerializeField] List<Button> plusButtons = new List<Button>();
    [SerializeField] List<TextMeshProUGUI> mainStatTexts = new List<TextMeshProUGUI>();
    List<int> mainStatsNum = new List<int>();
    [SerializeField] TextMeshProUGUI hpText, MpText, defenceText, initiativeText, evasionText, accuracyText, meleeText, rangeText;
    [SerializeField] TextMeshProUGUI fireResText, waterResText, iceResText, earthResText, areResText, darkResText;
    [SerializeField] TextMeshProUGUI heroSkillPoints;
    [SerializeField] int moneyForSkillsPoint = 100;
    [SerializeField] AnimationCurve skillRaiseGraph;
    [SerializeField] CameraOrder  cameraOrder;
    [SerializeField] List<TextMeshProUGUI> moneyTexts = new List<TextMeshProUGUI>();
    [SerializeField] TextMeshProUGUI heroName;
    int skillsToSpend = 0, moneySpent = 0;
    List<int> mainstatup = new List<int>();

    Dictionary<MainStat, int> mainStatsPure = new Dictionary<MainStat, int>();
    Dictionary<DependedStat, int> dependStatsPure = new Dictionary<DependedStat, int>();
    Dictionary<SkillsStat, int> skillStatsPure = new Dictionary<SkillsStat, int>(); 


    private void Start()
    {
        foreach (Button button in plusButtons) { heroSkillNum.Add(0); mainstatup.Add(0); }
        foreach (TextMeshProUGUI t in mainStatTexts) { mainStatsNum.Add(0);  } ;
    }

    public void OpenTrainingShop()
    {
        menus.SetActive(true);
        infoBlock.SetActive(true);
        //Get stats from active hero
        RefreshStatsInTraining();

    }

    public void AddPointToSkill(int skillIndex)
    {
        if(skillsToSpend <= 0) { skillsToSpend = 0; return; }
        if (GameInstance.party.SellBuyMoneyCheck(moneyForSkillsPoint) < 0) return;
        GetPlayersCoins();
        skillsToSpend--;
        heroSkillNum[skillIndex] = heroSkillNum[skillIndex] + 1;
        skillsTexts[skillIndex].text = heroSkillNum[skillIndex].ToString();
        heroSkillPoints.text = skillsToSpend.ToString();

        if(heroSkillNum[skillIndex]>= skillStatsPure[(SkillsStat)skillIndex + 1] + 10)
        {
            int _delta = heroSkillNum[skillIndex] - skillStatsPure[(SkillsStat)skillIndex + 1];
            int _pointsCal = _delta / 10;
            if (mainstatup[skillIndex] >= _pointsCal) return;
            int _points = 0;
            _points = _pointsCal - mainstatup[skillIndex];
            mainstatup[skillIndex] = _pointsCal;
            switch ((SkillsStat)skillIndex+1)
            {
                case SkillsStat.BluntWeapons:
                    mainStatsNum[3] += _points;
                    mainStatTexts[3].text = mainStatsNum[3].ToString();
                    break;
                case SkillsStat.BladedWeapons:
                    mainStatsNum[1] += _points;
                    mainStatTexts[1].text = mainStatsNum[1].ToString();
                    break;
                case SkillsStat.Polearms:
                    mainStatsNum[0] += _points;
                    mainStatTexts[0].text = mainStatsNum[0].ToString();
                    break;
                case SkillsStat.RangedWeapons:
                    mainStatsNum[1] += _points;
                    mainStatTexts[1].text = mainStatsNum[1].ToString();
                    break;
                case SkillsStat.HeavyArmour:
                    mainStatsNum[3] += _points;
                    mainStatTexts[3].text = mainStatsNum[3].ToString();
                    break;
                case SkillsStat.LightArmour:
                    mainStatsNum[1] += _points;
                    mainStatTexts[1].text = mainStatsNum[1].ToString();
                    break;
                case SkillsStat.LightMagic:
                    mainStatsNum[2] += _points;
                    mainStatTexts[2].text = mainStatsNum[2].ToString();
                    break;
                case SkillsStat.DarkMagic:
                    mainStatsNum[4] += _points;
                    mainStatTexts[4].text = mainStatsNum[4].ToString();
                    break;
                case SkillsStat.ElementalMagic:
                    mainStatsNum[2] += _points;
                    mainStatTexts[2].text = mainStatsNum[2].ToString();
                    break;
                case SkillsStat.Identify:
                    mainStatsNum[5] += _points;
                    mainStatTexts[5].text = mainStatsNum[5].ToString();
                    break;
                case SkillsStat.SpotSecret:
                    mainStatsNum[5] += _points;
                    mainStatTexts[5].text = mainStatsNum[5].ToString();
                    break;
            }
        }

        GameInstance.party.MoneyGoes((int)(moneyForSkillsPoint * skillRaiseGraph.Evaluate(GameInstance.party.GetPartyLevel())));
        moneySpent += (int)(moneyForSkillsPoint * skillRaiseGraph.Evaluate(GameInstance.party.GetPartyLevel()));

    }

    void GetPlayersCoins()
    {
        var money = GameInstance.party.GetCoinsForUI();
        for (int i = 0; i < money.Count; i++)
        {
            moneyTexts[i].text = money[i].ToString();
        }
    }

    public void RefreshStatsInTraining()
    {
        skillsToSpend = 0;
        mainStatsPure.Clear();
        dependStatsPure.Clear();
        skillStatsPure.Clear();
        mainstatup.Clear();
        GameInstance.party.MoneyGoes(-moneySpent);
        moneySpent = 0;
        foreach (Button button in plusButtons) { mainstatup.Add(0); }
        GameInstance.party.activeHero.GetPureStats( out mainStatsPure, 
                                                    out dependStatsPure, 
                                                    out skillStatsPure);

        for (int i = 0; i < skillsTexts.Count; i++)
        {
            int _amount = skillStatsPure[(SkillsStat)i + 1];
            skillsTexts[i].text = _amount.ToString();
            heroSkillNum[i] = _amount;
        }

        heroSkillPoints.text = GameInstance.party.activeHero.GetSkillPoints().ToString();
        skillsToSpend = GameInstance.party.activeHero.GetSkillPoints();


        for(int i = 0;i < mainStatTexts.Count; i++)
        {
            mainStatsNum[i] = mainStatsPure[(MainStat)i + 1];
            mainStatTexts[i].text = mainStatsNum[i].ToString();
        }

        heroName.text = GameInstance.party.activeHero.HeroName();
        GetPlayersCoins();
    }
    public void CameraOut()
    {
        cameraOrder.BattleLogWithGameplay();
    }

    public void CloseShop()
    {
        menus.SetActive(false);
        buttons.SetActive(false);
        infoBlock.SetActive(false);
        CameraOut();
        GameInstance.playerController.shopIsOpened = false;
        GameInstance.party.MoneyGoes(-moneySpent);
        moneySpent = 0;
    }
    

    public void Confirm()
    {
        for(int i=0;i< heroSkillNum.Count;i++)
        {
            GameInstance.party.activeHero.SetSKillStat((SkillsStat)i+1, heroSkillNum[i]);
        }
        for (int i = 0; i < mainStatsNum.Count; i++)
        {
            GameInstance.party.activeHero.AddToMainStat((MainStat)i + 1, mainStatsNum[i] - mainStatsPure[(MainStat)i+1]);
        }
        GameInstance.party.activeHero.SetSkillPoints( skillsToSpend);
        moneySpent = 0;
    }

}
