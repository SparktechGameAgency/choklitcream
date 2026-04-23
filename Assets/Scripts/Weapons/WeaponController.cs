// Scripts/Weapons/WeaponController.cs
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [HideInInspector] public WeaponData data;

    private float fireTimer;
    private Transform player;

    public void Initialize(WeaponData weaponData, Transform playerTransform)
    {
        data = weaponData;
        player = playerTransform;
        fireTimer = 1f / data.fireRate;

        Debug.Log("[Weapon] Initialized: " + data.weaponName
                + " | PoolTag: " + data.PoolTag);
    }

    void Update()
    {
        if (data == null || player == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            TryShoot();
            fireTimer = 1f / data.fireRate;
        }
    }

    void TryShoot()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var e in enemies)
        {
            if (!e.activeInHierarchy) continue;
            float d = Vector2.Distance(player.position, e.transform.position);
            if (d < data.range && d < minDist)
            {
                minDist = d;
                nearest = e.transform;
            }
        }

        if (nearest == null) return;

        Shoot(nearest.position);
    }

    void Shoot(Vector3 targetPos)
    {
        Vector2 dir = (targetPos - player.position).normalized;
        string poolTag = data.PoolTag; // ← uses the safe unique tag

        if (!ObjectPool.Instance.HasPool(poolTag))
        {
            Debug.LogError("[Weapon] Pool not found: " + poolTag);
            return;
        }

        GameObject proj = ObjectPool.Instance.Get(poolTag, player.position);
        if (proj == null) return;

        Projectile p = proj.GetComponent<Projectile>();
        if (p == null)
        {
            Debug.LogError("[Weapon] Projectile component missing on prefab: "
                         + proj.name);
            return;
        }

        p.Initialize(dir, data);
    }
}