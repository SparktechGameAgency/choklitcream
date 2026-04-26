using System.Collections;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro tmp;

    [Header("Animation")]
    public float moveSpeed = 1.5f;
    public float lifetime = 0.8f;

    [Header("Player Damage")]
    public Color playerColor = new Color(1f, 0.2f, 0.2f, 1f);
    public float playerFontSize = 3f;

    [Header("Enemy Damage")]
    public Color enemyColor = new Color(1f, 0.95f, 0.3f, 1f);
    public float enemyFontSize = 2.5f;

    private Color textColor;

    void Awake() => tmp = GetComponent<TextMeshPro>();

    // ── Static spawn helper ──────────────────────────────────

    public static void Spawn(Vector3 position, float damage, bool isPlayer = false)
    {
        if (!ObjectPool.Instance.HasPool("DamagePopup"))
        {
            Debug.LogError("[DamagePopup] Pool not registered!");
            return;
        }

        GameObject obj = ObjectPool.Instance.Get("DamagePopup", position);
        if (obj == null) return;

        DamagePopup popup = obj.GetComponent<DamagePopup>();
        if (popup == null) return;

        popup.Setup(damage, isPlayer);
    }

    public void Setup(float damage, bool isPlayer)
    {
        if (isPlayer)
        {
            // Show the full rounded damage value — no CeilToInt on tiny deltaTime fractions
            tmp.text = "-" + Mathf.RoundToInt(damage).ToString();
            tmp.color = playerColor;
            tmp.fontSize = playerFontSize;
        }
        else
        {
            tmp.text = Mathf.RoundToInt(damage).ToString();
            tmp.color = enemyColor;
            tmp.fontSize = enemyFontSize;
        }

        textColor = tmp.color;

        // Slight random horizontal offset so numbers don't stack
        transform.position += new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);

        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float elapsed = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;

            // Float upward
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // Fade out in the second half of lifetime
            float alpha = 1f - Mathf.Clamp01((elapsed / lifetime - 0.5f) * 2f);
            textColor.a = alpha;
            tmp.color = textColor;

            yield return null;
        }

        ObjectPool.Instance.Return("DamagePopup", gameObject);
    }

    void OnDisable() => StopAllCoroutines();
}