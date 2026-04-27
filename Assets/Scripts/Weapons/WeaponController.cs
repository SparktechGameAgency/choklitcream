
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
        fireTimer = GetFireInterval();
    }

    void Update()
    {
        if (data == null || player == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            TryShoot();
            fireTimer = GetFireInterval();
        }
    }

    /// <summary>
    /// Returns the seconds between shots, accounting for the
    /// FireRateMultiplier ability (higher multiplier = shorter interval).
    /// </summary>
    float GetFireInterval()
    {
        float multiplier = AbilityManager.Instance != null
            ? AbilityManager.Instance.FireRateMultiplier
            : 1f;

        return 1f / (data.fireRate * multiplier);
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
        string poolTag = data.PoolTag;

        if (!ObjectPool.Instance.HasPool(poolTag)) return;

        GameObject proj = ObjectPool.Instance.Get(poolTag, player.position);
        if (proj == null) return;

        proj.GetComponent<Projectile>()?.Initialize(dir, data);
    }
}