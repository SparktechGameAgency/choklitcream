
using UnityEngine;

public class HealthOrbSpawner : MonoBehaviour
{
    public static HealthOrbSpawner Instance;

    [Header("Spawn Settings")]
    [Tooltip("How many kills needed before first health orb spawns")]
    public int killsPerOrb = 15;
    [Tooltip("Random extra kills added so it's not perfectly predictable")]
    public int killVariance = 5;
    [Tooltip("How much health the orb restores")]
    public float healAmount = 20f;
    [Tooltip("How far from player the orb spawns")]
    public float spawnRadius = 3f;

    private int killsUntilNextOrb;
    private int totalKillsTracked;

    void Awake()
    {
        Instance = this;
        ResetKillThreshold();
    }

    void Start()
    {
        if (KillTracker.Instance != null)
            KillTracker.Instance.onKillCountChanged.AddListener(OnKillChanged);
        else
            Debug.LogError("[HealthOrbSpawner] KillTracker not found!");
    }

    void OnKillChanged(int totalKills)
    {
        totalKillsTracked = totalKills;

        // Check if we hit the threshold
        if (totalKills >= killsUntilNextOrb)
        {
            SpawnOrb();
            ResetKillThreshold();
        }
    }

    void SpawnOrb()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        // Spawn at a random position near the player
        Vector2 randomOffset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = playerObj.transform.position
                             + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Debug.Log("[HealthOrbSpawner] Spawning health orb!"
                + " | Total kills: " + totalKillsTracked
                + " | Next orb at: " + killsUntilNextOrb
                + " | Spawn pos: " + spawnPos);

        HealthOrb.Spawn(spawnPos, healAmount);
    }

    void ResetKillThreshold()
    {
        // Next orb requires kills + random variance so it feels natural
        int variance = Random.Range(0, killVariance + 1);
        killsUntilNextOrb = totalKillsTracked + killsPerOrb + variance;

        Debug.Log("[HealthOrbSpawner] Next health orb at kill: "
                + killsUntilNextOrb);
    }
}