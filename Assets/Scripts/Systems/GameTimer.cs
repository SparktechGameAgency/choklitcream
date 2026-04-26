using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    [Header("Settings")]
    public float survivalDuration = 600f; // 10 min — change in Inspector

    [Header("Events")]
    public UnityEvent onTimerComplete; // optional extra hook in Inspector

    public float TimeRemaining { get; private set; }
    public bool IsRunning { get; private set; }

    public static GameTimer Instance;

    void Awake()
    {
        Instance = this;
        TimeRemaining = survivalDuration;
        IsRunning = true;
    }

    void Update()
    {
        if (!IsRunning) return;

        TimeRemaining -= Time.deltaTime;

        if (TimeRemaining <= 0)
        {
            TimeRemaining = 0;
            IsRunning = false;

            // Show the You Win panel
            GameUIManager.Instance?.ShowWinPanel();

            // Fire the inspector event too (optional hooks)
            onTimerComplete?.Invoke();
        }
    }

    // Call this when the player dies so the timer stops
    public void StopTimer() => IsRunning = false;

    // Returns "9:45" style string for the UI
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(TimeRemaining / 60);
        int seconds = Mathf.FloorToInt(TimeRemaining % 60);
        return string.Format("{0}:{1:00}", minutes, seconds);
    }
}