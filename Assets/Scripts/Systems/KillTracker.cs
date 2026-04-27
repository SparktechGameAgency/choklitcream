// Scripts/Systems/KillTracker.cs
using UnityEngine;
using UnityEngine.Events;

public class KillTracker : MonoBehaviour
{
    public static KillTracker Instance;

    public int KillCount { get; private set; }

    public UnityEvent<int> onKillCountChanged = new UnityEvent<int>();

    void Awake()
    {
        Instance  = this;
        KillCount = 0;
    }

    public void RegisterKill()
    {
        KillCount++;
        onKillCountChanged?.Invoke(KillCount);
    }
}
