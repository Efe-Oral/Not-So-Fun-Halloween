using UnityEngine;

// How this enemy hurts the player. Right now it only does contact damage - the player
// loses health while the enemy is touching them. Phase 2 grows this same script into
// the windup/lunge/ranged attacks driven by config.attackType.
//
// Put this on the enemy, next to Health and EnemyAI. It works with the setup you already
// have - a solid (non-trigger) collider on both the enemy and the player - and also works
// if you later switch the enemy to a trigger collider, since it listens for both kinds
// of contact message.
//
// The damage numbers come from the same EnemyConfig the AI uses, so Easy/Medium/Hard
// automatically hit for different amounts.
[RequireComponent(typeof(Health))]
public class EnemyAttack : MonoBehaviour
{
    [Tooltip("Leave empty - it is taken from the EnemyAI on this object automatically.")]
    [SerializeField] EnemyConfig config;

    Health health;

    // Counts down after each hit. While it's above zero we refuse to deal damage again,
    // so leaning on an enemy costs you one hit per interval instead of ~50 hits a second.
    float cooldownTimer;

    void Awake()
    {
        health = GetComponent<Health>();

        // No config dragged in? Borrow the one the AI is already using.
        if (config == null)
        {
            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null)
            {
                config = ai.Config;
            }
        }
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    // Unity calls these for us. Enter fires on the first frame of contact, Stay keeps
    // firing while the two stay pressed together - we want damage in both cases.
    void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.collider);
    void OnCollisionStay2D(Collision2D collision) => TryDamage(collision.collider);
    void OnTriggerEnter2D(Collider2D other) => TryDamage(other);
    void OnTriggerStay2D(Collider2D other) => TryDamage(other);

    void TryDamage(Collider2D other)
    {
        // A dead enemy doesn't hurt anyone.
        if (health.IsDead) return;

        // Still recovering from the last hit.
        if (cooldownTimer > 0f) return;

        // We only bump the player. Walls, props and other enemies are ignored.
        if (!other.CompareTag("Player")) return;

        // Without a config we don't know how hard to hit, so do nothing.
        if (config == null) return;

        // Find whatever on the player can take damage. GetComponentInParent covers the case
        // where the collider sits on a child object and Health lives on the root.
        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;
        if (target.IsDead) return;

        target.TakeDamage(config.contactDamage);
        cooldownTimer = config.contactDamageInterval;
    }
}
