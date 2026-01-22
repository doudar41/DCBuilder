using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI heroName;

    [SerializeField] MainStatTextUI mainStatsUITexts;
    [SerializeField] DependedStatTextUI dependedStatsUIText;
    [SerializeField] SkillsStatTextUI skillsStatsUIText;
    [SerializeField] GameObject panelStats, paperDoll;
    [SerializeField] Slider carryingWeight;
    [SerializeField] Slider hungerLevel;
    [SerializeField] TextMeshProUGUI weightHero, weightParty;
    [SerializeField] TextMeshProUGUI foodText;
    [SerializeField] TextMeshProUGUI expPoints;
    [SerializeField] TextMeshProUGUI partyLevel;
    [SerializeField] List<TextMeshProUGUI> moneyInCoins;
    [SerializeField] TextMeshProUGUI gemsAmount;

    private void Start()
    {
        mainStatsUITexts.KeyPairFill();
        dependedStatsUIText.KeyPairFill();
        skillsStatsUIText.KeyPairFill();

    }

    public void EnableStatPanel(bool active)
    {
        if (GameInstance.playerController.shopIsOpened)
        {
            paperDoll.SetActive(false);
            panelStats.SetActive(false);
            return;
        }
        if (active)
        { 
            panelStats.SetActive(true);
            paperDoll.SetActive(true);
            RefreshStats();

        }
        else
        {
            paperDoll.SetActive(false);
            panelStats.SetActive(false);
        }

    }

    public void RefreshStats()
    {
        Dictionary<MainStat, int> mainS = GameInstance.party.activeHero.GetMainStatsForUI();
        foreach (KeyValuePair<MainStat, int> k in mainS)
        {
            mainStatsUITexts.GetValue(k.Key).text = k.Value.ToString();
        }
        Dictionary<SkillsStat, int> skills = GameInstance.party.activeHero.GetSkillStatsForUI();
        foreach (KeyValuePair<SkillsStat, int> k in skills)
        {
            skillsStatsUIText.GetValue(k.Key).text = k.Value.ToString();
        }
        Dictionary<DependedStat, int> dependstat = GameInstance.party.activeHero.GetDependedStatsForUI();
        foreach (KeyValuePair<DependedStat, int> k in dependstat)
        {
            if(dependedStatsUIText.GetValue(k.Key) != null)
            dependedStatsUIText.GetValue(k.Key).text = k.Value.ToString();
        }
        //print("equipment weight "+GameInstance.inventory.GetCurrentHeroWeight() + " hero eqquipment capacity "+ GameInstance.party.activeHero.GetDependedStat(DependedStat.CarryingCapacity));
        carryingWeight.value = GameInstance.inventory.GetCurrentHeroWeight();
        weightHero.text = GameInstance.party.activeHero.GetMaxDependedStat(DependedStat.CarryingCapacity).ToString()+"/"+ GameInstance.party.activeHero.GetHeroWeight().ToString();
        weightParty.text = GameInstance.party.GetPartyWeight().ToString();
        hungerLevel.value = GameInstance.party.activeHero.GetHungerLevelPercents();

        foodText.text = (GameInstance.party.CheckFoodSupply(0)/10).ToString();
        expPoints.text = GameInstance.party.addExperiencePoints(0).ToString();
        for (int i=0;i< moneyInCoins.Count;i++)
        {
            moneyInCoins[i].text = GameInstance.party.GetCoinsForUI()[i].ToString();
        }
        heroName.text = GameInstance.party.activeHero.HeroName();
        partyLevel.text = GameInstance.party.GetPartyLevel().ToString();
        gemsAmount.text = GameInstance.party.CheckGems(0).ToString();   
    }
}


[System.Serializable]
public class MainStatTextUI
{
    public List<MainStat> key;
    public List<TextMeshProUGUI> textUI;

    Dictionary<MainStat, TextMeshProUGUI> keyPair = new Dictionary<MainStat, TextMeshProUGUI>();


    public void SetKeyPair(MainStat s, TextMeshProUGUI t)
    {
        keyPair.Add(s, t);
    }

    public void KeyPairFill()
    {
        if (key.Count == textUI.Count)
        {
            for (int i=0;i<key.Count;i++)
            {
                keyPair.Add(key[i], textUI[i]);
            }
        }
        else
        {
            Debug.Log("Stats key should have the same number of text fields");
        }
    }

    public TextMeshProUGUI GetValue(MainStat m)
    {
        return keyPair[m];
    }

}

[System.Serializable]
public class DependedStatTextUI
{
    public List<DependedStat> key;
    public List<TextMeshProUGUI> textUI;

    Dictionary<DependedStat, TextMeshProUGUI> keyPair = new Dictionary<DependedStat, TextMeshProUGUI>();


    public void SetKeyPair(DependedStat s, TextMeshProUGUI t)
    {
        keyPair.Add(s, t);
    }

    public void KeyPairFill()
    {
        if (key.Count == textUI.Count)
        {
            for (int i = 0; i < key.Count; i++)
            {
                keyPair.Add(key[i], textUI[i]);
            }
        }
        else
        {
            Debug.Log("Stats key should have the same number of text fields");
        }
    }

    public TextMeshProUGUI GetValue(DependedStat m)
    {
        if (!keyPair.ContainsKey(m)) return null;
        return keyPair[m];
    }

}

[System.Serializable]
public class SkillsStatTextUI
{
    public List<SkillsStat> key;
    public List<TextMeshProUGUI> textUI;

    Dictionary<SkillsStat, TextMeshProUGUI> keyPair = new Dictionary<SkillsStat, TextMeshProUGUI>();


    public void SetKeyPair(SkillsStat s, TextMeshProUGUI t)
    {
        keyPair.Add(s, t);
    }

    public void KeyPairFill()
    {
        if (key.Count == textUI.Count)
        {
            for (int i = 0; i < key.Count; i++)
            {
                keyPair.Add(key[i], textUI[i]);
            }
        }
        else
        {
            Debug.Log("Stats key should have the same number of text fields");
        }
    }

    public TextMeshProUGUI GetValue(SkillsStat m)
    {
        return keyPair[m];
    }




}
