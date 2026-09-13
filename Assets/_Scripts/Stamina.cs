using System;
using UnityEngine;

// Holds a regenerating resource, same shape as Health: doesn't know what spends it or what
// reacts to it changing (a UI bar, in this case) - it just tracks the number and fires an
// event. Regen happens continuously every frame, not in bursts, so a UI reading this value
// directly (no tweening needed) already looks smooth.
public class Stamina : MonoBehaviour
{
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float regenPerSecond = 25f;

    public float MaxStamina => maxStamina;
    public float CurrentStamina { get; private set; }

    // Passes the new current value, same convention as Health.OnDamaged passing the amount.
    public event Action<float> OnStaminaChanged;

    void Awake()
    {
        CurrentStamina = maxStamina;
    }

    void Update()
    {
        if (CurrentStamina >= maxStamina) return;

        CurrentStamina = Mathf.Min(CurrentStamina + regenPerSecond * Time.deltaTime, maxStamina);
        OnStaminaChanged?.Invoke(CurrentStamina);
    }

    // Checks and consumes in one call so callers can't spend based on a stale read.
    public bool TrySpend(float amount)
    {
        if (amount <= 0f || CurrentStamina < amount) return false;

        CurrentStamina -= amount;
        OnStaminaChanged?.Invoke(CurrentStamina);
        return true;
    }
}
