using System;
using UnityEngine;

// Holds hit points for ANY object that can be hurt - enemies AND the player use this
// same script. It doesn't know about flashing, sound, death animations, or score.
// Instead it FIRES EVENTS when things happen, and other scripts LISTEN and react:
//
//   OnDamaged  -> the white hit-flash listens to this
//   OnDied     -> the death handler / spawner / score will listen to this
//
// This keeps Health simple and lets you add reactions later without touching it.
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] float maxHealth = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }


    // Listeners subscribe to these. The float on OnDamaged is how much damage was dealt.
    public event Action<float> OnDamaged;
    public event Action OnDied;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        // Ignore damage if already dead, or if the amount is zero/negative (that would be healing).
        if (IsDead || amount <= 0f) return;

        CurrentHealth = CurrentHealth - amount;
        OnDamaged?.Invoke(amount);   // tell listeners "I just got hurt"

        Debug.Log($"{name} took {amount} damage, HP now {CurrentHealth}/{maxHealth}", this);

        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
            IsDead = true;
            OnDied?.Invoke();        // tell listeners "I just died"

            Debug.Log($"{name} DIED", this);
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    // Lets a spawner or difficulty config set HP at runtime (e.g. a Hard enemy gets more HP).
    // Call this BEFORE the fight starts; by default it also refills to full.
    public void SetMaxHealth(float newMax, bool refill = true)
    {
        maxHealth = newMax;
        if (refill) CurrentHealth = maxHealth;
        IsDead = false;
    }
}
