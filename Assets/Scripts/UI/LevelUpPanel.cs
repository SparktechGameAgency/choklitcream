
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;        // ← regular UI Text

public class LevelUpPanel : MonoBehaviour
{
    public static LevelUpPanel Instance;

    [Header("Panel")]
    public GameObject panelRoot;

    [Header("Weapon Cards — exactly 3")]
    public Button[] weaponButtons;
    public Image[] weaponIcons;
    public Text[] weaponNames;    // ← regular Text not TMP
    public Text[] weaponDescs;    // ← regular Text not TMP

    private List<WeaponData> offeredWeapons = new();
    private bool subscribed = false;

    void Awake()
    {
        Instance = this;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void Update()
    {
        // Keep trying to subscribe until PlayerXP is ready
        if (!subscribed && PlayerXP.Instance != null)
        {
            PlayerXP.Instance.onLevelUp.AddListener(OnLevelUp);
            subscribed = true;
            Debug.Log("[LevelUpPanel] Subscribed to PlayerXP!");
        }
    }

    void OnLevelUp(int newLevel)
    {
        Debug.Log("[LevelUpPanel] Level up received! Level: " + newLevel);
        ShowPanel();
    }

    public void ShowPanel()
    {
        if (WeaponManager.Instance == null)
        {
            Debug.LogError("[LevelUpPanel] WeaponManager not found!");
            return;
        }

        offeredWeapons = GetRandomWeaponOffers(weaponButtons.Length);

        Debug.Log("[LevelUpPanel] Showing panel with "
                + offeredWeapons.Count + " weapons");

        for (int i = 0; i < weaponButtons.Length; i++)
        {
            if (i >= offeredWeapons.Count)
            {
                weaponButtons[i].gameObject.SetActive(false);
                continue;
            }

            weaponButtons[i].gameObject.SetActive(true);

            WeaponData wd = offeredWeapons[i];
            int idx = i;

            // Set icon
            if (weaponIcons[i] != null)
                weaponIcons[i].sprite = wd.weaponIcon != null
                    ? wd.weaponIcon
                    : wd.projectileSprite;

            if (weaponNames[i] != null)
                weaponNames[i].text = wd.weaponName;

            if (weaponDescs[i] != null)
                weaponDescs[i].text = BuildDescription(wd);

            weaponButtons[i].onClick.RemoveAllListeners();
            weaponButtons[i].onClick.AddListener(() => OnWeaponChosen(idx));
        }

        // Pause and show
        Time.timeScale = 0f;
        panelRoot.SetActive(true);
    }

    void OnWeaponChosen(int index)
    {
        if (index >= offeredWeapons.Count) return;

        WeaponData chosen = offeredWeapons[index];
        WeaponManager.Instance.EquipWeapon(chosen);

        Debug.Log("[LevelUpPanel] Player chose: " + chosen.weaponName);
        HidePanel();
    }

    void HidePanel()
    {
        panelRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    List<WeaponData> GetRandomWeaponOffers(int count)
    {
        List<WeaponData> all = WeaponManager.Instance.GetAvailableWeaponDatas();
        List<WeaponData> equipped = WeaponManager.Instance.GetEquippedWeaponDatas();

        List<WeaponData> unequipped = new();
        List<WeaponData> equippedList = new();

        foreach (var w in all)
        {
            bool isEquipped = false;
            foreach (var e in equipped)
                if (e.weaponName == w.weaponName) { isEquipped = true; break; }

            if (isEquipped) equippedList.Add(w);
            else unequipped.Add(w);
        }

        Shuffle(unequipped);
        Shuffle(equippedList);

        List<WeaponData> result = new();

        foreach (var w in unequipped)
        {
            if (result.Count >= count) break;
            result.Add(w);
        }

        foreach (var w in equippedList)
        {
            if (result.Count >= count) break;
            result.Add(w);
        }

        return result;
    }

    void Shuffle(List<WeaponData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    string BuildDescription(WeaponData wd)
    {
        string desc = "DMG " + wd.damage + "  |  " + wd.fireRate + "/s";
        if (wd.isAoE) desc += "  |  AoE";
        if (wd.pierce > 1) desc += "  |  Pierce " + wd.pierce;
        return desc;
    }
}