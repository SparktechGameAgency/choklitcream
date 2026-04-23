
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "CatSurvivors/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public Sprite weaponIcon;

    [Header("Projectile")]
    public GameObject projectilePrefab;  // ← moved here from WeaponManager
    public Sprite projectileSprite;
    public float projectileSpeed = 8f;
    public float range = 10f;
    public float scale = 1f;

    [Header("Damage")]
    public float damage = 20f;
    public float pierce = 1;

    [Header("Fire Rate")]
    public float fireRate = 1f;

    [Header("Special")]
    public bool isAoE = false;
    public float aoeRadius = 0f;
    public float knockback = 0f;

    public string PoolTag => weaponName.Replace(" ", "_") + "_proj";
}