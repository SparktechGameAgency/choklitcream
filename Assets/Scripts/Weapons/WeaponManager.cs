
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance;

    [Header("Weapons — drag WeaponData assets here")]
    public List<WeaponData> availableWeapons = new();

    [Header("References")]
    public Transform player;

    private List<WeaponController> equippedWeapons = new();

    void Awake() => Instance = this;

    void Start()
    {
        RegisterWeaponPools();

        if (availableWeapons.Count > 0)
            EquipWeapon(availableWeapons[0]);
    }

    void RegisterWeaponPools()
    {
        foreach (var data in availableWeapons)
        {
            if (data == null)
            {
                Debug.LogError("[WeaponManager] Null entry in Available Weapons!");
                continue;
            }

            if (data.projectilePrefab == null)
            {
                Debug.LogError("[WeaponManager] " + data.weaponName
                             + " has no projectile prefab in WeaponData!");
                continue;
            }

            ObjectPool.Instance.AddPool(new ObjectPool.Pool
            {
                tag = data.PoolTag,
                prefab = data.projectilePrefab,
                initialSize = 20
            });

            Debug.Log("[WeaponManager] Pool registered: " + data.PoolTag);
        }
    }

    public void EquipWeapon(WeaponData data)
    {
        foreach (var wc in equippedWeapons)
        {
            if (wc.data.weaponName == data.weaponName)
            {
                Debug.Log("[WeaponManager] Already equipped: " + data.weaponName);
                return;
            }
        }

        GameObject obj = new GameObject("Weapon_" + data.weaponName);
        obj.transform.SetParent(player);
        obj.transform.localPosition = Vector3.zero;

        WeaponController wc2 = obj.AddComponent<WeaponController>();
        wc2.Initialize(data, player);
        equippedWeapons.Add(wc2);

        Debug.Log("[WeaponManager] Equipped: " + data.weaponName
                + " | Total: " + equippedWeapons.Count);
    }

    public List<WeaponData> GetAvailableWeaponDatas()
    {
        List<WeaponData> list = new();
        foreach (var d in availableWeapons)
            if (d != null) list.Add(d);
        return list;
    }

    public List<WeaponData> GetEquippedWeaponDatas()
    {
        List<WeaponData> list = new();
        foreach (var wc in equippedWeapons)
            if (wc != null && wc.data != null) list.Add(wc.data);
        return list;
    }
}