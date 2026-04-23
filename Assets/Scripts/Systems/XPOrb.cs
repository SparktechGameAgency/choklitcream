
using UnityEngine;

public class XPOrb : MonoBehaviour
{
    internal float xpAmount;
    internal bool attracting = false;
    internal float attractSpeed = 0f;

    public float minAttractSpeed = 5f;
    public float maxAttractSpeed = 14f;

    internal Transform player;

    private static readonly string POOL_TAG = "XPOrb";

    public static void Spawn(Vector3 position, float amount)
    {
        if (!ObjectPool.Instance.HasPool(POOL_TAG))
        {
            Debug.LogError("[XPOrb] Pool not registered!");
            return;
        }

        GameObject obj = ObjectPool.Instance.Get(POOL_TAG, position);
        if (obj == null) return;

        XPOrb orb = obj.GetComponent<XPOrb>();
        if (orb == null)
        {
            Debug.LogError("[XPOrb] XPOrb script missing on prefab!");
            return;
        }

        orb.xpAmount = amount;
        orb.attracting = false;
        orb.attractSpeed = orb.minAttractSpeed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) orb.player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Get attract radius from AbilityManager (includes magnet upgrades)
        float radius = AbilityManager.Instance != null
            ? AbilityManager.Instance.AttractRadius : 3f;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= radius)
            attracting = true;

        if (attracting)
        {
            attractSpeed = Mathf.Lerp(maxAttractSpeed, minAttractSpeed,
                                      dist / radius);

            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                attractSpeed * Time.deltaTime);

            if (dist <= 0.2f) Collect();
        }
    }

    void Collect()
    {
        // Apply XP multiplier from AbilityManager
        float multiplier = AbilityManager.Instance != null
            ? AbilityManager.Instance.XPMultiplier : 1f;

        PlayerXP.Instance?.GainXP(xpAmount * multiplier);
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