using TMPro;
using UnityEngine;

// Displays the player's coin total. Subscribes to CoinWallet.OnCoinsChanged in OnEnable, but
// reads the *initial* value in Start rather than OnEnable - CoinWallet's own Awake doesn't set
// TotalCoins to anything but 0 by default so there's no race to worry about here, but Start is
// still the safe habit for reading another object's state (see HealthBarUI for the case where
// this ordering actually mattered).
public class CoinCounterUI : MonoBehaviour
{
    [SerializeField] CoinWallet wallet;
    [SerializeField] TextMeshProUGUI coinText;

    void OnEnable()
    {
        if (wallet != null) wallet.OnCoinsChanged += HandleCoinsChanged;
    }

    void OnDisable()
    {
        if (wallet != null) wallet.OnCoinsChanged -= HandleCoinsChanged;
    }

    void Start()
    {
        if (wallet != null) HandleCoinsChanged(wallet.TotalCoins);
    }

    void HandleCoinsChanged(int total)
    {
        coinText.text = total.ToString();
    }
}
