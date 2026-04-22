
using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [HideInInspector] public float xpAmount;

    private Transform player;
    private bool attracting = false;
    private float attractSpeed = 0f;

    [Header("Settings")]
    public float attractRadius = 3f;
    public float minAttractSpeed = 5f;
    public float maxAttractSpeed = 14f;

    private static readonly string POOL_TAG = "XPOrb";

    // ── Static spawn helper ──────────────────────────────────

    public static void Spawn(Vector3 position, float amount)
    {
        if (!ObjectPool.Instance.HasPool(POOL_TAG))
        {
            Debug.LogError("[XPOrb] Pool not registered! Add XPOrb pool to ObjectPool.");
            return;
        }

        GameObject obj = ObjectPool.Instance.Get(POOL_TAG, position);
        if (obj == null) return;

        XPOrb orb = obj.GetComponent<XPOrb>();
        orb.xpAmount = amount;
        orb.attracting = false;
        orb.attractSpeed = orb.minAttractSpeed;

        // Cache player reference
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) orb.player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Start attracting when player is close enough
        if (dist <= attractRadius)
            attracting = true;

        if (attracting)
        {
            // Speed up as it gets closer — feels satisfying
            attractSpeed = Mathf.Lerp(maxAttractSpeed, minAttractSpeed,
                                      dist / attractRadius);

            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                attractSpeed * Time.deltaTime);

            // Collect when close enough
            if (dist <= 0.2f)
                Collect();
        }
    }

    void Collect()
    {
        PlayerXP.Instance?.GainXP(xpAmount);
        ObjectPool.Instance.Return(POOL_TAG, gameObject);
    }

    // If player walks over orb directly
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            Collect();
    }

    void OnDisable()
    {
        attracting = false;
        attractSpeed = minAttractSpeed;
    }
}