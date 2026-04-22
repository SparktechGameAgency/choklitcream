
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [System.Serializable]
    public class EnemyWaveEntry
    {
        public EnemyData data;
        [Tooltip("How many seconds into the game before this enemy starts spawning")]
        public float unlockAtTime = 0f;
        [Range(0f, 1f)]
        [Tooltip("Relative spawn weight — higher = spawns more often")]
        public float spawnWeight = 1f;
    }

    [Header("Enemy Types")]
    [Tooltip("Add as many enemy types as you want here")]
    public List<EnemyWaveEntry> enemyEntries = new();

    [Header("Spawn Settings")]
    public Transform player;
    public float spawnRadius = 12f;
    public float baseSpawnInterval = 2f;
    [Tooltip("How much faster enemies spawn per minute survived")]
    public float difficultyRampPerMinute = 0.3f;
    [Tooltip("Minimum possible spawn interval (seconds)")]
    public float minSpawnInterval = 0.3f;

    private float spawnTimer;
    private float elapsedTime;

    void Awake() => Instance = this;

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Escalate difficulty over time
        float ramp = (elapsedTime / 60f) * difficultyRampPerMinute;
        float interval = Mathf.Max(minSpawnInterval, baseSpawnInterval - ramp);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = interval;
        }
    }

    void SpawnEnemy()
    {
        EnemyWaveEntry entry = PickRandomEnemy();
        if (entry == null) return;

        // Spawn just outside the camera view
        Vector2 spawnPos = (Vector2)player.position
                         + Random.insideUnitCircle.normalized * spawnRadius;

        GameObject obj = ObjectPool.Instance.Get(entry.data.enemyName, spawnPos);
        if (obj == null) return;

        obj.GetComponent<EnemyController>().Initialize(player, entry.data);
    }

    // Weighted random pick — respects unlockAtTime and spawnWeight
    EnemyWaveEntry PickRandomEnemy()
    {
        List<EnemyWaveEntry> available = new();
        float totalWeight = 0f;

        foreach (var entry in enemyEntries)
        {
            if (elapsedTime >= entry.unlockAtTime)
            {
                available.Add(entry);
                totalWeight += entry.spawnWeight;
            }
        }

        if (available.Count == 0) return null;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in available)
        {
            cumulative += entry.spawnWeight;
            if (roll <= cumulative) return entry;
        }

        return available[^1];
    }
}