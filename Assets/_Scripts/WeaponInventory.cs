using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// Handles the two weapon slots.
// Press 1 to equip the melee weapon (sword), press 2 to equip the projectile weapon (pumpkin seeds).
// "Equipping" just means turning the chosen weapon ON and the other one OFF.
public class WeaponInventory : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] GameObject meleeWeapon;       // Slot 1: the sword
    [SerializeField] GameObject projectileWeapon;  // Slot 2: the pumpkin seed shooter

    [Header("UI Slots")]
    [SerializeField] Image meleeSlot;       // the square Image in the corner for slot 1
    [SerializeField] Image projectileSlot;  // the square Image in the corner for slot 2

    [Header("Slot Colors")]
    [SerializeField] Color selectedColor = Color.yellow;
    [SerializeField] Color normalColor = Color.gray;

    void Start()
    {
        // Start the game with the melee weapon equipped.
        EquipMelee();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquipMelee();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            EquipProjectile();
    }

    void EquipMelee()
    {
        // Turn the sword on, the shooter off.
        meleeWeapon.SetActive(true);
        projectileWeapon.SetActive(false);

        // Highlight slot 1 in the UI.
        meleeSlot.color = selectedColor;
        projectileSlot.color = normalColor;
    }

    void EquipProjectile()
    {
        // Turn the shooter on, the sword off.
        meleeWeapon.SetActive(false);
        projectileWeapon.SetActive(true);

        // Highlight slot 2 in the UI.
        meleeSlot.color = normalColor;
        projectileSlot.color = selectedColor;
    }
}
