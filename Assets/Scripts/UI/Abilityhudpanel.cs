
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityHUDPanel : MonoBehaviour
{
    public static AbilityHUDPanel Instance;

    [Header("References")]
    [Tooltip("The RectTransform that holds all rows (top-right corner)")]
    public RectTransform container;

    [Tooltip("Prefab: HorizontalLayoutGroup with an Image (icon) and Text (label) child")]
    public GameObject rowPrefab;

    [Header("Section Headers (optional)")]
    [Tooltip("If assigned, a small header label is shown above each section")]
    public bool showSectionHeaders = true;
    public Color abilityHeaderColor = new Color(1f, 0.85f, 0.3f);   // gold
    public Color weaponHeaderColor = new Color(0.4f, 0.9f, 1f);    // cyan

    // row pools — keyed by name
    private Dictionary<string, GameObject> abilityRows = new();
    private Dictionary<string, GameObject> weaponRows = new();

    // optional header rows
    private GameObject abilityHeader;
    private GameObject weaponHeader;

    void Awake() => Instance = this;

    void Start() => Refresh();

    // ───────────────────────────────────────────────────────────────────────
    // Called by AbilityManager and SpecialWeaponManager on every change
    // ───────────────────────────────────────────────────────────────────────
    public void Refresh()
    {
        bool anyAbility = RefreshAbilities();
        bool anyWeapon = RefreshSpecialWeapons();

        // Show / hide section headers based on whether that section has content
        if (showSectionHeaders)
        {
            SetHeaderActive(ref abilityHeader, "ABILITIES", abilityHeaderColor, anyAbility);
            SetHeaderActive(ref weaponHeader, "WEAPONS", weaponHeaderColor, anyWeapon);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }

    // ── Abilities ───────────────────────────────────────────────────────────
    bool RefreshAbilities()
    {
        if (AbilityManager.Instance == null) return false;

        bool hasAny = false;

        foreach (AbilityData ability in AbilityManager.Instance.allAbilities)
        {
            int level = AbilityManager.Instance.GetLevel(ability);

            if (level <= 0)
            {
                HideRow(abilityRows, ability.abilityName);
                continue;
            }

            hasAny = true;
            GameObject row = GetOrCreateRow(abilityRows, ability.abilityName);
            ApplyRowData(row, ability.abilityIcon, ability.abilityName, level,
                         ability.MaxLevel, abilityHeaderColor);
        }

        return hasAny;
    }

    // ── Special Weapons ─────────────────────────────────────────────────────
    bool RefreshSpecialWeapons()
    {
        if (SpecialWeaponManager.Instance == null) return false;

        bool hasAny = false;

        foreach (SpecialWeaponData weapon in SpecialWeaponManager.Instance.allWeapons)
        {
            int level = SpecialWeaponManager.Instance.GetLevel(weapon);

            if (level <= 0)
            {
                HideRow(weaponRows, weapon.weaponName);
                continue;
            }

            hasAny = true;
            GameObject row = GetOrCreateRow(weaponRows, weapon.weaponName);
            ApplyRowData(row, weapon.weaponIcon, weapon.weaponName, level,
                         weapon.MaxLevel, weaponHeaderColor);
        }

        return hasAny;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    GameObject GetOrCreateRow(Dictionary<string, GameObject> pool, string key)
    {
        if (!pool.TryGetValue(key, out GameObject row) || row == null)
        {
            row = Instantiate(rowPrefab, container);
            pool[key] = row;
        }
        row.SetActive(true);
        return row;
    }

    void HideRow(Dictionary<string, GameObject> pool, string key)
    {
        if (pool.TryGetValue(key, out GameObject row) && row != null)
            row.SetActive(false);
    }

    void ApplyRowData(GameObject row, Sprite icon, string itemName,
                      int level, int maxLevel, Color labelColor)
    {
        // Icon
        Image img = row.GetComponentInChildren<Image>(true);
        if (img != null && icon != null)
            img.sprite = icon;

        // Label — "Name  Lv.3 / 5"
        Text label = row.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.text = itemName + "  <b>Lv." + level + "</b>/" + maxLevel;
            label.color = labelColor;
        }
    }

    void SetHeaderActive(ref GameObject headerObj, string title,
                         Color col, bool visible)
    {
        if (!visible)
        {
            if (headerObj != null) headerObj.SetActive(false);
            return;
        }

        if (headerObj == null)
        {
            headerObj = Instantiate(rowPrefab, container);
            // Hide icon, style label as a header
            Image img = headerObj.GetComponentInChildren<Image>(true);
            if (img != null) img.enabled = false;

            Text lbl = headerObj.GetComponentInChildren<Text>(true);
            if (lbl != null)
            {
                lbl.fontSize = 11;
                lbl.fontStyle = FontStyle.Bold;
                lbl.color = col;
            }
        }

        headerObj.SetActive(true);

        Text header = headerObj.GetComponentInChildren<Text>(true);
        if (header != null) header.text = "— " + title + " —";

        // Make sure headers appear before their rows by moving them to the top
        // of their respective sibling groups
        headerObj.transform.SetAsFirstSibling();
    }
}