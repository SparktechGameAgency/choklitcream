
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "CatSurvivors/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float moveSpeed = 2f;
    public float maxHealth = 30f;
    public float damage = 10f;
    public float xpValue = 5f;
    public Sprite sprite;
}