
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Setup")]
    public GameObject enemyPrefab;
    public EnemyData[] enemyTypes;
    public Transform player;

    [Header("Spawning")]
    public float spawnRadius = 12f;
    public float baseSpawnInterval = 2f;

    private List<GameObject> pool = new();
    private float timer;
    private float elapsedTime;

    void Awake() => Instance = this;

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Difficulty: spawn faster as time goes on (min 0.3s interval)
        float interval = Mathf.Max(0.3f, baseSpawnInterval - elapsedTime * 0.02f);

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnEnemy();
            timer = interval;
        }
    }

    void SpawnEnemy()
    {
        // Pick a random angle outside the screen
        Vector2 spawnPos = (Vector2)player.position +
                           Random.insideUnitCircle.normalized * spawnRadius;

        GameObject enemy = GetFromPool();
        enemy.transform.position = spawnPos;

        // Pick enemy type (later: weight by time)
        EnemyData data = enemyTypes[Random.Range(0, enemyTypes.Length)];
        enemy.GetComponent<EnemyController>().Initialize(player);
        enemy.GetComponent<EnemyController>().data = data;
    }

    // --- Object Pool ---
    GameObject GetFromPool()
    {
        foreach (var obj in pool)
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }

        // Pool is empty — instantiate a new one
        var newObj = Instantiate(enemyPrefab);
        pool.Add(newObj);
        return newObj;
    }

    public void ReturnToPool(GameObject obj) => obj.SetActive(false);
}