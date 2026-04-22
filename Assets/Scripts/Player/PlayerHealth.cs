using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Hit-Stop (Slow-Mo on Damage)")]
    [Tooltip("How slow time gets on hit. 0.05 = almost frozen, 0.2 = noticeable slowdown")]
    public float hitStopTimeScale = 0.05f;
    [Tooltip("How long the slow-mo lasts in REAL seconds")]
    public float hitStopDuration = 0.08f;

    private bool hitStopActive = false;

    void Start() => currentHealth = maxHealth;

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // --- Camera shake ---
        CameraFollow.Instance?.Shake();

        // --- Hit-stop (slow-mo freeze) ---
        if (!hitStopActive)
            StartCoroutine(HitStop());

        if (currentHealth <= 0) Die();
    }

    IEnumerator HitStop()
    {
        hitStopActive = true;
        Time.timeScale = hitStopTimeScale;

        yield return new WaitForSecondsRealtime(hitStopDuration);

        Time.timeScale = 1f;
        hitStopActive = false;
    }

    void Die()
    {
        GameTimer.Instance?.StopTimer();
        Debug.Log("Player died!");
        gameObject.SetActive(false);
    }
}