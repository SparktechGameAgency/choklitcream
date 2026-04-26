
using UnityEngine;

public enum SpecialWeaponType { Saw, Grenade, Beam, Axe, Boomerang }

[CreateAssetMenu(fileName = "SpecialWeaponData",
                 menuName = "CatSurvivors/Special Weapon Data")]
public class SpecialWeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public Sprite weaponIcon;
    [TextArea]
    public string description;
    public SpecialWeaponType weaponType;

    [Header("Per Level — 5 values each")]
    [Tooltip("How many instances spawn at each level")]
    public int[] countPerLevel = new int[5] { 1, 2, 2, 3, 3 };
    public float[] damagePerLevel = new float[5] { 10f, 15f, 20f, 28f, 40f };
    public float[] cooldownPerLevel = new float[5] { 3f, 2.5f, 2f, 1.5f, 1f };

    [Header("Saw Settings")]
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 180f;
    public float sawPause = 1f;

    [Header("Grenade Settings")]
    public float grenadeSpeed = 4f;
    public float explosionDelay = 1f;
    public float explosionRadius = 2f;

    [Header("Beam Settings")]
    public float beamRange = 8f;
    public float beamWidth = 0.08f;
    public float beamDuration = 0.4f;

    [Header("Axe Settings")]
    public float axeArcHeight = 3.5f;  // how high the axe goes
    public float axeTravelDist = 4f;    // how far forward it flies
    public float axeTotalTime = 1.2f;  // how long the full arc takes
    public float axeSpread = 20f;   // angle between multiple axes
    public float axeSpinSpeed = 600f;  // rotation speed while flying

    [Header("Boomerang Settings")]
    public float boomerangMaxDist = 5f;    // how far it travels before returning
    public float boomerangSpeed = 6f;    // movement speed
    public float boomerangSpinSpeed = 400f;  // rotation speed
    public float boomerangCurveAngle = 80f;   // how much it curves going out

    public int MaxLevel => countPerLevel.Length;
    public int GetCount(int lv) => countPerLevel[Mathf.Clamp(lv - 1, 0, MaxLevel - 1)];
    public float GetDamage(int lv) => damagePerLevel[Mathf.Clamp(lv - 1, 0, MaxLevel - 1)];
    public float GetCooldown(int lv) => cooldownPerLevel[Mathf.Clamp(lv - 1, 0, MaxLevel - 1)];
}