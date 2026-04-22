
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance;

    [System.Serializable]
    public class WeaponEntry
    {
        public WeaponData data;
        [Tooltip("Projectile prefab for this weapon")]
        public GameObject projectilePrefab;
        [Tooltip("How many projectiles to pre-pool")]
        public int poolSize = 20;
    }

    [Header("Starting Weapons")]
    [Tooltip("Add all weapon types here — first one is given to player at start")]
    public List<WeaponEntry> availableWeapons = new();

    [Header("References")]
    public Transform player;

    private List<WeaponController> equippedWeapons = new();

    void Awake() => Instance = this;

    void Start()
    {
        // Register all weapon projectiles into the object pool
        RegisterWeaponPools();

        // Give player their starting weapon (first in list)
        if (availableWeapons.Count > 0)
            EquipWeapon(availableWeapons[0].data);
    }

    void RegisterWeaponPools()
    {
        foreach (var entry in availableWeapons)
        {
            string tag = entry.data.weaponName + "_projectile";

            // Dynamically add to ObjectPool at runtime
            ObjectPool.Pool pool = new ObjectPool.Pool
            {
                tag = tag,
                prefab = entry.projectilePrefab,
                initialSize = entry.poolSize
            };

            ObjectPool.Instance.AddPool(pool);
        }
    }

    // Call this when player picks an upgrade card
    public void EquipWeapon(WeaponData data)
    {
        // Check if already equipped — upgrade fire rate instead
        foreach (var wc in equippedWeapons)
        {
            if (wc.data.weaponName == data.weaponName)
            {
                Debug.Log(data.weaponName + " already equipped — upgrade here later!");
                return;
            }
        }

        // Create a new WeaponController child on the player
        GameObject obj = new GameObject("Weapon_" + data.weaponName);
        obj.transform.SetParent(player);
        obj.transform.localPosition = Vector3.zero;

        WeaponController wc2 = obj.AddComponent<WeaponController>();
        wc2.Initialize(data, player);
        equippedWeapons.Add(wc2);

        Debug.Log("Equipped: " + data.weaponName);
    }

    public List<WeaponData> GetAvailableWeaponDatas()
    {
        List<WeaponData> list = new();
        foreach (var e in availableWeapons) list.Add(e.data);
        return list;
    }
}