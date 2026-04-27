// Scripts/Systems/DifficultyScaler.cs
using UnityEngine;

public class DifficultyScaler : MonoBehaviour
{
    public static DifficultyScaler Instance;

    [Header("Scaling Interval")]
    [Tooltip("How many seconds between each difficulty increase")]
    public float scalingInterval = 120f;

    [Header("Amount added per interval")]
    public float damageIncrease = 2f;
    public float speedIncrease  = 0.5f;
    public float healthIncrease = 5f;

    public float BonusDamage { get; private set; } = 0f;
    public float BonusSpeed  { get; private set; } = 0f;
    public float BonusHealth { get; private set; } = 0f;

    private float timer = 0f;

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
        BonusDamage += damageIncrease;
        BonusSpeed  += speedIncrease;
        BonusHealth += healthIncrease;

        EnemyController[] active = FindObjectsByType<EnemyController>(
            FindObjectsSortMode.None);

        foreach (var enemy in active)
            if (enemy.gameObject.activeInHierarchy)
                enemy.ApplyDifficultyBonus(BonusDamage, BonusSpeed, BonusHealth);
    }
}
