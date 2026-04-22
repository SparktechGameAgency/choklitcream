
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBarUI : MonoBehaviour
{
    [Header("References")]
    public Image xpBarFill;
    public TextMeshProUGUI levelText;

    [Header("Animation")]
    public float smoothSpeed = 5f;

    private float targetFill = 0f;
    private float currentFill = 0f;
    private bool subscribed = false;

    void Update()
    {
        // Keep trying to subscribe until PlayerXP is ready
        if (!subscribed)
        {
            TrySubscribe();
            return;
        }

        // Smooth fill animation
        if (xpBarFill == null) return;
        currentFill = Mathf.Lerp(currentFill, targetFill, smoothSpeed * Time.deltaTime);
        xpBarFill.fillAmount = currentFill;
    }

    void TrySubscribe()
    {
        if (PlayerXP.Instance == null) return;

        // Reset bar to empty
        currentFill = 0f;
        targetFill = 0f;
        if (xpBarFill != null)
            xpBarFill.fillAmount = 0f;

        PlayerXP.Instance.onXPChanged.AddListener(OnXPChanged);
        PlayerXP.Instance.onLevelUp.AddListener(OnLevelUp);

        UpdateLevelText(PlayerXP.Instance.CurrentLevel);

        subscribed = true;
        Debug.Log("[XPBarUI] Successfully subscribed to PlayerXP!");
    }

    void OnXPChanged(float current, float required)
    {
        targetFill = Mathf.Clamp01(current / required);
    }

    void OnLevelUp(int newLevel)
    {
        currentFill = 0f;
        targetFill = 0f;
        if (xpBarFill != null)
            xpBarFill.fillAmount = 0f;
        UpdateLevelText(newLevel);
    }

    void UpdateLevelText(int level)
    {
        if (levelText != null)
            levelText.text = "Lv. " + level;
    }
}