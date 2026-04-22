
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedWeaponsHUD : MonoBehaviour
{
    public static EquippedWeaponsHUD Instance;

    [Header("References")]
    public GameObject iconPrefab;   // a prefab with just an Image component
    public Transform iconContainer; // horizontal layout group parent

    [Header("Settings")]
    public float iconSize = 48f;
    public float iconSpacing = 8f;

    private List<GameObject> iconObjects = new();

    void Awake() => Instance = this;

    // Called by WeaponManager every time a weapon is equipped
    public void AddWeaponIcon(WeaponData data)
    {
        if (data.weaponIcon == null)
        {
            Debug.LogWarning("[HUD] No icon set for weapon: " + data.weaponName);
            return;
        }

        GameObject obj = Instantiate(iconPrefab, iconContainer);
        Image img = obj.GetComponent<Image>();

        if (img != null)
            img.sprite = data.weaponIcon;

        // Size the icon
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
            rt.sizeDelta = new Vector2(iconSize, iconSize);

        iconObjects.Add(obj);

        Debug.Log("[HUD] Added icon for: " + data.weaponName);
    }
}