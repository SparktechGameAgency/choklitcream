// Scripts/Weapons/Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private WeaponData data;
    private Vector2 direction;
    private float traveledDistance;
    private int pierceCount;
    private SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Initialize(Vector2 dir, WeaponData weaponData)
    {
        direction = dir;
        data = weaponData;
        traveledDistance = 0f;
        pierceCount = 0;

        if (data.projectileSprite != null)
            sr.sprite = data.projectileSprite;

        transform.localScale = Vector3.one * data.scale;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        float step = data.projectileSpeed * Time.deltaTime;
        transform.Translate(Vector2.right * step);
        traveledDistance += step;

        if (traveledDistance >= data.range)
            ReturnToPool();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy")) return;

        // Apply damage multiplier from AbilityManager
        float multiplier = AbilityManager.Instance != null
            ? AbilityManager.Instance.DamageMultiplier : 1f;

        col.GetComponent<EnemyController>()
           ?.TakeDamage(data.damage * multiplier);

        // Knockback
        if (data.knockback > 0f)
        {
            Vector2 knockDir = (col.transform.position
                             - transform.position).normalized;
            col.GetComponent<Rigidbody2D>()
               ?.AddForce(knockDir * data.knockback, ForceMode2D.Impulse);
        }

        // AoE
        if (data.isAoE && data.aoeRadius > 0f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, data.aoeRadius,
                LayerMask.GetMask("Enemy"));

            foreach (var hit in hits)
                hit.GetComponent<EnemyController>()
                   ?.TakeDamage(data.damage * multiplier * 0.5f);
        }

        pierceCount++;
        if (pierceCount >= data.pierce)
            ReturnToPool();
    }

    void ReturnToPool()
    {
        ObjectPool.Instance.Return(data.PoolTag, gameObject);
    }

    void OnDisable() => traveledDistance = 0f;
}