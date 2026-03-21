using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class gameClock : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI clockTextDay, clockTextHour, clockTextMinute;


    public void UpdateClock(int day, int hour, int minute)
    {
        clockTextDay.text = day.ToString();
        clockTextHour.text = hour.ToString();
        clockTextMinute.text = minute.ToString();
    }
}
