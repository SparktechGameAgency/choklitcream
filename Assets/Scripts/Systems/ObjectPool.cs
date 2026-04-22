// Scripts/Systems/ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 10;
    }

    public static ObjectPool Instance;

    [Header("Pools — add enemies and any static pools here")]
    public List<Pool> pools = new();

    private Dictionary<string, Queue<GameObject>> poolDictionary = new();

    void Awake()
    {
        Instance = this;

        // Pre-spawn every pool defined in the Inspector
        foreach (Pool pool in pools)
            InitialisePool(pool);
    }

    // ── Internal ──────────────────────────────────────────────

    void InitialisePool(Pool pool)
    {
        if (poolDictionary.ContainsKey(pool.tag))
        {
            Debug.LogWarning("[ObjectPool] Pool already exists: " + pool.tag);
            return;
        }

        Queue<GameObject> queue = new();

        for (int i = 0; i < pool.initialSize; i++)
        {
            GameObject obj = CreateNew(pool.prefab);
            queue.Enqueue(obj);
        }

        poolDictionary[pool.tag] = queue;
    }

    GameObject CreateNew(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }

    // ── Public API ────────────────────────────────────────────

    // Called by WeaponManager at runtime to register projectile pools
    public void AddPool(Pool pool)
    {
        if (pool.prefab == null)
        {
            Debug.LogError("[ObjectPool] Tried to add pool with null prefab: " + pool.tag);
            return;
        }

        if (poolDictionary.ContainsKey(pool.tag)) return;

        pools.Add(pool);
        InitialisePool(pool);

        Debug.Log("[ObjectPool] Pool registered: " + pool.tag
                + " (size: " + pool.initialSize + ")");
    }

    // Pull an object from the pool
    public GameObject Get(string tag, Vector3 position)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogError("[ObjectPool] No pool found with tag: " + tag
                         + " — make sure AddPool() is called before Get()");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[tag];

        // Auto-expand pool if empty
        if (queue.Count == 0)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool == null || pool.prefab == null)
            {
                Debug.LogError("[ObjectPool] Pool prefab is null for tag: " + tag);
                return null;
            }

            Debug.LogWarning("[ObjectPool] Pool empty, expanding: " + tag);
            GameObject extra = CreateNew(pool.prefab);
            queue.Enqueue(extra);
        }

        GameObject obj = queue.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);
        return obj;
    }

    // Return an object back to the pool
    public void Return(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("[ObjectPool] Returning to unknown pool: " + tag
                           + " — destroying instead");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
    }

    // Check if a pool exists (useful for safety checks)
    public bool HasPool(string tag) => poolDictionary.ContainsKey(tag);
}