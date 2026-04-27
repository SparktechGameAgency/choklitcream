// Scripts/Enemies/EnemyController.cs
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public EnemyData data;

    private Transform player;
    private float     currentHealth;
    private float     maxHealth;
    private float     currentDamage;
    private float     currentSpeed;
    private bool      isDead = false;

    private Rigidbody2D    rb;
    private SpriteRenderer sr;

    [Header("Contact Damage")]
    public float damageCooldown = 1f;
    private float damageTimer   = 0f;

    [Header("Hit Flash")]
    public Color hitColor      = Color.white;
    public float flashDuration = 0.2f;

    private Color     originalColor;
    private Coroutine flashCoroutine;

    void Awake()
    {
        rb            = GetComponent<Rigidbody2D>();
        sr            = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public void Initialize(Transform playerTransform, EnemyData enemyData)
    {
        if (enemyData == null) return;

        player        = playerTransform;
        data          = enemyData;
        isDead        = false;
        damageTimer   = damageCooldown;
        currentDamage = data.damage;
        currentSpeed  = data.moveSpeed;
        maxHealth     = data.maxHealth;
        currentHealth = maxHealth;
        sr.color      = originalColor;

        if (data.sprite != null)
            sr.sprite = data.sprite;

        if (DifficultyScaler.Instance != null)
            ApplyDifficultyBonus(
                DifficultyScaler.Instance.BonusDamage,
                DifficultyScaler.Instance.BonusSpeed,
                DifficultyScaler.Instance.BonusHealth);
    }

    public void ApplyDifficultyBonus(float bonusDamage,
                                     float bonusSpeed,
                                     float bonusHealth)
    {
        if (data == null) return;

        currentDamage = data.damage    + bonusDamage;
        currentSpeed  = data.moveSpeed + bonusSpeed;

        float newMaxHealth = data.maxHealth + bonusHealth;

        if (newMaxHealth > maxHealth)
        {
            float ratio   = currentHealth / maxHealth;
            maxHealth     = newMaxHealth;
            currentHealth = maxHealth * ratio;
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null || data == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * currentSpeed;
        sr.flipX          = dir.x < 0;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || data == null) return;

        currentHealth -= amount;

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

        sr.color          = originalColor;
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
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageCooldown)
            {
                damageTimer = 0f;
                col.gameObject.GetComponent<PlayerHealth>()
                    ?.TakeDamage(currentDamage);
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            damageTimer = damageCooldown;
    }

    void OnDisable()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}
