
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public EnemyData data;

    private Transform player;
    private float currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isDead = false; // ← death lock prevents double-death

    [Header("Hit Flash")]
    public Color hitColor = Color.white;
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
        if (enemyData == null)
        {
            Debug.LogError("[Enemy] Initialize called with null EnemyData!");
            return;
        }

        player = playerTransform;
        data = enemyData;
        isDead = false; // reset death lock when re-pooled
        currentHealth = data.maxHealth;
        sr.color = originalColor;

        if (data.sprite != null)
            sr.sprite = data.sprite;
    }

    void FixedUpdate()
    {
        if (isDead || player == null || data == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * data.moveSpeed;
        sr.flipX = dir.x < 0;
    }

    public void TakeDamage(float amount)
    {
        // Ignore damage if already dead or not initialized
        if (isDead || data == null) return;

        currentHealth -= amount;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlash());

        if (currentHealth <= 0) Die();
    }

    IEnumerator HitFlash()
    {
        sr.color = hitColor;
        yield return new WaitForSeconds(flashDuration);

        // Only reset if still alive
        if (!isDead) sr.color = originalColor;
    }

    void Die()
    {
        if (isDead) return; // prevent double death
        isDead = true;

        // Stop flash coroutine cleanly
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        sr.color = originalColor;
        rb.linearVelocity = Vector2.zero;

        // Register kill
        KillTracker.Instance?.RegisterKill();

        // Spawn XP orb
        if (data != null)
            XPOrb.Spawn(transform.position, data.xpValue);

        // Return to pool — check everything exists first
        if (ObjectPool.Instance != null && data != null)
            ObjectPool.Instance.Return(data.enemyName, gameObject);
        else
        {
            Debug.LogWarning("[Enemy] Could not return to pool — disabling instead.");
            gameObject.SetActive(false);
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (isDead || data == null) return;

        if (col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<PlayerHealth>()
                ?.TakeDamage(data.damage * Time.deltaTime);
    }

    void OnDisable()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}