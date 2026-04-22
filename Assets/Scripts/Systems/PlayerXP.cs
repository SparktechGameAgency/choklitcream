
using UnityEngine;
using UnityEngine.Events;

public class PlayerXP : MonoBehaviour
{
    public static PlayerXP Instance;

    [Header("XP Settings")]
    public float xpToFirstLevel = 50f;
    [Tooltip("Multiplier applied to XP requirement each level")]
    public float xpScaling = 1.4f;

    // Read-only state
    public float CurrentXP { get; private set; }
    public float XPRequired { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    // Events — UI listens to these
    public UnityEvent<float, float> onXPChanged;   // (currentXP, requiredXP)
    public UnityEvent<int> onLevelUp;      // (newLevel)

    void Awake()
    {
        Instance = this;
        XPRequired = xpToFirstLevel;
    }

    public void GainXP(float amount)
    {
        CurrentXP += amount;
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

        Debug.Log("[XP] Level up! Now level " + CurrentLevel
                + " | Next level needs " + XPRequired + " XP");
    }
}