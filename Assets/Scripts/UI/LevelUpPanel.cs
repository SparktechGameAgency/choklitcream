
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpPanel : MonoBehaviour
{
    public static LevelUpPanel Instance;

    [Header("Panel")]
    public GameObject panelRoot;         // the whole panel GameObject

    [Header("Weapon Card Buttons")]
    public Button[] weaponButtons;       // exactly 3 buttons
    public Image[] weaponIcons;         // icon on each button
    public TextMeshProUGUI[] weaponNames;    // weapon name text
    public TextMeshProUGUI[] weaponDescs;    // short description text

    private List<WeaponData> offeredWeapons = new();

    void Awake()
    {
        Instance = this;
        panelRoot.SetActive(false);
    }

    void Start()
    {
        if (PlayerXP.Instance != null)
            PlayerXP.Instance.onLevelUp.AddListener(OnLevelUp);
    }

    void OnLevelUp(int newLevel)
    {
        ShowPanel();
    }

    public void ShowPanel()
    {
        // Pause the game
        Time.timeScale = 0f;

        offeredWeapons = GetRandomWeaponOffers(weaponButtons.Length);

        for (int i = 0; i < weaponButtons.Length; i++)
        {
            if (i >= offeredWeapons.Count)
            {
                weaponButtons[i].gameObject.SetActive(false);
                continue;
            }

            weaponButtons[i].gameObject.SetActive(true);

            WeaponData wd = offeredWeapons[i];
            int index = i; // capture for lambda

            // Set button visuals
            if (weaponIcons[i] != null && wd.projectileSprite != null)
                weaponIcons[i].sprite = wd.projectileSprite;

            if (weaponNames[i] != null)
                weaponNames[i].text = wd.weaponName;

            if (weaponDescs[i] != null)
                weaponDescs[i].text = BuildDescription(wd);

            // Clear old listeners and add new one
            weaponButtons[i].onClick.RemoveAllListeners();
            weaponButtons[i].onClick.AddListener(() => OnWeaponChosen(index));
        }

        panelRoot.SetActive(true);
    }

    void OnWeaponChosen(int index)
    {
        if (index >= offeredWeapons.Count) return;

        WeaponData chosen = offeredWeapons[index];
        WeaponManager.Instance.EquipWeapon(chosen);

        HidePanel();
    }

    void HidePanel()
    {
        panelRoot.SetActive(false);
        Time.timeScale = 1f; // resume game
    }

    // Pick 3 random weapons — prioritise ones not yet equipped
    List<WeaponData> GetRandomWeaponOffers(int count)
    {
        List<WeaponData> all = WeaponManager.Instance.GetAvailableWeaponDatas();
        List<WeaponData> shuffled = new(all);

        // Shuffle
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        // Take up to 'count'
        List<WeaponData> result = new();
        foreach (var w in shuffled)
        {
            if (result.Count >= count) break;
            result.Add(w);
        }

        return result;
    }

    string BuildDescription(WeaponData wd)
    {
        return "DMG " + wd.damage
             + "  |  Rate " + wd.fireRate + "/s"
             + "  |  Range " + wd.range
             + (wd.isAoE ? "  |  AoE" : "")
             + (wd.pierce > 1 ? "  |  Pierce " + wd.pierce : "");
    }
}