
using UnityEngine;
using UnityEngine.Events;

public class PlayerXP : MonoBehaviour
{
    public static PlayerXP Instance;

    [Header("XP Settings")]
    public float currentXP;
    public float xpToNextLevel = 50f;
    public int currentLevel = 1;

    public UnityEvent onLevelUp; // hook this in the Inspector

    void Awake() => Instance = this;

    public void GainXP(float amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            xpToNextLevel *= 1.4f; // each level needs more XP
            currentLevel++;
            onLevelUp?.Invoke(); // triggers UI card picker
        }
    }
}