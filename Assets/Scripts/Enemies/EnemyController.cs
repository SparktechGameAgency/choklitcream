
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyData data;

    private Transform player;
    private float currentHealth;
    private Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    // Called by the object pool when this enemy is activated
    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        currentHealth = data.maxHealth;
        GetComponent<SpriteRenderer>().sprite = data.sprite;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * data.moveSpeed;

        // Flip sprite to face the player
        GetComponent<SpriteRenderer>().flipX = dir.x < 0;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        // Drop XP, then return to pool
        XPOrb.Spawn(transform.position, data.xpValue);
        EnemySpawner.Instance.ReturnToPool(gameObject);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(data.damage * Time.deltaTime);
    }
}