
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private WeaponData data;
    private Vector2 direction;
    private float traveledDistance;
    private int pierceCount;
    private SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    // Called every time projectile is pulled from pool
    public void Initialize(Vector2 dir, WeaponData weaponData)
    {
        direction = dir;
        data = weaponData;
        traveledDistance = 0f;
        pierceCount = 0;

        // Apply sprite and scale
        if (data.projectileSprite != null)
            sr.sprite = data.projectileSprite;

        transform.localScale = Vector3.one * data.scale;

        // Rotate to face direction
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

        // Deal damage
        col.GetComponent<EnemyController>()?.TakeDamage(data.damage);

        // Knockback
        if (data.knockback > 0f)
        {
            Vector2 dir = (col.transform.position - transform.position).normalized;
            col.GetComponent<Rigidbody2D>()?.AddForce(dir * data.knockback, ForceMode2D.Impulse);
        }

        // AoE explosion
        if (data.isAoE && data.aoeRadius > 0f)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, data.aoeRadius,
                LayerMask.GetMask("Enemy"));

            foreach (var hit in hits)
                hit.GetComponent<EnemyController>()?.TakeDamage(data.damage * 0.5f);
        }

        // Pierce — keep going until pierce count is used up
        pierceCount++;
        if (pierceCount >= data.pierce)
            ReturnToPool();
    }

    void ReturnToPool()
    {
        ObjectPool.Instance.Return(data.weaponName + "_projectile", gameObject);
    }

    void OnDisable() => traveledDistance = 0f;
}