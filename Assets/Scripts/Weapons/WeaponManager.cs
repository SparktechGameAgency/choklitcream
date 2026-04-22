// Scripts/Weapons/WeaponManager.cs
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

        // Give player first weapon at start
        if (availableWeapons.Count > 0)
            EquipWeapon(availableWeapons[0].data);
    }

    void RegisterWeaponPools()
    {
        foreach (var entry in availableWeapons)
        {
            if (entry.data == null || entry.projectilePrefab == null)
            {
                Debug.LogError("[WeaponManager] Missing data or prefab in Available Weapons list!");
                continue;
            }

            string tag = entry.data.weaponName + "_projectile";
            ObjectPool.Instance.AddPool(new ObjectPool.Pool
            {
                tag = tag,
                prefab = entry.projectilePrefab,
                initialSize = entry.poolSize
            });
        }
    }

    public void EquipWeapon(WeaponData data)
    {
        // If already equipped — could add upgrade logic here later
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
                + " | Total weapons: " + equippedWeapons.Count);
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
            if (wc.data != null) list.Add(wc.data);
        return list;
    }
}