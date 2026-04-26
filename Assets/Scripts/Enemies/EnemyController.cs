using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public EnemyData data;

    private Transform player;
    private float currentHealth;
    private float maxHealth;
    private float currentDamage;
    private float currentSpeed;
    private bool isDead = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Contact Damage")]
    public float damageCooldown = 1f;          // seconds between each damage tick
    private float damageTimer = 0f;

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
        isDead = false;
        damageTimer = damageCooldown; // first contact hit fires immediately
        currentDamage = data.damage;
        currentSpeed = data.moveSpeed;
        maxHealth = data.maxHealth;
        currentHealth = maxHealth;

        sr.color = originalColor;
        if (data.sprite != null)
            sr.sprite = data.sprite;

        // Apply current difficulty bonus immediately on spawn
        if (DifficultyScaler.Instance != null)
            ApplyDifficultyBonus(
                DifficultyScaler.Instance.BonusDamage,
                DifficultyScaler.Instance.BonusSpeed,
                DifficultyScaler.Instance.BonusHealth);
    }

    // Called by DifficultyScaler every 2 minutes
    public void ApplyDifficultyBonus(float bonusDamage,
                                     float bonusSpeed,
                                     float bonusHealth)
    {
        if (data == null) return;

        currentDamage = data.damage + bonusDamage;
        currentSpeed = data.moveSpeed + bonusSpeed;

        float newMaxHealth = data.maxHealth + bonusHealth;

        // If health increased scale current health proportionally
        if (newMaxHealth > maxHealth)
        {
            float ratio = currentHealth / maxHealth;
            maxHealth = newMaxHealth;
            currentHealth = maxHealth * ratio;
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null || data == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * currentSpeed; // ← uses scaled speed
        sr.flipX = dir.x < 0;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || data == null) return;

        currentHealth -= amount;

        // Show yellow popup above enemy
        DamagePopup.Spawn(transform.position + Vector3.up * 0.5f,
                          amount, isPlayer: false);

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlash());

        if (currentHealth <= 0) Die();
    }

    IEnumerator HitFlash()
    {
        sr.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        if (!isDead) sr.color = originalColor;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        sr.color = originalColor;
        rb.linearVelocity = Vector2.zero;

        KillTracker.Instance?.RegisterKill();
        XPOrb.Spawn(transform.position, data.xpValue);

        if (ObjectPool.Instance != null && data != null)
            ObjectPool.Instance.Return(data.enemyName, gameObject);
        else
            gameObject.SetActive(false);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (isDead || data == null) return;

        if (col.gameObject.CompareTag("Player"))
        {
            // Advance the timer — fires once every damageCooldown seconds
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageCooldown)
            {
                damageTimer = 0f;
                // Pass full currentDamage so popup shows the real number (e.g. -8)
                col.gameObject.GetComponent<PlayerHealth>()
                    ?.TakeDamage(currentDamage);
            }
        }
    }

    // Reset timer when enemy leaves contact so it can't instantly tick again on re-entry
    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            damageTimer = damageCooldown; // re-entry hits immediately too
    }

    void OnDisable()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}