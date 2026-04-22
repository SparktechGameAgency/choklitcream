
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance;

    [System.Serializable]
    public class WeaponEntry
    {
        public WeaponData data;
        public GameObject projectilePrefab;
        public int poolSize = 20;
    }

    [Header("Weapons")]
    public List<WeaponEntry> availableWeapons = new();

    [Header("References")]
    public Transform player;

    private List<WeaponController> equippedWeapons = new();

    void Awake() => Instance = this;

    void Start()
    {
        RegisterWeaponPools();

        if (availableWeapons.Count > 0)
            EquipWeapon(availableWeapons[0].data);
    }

    void RegisterWeaponPools()
    {
        foreach (var entry in availableWeapons)
        {
            if (entry.data == null || entry.projectilePrefab == null)
            {
                Debug.LogError("[WeaponManager] Missing data or prefab!");
                continue;
            }

            ObjectPool.Instance.AddPool(new ObjectPool.Pool
            {
                tag = entry.data.weaponName + "_projectile",
                prefab = entry.projectilePrefab,
                initialSize = entry.poolSize
            });
        }
    }

    public void EquipWeapon(WeaponData data)
    {
        // Already equipped check
        foreach (var wc in equippedWeapons)
        {
            if (wc.data.weaponName == data.weaponName)
            {
                Debug.Log("[WeaponManager] Already equipped: " + data.weaponName);
                return;
            }
        }

        // Create WeaponController child on Player
        GameObject obj = new GameObject("Weapon_" + data.weaponName);
        obj.transform.SetParent(player);
        obj.transform.localPosition = Vector3.zero;

        WeaponController wc2 = obj.AddComponent<WeaponController>();
        wc2.Initialize(data, player);
        equippedWeapons.Add(wc2);

        // Notify the HUD to add the icon
        EquippedWeaponsHUD.Instance?.AddWeaponIcon(data);

        Debug.Log("[WeaponManager] Equipped: " + data.weaponName
                + " | Total: " + equippedWeapons.Count);
    }

    public List<WeaponData> GetAvailableWeaponDatas()
    {
        List<WeaponData> list = new();
        foreach (var e in availableWeapons)
            if (e.data != null) list.Add(e.data);
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