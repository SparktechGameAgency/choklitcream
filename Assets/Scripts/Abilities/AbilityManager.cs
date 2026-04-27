
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    [Header("All available abilities — drag assets here")]
    public List<AbilityData> allAbilities = new();

    private Dictionary<string, int> abilityLevels = new();

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    [Header("Base XP attract radius")]
    [Tooltip("Starting attract radius — tweak here")]
    public float baseAttractRadius = 3f;

    // ── Public read-only stats ──────────────────────────────────────────────

    public float DamageMultiplier { get; private set; } = 1f;
    public float XPMultiplier { get; private set; } = 1f;
    public float AttractRadius { get; private set; }

    /// <summary>Flat damage subtracted from every hit before it is applied.</summary>
    public float ArmorValue { get; private set; } = 0f;

    /// <summary>Multiplier on all weapon fire rates (e.g. 1.5 = 50% faster).</summary>
    public float FireRateMultiplier { get; private set; } = 1f;

    /// <summary>Multiplier on all XP gained from orbs.</summary>
    public float XPBoostMultiplier { get; private set; } = 1f;

    // ───────────────────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        foreach (var a in allAbilities)
            abilityLevels[a.abilityName] = 0;
    }

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        playerHealth = FindObjectOfType<PlayerHealth>();
        AttractRadius = baseAttractRadius;
    }

    public int GetLevel(AbilityData data)
        => abilityLevels.ContainsKey(data.abilityName)
            ? abilityLevels[data.abilityName] : 0;

    public bool IsMaxLevel(AbilityData data)
        => GetLevel(data) >= data.MaxLevel;

    public List<AbilityData> GetAvailableAbilities()
    {
        List<AbilityData> list = new();
        foreach (var a in allAbilities)
            if (!IsMaxLevel(a)) list.Add(a);
        return list;
    }

    public void UpgradeAbility(AbilityData data)
    {
        int currentLevel = GetLevel(data);
        if (currentLevel >= data.MaxLevel) return;

        int newLevel = currentLevel + 1;
        abilityLevels[data.abilityName] = newLevel;
        ApplyAbility(data, newLevel);

        // Notify the HUD so it updates immediately
        AbilityHUDPanel.Instance?.Refresh();
    }

    void ApplyAbility(AbilityData data, int level)
    {
        float value = data.GetValue(level);

        switch (data.abilityType)
        {
            // ── Existing abilities ─────────────────────────────────────────
            case AbilityType.AddDamage:
                DamageMultiplier = value;
                break;

            case AbilityType.AddMoveSpeed:
                if (playerMovement != null)
                    playerMovement.moveSpeed = value;
                break;

            case AbilityType.AddMagnet:
                AttractRadius = value;
                break;

            case AbilityType.AddXPMultiplier:
                XPMultiplier = value;
                break;

            case AbilityType.AddHealth:
                if (playerHealth != null)
                {
                    playerHealth.maxHealth += value;
                    playerHealth.currentHealth += value;
                }
                break;

            // ── New abilities ──────────────────────────────────────────────

            // Armor: flat damage absorbed per hit.
            // valuesPerLevel example: { 2, 4, 6, 9, 12 }
            case AbilityType.AddArmor:
                ArmorValue = value;
                if (playerHealth != null)
                    playerHealth.armor = ArmorValue;
                break;

            // Fire Rate: multiplier on every weapon's fire rate.
            // valuesPerLevel example: { 1.2, 1.4, 1.6, 1.85, 2.1 }
            case AbilityType.AddFireRate:
                FireRateMultiplier = value;
                break;

            // XP Booster: multiplier on all XP picked up.
            // valuesPerLevel example: { 1.25, 1.5, 1.75, 2.0, 2.5 }
            case AbilityType.AddXPBooster:
                XPBoostMultiplier = value;
                break;
        }
    }
}