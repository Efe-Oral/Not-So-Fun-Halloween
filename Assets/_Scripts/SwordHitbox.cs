using System.Collections.Generic;
using UnityEngine;

// Put this on the SWORD sprite object (the one with the BoxCollider2D).
// The collider must have "Is Trigger" checked. It only hits while the sword is
// swinging, and hits each target at most once per swing.
//
// It deals damage through the IDamageable contract, so it doesn't care WHAT it hits -
// any enemy (or breakable prop) with a Health component takes damage. It skips anything
// tagged "Player" so you can't cut yourself.
public class SwordHitbox : MonoBehaviour
{
    Collider2D hitboxCollider;

    // How much damage this swing deals. SwordController sets it from the SwordConfig
    // each time a swing begins.
    float damage;

    // Targets we already hit during this swing, so we don't hit them twice.
    readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.enabled = false;   // off until a swing starts
    }

    // SwordController calls this when a swing starts, passing the config's damage.
    public void BeginSwing(float swingDamage)
    {
        damage = swingDamage;
        alreadyHit.Clear();
        hitboxCollider.enabled = true;
    }

    // SwordController calls this when the swing ends.
    public void EndSwing()
    {
        hitboxCollider.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Never hit the player with the player's own sword.
        if (other.CompareTag("Player")) return;

        // Only react to things that can be damaged.
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null || target.IsDead) return;

        // Don't hit the same target twice in one swing.
        if (!alreadyHit.Add(target)) return;

        target.TakeDamage(damage);
    }
}
