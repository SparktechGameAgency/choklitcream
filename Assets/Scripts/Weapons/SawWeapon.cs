// Scripts/Weapons/SawWeapon.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawWeapon : MonoBehaviour
{
    private SpecialWeaponData data;
    private int               currentLevel;
    private GameObject        sawPrefab;
    private Transform         player;
    private List<GameObject>  saws = new();

    public void Initialize(SpecialWeaponData d, int level,
                           GameObject prefab, Transform playerT)
    {
        data         = d;
        sawPrefab    = prefab;
        player       = playerT;
        currentLevel = level;
        SpawnSaws();
    }

    public void Upgrade(int newLevel)
    {
        currentLevel = newLevel;
        StopAllCoroutines();

        foreach (var s in saws)
            if (s != null) Destroy(s);
        saws.Clear();

        SpawnSaws();
    }

    void SpawnSaws()
    {
        int   count     = data.GetCount(currentLevel);
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
            StartCoroutine(OrbitSaw(i * angleStep));
    }

    IEnumerator OrbitSaw(float startAngle)
    {
        GameObject saw = Instantiate(sawPrefab,
            player.position, Quaternion.identity);
        saw.transform.SetParent(transform);
        saws.Add(saw);

        while (true)
        {
            float angle       = startAngle;
            float damage      = data.GetDamage(currentLevel);
            float speed       = data.orbitSpeed;
            float radius      = data.orbitRadius;
            float mult        = AbilityManager.Instance != null
                                  ? AbilityManager.Instance.DamageMultiplier : 1f;
            float finalDamage = damage * mult;

            HashSet<EnemyController> hitThisPass = new();

            while (angle - startAngle < 360f)
            {
                if (saw == null) yield break;

                float rad = angle * Mathf.Deg2Rad;
                saw.transform.position = player.position + new Vector3(
                    Mathf.Cos(rad) * radius,
                    Mathf.Sin(rad) * radius, 0f);

                saw.transform.Rotate(0f, 0f, speed * Time.deltaTime);

                Collider2D[] hits = Physics2D.OverlapCircleAll(
                    saw.transform.position, 0.4f);

                foreach (var h in hits)
                {
                    if (!h.CompareTag("Enemy")) continue;
                    var enemy = h.GetComponent<EnemyController>();
                    if (enemy == null || hitThisPass.Contains(enemy)) continue;
                    enemy.TakeDamage(finalDamage);
                    hitThisPass.Add(enemy);
                }

                angle += speed * Time.deltaTime;
                yield return null;
            }

            if (saw != null) saw.SetActive(false);
            yield return new WaitForSeconds(data.sawPause);
            if (saw != null) saw.SetActive(true);
        }
    }

    void OnDestroy()
    {
        foreach (var s in saws)
            if (s != null) Destroy(s);
    }
}
