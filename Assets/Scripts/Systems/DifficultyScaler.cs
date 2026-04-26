
using UnityEngine;

public class DifficultyScaler : MonoBehaviour
{
    public static DifficultyScaler Instance;

    [Header("Scaling Interval")]
    [Tooltip("How many seconds between each difficulty increase")]
    public float scalingInterval = 120f; // 2 minutes

    [Header("Amount added per interval")]
    public float damageIncrease = 2f;
    public float speedIncrease = 0.5f;
    public float healthIncrease = 5f;

    // Current bonus values — applied on top of EnemyData base stats
    public float BonusDamage { get; private set; } = 0f;
    public float BonusSpeed { get; private set; } = 0f;
    public float BonusHealth { get; private set; } = 0f;

    private float timer = 0f;
    private int scaleCount = 0;

    void Awake() => Instance = this;

    void Update()
    {
        if (GameTimer.Instance == null || !GameTimer.Instance.IsRunning) return;

        timer += Time.deltaTime;

        if (timer >= scalingInterval)
        {
            timer = 0f;
            ApplyScale();
        }
    }

    void ApplyScale()
    {
        scaleCount++;

        BonusDamage += damageIncrease;
        BonusSpeed += speedIncrease;
        BonusHealth += healthIncrease;

        Debug.Log("[Difficulty] Scale " + scaleCount
                + " | Bonus DMG: " + BonusDamage
                + " | Bonus Speed: " + BonusSpeed
                + " | Bonus Health: " + BonusHealth);

        // Apply to all currently active enemies immediately
        EnemyController[] active = FindObjectsByType<EnemyController>(
            FindObjectsSortMode.None);

        foreach (var enemy in active)
            if (enemy.gameObject.activeInHierarchy)
                enemy.ApplyDifficultyBonus(BonusDamage, BonusSpeed, BonusHealth);
    }
}