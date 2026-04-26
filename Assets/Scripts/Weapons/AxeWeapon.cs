
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeWeapon : MonoBehaviour
{
    private SpecialWeaponData data;
    private int currentLevel;
    private GameObject axePrefab;
    private Transform player;

    public void Initialize(SpecialWeaponData d, int level,
                           GameObject prefab, Transform playerT)
    {
        data = d;
        axePrefab = prefab;
        player = playerT;
        currentLevel = level;

        Debug.Log("[Axe] Initialized"
                + " | Level: " + currentLevel
                + " | Count: " + data.GetCount(currentLevel)
                + " | Damage: " + data.GetDamage(currentLevel)
                + " | Cooldown: " + data.GetCooldown(currentLevel));

        StartCoroutine(ThrowLoop());
    }

    public void Upgrade(int newLevel)
    {
        currentLevel = newLevel;

        Debug.Log("[Axe] Upgraded to level " + currentLevel
                + " | Count: " + data.GetCount(currentLevel)
                + " | Damage: " + data.GetDamage(currentLevel)
                + " | Cooldown: " + data.GetCooldown(currentLevel));
    }

    IEnumerator ThrowLoop()
    {
        while (true)
        {
            float cooldown = data.GetCooldown(currentLevel);

            Debug.Log("[Axe] Waiting " + cooldown + "s before next throw");

            yield return new WaitForSeconds(cooldown);
            ThrowAxes();
        }
    }

    void ThrowAxes()
    {
        int count = data.GetCount(currentLevel);
        float angleStep = 360f / count;
        float startOffset = GetPlayerFacingDir() > 0 ? 0f : 180f;

        Debug.Log("[Axe] ── Throwing " + count + " axes ──"
                + " | Damage each: " + data.GetDamage(currentLevel)
                + " | Facing dir: " + (GetPlayerFacingDir() > 0 ? "Right" : "Left")
                + " | Angle step: " + angleStep + "°");

        for (int i = 0; i < count; i++)
        {
            float angle = startOffset + angleStep * i;

            Debug.Log("[Axe] Axe " + (i + 1) + "/" + count
                    + " → direction: " + angle + "°");

            StartCoroutine(AxeArc(angle, i + 1, count));
        }
    }

    IEnumerator AxeArc(float throwAngleDeg, int axeIndex, int totalAxes)
    {
        if (axePrefab == null)
        {
            Debug.LogError("[Axe] axePrefab is null! "
                         + "Assign it in SpecialWeaponManager.");
            yield break;
        }

        GameObject axe = Instantiate(axePrefab,
            player.position, Quaternion.identity);

        float damage = data.GetDamage(currentLevel);
        float mult = AbilityManager.Instance != null
                              ? AbilityManager.Instance.DamageMultiplier : 1f;
        float finalDamage = damage * mult;

        float totalTime = data.axeTotalTime;
        float arcHeight = data.axeArcHeight;
        float travelDist = data.axeTravelDist;
        float spinSpeed = data.axeSpinSpeed;

        float rad = throwAngleDeg * Mathf.Deg2Rad;
        Vector2 travelDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 startPos = player.position;
        float facingDir = GetPlayerFacingDir();

        Debug.Log("[Axe] Axe " + axeIndex + " launched"
                + " | Angle: " + throwAngleDeg + "°"
                + " | Travel dir: " + travelDir
                + " | Final DMG: " + finalDamage
                + " | Arc H: " + arcHeight
                + " | Dist: " + travelDist
                + " | Time: " + totalTime + "s");

        float elapsed = 0f;
        int hitCount = 0;
        HashSet<EnemyController> alreadyHit = new();

        while (elapsed < totalTime)
        {
            if (axe == null)
            {
                Debug.LogWarning("[Axe] Axe " + axeIndex
                               + " was destroyed mid-arc!");
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / totalTime;

            // Parabolic arc
            Vector2 flatMove = travelDir * travelDist * t;
            float arcOffset = arcHeight * 4f * t * (1f - t);

            axe.transform.position = startPos
                + flatMove
                + Vector2.up * arcOffset;

            // Spin
            axe.transform.Rotate(0f, 0f,
                -spinSpeed * Time.deltaTime * facingDir);

            // Hit detection
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                axe.transform.position, 0.5f);

            foreach (var h in hits)
            {
                if (!h.CompareTag("Enemy")) continue;

                var enemy = h.GetComponent<EnemyController>();
                if (enemy == null || alreadyHit.Contains(enemy)) continue;

                hitCount++;

                Debug.Log("[Axe] Axe " + axeIndex
                        + " HIT enemy: " + h.gameObject.name
                        + " | Damage: " + finalDamage
                        + " | Direction: " + throwAngleDeg + "°"
                        + " | Hit #" + hitCount
                        + " | Arc t: " + t.ToString("F2")
                        + " | Multiplier: " + mult);

                // TakeDamage fires flash + damage popup automatically
                enemy.TakeDamage(finalDamage);
                alreadyHit.Add(enemy);
            }

            yield return null;
        }

        // Arc finished
        Debug.Log("[Axe] Axe " + axeIndex + " arc complete"
                + " | Total enemies hit: " + hitCount
                + " | Direction was: " + throwAngleDeg + "°");

        if (axe != null) Destroy(axe);
    }

    float GetPlayerFacingDir()
    {
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        return (sr != null && sr.flipX) ? -1f : 1f;
    }
}