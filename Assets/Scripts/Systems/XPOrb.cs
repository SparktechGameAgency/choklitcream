
using UnityEngine;

public class XPOrb : MonoBehaviour
{
    public static GameObject orbPrefab; // assign in Resources or a Manager
    public float xpAmount;
    public float attractSpeed = 5f;
    public float attractRadius = 3f;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public static void Spawn(Vector3 position, float amount)
    {
        if (orbPrefab == null) return;
        var orb = Instantiate(orbPrefab, position, Quaternion.identity);
        orb.GetComponent<XPOrb>().xpAmount = amount;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < attractRadius)
        {
            // Fly toward player
            transform.position = Vector2.MoveTowards(
                transform.position, player.position,
                attractSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerXP.Instance?.GainXP(xpAmount);
            Destroy(gameObject);
        }
    }
}