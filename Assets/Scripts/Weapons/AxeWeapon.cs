// Scripts/Weapons/AxeWeapon.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeWeapon : MonoBehaviour
{
    private SpecialWeaponData data;
    private int               currentLevel;
    private GameObject        axePrefab;
    private Transform         player;

    public void Initialize(SpecialWeaponData d, int level,
                           GameObject prefab, Transform playerT)
    {
        data         = d;
        axePrefab    = prefab;
        player       = playerT;
        currentLevel = level;
        StartCoroutine(ThrowLoop());
    }

    public void Upgrade(int newLevel) => currentLevel = newLevel;

    IEnumerator ThrowLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(data.GetCooldown(currentLevel));
            ThrowAxes();
        }
    }

    void ThrowAxes()
    {
        int   count       = data.GetCount(currentLevel);
        float angleStep   = 360f / count;
        float startOffset = GetPlayerFacingDir() > 0 ? 0f : 180f;

        for (int i = 0; i < count; i++)
        {
            float angle = startOffset + angleStep * i;
            StartCoroutine(AxeArc(angle, i + 1));
        }
    }

    IEnumerator AxeArc(float throwAngleDeg, int axeIndex)
    {
        if (axePrefab == null) yield break;

        GameObject axe = Instantiate(axePrefab,
            player.position, Quaternion.identity);

        float damage      = data.GetDamage(currentLevel);
        float mult        = AbilityManager.Instance != null
                              ? AbilityManager.Instance.DamageMultiplier : 1f;
        float finalDamage = damage * mult;

        float totalTime  = data.axeTotalTime;
        float arcHeight  = data.axeArcHeight;
        float travelDist = data.axeTravelDist;
        float spinSpeed  = data.axeSpinSpeed;

        float   rad       = throwAngleDeg * Mathf.Deg2Rad;
        Vector2 travelDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 startPos  = player.position;
        float   facingDir = GetPlayerFacingDir();
        float   elapsed   = 0f;

        HashSet<EnemyController> alreadyHit = new();

        while (elapsed < totalTime)
        {
            if (axe == null) yield break;

            elapsed += Time.deltaTime;
            float t  = elapsed / totalTime;

            // Asymmetric arc — rises smooth, falls controlled
            float arcOffset;
            if (t < 0.45f)
            {
                float riseT = t / 0.45f;
                arcOffset   = arcHeight * (1f - (1f - riseT) * (1f - riseT));
            }
            else
            {
                float fallT = (t - 0.45f) / 0.55f;
                arcOffset   = arcHeight * (1f - fallT * fallT);
            }

            axe.transform.position = startPos
                + travelDir * travelDist * t
                + Vector2.up * arcOffset;

            axe.transform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime * facingDir);

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                axe.transform.position, 0.5f);

            foreach (var h in hits)
            {
                if (!h.CompareTag("Enemy")) continue;
                var enemy = h.GetComponent<EnemyController>();
                if (enemy == null || alreadyHit.Contains(enemy)) continue;
                enemy.TakeDamage(finalDamage);
                alreadyHit.Add(enemy);
            }

            yield return null;
        }

        if (axe != null) Destroy(axe);
    }

    float GetPlayerFacingDir()
    {
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        return (sr != null && sr.flipX) ? -1f : 1f;
    }
}
