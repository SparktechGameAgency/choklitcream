
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
                + " | Player: " + (player != null ? player.name : "NULL"));
    }

    void Update()
    {
        if (data == null) { Debug.LogError("[Weapon] data is NULL!"); return; }
        if (player == null) { Debug.LogError("[Weapon] player is NULL!"); return; }

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            TryShoot();
            fireTimer = 1f / data.fireRate;
        }
    }

    void TryShoot()
    {
        // Use tag instead of layer — avoids layer name mismatch issues
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Debug.Log("[Weapon] TryShoot — enemies found: " + enemies.Length
                + " | range: " + data.range
                + " | player pos: " + player.position);

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

        if (nearest == null)
        {
            Debug.Log("[Weapon] No enemy in range (" + data.range + ")");
            return;
        }

        Debug.Log("[Weapon] Shooting at: " + nearest.name
                + " dist: " + minDist);
        Shoot(nearest.position);
    }

    void Shoot(Vector3 targetPos)
    {
        Vector2 dir = (targetPos - player.position).normalized;
        string poolTag = data.weaponName + "_projectile";

        Debug.Log("[Weapon] Shoot — poolTag: " + poolTag
                + " | pool exists: " + ObjectPool.Instance.HasPool(poolTag));

        if (!ObjectPool.Instance.HasPool(poolTag))
        {
            Debug.LogError("[Weapon] Pool not found: " + poolTag
                         + " — WeaponData.weaponName = '" + data.weaponName + "'");
            return;
        }

        GameObject proj = ObjectPool.Instance.Get(poolTag, player.position);

        if (proj == null)
        {
            Debug.LogError("[Weapon] Pool returned null for: " + poolTag);
            return;
        }

        Projectile p = proj.GetComponent<Projectile>();
        if (p == null)
        {
            Debug.LogError("[Weapon] Projectile component missing on prefab!");
            return;
        }

        p.Initialize(dir, data);
        Debug.Log("[Weapon] Projectile spawned successfully!");
    }
}