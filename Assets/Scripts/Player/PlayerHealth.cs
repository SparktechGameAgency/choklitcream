
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    /// <summary>
    /// Flat damage reduction per hit. Set automatically by AbilityManager
    /// when the Armor ability is upgraded, but can also be edited in the Inspector.
    /// </summary>
    [Header("Armor")]
    public float armor = 0f;

    [Header("Hit Flash")]
    public Color hitColor = new Color(1f, 0.3f, 0.3f, 1f);
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

        // Apply flat armor reduction — minimum 1 damage always gets through
        float reducedAmount = Mathf.Max(1f, amount - armor);

        currentHealth -= reducedAmount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        DamagePopup.Spawn(transform.position + Vector3.up * 0.5f,
                          reducedAmount, isPlayer: true);
        CameraFollow.Instance?.Shake();

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
        gameObject.SetActive(false);
        GameUIManager.Instance?.ShowGameOverPanel();
    }
}