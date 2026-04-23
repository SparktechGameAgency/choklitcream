
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpPanel : MonoBehaviour
{
    public static LevelUpPanel Instance;

    [Header("Panel")]
    public GameObject panelRoot;

    [Header("Weapon Cards — exactly 3")]
    public Button[] weaponButtons;
    public Image[] weaponIcons;
    public Text[] weaponNames;
    public Text[] weaponDescs;

    private enum CardType { Weapon, Ability }

    private struct Card
    {
        public CardType type;
        public WeaponData weapon;
        public AbilityData ability;
    }

    private List<Card> offeredCards = new();
    private bool subscribed = false;

    void Awake()
    {
        Instance = this;
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    void Update()
    {
        if (!subscribed && PlayerXP.Instance != null)
        {
            PlayerXP.Instance.onLevelUp.AddListener(OnLevelUp);
            subscribed = true;
            Debug.Log("[LevelUpPanel] Subscribed to PlayerXP!");
        }
    }

    void OnLevelUp(int newLevel)
    {
        Debug.Log("[LevelUpPanel] Level up! Level: " + newLevel);
        ShowPanel();
    }

    public void ShowPanel()
    {
        offeredCards = GetRandomCards(weaponButtons.Length);

        if (offeredCards.Count == 0)
        {
            Debug.Log("[LevelUpPanel] No cards available — skipping.");
            return;
        }

        for (int i = 0; i < weaponButtons.Length; i++)
        {
            if (i >= offeredCards.Count)
            {
                weaponButtons[i].gameObject.SetActive(false);
                continue;
            }

            weaponButtons[i].gameObject.SetActive(true);

            Card card = offeredCards[i];
            int idx = i;

            if (card.type == CardType.Weapon)
                SetupWeaponCard(i, card.weapon);
            else
                SetupAbilityCard(i, card.ability);

            weaponButtons[i].onClick.RemoveAllListeners();
            weaponButtons[i].onClick.AddListener(() => OnCardChosen(idx));
        }

        Time.timeScale = 0f;
        panelRoot.SetActive(true);
    }

    void SetupWeaponCard(int i, WeaponData wd)
    {
        // Guard every array access
        if (weaponIcons != null && i < weaponIcons.Length && weaponIcons[i] != null)
            weaponIcons[i].sprite = wd.weaponIcon;

        if (weaponNames != null && i < weaponNames.Length && weaponNames[i] != null)
            weaponNames[i].text = wd.weaponName;

        if (weaponDescs != null && i < weaponDescs.Length && weaponDescs[i] != null)
        {
            bool equipped = IsWeaponEquipped(wd);
            weaponDescs[i].text = equipped
                ? "(already equipped)"
                : "DMG " + wd.damage + "  |  " + wd.fireRate + "/s"
                  + (wd.isAoE ? "  |  AoE" : "")
                  + (wd.pierce > 1 ? "  |  Pierce " + wd.pierce : "");
        }
    }

    void SetupAbilityCard(int i, AbilityData ad)
    {
        int currentLevel = AbilityManager.Instance.GetLevel(ad);
        int nextLevel = currentLevel + 1;

        // Guard every array access
        if (weaponIcons != null && i < weaponIcons.Length && weaponIcons[i] != null)
            weaponIcons[i].sprite = ad.abilityIcon;

        if (weaponNames != null && i < weaponNames.Length && weaponNames[i] != null)
            weaponNames[i].text = ad.abilityName + "  Lv." + nextLevel;

        if (weaponDescs != null && i < weaponDescs.Length && weaponDescs[i] != null)
            weaponDescs[i].text = ad.description
                                + "\n+" + ad.GetValue(nextLevel);
    }

    void OnCardChosen(int index)
    {
        if (index >= offeredCards.Count) return;

        Card card = offeredCards[index];

        if (card.type == CardType.Weapon)
            WeaponManager.Instance.EquipWeapon(card.weapon);
        else
            AbilityManager.Instance.UpgradeAbility(card.ability);

        HidePanel();
    }

    void HidePanel()
    {
        panelRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    List<Card> GetRandomCards(int count)
    {
        List<Card> pool = new();

        // Add unequipped weapons
        foreach (var w in WeaponManager.Instance.GetAvailableWeaponDatas())
            if (!IsWeaponEquipped(w))
                pool.Add(new Card { type = CardType.Weapon, weapon = w });

        // Add abilities not yet at max level
        foreach (var a in AbilityManager.Instance.GetAvailableAbilities())
            pool.Add(new Card { type = CardType.Ability, ability = a });

        // Shuffle
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        // Take up to count
        List<Card> result = new();
        foreach (var c in pool)
        {
            if (result.Count >= count) break;
            result.Add(c);
        }

        return result;
    }

    bool IsWeaponEquipped(WeaponData wd)
    {
        foreach (var e in WeaponManager.Instance.GetEquippedWeaponDatas())
            if (e.weaponName == wd.weaponName) return true;
        return false;
    }
}