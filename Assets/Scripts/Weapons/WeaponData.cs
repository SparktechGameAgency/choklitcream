
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "CatSurvivors/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damage = 20f;
    public float fireRate = 1f;   // shots per second
    public float projectileSpeed = 8f;
    public float range = 10f;
    public Sprite projectileSprite;
}