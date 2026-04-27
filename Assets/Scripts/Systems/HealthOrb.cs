// Scripts/Systems/HealthOrb.cs
using UnityEngine;

public class HealthOrb : MonoBehaviour
{
    internal float     healAmount;
    internal bool      attracting   = false;
    internal float     attractSpeed = 0f;

    public float minAttractSpeed = 5f;
    public float maxAttractSpeed = 12f;
    public float attractRadius   = 3f;

    private Transform  player;
    private bool       collected = false;

    private static readonly string POOL_TAG = "HealthOrb";

    public static void Spawn(Vector3 position, float amount)
    {
        if (!ObjectPool.Instance.HasPool(POOL_TAG)) return;

        GameObject obj = ObjectPool.Instance.Get(POOL_TAG, position);
        if (obj == null) return;

        HealthOrb orb = obj.GetComponent<HealthOrb>();
        if (orb == null) return;

        orb.healAmount   = amount;
        orb.attracting   = false;
        orb.collected    = false;
        orb.attractSpeed = orb.minAttractSpeed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            orb.player = playerObj.transform;
    }

    void Update()
    {
        if (collected) return;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attractRadius)
            attracting = true;

        if (attracting)
        {
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
        if (collected) return;
        collected = true;

        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.Heal(healAmount);
        }

        ObjectPool.Instance.Return(POOL_TAG, gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (collected) return;
        if (!col.CompareTag("Player")) return;

        player = col.transform;
        Collect();
    }

    void OnDisable()
    {
        attracting   = false;
        collected    = false;
        attractSpeed = minAttractSpeed;
        player       = null;
    }
}
