using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] MainStatStruct mainStatsUITexts;
    [SerializeField] DependedStatStruct dependedStatsUIText;
    [SerializeField] SkillsStatStruct skillsStatsUIText;
    [SerializeField] GameObject panelStats;

    private void Start()
    {
        mainStatsUITexts.KeyPairFill();
        dependedStatsUIText.KeyPairFill();
        skillsStatsUIText.KeyPairFill();
/*        foreach(MainStat m in mainStatsUITexts.key)
        {
            mainStatsUITexts.GetValue(m).fontSize = 10;
        }
        foreach (SkillsStat m in mainStatsUITexts.key)
        {
            skillsStatsUIText.GetValue(m).fontSize = 10;
        }*/
    }

    public void EnableStatPanel(bool active)
    {
        if (active)
        { 
            panelStats.SetActive(true);
            RefreshStats();

    }
        else
        {
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
    }
}


[System.Serializable]
public class MainStatStruct
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
public class DependedStatStruct
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
        return keyPair[m];
    }

}

[System.Serializable]
public class SkillsStatStruct
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
