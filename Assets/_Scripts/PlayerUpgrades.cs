using System;
using UnityEngine;

// Tracks the player's purchased upgrade levels, their escalating cost, and applies their
// effects. Move speed and max health are applied directly - PlayerController and Health live
// on this same object, so a normal reference is the natural fit. Attack damage is exposed as a
// static multiplier instead: SwordController and PumpkinSeed are separate, decoupled weapon
// scripts that would otherwise each need this wired in by hand, same reasoning as
// CoinDropTable's static accessors.
public class PlayerUpgrades : MonoBehaviour
{
    public enum UpgradeType
    {
        MoveSpeed,
        Health,
        Damage,
    }

    [Serializable]
    public class UpgradeDefinition
    {
        public string displayName;

        [Tooltip(
            "Bonus granted per level - a fraction for move speed/damage (0.1 = +10%), a "
                + "flat amount for health."
        )]
        public float amountPerLevel;
        public int baseCost;
        public int costIncreasePerLevel;
    }

    [SerializeField]
    UpgradeDefinition moveSpeedUpgrade = new UpgradeDefinition
    {
        displayName = "Move Speed",
        amountPerLevel = 0.1f,
        baseCost = 20,
        costIncreasePerLevel = 10,
    };

    [SerializeField]
    UpgradeDefinition healthUpgrade = new UpgradeDefinition
    {
        displayName = "Max Health",
        amountPerLevel = 10f,
        baseCost = 20,
        costIncreasePerLevel = 10,
    };

    [SerializeField]
    UpgradeDefinition damageUpgrade = new UpgradeDefinition
    {
        displayName = "Attack Damage",
        amountPerLevel = 0.15f,
        baseCost = 25,
        costIncreasePerLevel = 12,
    };

    [Tooltip("Left empty, these look for the matching component on this object.")]
    [SerializeField]
    PlayerController playerController;

    [SerializeField]
    Health health;

    [SerializeField]
    CoinWallet wallet;

    public int MoveSpeedLevel { get; private set; }
    public int HealthLevel { get; private set; }
    public int DamageLevel { get; private set; }

    static float damageMultiplier = 1f;
    public static float DamageMultiplier => damageMultiplier;

    // Fired after any purchase, so UI can refresh levels/costs without polling.
    public event Action OnUpgradesChanged;

    void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        if (health == null)
            health = GetComponent<Health>();
        if (wallet == null)
            wallet = GetComponent<CoinWallet>();

        // Reset static state - matters if this component is ever re-created (e.g. stopping
        // and re-entering Play Mode in the Editor keeps static fields from the last run).
        damageMultiplier = 1f;
    }

    public int GetLevel(UpgradeType type) =>
        type switch
        {
            UpgradeType.MoveSpeed => MoveSpeedLevel,
            UpgradeType.Health => HealthLevel,
            UpgradeType.Damage => DamageLevel,
            _ => 0,
        };

    public string GetDisplayName(UpgradeType type) => GetDefinition(type).displayName;

    public int GetCost(UpgradeType type)
    {
        UpgradeDefinition def = GetDefinition(type);
        return def.baseCost + GetLevel(type) * def.costIncreasePerLevel;
    }

    public bool TryPurchase(UpgradeType type)
    {
        if (wallet == null || !wallet.TrySpend(GetCost(type)))
            return false;

        ApplyUpgrade(type);
        OnUpgradesChanged?.Invoke();
        return true;
    }

    void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.MoveSpeed:
                MoveSpeedLevel++;
                if (playerController != null)
                    playerController.SpeedMultiplier =
                        1f + moveSpeedUpgrade.amountPerLevel * MoveSpeedLevel;
                break;

            case UpgradeType.Health:
                HealthLevel++;
                if (health != null)
                    health.IncreaseMaxHealth(healthUpgrade.amountPerLevel);
                break;

            case UpgradeType.Damage:
                DamageLevel++;
                damageMultiplier = 1f + damageUpgrade.amountPerLevel * DamageLevel;
                break;
        }
    }

    UpgradeDefinition GetDefinition(UpgradeType type) =>
        type switch
        {
            UpgradeType.MoveSpeed => moveSpeedUpgrade,
            UpgradeType.Health => healthUpgrade,
            UpgradeType.Damage => damageUpgrade,
            _ => null,
        };
}
