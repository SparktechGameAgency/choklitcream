
using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    public Transform fill;          // drag HealthBarFill here
    public PlayerHealth playerHealth;  // drag Player here

    [Header("Settings")]
    public float maxWidth = 1f;     // matches your fill's base X scale
    public Vector3 offset = new Vector3(0f, -0.7f, 0f); // position below player

    private Transform playerTransform;

    void Start()
    {
        playerTransform = playerHealth.transform;
    }

    void LateUpdate()
    {
        // Follow the player in world space
        transform.position = playerTransform.position + offset;

        // Always face the camera (stops bar rotating with player)
        transform.rotation = Quaternion.identity;

        // Scale the fill based on current health percentage
        float pct = playerHealth.currentHealth / playerHealth.maxHealth;
        pct = Mathf.Clamp01(pct);

        // Scale from left side — not center
        fill.localScale = new Vector3(maxWidth * pct, fill.localScale.y, 1f);
        fill.localPosition = new Vector3(-maxWidth / 2f + (maxWidth * pct / 2f), 0f, 0f);
    }
}