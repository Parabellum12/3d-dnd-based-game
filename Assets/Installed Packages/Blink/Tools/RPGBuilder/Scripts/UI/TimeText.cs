using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeText : MonoBehaviour
{
    public TextMeshProUGUI text;
    
    private void OnEnable()
    {
        WorldEvents.TimeChange += TimeChanged;
    }

    private void OnDisable()
    {
        WorldEvents.TimeChange -= TimeChanged;
    }

    private void TimeChanged(CharacterEntries.TimeData timeData)
    {
        string minuteText = timeData.CurrentMinute >= 10
            ? timeData.CurrentMinute.ToString()
            : "0" + timeData.CurrentMinute;
        text.text = timeData.CurrentHour + ":" + minuteText;
    }
}
