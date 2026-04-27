// Scripts/Weapons/BoomerangWeapon.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangWeapon : MonoBehaviour
{
    private SpecialWeaponData data;
    private int               currentLevel;
    private GameObject        boomerangPrefab;
    private Transform         player;

    public void Initialize(SpecialWeaponData d, int level,
                           GameObject prefab, Transform playerT)
    {
        data            = d;
        boomerangPrefab = prefab;
        player          = playerT;
        currentLevel    = level;
        StartCoroutine(ThrowLoop());
    }

    public void Upgrade(int newLevel) => currentLevel = newLevel;

    IEnumerator ThrowLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(data.GetCooldown(currentLevel));
            ThrowBoomerangs();
        }
    }

    void ThrowBoomerangs()
    {
        int   count     = data.GetCount(currentLevel);
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
            StartCoroutine(BoomerangArc(angleStep * i, i + 1));
    }

    IEnumerator BoomerangArc(float launchAngleDeg, int index)
    {
        if (boomerangPrefab == null) yield break;

        GameObject boom = Instantiate(boomerangPrefab,
            player.position, Quaternion.identity);

        float damage      = data.GetDamage(currentLevel);
        float mult        = AbilityManager.Instance != null
                              ? AbilityManager.Instance.DamageMultiplier : 1f;
        float finalDamage = damage * mult;

        float maxDist    = data.boomerangMaxDist;
        float speed      = data.boomerangSpeed;
        float spinSpeed  = data.boomerangSpinSpeed;
        float curveAngle = data.boomerangCurveAngle;

        float   currentAngle = launchAngleDeg;
        Vector2 currentPos   = player.position;
        bool    returning    = false;

        HashSet<EnemyController> hitGoing     = new();
        HashSet<EnemyController> hitReturning = new();

        // Going out
        while (!returning)
        {
            if (boom == null) yield break;

            currentAngle += curveAngle * Time.deltaTime;
            float   curvedRad = currentAngle * Mathf.Deg2Rad;
            Vector2 dir       = new Vector2(Mathf.Cos(curvedRad),
                                            Mathf.Sin(curvedRad));

            currentPos            += dir * speed * Time.deltaTime;
            boom.transform.position = currentPos;
            boom.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                boom.transform.position, 0.4f);

            foreach (var h in hits)
            {
                if (!h.CompareTag("Enemy")) continue;
                var enemy = h.GetComponent<EnemyController>();
                if (enemy == null || hitGoing.Contains(enemy)) continue;
                enemy.TakeDamage(finalDamage);
                hitGoing.Add(enemy);
            }

            if (Vector2.Distance(boom.transform.position, player.position) >= maxDist)
                returning = true;

            yield return null;
        }

        // Returning
        while (true)
        {
            if (boom == null) yield break;

            float distToPlayer = Vector2.Distance(
                boom.transform.position, player.position);

            boom.transform.position = Vector2.MoveTowards(
                boom.transform.position,
                player.position,
                speed * 1.3f * Time.deltaTime);

            boom.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                boom.transform.position, 0.4f);

            foreach (var h in hits)
            {
                if (!h.CompareTag("Enemy")) continue;
                var enemy = h.GetComponent<EnemyController>();
                if (enemy == null || hitReturning.Contains(enemy)) continue;
                enemy.TakeDamage(finalDamage);
                hitReturning.Add(enemy);
            }

            if (distToPlayer <= 0.3f) break;

            yield return null;
        }

        if (boom != null) Destroy(boom);
    }
}
