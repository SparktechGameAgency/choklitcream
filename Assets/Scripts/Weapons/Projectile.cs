
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private WeaponData data;
    private Vector2 direction;
    private float traveledDistance;

    public void Initialize(Vector2 dir, WeaponData weaponData)
    {
        direction = dir;
        data = weaponData;
        GetComponent<SpriteRenderer>().sprite = weaponData.projectileSprite;

        // Rotate sprite to face direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        float step = data.projectileSpeed * Time.deltaTime;
        transform.Translate(Vector2.right * step);
        traveledDistance += step;

        if (traveledDistance >= data.range)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            col.GetComponent<EnemyController>()?.TakeDamage(data.damage);
            Destroy(gameObject);
        }
    }
}