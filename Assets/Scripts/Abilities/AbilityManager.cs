
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
    [Tooltip("This is the starting attract radius — tweak it here")]
    public float baseAttractRadius = 3f;

    private float baseMoveSpeed = 5f;

    public float DamageMultiplier { get; private set; } = 1f;
    public float XPMultiplier { get; private set; } = 1f;
    public float AttractRadius { get; private set; }

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

        if (playerMovement != null)
            baseMoveSpeed = playerMovement.moveSpeed;

        // Set starting attract radius from Inspector value
        AttractRadius = baseAttractRadius;
    }

    // ── Public API ─────────────────────────────────────────────

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

        if (currentLevel >= data.MaxLevel)
        {
            Debug.Log("[Ability] " + data.abilityName + " already max level!");
            return;
        }

        int newLevel = currentLevel + 1;
        abilityLevels[data.abilityName] = newLevel;

        ApplyAbility(data, newLevel);

        Debug.Log("[Ability] " + data.abilityName
                + " → Level " + newLevel
                + " | Value: " + data.GetValue(newLevel));
    }

    // ── Apply Effects ──────────────────────────────────────────

    void ApplyAbility(AbilityData data, int level)
    {
        float value = data.GetValue(level);

        switch (data.abilityType)
        {
            case AbilityType.AddDamage:
                DamageMultiplier = value;
                Debug.Log("[Ability] Damage multiplier: " + DamageMultiplier);
                break;

            case AbilityType.AddMoveSpeed:
                if (playerMovement != null)
                    playerMovement.moveSpeed = value;
                Debug.Log("[Ability] Move speed: " + value);
                break;

            case AbilityType.AddMagnet:
                AttractRadius = value;
                Debug.Log("[Ability] Attract radius: " + value);
                break;

            case AbilityType.AddXPMultiplier:
                XPMultiplier = value;
                Debug.Log("[Ability] XP multiplier: " + value);
                break;

            case AbilityType.AddHealth:
                if (playerHealth != null)
                {
                    playerHealth.maxHealth += value;
                    playerHealth.currentHealth += value;
                    Debug.Log("[Ability] Max health increased by "
                            + value + " → " + playerHealth.maxHealth);
                }
                break;
        }
    }
}