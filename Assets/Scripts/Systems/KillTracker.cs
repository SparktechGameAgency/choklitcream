
using UnityEngine;
using UnityEngine.Events;

public class KillTracker : MonoBehaviour
{
    public static KillTracker Instance;

    public int KillCount { get; private set; }

    public UnityEvent<int> onKillCountChanged = new UnityEvent<int>();

    void Awake()
    {
        Instance = this;
        KillCount = 0;
    }

    public void RegisterKill()
    {
        KillCount++;
        Debug.Log("[KillTracker] Kill count: " + KillCount);
        onKillCountChanged?.Invoke(KillCount);
    }
}