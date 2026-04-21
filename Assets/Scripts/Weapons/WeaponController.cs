
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public WeaponData data;
    public GameObject projectilePrefab;

    private float fireTimer;

    void Update()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            TryShoot();
            fireTimer = 1f / data.fireRate;
        }
    }

    void TryShoot()
    {
        // Find the nearest enemy within range
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, data.range,
            LayerMask.GetMask("Enemy"));

        if (hits.Length == 0) return;

        // Pick closest
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var h in hits)
        {
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < minDist) { minDist = d; nearest = h.transform; }
        }

        if (nearest != null) Shoot(nearest);
    }

    void Shoot(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Projectile>().Initialize(dir, data);
    }
}