using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Hit Flash (Stun Effect)")]
    public Color hitColor = new Color(1f, 0.3f, 0.3f, 1f);  // red tint
    public float flashDuration = 0.15f;

    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine flashCoroutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    void Start() => currentHealth = maxHealth;

    public void TakeDamage(float amount)
    {
        if (amount <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        // Show red popup on player
        DamagePopup.Spawn(transform.position + Vector3.up * 0.5f,
                          amount, isPlayer: true);

        // ── Camera shake ───────────────────────────────────────
        CameraFollow.Instance?.Shake();

        // ── Player hit flash (stun visual) ────────────────────
        if (sr != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlash());
        }

        if (currentHealth <= 0) Die();
    }

    IEnumerator HitFlash()
    {
        sr.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
        flashCoroutine = null;
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    void Die()
    {
        GameTimer.Instance?.StopTimer();
        Debug.Log("Player died!");
        gameObject.SetActive(false);

        // Show the Game Over panel
        GameUIManager.Instance?.ShowGameOverPanel();
    }
}