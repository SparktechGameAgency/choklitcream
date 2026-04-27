
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityHUDPanel : MonoBehaviour
{
    public static AbilityHUDPanel Instance;

    [Header("References")]
    [Tooltip("The panel RectTransform that holds all rows (top-right corner)")]
    public RectTransform container;

    [Tooltip("Prefab with an Image (icon) and a Text (label) as children")]
    public GameObject rowPrefab;

    // ── Runtime row pool ────────────────────────────────────────────────────
    // Key: abilityName  →  Value: the live row GameObject
    private Dictionary<string, GameObject> activeRows = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Build initial state (probably all empty, but safe to call)
        Refresh();
    }

    public void Refresh()
    {
        if (AbilityManager.Instance == null) return;

        foreach (AbilityData ability in AbilityManager.Instance.allAbilities)
        {
            int level = AbilityManager.Instance.GetLevel(ability);

            if (level <= 0)
            {
                // Not yet acquired — hide the row if it exists
                if (activeRows.TryGetValue(ability.abilityName, out GameObject oldRow))
                    oldRow.SetActive(false);
                continue;
            }

            // Acquire or reuse a row
            if (!activeRows.TryGetValue(ability.abilityName, out GameObject row)
                || row == null)
            {
                row = Instantiate(rowPrefab, container);
                activeRows[ability.abilityName] = row;
            }

            row.SetActive(true);
            ApplyRowData(row, ability, level);
        }

        // Force the layout to recalculate so the panel resizes correctly
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }

    // ───────────────────────────────────────────────────────────────────────
    // Fill one row with icon + "AbilityName  Lv.X"
    // ───────────────────────────────────────────────────────────────────────
    void ApplyRowData(GameObject row, AbilityData ability, int level)
    {
        // Icon — first Image child
        Image icon = row.GetComponentInChildren<Image>(true);
        if (icon != null && ability.abilityIcon != null)
            icon.sprite = ability.abilityIcon;

        // Label — first Text child
        Text label = row.GetComponentInChildren<Text>(true);
        if (label != null)
            label.text = ability.abilityName + "  <b>Lv." + level + "</b>";
    }
}