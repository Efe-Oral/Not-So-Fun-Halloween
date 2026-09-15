using System;
using UnityEngine;

// Tracks the player's running coin total. Listens for CoinPickup.OnCollected (a static event -
// there's only one player/wallet in this game, so any collected coin is unambiguously theirs)
// and fires OnCoinsChanged so UI (or anything spending coins later) can react without polling.
public class CoinWallet : MonoBehaviour
{
    public int TotalCoins { get; private set; }

    // Passes the new total, same convention as Health's events passing the relevant value.
    public event Action<int> OnCoinsChanged;

    void OnEnable()
    {
        CoinPickup.OnCollected += HandleCollected;
    }

    void OnDisable()
    {
        CoinPickup.OnCollected -= HandleCollected;
    }

    void HandleCollected(int amount)
    {
        TotalCoins += amount;
        OnCoinsChanged?.Invoke(TotalCoins);
    }

    // Checks and consumes in one call so callers can't spend based on a stale read - same
    // pattern as Stamina.TrySpend.
    public bool TrySpend(int amount)
    {
        if (amount <= 0 || TotalCoins < amount) return false;

        TotalCoins -= amount;
        OnCoinsChanged?.Invoke(TotalCoins);
        return true;
    }
}
