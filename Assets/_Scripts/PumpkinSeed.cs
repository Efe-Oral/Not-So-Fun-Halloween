using System;
using UnityEngine;

// A pumpkin seed fired by the slingshot. Doesn't know it's pooled - it just fires OnDespawn
// when it's done (hit something, or timed out) and lets whoever's listening (PumpkinSeedPool)
// decide what "done" means. Same event-driven decoupling as the rest of this project: this
// script could be Destroy()'d by a listener instead, and it would never know the difference.
//
// Its collider should have "Is Trigger" checked (so it passes through until it finds a hit),
// and it needs a Rigidbody2D, which OnTriggerEnter2D needs to fire.
[RequireComponent(typeof(Rigidbody2D))]
public class PumpkinSeed : MonoBehaviour
{
    [SerializeField] float damage = 1f;
    [SerializeField] float lifetime = 3f;

    public event Action<PumpkinSeed> OnDespawn;

    Rigidbody2D rb;
    float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Runs every time this seed is re-enabled from the pool, not just once ever - so a reused
    // seed's countdown always restarts fresh instead of carrying over from its last flight.
    void OnEnable()
    {
        timer = lifetime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) Despawn();
    }

    // PumpkinSeedPool calls this right after handing out a seed, instead of this script
    // reading a spawn-time velocity itself - keeps "how fast" entirely the shooter's call.
    public void Launch(Vector2 velocity)
    {
        rb.velocity = velocity;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the player who fired it.
        if (other.CompareTag("Player")) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null || target.IsDead) return;

        target.TakeDamage(damage * PlayerUpgrades.DamageMultiplier);
        Despawn();
    }

    void Despawn()
    {
        rb.velocity = Vector2.zero;
        OnDespawn?.Invoke(this);
    }
}
