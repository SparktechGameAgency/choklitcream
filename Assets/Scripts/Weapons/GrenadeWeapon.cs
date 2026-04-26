
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeWeapon : MonoBehaviour
{
    private SpecialWeaponData data;
    private int currentLevel;
    private GameObject grenadePrefab;
    private Transform player;

    public void Initialize(SpecialWeaponData d, int level,
                           GameObject prefab, Transform playerT)
    {
        data = d;
        grenadePrefab = prefab;
        player = playerT;
        currentLevel = level;
        StartCoroutine(ThrowLoop());
    }

    public void Upgrade(int newLevel) => currentLevel = newLevel;

    IEnumerator ThrowLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(data.GetCooldown(currentLevel));
            ThrowGrenades();
        }
    }

    void ThrowGrenades()
    {
        int count = data.GetCount(currentLevel);
        List<Transform> targets = GetNNearest(count);

        Debug.Log("[Grenade] Throwing " + count
                + " grenades | Targets found: " + targets.Count);

        for (int i = 0; i < count; i++)
        {
            Transform target = i < targets.Count ? targets[i] : null;
            if (target == null) continue;

            GameObject g = Instantiate(grenadePrefab,
                player.position, Quaternion.identity);
            StartCoroutine(GrenadeSeek(g, target));
        }
    }

    IEnumerator GrenadeSeek(GameObject grenade, Transform target)
    {
        float elapsed = 0f;
        float delay = data.explosionDelay;
        float speed = data.grenadeSpeed;

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            if (grenade == null) yield break;

            if (target != null && target.gameObject.activeInHierarchy)
                grenade.transform.position = Vector2.MoveTowards(
                    grenade.transform.position,
                    target.position, speed * Time.deltaTime);

            grenade.transform.Rotate(0f, 0f, 300f * Time.deltaTime);
            yield return null;
        }

        if (grenade == null) yield break;

        Explode(grenade.transform.position);
        Destroy(grenade);
    }

    void Explode(Vector3 pos)
    {
        float damage = data.GetDamage(currentLevel);
        float mult = AbilityManager.Instance != null
                              ? AbilityManager.Instance.DamageMultiplier : 1f;
        float finalDamage = damage * mult;

        // Use tag instead of layer
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, data.explosionRadius);

        int hitCount = 0;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            var enemy = hit.GetComponent<EnemyController>();
            if (enemy == null) continue;

            // ── Debug log ──────────────────────────────────────
            Debug.Log("[Grenade] Hit enemy: " + hit.gameObject.name
                    + " | Damage: " + finalDamage);

            enemy.TakeDamage(finalDamage); // flash + popup inside here
            hitCount++;
        }

        Debug.Log("[Grenade] Exploded at " + pos
                + " | Enemies hit: " + hitCount
                + " | Radius: " + data.explosionRadius);
    }

    List<Transform> GetNNearest(int n)
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> active = new();

        foreach (var e in all)
            if (e.activeInHierarchy) active.Add(e);

        active.Sort((a, b) =>
            Vector2.Distance(player.position, a.transform.position)
            .CompareTo(Vector2.Distance(player.position, b.transform.position)));

        List<Transform> result = new();
        for (int i = 0; i < Mathf.Min(n, active.Count); i++)
            result.Add(active[i].transform);
        return result;
    }
}