// Scripts/Weapons/SpecialWeaponManager.cs
using System.Collections.Generic;
using UnityEngine;

public class SpecialWeaponManager : MonoBehaviour
{
    public static SpecialWeaponManager Instance;

    [Header("All special weapons — drag assets here")]
    public List<SpecialWeaponData> allWeapons = new();

    [Header("References")]
    public Transform   player;
    public GameObject  sawPrefab;
    public GameObject  grenadePrefab;
    public GameObject  axePrefab;
    public GameObject  boomerangPrefab;

    private Dictionary<string, int>           weaponLevels     = new();
    private Dictionary<string, MonoBehaviour> weaponComponents = new();

    void Awake()
    {
        Instance = this;
        foreach (var w in allWeapons)
            weaponLevels[w.weaponName] = 0;
    }

    public int GetLevel(SpecialWeaponData data)
        => weaponLevels.ContainsKey(data.weaponName)
            ? weaponLevels[data.weaponName] : 0;

    public bool IsMaxLevel(SpecialWeaponData data)
        => GetLevel(data) >= data.MaxLevel;

    public List<SpecialWeaponData> GetAvailableWeapons()
    {
        List<SpecialWeaponData> list = new();
        foreach (var w in allWeapons)
            if (!IsMaxLevel(w)) list.Add(w);
        return list;
    }

    public void EquipOrUpgrade(SpecialWeaponData data)
    {
        int currentLevel = GetLevel(data);
        if (currentLevel >= data.MaxLevel) return;

        int newLevel = currentLevel + 1;
        weaponLevels[data.weaponName] = newLevel;

        if (currentLevel == 0) CreateWeapon(data, newLevel);
        else                   UpgradeWeapon(data, newLevel);
    }

    void CreateWeapon(SpecialWeaponData data, int level)
    {
        GameObject obj = new GameObject("SpecialWeapon_" + data.weaponName);
        obj.transform.SetParent(player);
        obj.transform.localPosition = Vector3.zero;

        switch (data.weaponType)
        {
            case SpecialWeaponType.Saw:
                if (sawPrefab == null) return;
                var saw = obj.AddComponent<SawWeapon>();
                saw.Initialize(data, level, sawPrefab, player);
                weaponComponents[data.weaponName] = saw;
                break;

            case SpecialWeaponType.Grenade:
                if (grenadePrefab == null) return;
                var grenade = obj.AddComponent<GrenadeWeapon>();
                grenade.Initialize(data, level, grenadePrefab, player);
                weaponComponents[data.weaponName] = grenade;
                break;

            case SpecialWeaponType.Beam:
                var beam = obj.AddComponent<BeamWeapon>();
                beam.Initialize(data, level, player);
                weaponComponents[data.weaponName] = beam;
                break;

            case SpecialWeaponType.Axe:
                if (axePrefab == null) return;
                var axe = obj.AddComponent<AxeWeapon>();
                axe.Initialize(data, level, axePrefab, player);
                weaponComponents[data.weaponName] = axe;
                break;

            case SpecialWeaponType.Boomerang:
                if (boomerangPrefab == null) return;
                var boom = obj.AddComponent<BoomerangWeapon>();
                boom.Initialize(data, level, boomerangPrefab, player);
                weaponComponents[data.weaponName] = boom;
                break;
        }
    }

    void UpgradeWeapon(SpecialWeaponData data, int newLevel)
    {
        if (!weaponComponents.ContainsKey(data.weaponName)) return;

        var comp = weaponComponents[data.weaponName];
        if      (comp is SawWeapon       saw)     saw.Upgrade(newLevel);
        else if (comp is GrenadeWeapon   grenade)  grenade.Upgrade(newLevel);
        else if (comp is BeamWeapon      beam)     beam.Upgrade(newLevel);
        else if (comp is AxeWeapon       axe)      axe.Upgrade(newLevel);
        else if (comp is BoomerangWeapon boom)     boom.Upgrade(newLevel);
    }
}
