// Scripts/Systems/ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string     tag;
        public GameObject prefab;
        public int        initialSize = 10;
    }

    public static ObjectPool Instance;

    [Header("Pools — add enemies and static pools here")]
    public List<Pool> pools = new();

    private Dictionary<string, Queue<GameObject>> poolDictionary = new();

    void Awake()
    {
        Instance = this;
        foreach (Pool pool in pools)
            InitialisePool(pool);
    }

    void InitialisePool(Pool pool)
    {
        if (poolDictionary.ContainsKey(pool.tag)) return;

        Queue<GameObject> queue = new();

        for (int i = 0; i < pool.initialSize; i++)
            queue.Enqueue(CreateNew(pool.prefab));

        poolDictionary[pool.tag] = queue;
    }

    GameObject CreateNew(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }

    public void AddPool(Pool pool)
    {
        if (pool.prefab == null)                   return;
        if (poolDictionary.ContainsKey(pool.tag))  return;

        pools.Add(pool);
        InitialisePool(pool);
    }

    public GameObject Get(string tag, Vector3 position)
    {
        if (!poolDictionary.ContainsKey(tag)) return null;

        Queue<GameObject> queue = poolDictionary[tag];

        if (queue.Count == 0)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool == null || pool.prefab == null) return null;
            queue.Enqueue(CreateNew(pool.prefab));
        }

        GameObject obj = queue.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);
        return obj;
    }

    public void Return(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
    }

    public bool HasPool(string tag) => poolDictionary.ContainsKey(tag);
}
