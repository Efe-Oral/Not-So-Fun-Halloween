using UnityEngine;

// A pumpkin seed fired by the slingshot. ProjectileWeapon sets its speed when it spawns.
// When it touches something damageable, it deals damage and is destroyed. If it hits
// nothing, it deletes itself after 'lifetime' seconds so seeds don't pile up forever.
//
// Its collider should have "Is Trigger" checked (so it passes through until it finds a hit),
// and the seed already has a Rigidbody2D, which OnTriggerEnter2D needs to fire.
public class PumpkinSeed : MonoBehaviour
{
    [SerializeField] float damage = 1f;
    [SerializeField] float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't hit the player who fired it.
        if (other.CompareTag("Player")) return;

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null || target.IsDead) return;

        target.TakeDamage(damage);
        Destroy(this.gameObject);   // the seed is used up on a hit
    }
}
