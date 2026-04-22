
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "CatSurvivors/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public Sprite weaponIcon;          // ← icon shown in HUD + level up panel
    public Sprite projectileSprite;

    [Header("Damage")]
    public float damage = 20f;
    public float pierce = 1;

    [Header("Fire Rate")]
    public float fireRate = 1f;

    [Header("Projectile")]
    public float projectileSpeed = 8f;
    public float range = 10f;
    public float scale = 1f;

    [Header("Special")]
    public bool isAoE = false;
    public float aoeRadius = 0f;
    public float knockback = 0f;
}