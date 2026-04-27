
using UnityEngine;
using UnityEngine.Events;

public class PlayerXP : MonoBehaviour
{
    public static PlayerXP Instance;

    [Header("XP Settings")]
    public float xpToFirstLevel = 50f;

    [Tooltip("Multiplier applied to XP requirement each level")]
    public float xpScaling = 1.4f;

    public float CurrentXP { get; private set; }
    public float XPRequired { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    public UnityEvent<float, float> onXPChanged;
    public UnityEvent<int> onLevelUp;

    void Awake()
    {
        Instance = this;
        XPRequired = xpToFirstLevel;
    }

    /// <summary>
    /// Call this from XPOrb (or anywhere XP is awarded).
    /// Applies both XPMultiplier and XPBoostMultiplier from AbilityManager.
    /// </summary>
    public void GainXP(float amount)
    {
        float boosted = amount;

        if (AbilityManager.Instance != null)
        {
            // XPMultiplier: flat set (e.g. 1.5x)
            // XPBoostMultiplier: stacked on top (e.g. 2.0x)
            boosted *= AbilityManager.Instance.XPMultiplier
                     * AbilityManager.Instance.XPBoostMultiplier;
        }

        CurrentXP += boosted;
        onXPChanged?.Invoke(CurrentXP, XPRequired);

        if (CurrentXP >= XPRequired)
            LevelUp();
    }

    void LevelUp()
    {
        CurrentXP -= XPRequired;
        XPRequired = Mathf.Round(XPRequired * xpScaling);
        CurrentLevel++;
        onXPChanged?.Invoke(CurrentXP, XPRequired);
        onLevelUp?.Invoke(CurrentLevel);
    }
}