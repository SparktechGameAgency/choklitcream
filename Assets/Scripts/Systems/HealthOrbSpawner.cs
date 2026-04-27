// Scripts/Systems/HealthOrbSpawner.cs
using UnityEngine;

public class HealthOrbSpawner : MonoBehaviour
{
    public static HealthOrbSpawner Instance;

    [Header("Spawn Settings")]
    [Tooltip("Base kills needed between each health orb")]
    public int   killsPerOrb  = 15;
    [Tooltip("Random extra kills added so spawns feel natural")]
    public int   killVariance = 5;
    [Tooltip("How much health the orb restores")]
    public float healAmount   = 20f;
    [Tooltip("How far from player the orb spawns")]
    public float spawnRadius  = 3f;

    // Simple counter — resets every time an orb spawns
    private int killsSinceLastOrb = 0;
    private int nextOrbTarget     = 0;

    void Awake()
    {
        Instance = this;
        SetNextTarget();
    }

    void Start()
    {
        if (KillTracker.Instance != null)
            KillTracker.Instance.onKillCountChanged.AddListener(OnKillChanged);
    }

    void OnKillChanged(int totalKills)
    {
        killsSinceLastOrb++;

        if (killsSinceLastOrb >= nextOrbTarget)
        {
            SpawnOrb();
            killsSinceLastOrb = 0;
            SetNextTarget();
        }
    }

    void SetNextTarget()
    {
        // Always at least 1 kill required — never 0
        nextOrbTarget = Mathf.Max(1, killsPerOrb
                      + Random.Range(0, Mathf.Max(0, killVariance) + 1));
    }

    void SpawnOrb()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Vector2 randomOffset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos     = playerObj.transform.position
                             + new Vector3(randomOffset.x, randomOffset.y, 0f);

        HealthOrb.Spawn(spawnPos, healAmount);
    }
}
