// Scripts/Weapons/BeamWeapon.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamWeapon : MonoBehaviour
{
    private SpecialWeaponData  data;
    private int                currentLevel;
    private Transform          player;
    private List<LineRenderer> beams = new();

    [Header("Beam Color")]
    public Color beamStartColor = new Color(0.3f, 0.9f, 1f, 1f);
    public Color beamEndColor   = new Color(0.3f, 0.9f, 1f, 0f);

    public void Initialize(SpecialWeaponData d, int level, Transform playerT)
    {
        data         = d;
        player       = playerT;
        currentLevel = level;
        CreateBeams();
        StartCoroutine(BeamLoop());
    }

    public void Upgrade(int newLevel)
    {
        currentLevel = newLevel;
        foreach (var b in beams)
            if (b != null) Destroy(b.gameObject);
        beams.Clear();
        CreateBeams();
    }

    void CreateBeams()
    {
        int count = data.GetCount(currentLevel);

        for (int i = 0; i < count; i++)
        {
            GameObject obj = new GameObject("Beam_" + i);
            obj.transform.SetParent(transform);

            LineRenderer lr  = obj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth    = data.beamWidth;
            lr.endWidth      = data.beamWidth * 0.3f;
            lr.material      = new Material(Shader.Find("Sprites/Default"));
            lr.startColor    = beamStartColor;
            lr.endColor      = beamEndColor;
            lr.sortingOrder  = 5;
            lr.enabled       = false;
            lr.useWorldSpace = true;

            beams.Add(lr);
        }
    }

    IEnumerator BeamLoop()
    {
        while (true)
        {
            List<Transform> targets = GetNNearest(beams.Count);

            for (int i = 0; i < beams.Count; i++)
            {
                Transform target = i < targets.Count ? targets[i] : null;
                if (target != null)
                    StartCoroutine(FireBeam(beams[i], target));
            }

            yield return new WaitForSeconds(data.GetCooldown(currentLevel));
        }
    }

    IEnumerator FireBeam(LineRenderer lr, Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            yield break;

        float damage      = data.GetDamage(currentLevel);
        float mult        = AbilityManager.Instance != null
                              ? AbilityManager.Instance.DamageMultiplier : 1f;
        float finalDamage = damage * mult;

        target.GetComponent<EnemyController>()?.TakeDamage(finalDamage);

        lr.enabled = true;
        float elapsed = 0f;

        while (elapsed < data.beamDuration)
        {
            elapsed += Time.deltaTime;

            lr.SetPosition(0, player.position);
            lr.SetPosition(1,
                target != null && target.gameObject.activeInHierarchy
                    ? target.position
                    : player.position + Vector3.up * data.beamRange);

            float alpha   = 1f - elapsed / data.beamDuration;
            lr.startColor = new Color(beamStartColor.r, beamStartColor.g,
                                      beamStartColor.b, alpha);
            lr.endColor   = new Color(beamEndColor.r, beamEndColor.g,
                                      beamEndColor.b, alpha * 0.3f);
            yield return null;
        }

        lr.enabled = false;
    }

    List<Transform> GetNNearest(int n)
    {
        GameObject[]     all    = GameObject.FindGameObjectsWithTag("Enemy");
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

    void OnDestroy()
    {
        foreach (var b in beams)
            if (b != null) Destroy(b.gameObject);
    }
}
