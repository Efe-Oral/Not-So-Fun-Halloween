using System.Collections.Generic;
using UnityEngine;

// Put this on the SWORD sprite object (the one with the BoxCollider2D).
// The collider must have "Is Trigger" checked. It only hits enemies while the sword is
// swinging, and hits each enemy at most once per swing.
public class SwordHitbox : MonoBehaviour
{
    Collider2D hitboxCollider;

    // Enemies we already hit during this swing, so we don't hit them twice.
    readonly HashSet<EnemyHitFlash> alreadyHit = new HashSet<EnemyHitFlash>();

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.enabled = false;   // off until a swing starts
    }

    // SwordController calls this when a swing starts.
    public void BeginSwing()
    {
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
        // Only react to enemies (objects that have the EnemyHitFlash script).
        EnemyHitFlash enemy = other.GetComponent<EnemyHitFlash>();
        if (enemy == null) return;

        // Don't hit the same enemy twice in one swing.
        if (alreadyHit.Contains(enemy)) return;

        alreadyHit.Add(enemy);
        enemy.Flash();
    }
}
