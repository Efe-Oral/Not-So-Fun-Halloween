using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One row in the store: shows an upgrade's name, current level, and next cost, buys it on
// click, and disables the button when the player can't afford it. Reads/writes entirely
// through PlayerUpgrades/CoinWallet - doesn't know the upgrade math or cost formula itself.
public class UpgradeRowUI : MonoBehaviour
{
    [SerializeField] PlayerUpgrades upgrades;
    [SerializeField] PlayerUpgrades.UpgradeType upgradeType;
    [Tooltip("Left empty, this looks for a CoinWallet on the same object as PlayerUpgrades.")]
    [SerializeField] CoinWallet wallet;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Button buyButton;

    void Awake()
    {
        if (wallet == null && upgrades != null) wallet = upgrades.GetComponent<CoinWallet>();
    }

    void OnEnable()
    {
        if (upgrades != null) upgrades.OnUpgradesChanged += Refresh;
        if (wallet != null) wallet.OnCoinsChanged += HandleCoinsChanged;
        if (buyButton != null) buyButton.onClick.AddListener(HandleBuyClicked);

        Refresh();
    }

    void OnDisable()
    {
        if (upgrades != null) upgrades.OnUpgradesChanged -= Refresh;
        if (wallet != null) wallet.OnCoinsChanged -= HandleCoinsChanged;
        if (buyButton != null) buyButton.onClick.RemoveListener(HandleBuyClicked);
    }

    void HandleBuyClicked()
    {
        if (upgrades != null) upgrades.TryPurchase(upgradeType);
    }

    void HandleCoinsChanged(int total) => Refresh();

    void Refresh()
    {
        if (upgrades == null) return;

        int cost = upgrades.GetCost(upgradeType);

        if (nameText != null) nameText.text = upgrades.GetDisplayName(upgradeType);
        if (levelText != null) levelText.text = "Lv. " + upgrades.GetLevel(upgradeType);
        if (costText != null) costText.text = cost + " candies";
        if (buyButton != null) buyButton.interactable = wallet == null || wallet.TotalCoins >= cost;
    }
}
