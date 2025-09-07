
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events; 
using UnityEngine.EventSystems;

public class SkillChoiceIcon : MonoBehaviour, IPointerClickHandler
{
    public  SkillsStat skillsStat;
    [SerializeField] TextMeshProUGUI skillName;

    public UnityEvent<SkillsStat> SendSkillStat;


    public void OnPointerClick(PointerEventData eventData)
    {
        SendSkillStat.Invoke(skillsStat);
    }

    private void Start()
    {
        skillName.text = skillsStat.ToString();
    }

    public void SetTextActive(bool active)
    {
        if (active)
        {
            skillName.color = Color.yellow;
        }
        else
        {
            skillName.color = Color.white;
        }

    }


}
