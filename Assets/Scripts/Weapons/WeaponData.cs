
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "CatSurvivors/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public Sprite projectileSprite;

    [Header("Damage")]
    public float damage = 20f;
    public float pierce = 1;        // how many enemies it passes through

    [Header("Fire Rate")]
    public float fireRate = 1f;     // shots per second

    [Header("Projectile")]
    public float projectileSpeed = 8f;
    public float range = 10f;       // max travel distance before despawn
    public float scale = 1f;        // size of the projectile sprite

    [Header("Special")]
    public bool isAoE = false;      // explodes on impact
    public float aoeRadius = 0f;    // AoE explosion radius
    public float knockback = 0f;    // pushes enemy on hit
}