
using UnityEngine;

public enum AbilityType
{
    AddDamage,
    AddMoveSpeed,
    AddMagnet,
    AddXPMultiplier,
    AddHealth           
}

[CreateAssetMenu(fileName = "AbilityData", menuName = "CatSurvivors/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("Identity")]
    public string abilityName;
    public Sprite abilityIcon;
    [TextArea]
    public string description;
    public AbilityType abilityType;

    [Header("Levels — exactly 5 values")]
    [Tooltip("Value added per level. Index 0 = Lv1, Index 4 = Lv5")]
    public float[] valuesPerLevel = new float[5];

    public int MaxLevel => valuesPerLevel.Length;
    public float GetValue(int level)
        => valuesPerLevel[Mathf.Clamp(level - 1, 0, valuesPerLevel.Length - 1)];
}