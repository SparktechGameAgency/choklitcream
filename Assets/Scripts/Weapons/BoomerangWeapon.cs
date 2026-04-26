
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangWeapon : MonoBehaviour
{
    private SpecialWeaponData data;
    private int currentLevel;
    private GameObject boomerangPrefab;
    private Transform player;

    public void Initialize(SpecialWeaponData d, int level,
                           GameObject prefab, Transform playerT)
    {
        data = d;
        boomerangPrefab = prefab;
        player = playerT;
        currentLevel = level;

        Debug.Log("[Boomerang] Initialized"
                + " | Level: " + currentLevel
                + " | Count: " + data.GetCount(currentLevel)
                + " | Damage: " + data.GetDamage(currentLevel)
                + " | Cooldown: " + data.GetCooldown(currentLevel));

        StartCoroutine(ThrowLoop());
    }

    public void Upgrade(int newLevel)
    {
        currentLevel = newLevel;

        Debug.Log("[Boomerang] Upgraded to level " + currentLevel
                + " | Count: " + data.GetCount(currentLevel)
                + " | Damage: " + data.GetDamage(currentLevel)
                + " | Cooldown: " + data.GetCooldown(currentLevel));
    }

    IEnumerator ThrowLoop()
    {
        while (true)
        {
            float cooldown = data.GetCooldown(currentLevel);

            Debug.Log("[Boomerang] Waiting " + cooldown + "s before next throw");

            yield return new WaitForSeconds(cooldown);
            ThrowBoomerangs();
        }
    }

    void ThrowBoomerangs()
    {
        int count = data.GetCount(currentLevel);
        float angleStep = 360f / count;

        Debug.Log("[Boomerang] ── Throwing " + count + " boomerangs ──"
                + " | Damage each: " + data.GetDamage(currentLevel));

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i;

            Debug.Log("[Boomerang] Boomerang " + (i + 1) + "/" + count
                    + " → launch angle: " + angle + "°");

            StartCoroutine(BoomerangArc(angle, i + 1));
        }
    }

    IEnumerator BoomerangArc(float launchAngleDeg, int index)
    {
        if (boomerangPrefab == null)
        {
            Debug.LogError("[Boomerang] Prefab is null!");
            yield break;
        }

        GameObject boom = Instantiate(boomerangPrefab,
            player.position, Quaternion.identity);

        float damage = data.GetDamage(currentLevel);
        float mult = AbilityManager.Instance != null
                              ? AbilityManager.Instance.DamageMultiplier : 1f;
        float finalDamage = damage * mult;

        // Settings from ScriptableObject
        float maxDist = data.boomerangMaxDist;
        float speed = data.boomerangSpeed;
        float spinSpeed = data.boomerangSpinSpeed;
        float curveAngle = data.boomerangCurveAngle;

        // Launch direction
        float rad = launchAngleDeg * Mathf.Deg2Rad;
        Vector2 launchDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        Debug.Log("[Boomerang] " + index + " launched"
                + " | Final DMG: " + finalDamage
                + " | Max dist: " + maxDist
                + " | Speed: " + speed
                + " | Curve angle: " + curveAngle + "°/s");

        HashSet<EnemyController> hitGoing = new(); // hit on the way out
        HashSet<EnemyController> hitReturning = new(); // hit on the way back

        Vector2 currentPos = player.position;
        float currentAngle = launchAngleDeg;
        bool returning = false;
        int totalHits = 0;

        // ── GOING OUT ──────────────────────────────────────────
        while (!returning)
        {
            if (boom == null) yield break;

            // Curve the direction gradually
            currentAngle += curveAngle * Time.deltaTime;
            float curvedRad = currentAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(curvedRad),
                                          Mathf.Sin(curvedRad));

            currentPos += dir * speed * Time.deltaTime;
            boom.transform.position = currentPos;

            // Spin sprite
            boom.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            // Hit detection going out
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                boom.transform.position, 0.4f);

            foreach (var h in hits)
            {
                if (!h.CompareTag("Enemy")) continue;
                var enemy = h.GetComponent<EnemyController>();
                if (enemy == null || hitGoing.Contains(enemy)) continue;

                totalHits++;
                Debug.Log("[Boomerang] " + index
                        + " HIT (going) enemy: " + h.gameObject.name
                        + " | Damage: " + finalDamage
                        + " | Hit #" + totalHits);

                enemy.TakeDamage(finalDamage);
                hitGoing.Add(enemy);
            }

            // Check if far enough — start returning
            float distFromPlayer = Vector2.Distance(
                boom.transform.position, player.position);

            if (distFromPlayer >= maxDist)
            {
                returning = true;
                Debug.Log("[Boomerang] " + index
                        + " reached max distance (" + maxDist
                        + ") — returning to player");
            }

            yield return null;
        }

        // ── RETURNING ──────────────────────────────────────────
        while (true)
        {
            if (boom == null) yield break;

            // Fly directly back toward player current position
            Vector2 toPlayer = (Vector2)player.position
                             - (Vector2)boom.transform.position;

            float distToPlayer = toPlayer.magnitude;

            // Speed up slightly on return — feels satisfying
            float returnSpeed = speed * 1.3f;

            boom.transform.position = Vector2.MoveTowards(
                boom.transform.position,
                player.position,
                returnSpeed * Time.deltaTime);

            // Keep spinning
            boom.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            // Hit detection returning
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                boom.transform.position, 0.4f);

            foreach (var h in hits)
            {
                if (!h.CompareTag("Enemy")) continue;
                var enemy = h.GetComponent<EnemyController>();
                if (enemy == null || hitReturning.Contains(enemy)) continue;

                totalHits++;
                Debug.Log("[Boomerang] " + index
                        + " HIT (returning) enemy: " + h.gameObject.name
                        + " | Damage: " + finalDamage
                        + " | Hit #" + totalHits);

                enemy.TakeDamage(finalDamage);
                hitReturning.Add(enemy);
            }

            // Caught by player
            if (distToPlayer <= 0.3f)
            {
                Debug.Log("[Boomerang] " + index
                        + " returned to player"
                        + " | Total enemies hit: " + totalHits);
                break;
            }

            yield return null;
        }

        if (boom != null) Destroy(boom);
    }
}