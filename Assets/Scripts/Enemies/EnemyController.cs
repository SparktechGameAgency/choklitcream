// Scripts/Enemies/EnemyController.cs
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public EnemyData data;

    private Transform player;
    private float currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Hit Flash")]
    public Color hitColor = Color.white;   // flashes white on hit
    public float flashDuration = 0.2f;

    private Color originalColor;
    private Coroutine flashCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public void Initialize(Transform playerTransform, EnemyData enemyData)
    {
        player = playerTransform;
        data = enemyData;
        currentHealth = data.maxHealth;
        sr.color = originalColor; // reset color when re-pooled

        if (data.sprite != null)
            sr.sprite = data.sprite;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * data.moveSpeed;
        sr.flipX = dir.x < 0;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Trigger the hit flash
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlash());

        if (currentHealth <= 0) Die();
    }

    IEnumerator HitFlash()
    {
        sr.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
    }

    void Die()
    {
        // Stop flash if dying mid-flash
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        sr.color = originalColor;
        XPOrb.Spawn(transform.position, data.xpValue);
        ObjectPool.Instance.Return(data.enemyName, gameObject);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<PlayerHealth>()
                ?.TakeDamage(data.damage * Time.deltaTime);
    }

    void OnDisable()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}