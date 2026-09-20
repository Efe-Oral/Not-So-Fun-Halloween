using TMPro;
using UnityEngine;

// Displays the player's coin total. Subscribes to CoinWallet.OnCoinsChanged AND re-reads the
// current total in OnEnable, not Start: this can live on something that gets hidden and re-shown
// (the store panel), and Start only runs once, so a re-shown counter would display whatever the
// total was the first time it opened. While disabled it's also unsubscribed, so it misses
// every coin collected in between - the re-read on enable is what catches it up.
// Reading in OnEnable is safe here because CoinWallet.TotalCoins has no Awake dependency (it's
// just 0 until coins arrive); HealthBarUI is the case where that ordering did matter.
public class CoinCounterUI : MonoBehaviour
{
    [SerializeField] CoinWallet wallet;
    [SerializeField] TextMeshProUGUI coinText;

    void OnEnable()
    {
        if (wallet == null) return;

        wallet.OnCoinsChanged += HandleCoinsChanged;
        HandleCoinsChanged(wallet.TotalCoins);
    }

    void OnDisable()
    {
        if (wallet != null) wallet.OnCoinsChanged -= HandleCoinsChanged;
    }

    void HandleCoinsChanged(int total)
    {
        coinText.text = total.ToString();
    }
}
