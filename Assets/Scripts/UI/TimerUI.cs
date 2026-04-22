
using UnityEngine;
using TMPro; 

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI timerText; 

    void Update()
    {
        if (GameTimer.Instance == null) return;
        timerText.text = GameTimer.Instance.GetFormattedTime();
    }
}