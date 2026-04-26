
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpPanel : MonoBehaviour
{
    public static LevelUpPanel Instance;

    [Header("Panel")]
    public GameObject panelRoot;

    [Header("Cards — exactly 3")]
    public Button[] weaponButtons;
    public Image[] weaponIcons;
    public Text[] weaponNames;
    public Text[] weaponDescs;

    private enum CardType { RegularWeapon, SpecialWeapon, Ability }

    private struct Card
    {
        public CardType type;
        public WeaponData regularWeapon;
        public SpecialWeaponData specialWeapon;
        public AbilityData ability;
    }

    private List<Card> offeredCards = new();
    private bool subscribed = false;

    void Awake()
    {
        Instance = this;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    void Update()
    {
        if (!subscribed && PlayerXP.Instance != null)
        {
            PlayerXP.Instance.onLevelUp.AddListener(OnLevelUp);
            subscribed = true;
            Debug.Log("[LevelUpPanel] Subscribed!");
        }
    }

    void OnLevelUp(int newLevel)
    {
        Debug.Log("[LevelUpPanel] Level up → " + newLevel);
        ShowPanel();
    }

    public void ShowPanel()
    {
        offeredCards = GetRandomCards(weaponButtons.Length);

        if (offeredCards.Count == 0)
        {
            Debug.Log("[LevelUpPanel] Nothing to offer — skipping.");
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

            switch (card.type)
            {
                case CardType.RegularWeapon:
                    SetupRegularWeaponCard(i, card.regularWeapon);
                    break;
                case CardType.SpecialWeapon:
                    SetupSpecialWeaponCard(i, card.specialWeapon);
                    break;
                case CardType.Ability:
                    SetupAbilityCard(i, card.ability);
                    break;
            }

            weaponButtons[i].onClick.RemoveAllListeners();
            weaponButtons[i].onClick.AddListener(() => OnCardChosen(idx));
        }

        Time.timeScale = 0f;
        panelRoot.SetActive(true);
    }

    // ── Card setup ─────────────────────────────────────────────

    void SetupRegularWeaponCard(int i, WeaponData wd)
    {
        SetIcon(i, wd.weaponIcon);
        SetName(i, wd.weaponName);
        SetDesc(i, "DMG " + wd.damage
                 + "  |  " + wd.fireRate + "/s"
                 + (wd.isAoE ? "  |  AoE" : "")
                 + (wd.pierce > 1 ? "  |  Pierce " + wd.pierce : ""));
    }

    void SetupSpecialWeaponCard(int i, SpecialWeaponData sd)
    {
        int currentLevel = SpecialWeaponManager.Instance.GetLevel(sd);
        int nextLevel = currentLevel + 1;

        SetIcon(i, sd.weaponIcon);
        SetName(i, sd.weaponName + "  Lv." + nextLevel);
        SetDesc(i, sd.description
               + "\nDMG " + sd.GetDamage(nextLevel)
               + "  |  Count: " + sd.GetCount(nextLevel));
    }

    void SetupAbilityCard(int i, AbilityData ad)
    {
        int currentLevel = AbilityManager.Instance.GetLevel(ad);
        int nextLevel = currentLevel + 1;

        SetIcon(i, ad.abilityIcon);
        SetName(i, ad.abilityName + "  Lv." + nextLevel);
        SetDesc(i, ad.description + "\n+" + ad.GetValue(nextLevel));
    }

    void SetIcon(int i, Sprite s)
    {
        if (weaponIcons != null && i < weaponIcons.Length && weaponIcons[i] != null)
            weaponIcons[i].sprite = s;
    }

    void SetName(int i, string text)
    {
        if (weaponNames != null && i < weaponNames.Length && weaponNames[i] != null)
            weaponNames[i].text = text;
    }

    void SetDesc(int i, string text)
    {
        if (weaponDescs != null && i < weaponDescs.Length && weaponDescs[i] != null)
            weaponDescs[i].text = text;
    }

    // ── Card chosen ────────────────────────────────────────────

    void OnCardChosen(int index)
    {
        if (index >= offeredCards.Count) return;

        Card card = offeredCards[index];

        switch (card.type)
        {
            case CardType.RegularWeapon:
                WeaponManager.Instance.EquipWeapon(card.regularWeapon);
                break;
            case CardType.SpecialWeapon:
                SpecialWeaponManager.Instance.EquipOrUpgrade(card.specialWeapon);
                break;
            case CardType.Ability:
                AbilityManager.Instance.UpgradeAbility(card.ability);
                break;
        }

        HidePanel();
    }

    void HidePanel()
    {
        panelRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    // ── Card pool building ─────────────────────────────────────

    List<Card> GetRandomCards(int count)
    {
        List<Card> pool = new();

        // Regular weapons not yet equipped
        foreach (var w in WeaponManager.Instance.GetAvailableWeaponDatas())
            if (!IsRegularWeaponEquipped(w))
                pool.Add(new Card { type = CardType.RegularWeapon, regularWeapon = w });

        // Special weapons not yet at max level
        foreach (var sw in SpecialWeaponManager.Instance.GetAvailableWeapons())
            pool.Add(new Card { type = CardType.SpecialWeapon, specialWeapon = sw });

        // Abilities not yet at max level
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

    bool IsRegularWeaponEquipped(WeaponData wd)
    {
        foreach (var e in WeaponManager.Instance.GetEquippedWeaponDatas())
            if (e.weaponName == wd.weaponName) return true;
        return false;
    }
}