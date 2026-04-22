
using UnityEngine;
using TMPro;

public class KillCountUI : MonoBehaviour
{
    public TextMeshProUGUI killText;

    void Start()
    {
        UpdateText(0);
    }

    // Use Update to poll instead of event — avoids subscribe timing issues
    void Update()
    {
        if (KillTracker.Instance == null) return;
        UpdateText(KillTracker.Instance.KillCount);
    }

    void UpdateText(int count)
    {
        if (killText != null)
            killText.text = "Kills: " + count;
    }
}