
using UnityEngine;
using UnityEngine.UI;

public class HealthImage : MonoBehaviour
{
    [SerializeField] Image image;

    [SerializeField] Image Bar, Hill01, Hill02, TopOrb;
    [SerializeField] Sprite activeHill, activeBar, deactiveHill, deactiveBar;


    public void ProgressBarFill(float amount)
    {
        image.fillAmount = amount;
    }

    public void SetActiveBar(bool active)
    {
        if (active)
        {
            Bar.sprite = activeBar;
            if(Hill01!=null) Hill01.sprite = activeHill;
            if (Hill02 != null) Hill02.sprite = activeHill;
            if (TopOrb != null) TopOrb.color = Color.white;
        }
        else
        {
            Bar.sprite = deactiveBar;
            if (Hill01 != null) Hill01.sprite = deactiveHill;
            if (Hill02 != null) Hill02.sprite = deactiveHill;
            if(TopOrb !=null) TopOrb.color = Color.clear;
        }
    }

}
