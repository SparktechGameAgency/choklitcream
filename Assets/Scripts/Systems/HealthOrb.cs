
using UnityEngine;

public class HealthOrb : MonoBehaviour
{
    internal float healAmount;
    internal bool attracting = false;
    internal float attractSpeed = 0f;
    internal Transform player;

    public float minAttractSpeed = 5f;
    public float maxAttractSpeed = 12f;
    public float attractRadius = 3f;

    private static readonly string POOL_TAG = "HealthOrb";

    // ── Static spawn helper ──────────────────────────────────

    public static void Spawn(Vector3 position, float amount)
    {
        if (!ObjectPool.Instance.HasPool(POOL_TAG))
        {
            Debug.LogError("[HealthOrb] Pool not registered! "
                         + "Add HealthOrb pool to ObjectPool Inspector.");
            return;
        }

        GameObject obj = ObjectPool.Instance.Get(POOL_TAG, position);
        if (obj == null) return;

        HealthOrb orb = obj.GetComponent<HealthOrb>();
        if (orb == null)
        {
            Debug.LogError("[HealthOrb] HealthOrb script missing on prefab!");
            return;
        }

        orb.healAmount = amount;
        orb.attracting = false;
        orb.attractSpeed = orb.minAttractSpeed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) orb.player = playerObj.transform;

        Debug.Log("[HealthOrb] Spawned at " + position
                + " | Heal amount: " + amount);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attractRadius)
            attracting = true;

        if (attracting)
        {
            // Speed up as it gets closer
            attractSpeed = Mathf.Lerp(maxAttractSpeed, minAttractSpeed,
                                      dist / attractRadius);

            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                attractSpeed * Time.deltaTime);

            if (dist <= 0.2f) Collect();
        }
    }

    void Collect()
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            float actual = Mathf.Min(healAmount,
                           ph.maxHealth - ph.currentHealth);
            ph.Heal(healAmount);

            Debug.Log("[HealthOrb] Collected! Healed: " + actual
                    + " | Current HP: " + ph.currentHealth
                    + " / " + ph.maxHealth);
        }

        ObjectPool.Instance.Return(POOL_TAG, gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) Collect();
    }

    void OnDisable()
    {
        attracting = false;
        attractSpeed = minAttractSpeed;
    }
}